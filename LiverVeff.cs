using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;


// TODO: Replace the following version attributes by creating AssemblyInfo.cs. You can do this in the properties of the Visual Studio project.
[assembly: AssemblyVersion("1.0.0.1")]
[assembly: AssemblyFileVersion("1.0.0.1")]
[assembly: AssemblyInformationalVersion("1.0")]

// TODO: Uncomment the following line if the script requires write access.
// [assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS
{
  public class Script
  {
        public Script()
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        

        public void Execute(ScriptContext scriptContext, Window mainWindow /*, System.Windows.Window window, ScriptEnvironment environment*/)
        {
            Run(scriptContext.CurrentUser,
              scriptContext.Patient,
              scriptContext.Image,
              scriptContext.StructureSet,
              scriptContext.PlanSetup,
              scriptContext.PlansInScope,
              scriptContext.PlanSumsInScope,
              mainWindow);
        }

        public void Run(
            User user,
            Patient patient,
            Image image,
            StructureSet structureSet,
            PlanSetup planSetup,
            IEnumerable<PlanSetup> planSetupsInScope,
            IEnumerable<PlanSum> planSumsInScope,
            Window window)

        {
           
            // If there's no selected plan with calculated dose throw an exception
            if (planSetup == null || planSetup.Dose == null)
                throw new ApplicationException("Please open a calculated plan before using this script.");

            // Retrieve StructureSet
            structureSet = planSetup.StructureSet;
            if (structureSet == null)
                throw new ApplicationException("The selected plan does not reference a StructureSet.");

            //****** will need to be Liver-GTV   ************** //
            Structure Liver = structureSet.Structures.Where(s => s.Id.Equals("Liver")).Single();    
            

            // Retrieve DVH data
            DVHData dvhData = planSetup.GetDVHCumulativeData(Liver,
                                          DoseValuePresentation.Absolute,
                                          VolumePresentation.Relative, 1);  //can decrease resolution to .1 if desired
            if (dvhData == null)
                throw new ApplicationException("DVH data does not exist. Script execution cancelled.");

            //Reformat DVH data into array
            int nbins = dvhData.CurveData.Length - 1;
            double[] dvhdosecGy = new double[nbins];
            double[] dvhvolumepercent = new double[nbins];
            for (int i = 0; i < nbins; i++)
            {
                dvhdosecGy[i] = dvhData.CurveData[i].DoseValue.Dose;
                dvhvolumepercent[i] = dvhData.CurveData[i].Volume;
            }

            //Convert Cumulative DVH to Differential DVH
            double[] DiffdvhdosecGy = dvhdosecGy;
            double[] Diffdvhvolumepercent = new double[nbins];
            for (int i = 0; i < nbins - 1; i++)
            {
                Diffdvhvolumepercent[i] = dvhvolumepercent[i] - dvhvolumepercent[i + 1];  //Reference Clinical Radiotherapy Physics with MATLAB: A Problem-Solving Approach By Pavel Dvorak
            }

            

            /*
            // Add existing WPF control to the script window.  Uncomment to Plot Cumulative and Differential DVH
            var mainControl = new LiverVeff.MainControl();
            window.Content = mainControl;
            window.Width = 610;
            window.Height = 460;

            window.Title = "Plan : " + planSetup.Id + ", Structure : " + Liver.Id;

            // Draw DVH
            mainControl.DrawDVH(dvhData);
            mainControl.DrawDiffDVH(DiffdvhdosecGy, Diffdvhvolumepercent);

            */

            //Mean Liver Dose
            var MeanLiverDose = dvhData.MeanDose;


            //Compute Veff
            double Dref_cGy = planSetup.TotalPrescribedDose.Dose;
            double Veffpercent = 0;
            for (int i = 0; i < nbins - 1; i++)
            {
                Veffpercent = Veffpercent + Diffdvhvolumepercent[i] * DiffdvhdosecGy[i] / Dref_cGy;    //Reference RTOG 1112
            }



            /*  this doesn't work... ignore until developed further

            //Iterative Prescription dose reduction
            if (Veffpercent > 25)
            {
                DrefNEW_cGy = 4500;
                double VeffNEWpercent = 0;
                for (int i = 0; i < nbins - 1; i++)
                {
                    VeffNEWpercentNEW = VeffNEWpercent + Diffdvhvolumepercent[i] * DiffdvhdosecGy[i] / DrefNEW_cGy;
                }

                if (VeffNEWpercent > 29)
                {
                    DrefNEW_cGy = 4000;
                    double VeffNEWpercent = 0;
                    for (int i = 0; i < nbins - 1; i++)
                    {
                        VeffNEWpercentNEW = VeffNEWpercent + Diffdvhvolumepercent[i] * DiffdvhdosecGy[i] / DrefNEW_cGy;
                    }

                    if (VeffNEWpercent > 34)
                    {
                        DrefNEW_cGy = 3500;
                        double VeffNEWpercent = 0;
                        for (int i = 0; i < nbins - 1; i++)
                        {
                            VeffNEWpercentNEW = VeffNEWpercent + Diffdvhvolumepercent[i] * DiffdvhdosecGy[i] / DrefNEW_cGy;
                        }

                        if (VeffNEWpercent > 44)
                        {
                            DrefNEW_cGy = 3000;
                            double VeffNEWpercent = 0;
                            for (int i = 0; i < nbins - 1; i++)
                            {
                                VeffNEWpercentNEW = VeffNEWpercent + Diffdvhvolumepercent[i] * DiffdvhdosecGy[i] / DrefNEW_cGy;
                            }

                            if (VeffNEWpercent > 54)
                            {
                                DrefNEW_cGy = 2750;
                                double VeffNEWpercent = 0;
                                for (int i = 0; i < nbins - 1; i++)
                                {
                                    VeffNEWpercentNEW = VeffNEWpercent + Diffdvhvolumepercent[i] * DiffdvhdosecGy[i] / DrefNEW_cGy;
                                }
                                if (VeffNEWpercent > 64)
                                {
                                    string Flag = "ineligible";
                                }
                            }
                        }
                    }
                }


           */
                




            //Report Calculation results
            string msg = "Veff = " + Veffpercent.ToString() + "%" + "\n" 
                + "Dref = " + Dref_cGy.ToString() + " cGy" + "\n"
                + "Mean Liver Dose = " + MeanLiverDose.ToString();

                           
            MessageBox.Show(msg);
        }
  }
}

