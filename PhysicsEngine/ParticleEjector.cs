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
    public class ParticleEjector : Component
    {
        private Rectangle _rect = new Rectangle();
        private RotateTransform RotationTransform { get; set; }

        public bool IsPaused { get; set; } = false;

        private Coord posiiton;
        public Coord Position
        {
            get => posiiton;
            set
            {
                posiiton = value;
                Canvas.SetLeft(_rect, posiiton.X);
                Canvas.SetTop(_rect, posiiton.Y);
            }
        }

        private readonly Size EJECTOR_SIZE = new Size(50, 20);

        private Color fill;
        public Color FillColor
        {
            get => fill;
            set
            {
                fill = value;
                if (FillBrush == null)
                    FillBrush = new SolidColorBrush();
                FillBrush.Color = fill;
                if (_rect.Fill != FillBrush)
                    _rect.Fill = FillBrush;
            }
        }

        private double rotationAngle = 0.0;
        public double RotationAngle
        {
            get => rotationAngle;
            set
            {
                rotationAngle = value;
                RotationTransform.Angle = rotationAngle;
            }
        }

        public double ParticleRate { get; private set; }
        public double ParticleVelocity { get; private set; }
        public double ParticleElasticity { get; set; }
        public double ParticleRadius { get; set; }
        private int ParticlesEjected { get; set; }
        public int ParticleLimit { get; private set; }
        private double ParticleTimer { get; set; }

        private void Initialize()
        {
            _rect.Tag = this;
            _rect.PointerPressed += Rect_PointerPressed;
            _rect.PointerReleased += Rect_PointerReleased;
            _rect.PointerMoved += Rect_PointerMoved;
            RotationTransform = new RotateTransform();
            RotationTransform.CenterX = EJECTOR_SIZE.Width / 2.0;
            RotationTransform.CenterY = EJECTOR_SIZE.Height / 2.0;
            _rect.RenderTransform = RotationTransform;
        }

        public ParticleEjector(Coord position, double rotationAngle, int particleLimit, double ratePerSecond = 3.0, double particleVelocity = 0.5)
        {
            Initialize();
            Position = position;
            Canvas.SetZIndex(_rect, 1);
            _rect.Width = EJECTOR_SIZE.Width;
            _rect.Height = EJECTOR_SIZE.Height;
            RotationAngle = rotationAngle;
            ParticleLimit = particleLimit;
            ParticleRate = ratePerSecond;
            ParticleVelocity = particleVelocity;
            ParticleElasticity = 1.0;
            ParticleRadius = 5.0;
            ParticlesEjected = 0;
            ParticleTimer = 0;
            FillColor = Colors.Black;
        }

        private void Rect_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            IsBeingDragged = true;
            _rect.CapturePointer(e.Pointer);

            //Get position of pointer relative to shape movement center for smoother pickups
            Point pointerCoord = e.GetCurrentPoint(MainPage.MainScene).Position;
            PointerDragPoint = new Coord(pointerCoord.X - Position.X, pointerCoord.Y - Position.Y);

            //Drag mode on if user hold control
            if (Window.Current.CoreWindow.GetKeyState(VirtualKey.Control) == CoreVirtualKeyStates.Down)
            {
                IsMouseDragMode = true;
                _rect.Opacity = 0.6;
            }
        }
        private void Rect_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IsBeingDragged = false;
            _rect.ReleasePointerCapture(e.Pointer);
            IsMouseDragMode = false;
            _rect.Opacity = 1.0;
        }
        private void Rect_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!IsBeingDragged) return;

            Point pointerCoord = e.GetCurrentPoint(MainPage.MainScene).Position;

            double posx = pointerCoord.X - PointerDragPoint.X;
            double posy = pointerCoord.Y - PointerDragPoint.Y;
            if (MainPage.IsSnappableGridEnabled && IsMouseDragMode)
            {
                posx = Math.Round(posx / MainPage.SnapCellSize) * MainPage.SnapCellSize;
                posy = Math.Round(posy / MainPage.SnapCellSize) * MainPage.SnapCellSize;
            }
            Position = new Coord(posx, posy);
        }



        public override Shape GetUIElement() => _rect;

        public override void Update()
        {
            base.Update();

            //If particle limit has not been reached
            if (ParticlesEjected < ParticleLimit)
            {
                //if ejector is not paused, add to timer
                if (!IsPaused) ParticleTimer += Timer.DeltaTime;

                //if timer is reached, reset timer and eject particle
                if (ParticleTimer > 1000.0 / ParticleRate)
                {
                    ParticleTimer = 0;

                    Particle particle = new Particle(new Coord(Position.X + EJECTOR_SIZE.Width / 2.0, Position.Y + EJECTOR_SIZE.Height / 2.0), ParticleRadius);
                    particle.Phys.Elasticity = ParticleElasticity;

                    //Set Eject Velocity
                    double rotationRadians = RotationAngle * Math.PI / 180.0;
                    particle.Phys.ApplyForce(new Coord(Math.Sin(rotationRadians) * ParticleVelocity, Math.Cos(rotationRadians) * ParticleVelocity));

                    //Create
                    MainPage.MainScene.Children.Add(particle.GetUIElement());

                    ParticlesEjected++;
                }
            }
        }
    }
}
