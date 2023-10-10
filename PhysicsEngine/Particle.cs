using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
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


        private void Initialize()
        {
            _ellipse.Tag = this;
            _ellipse.PointerPressed += Ellipse_PointerPressed;
            _ellipse.PointerReleased += Ellipse_PointerReleased;
            _ellipse.PointerMoved += Ellipse_PointerMoved;
        }

        public Particle()
        {
            Initialize();
            Position = new Coord(0, 0);
            Radius = 5.0;
            Mass = 1.0;
            Fill = Colors.Red;
        }
        public Particle(Coord position, double radius = 5.0)
        {
            Initialize();
            Position = position;
            Radius = radius;
            Mass = 1.0;
            Fill = Colors.Red;
        }


        private void Ellipse_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            IsBeingDragged = true;
            _ellipse.Opacity = 0.6;
            _ellipse.CapturePointer(e.Pointer);
        }
        private void Ellipse_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IsBeingDragged = false;
            _ellipse.Opacity = 1.0;
            _ellipse.ReleasePointerCapture(e.Pointer);
        }
        private void Ellipse_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!IsBeingDragged) return;

            Point pointerCoord = e.GetCurrentPoint(MainPage.MainScene).Position;
            Position = new Coord(pointerCoord.X - PointerDragPoint.X, pointerCoord.Y - PointerDragPoint.Y);
        }


        public override Shape GetUIElement() => _ellipse;
        public override void Update()
        {
            base.Update();

            if (IsBeingDragged) return;


            //This is temporarily for testing, remove once physics is added
            Position = new Coord(Position.X + 5, Position.Y);
        }

    }
}
