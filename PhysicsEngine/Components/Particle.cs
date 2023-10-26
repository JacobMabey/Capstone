using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
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
        public override Coord Position
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



        public override void Initialize()
        {
            base.Initialize();

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
            Fill = Colors.Red;
        }
        public Particle(Coord position, double radius = 5.0)
        {
            Initialize();
            Position = position;
            Radius = radius;
            Fill = Colors.Red;
        }


        private void Ellipse_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            IsBeingDragged = true;
            _ellipse.CapturePointer(e.Pointer);

            //Drag mode on if user hold control
            if (Window.Current.CoreWindow.GetKeyState(VirtualKey.Control) == CoreVirtualKeyStates.Down)
            {
                IsMouseDragMode = true;
                _ellipse.Opacity = 0.6;
            }
        }
        private void Ellipse_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IsBeingDragged = false;
            _ellipse.ReleasePointerCapture(e.Pointer);
            IsMouseDragMode = false;
            _ellipse.Opacity = 1.0;
        }
        private void Ellipse_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!IsBeingDragged) return;

            Point pointerCoord = e.GetCurrentPoint(Scene.MainScene).Position;

            double posx = pointerCoord.X - PointerDragPoint.X;
            double posy = pointerCoord.Y - PointerDragPoint.Y;
            if (Scene.IsSnappableGridEnabled && IsMouseDragMode)
            {
                posx = Math.Round(posx / Scene.SnapCellSize) * Scene.SnapCellSize;
                posy = Math.Round(posy / Scene.SnapCellSize) * Scene.SnapCellSize;
            }
            Position = new Coord(posx, posy);
        }


        public override Shape GetUIElement() => _ellipse;
        public override void Update()
        {
            base.Update();
        }

    }
}
