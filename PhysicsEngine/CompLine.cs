using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI;

namespace PhysicsEngine
{
    public class CompLine
    {
        private SolidColorBrush FillColor { get; set; }
        public Line UILine { get; private set; }

        private Point posA;
        public Point PosA
        {
            get => posA;
            set
            {
                posA = value;
                UILine.X1 = posA.X;
                UILine.Y1 = posA.Y;
            }
        }

        private Point posB;
        public Point PosB
        {
            get => posB;
            set
            {
                posB = value;
                UILine.X2 = posB.X;
                UILine.Y2 = posB.Y;
            }
        }

        private double thickness;
        public double Thickness
        {
            get => thickness;
            set
            {
                thickness = value;
                UILine.StrokeThickness = thickness;
            }
        }

        private Color fill;
        public Color Fill
        {
            get => fill;
            set
            {
                fill = value;
                if (FillColor == null)
                    FillColor = new SolidColorBrush();
                FillColor.Color = fill;
                if (UILine.Stroke != FillColor)
                    UILine.Stroke = FillColor;
            }
        }

        public CompLine()
        {
            UILine = new Line();
            PosA = new Point(0, 0);
            PosB = new Point(50, 50);
            Fill = Colors.Red;
            Thickness = 5.0;
        }

        public CompLine(Point posA, Point posB)
        {
            UILine = new Line();
            PosA = posA;
            PosB = posB;
            Fill = Colors.Red;
            Thickness = 5.0;
        }

        public CompLine(Point posA, Point posB, Color fill, double thickness = 5.0)
        {
            UILine = new Line();
            PosA = posA;
            PosB = posB;
            Fill = fill;
            Thickness = thickness;
            
        }


        public void Update()
        {

        }
    }
}
