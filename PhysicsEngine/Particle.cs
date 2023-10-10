using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace PhysicsEngine
{
    public class Particle : Component
    {
        private Ellipse _ellipse = new Ellipse();

        private Coord pos;
        public Coord Position
        {
            get => pos;
            set
            {
                pos = value;
                Canvas.SetLeft(_ellipse, pos.X - radius);
                Canvas.SetTop(_ellipse, pos.Y - radius);
            }
        }

        private double radius;
        public double Radius
        {
            get => radius;
            set
            {
                radius = value;
                _ellipse.Width = radius * 2.0;
                _ellipse.Height = radius * 2.0;
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
                if (_ellipse.Fill != FillBrush)
                    _ellipse.Fill = FillBrush;
            }
        }

        public double Mass { get; set; }

        public Particle()
        {
            _ellipse.Tag = this;
            Position = new Coord(0, 0);
            Radius = 5.0;
            Mass = 1.0;
            Fill = Colors.Red;
        }
        public Particle(Coord position, double radius = 5.0)
        {
            _ellipse.Tag = this;
            Position = position;
            Radius = radius;
            Mass = 1.0;
            Fill = Colors.Red;
        }


        public override Shape GetUIElement() => _ellipse;
        public override void Update()
        {
            base.Update();

            //This is temporarily for testing, remove once physics is added
            Position = new Coord(Position.X + 5, Position.Y);
        }

    }
}
