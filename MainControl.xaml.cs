using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VMS.TPS.Common.Model.API;

namespace LiverVeff
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class MainControl : UserControl
    {
        public MainControl()
        {
            InitializeComponent();
        }

        public void DrawDVH(DVHData dvhData)
        {
            // Calculate multipliers for scaling DVH to canvas.
            double xCoeff = MainCanvas.Width / dvhData.MaxDose.Dose;
            double yCoeff = MainCanvas.Height / 100;

            // Set Y axis label
            //DoseMaxLabel.Content = string.Format("{0:F0}%", dvhData.MaxDose.Dose);

            // Draw  histogram 
            for (int i = 0; i < dvhData.CurveData.Length - 1; i++)
            {
                // Set drawing line parameters
                var line = new Line() { Stroke = Brushes.Blue, StrokeThickness = 3.0 };

                // Set line coordinates
                line.X1 = dvhData.CurveData[i].DoseValue.Dose * xCoeff;
                line.X2 = dvhData.CurveData[i + 1].DoseValue.Dose * xCoeff;
                // Y axis start point is top-left corner of window, convert it to bottom-left.
                line.Y1 = MainCanvas.Height - dvhData.CurveData[i].Volume * yCoeff;
                line.Y2 = MainCanvas.Height - dvhData.CurveData[i + 1].Volume * yCoeff;

                // Add line to the existing canvas
                MainCanvas.Children.Add(line);
            }
        }
        public void DrawDiffDVH(double[] DiffdvhdosecGy, double[] Diffdvhvolumepercent)
        {
            int bins = DiffdvhdosecGy.Length - 1;
            // Calculate multipliers for scaling DVH to canvas.
            double xCoeff = MainCanvas.Width / DiffdvhdosecGy[bins];
            double yCoeff = MainCanvas.Height / 100;

           
            // Draw  histogram 
            for (int i = 0; i < bins - 1; i++)
            {
                // Set drawing line parameters
                var line = new Line() { Stroke = Brushes.Red, StrokeThickness = 3.0 };

                // Set line coordinates
                line.X1 = DiffdvhdosecGy[i] * xCoeff;
                line.X2 = DiffdvhdosecGy[i + 1] * xCoeff;
                // Y axis start point is top-left corner of window, convert it to bottom-left.
                line.Y1 = MainCanvas.Height - Diffdvhvolumepercent[i] * yCoeff;
                line.Y2 = MainCanvas.Height - Diffdvhvolumepercent[i + 1] * yCoeff;

                // Add line to the existing canvas
                MainCanvas.Children.Add(line);
            }
        }
    }
}
