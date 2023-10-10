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
    public class CompLine : Component
    {
        private Line _line = new Line();

        private Coord posA;
        public Coord PosA
        {
            get => posA;
            set
            {
                posA = value;
                _line.X1 = posA.X;
                _line.Y1 = posA.Y;
            }
        }

        private Coord posB;
        public Coord PosB
        {
            get => posB;
            set
            {
                posB = value;
                _line.X2 = posB.X;
                _line.Y2 = posB.Y;
            }
        }

        private double thickness;
        public double Thickness
        {
            get => thickness;
            set
            {
                thickness = value;
                _line.StrokeThickness = thickness;
            }
        }

        private Color fill;
        public Color Fill
        {
            get => fill;
            set
            {
                fill = value;
                if (FillBrush == null)
                    FillBrush = new SolidColorBrush();
                FillBrush.Color = fill;
                if (_line.Stroke != FillBrush)
                    _line.Stroke = FillBrush;
            }
        }

        public CompLine()
        {
            _line.Tag = this;
            PosA = new Coord(0, 0);
            PosB = new Coord(50, 50);
            Fill = Colors.Red;
            Thickness = 5.0;
        }

        public CompLine(Coord posA, Coord posB)
        {
            _line.Tag = this;
            PosA = posA;
            PosB = posB;
            Fill = Colors.Red;
            Thickness = 5.0;
        }

        public CompLine(Coord posA, Coord posB, Color fill, double thickness = 5.0)
        {
            _line.Tag = this;
            PosA = posA;
            PosB = posB;
            Fill = fill;
            Thickness = thickness;
            
        }


        public override Shape GetUIElement() => _line;

        public override void Update()
        {
            base.Update();

        }

    }
}
