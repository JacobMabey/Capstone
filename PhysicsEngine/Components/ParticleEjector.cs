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

        private SolidColorBrush ParticleFillBrush { get; set; }

        public bool IsPaused { get; set; } = false;

        private Coord posiiton;
        public override Coord Position
        {
            get => posiiton;
            set
            {
                posiiton = value;
                Canvas.SetLeft(_rect, posiiton.X);
                Canvas.SetTop(_rect, posiiton.Y);
            }
        }

        private Size EJECTOR_SIZE { get; set; } = new Size(20, 50);

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

        private bool fillColorIsBasedOnParticle = false;
        public bool FillColorIsBasedOnParticle
        {
            get => fillColorIsBasedOnParticle;
            set
            {
                fillColorIsBasedOnParticle = value;
                if (fillColorIsBasedOnParticle)
                    FillColor = ParticleColor;
            }
        }

        private double rotationAngle = 0.0;
        public double RotationAngle
        {
            get => rotationAngle;
            set
            {
                rotationAngle = value;
                RotationTransform.Angle = rotationAngle + 90.0;
            }
        }

        public double ParticleScatterAngle { get; set; }
        public double ParticleRate { get; private set; }
        public double ParticleVelocity { get; private set; }
        public double ParticleElasticity { get; set; }
        public double ParticleFriction { get; set; }

        private double radius;
        public double ParticleRadius
        {
            get => radius;
            set
            {
                radius = value;
                _rect.Width = radius * 3.0;
                RotationTransform.CenterX = _rect.Width / 2.0;
            }
        }
        public double ParticleRadiusRange { get; set; }
        public int ParticlesEjected { get; private set; }
        public int ParticleLimit { get; private set; }
        private double ParticleTimer { get; set; }

        private Color particleFill;
        public Color ParticleColor
        {
            get => particleFill;
            set
            {
                particleFill = value;
                if (ParticleFillBrush == null)
                    ParticleFillBrush = new SolidColorBrush();
                ParticleFillBrush.Color = particleFill;
            }
        }

        public double ColorChangeRate { get; set; } = 0.0;
        public double ParticleColorChangeRate { get; set; } = 0.0;



        public override void Initialize()
        {
            base.Initialize();

            _rect.Tag = this;
            _rect.PointerPressed += Rect_PointerPressed;
            _rect.PointerReleased += Rect_PointerReleased;
            _rect.PointerMoved += Rect_PointerMoved;
            _rect.RightTapped += Rect_RightTapped;
            RotationTransform = new RotateTransform();
            RotationTransform.CenterX = EJECTOR_SIZE.Width / 2.0;
            RotationTransform.CenterY = EJECTOR_SIZE.Height / 2.0;
            _rect.RenderTransform = RotationTransform;
            _rect.Stroke = new SolidColorBrush(Colors.Black);
            _rect.StrokeThickness = 2.0;
        }

        public ParticleEjector(Coord position, double rotationAngle, int particleLimit, double ratePerSecond = 3.0, double particleVelocity = 5.0)
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
            ParticleScatterAngle = 0.0;
            ParticleElasticity = 1.0;
            ParticleFriction = 0.1;
            ParticleRadius = 5.0;
            ParticleRadiusRange = 0.0;
            ParticlesEjected = 0;
            ParticleTimer = 0;
            FillColorIsBasedOnParticle = true;
            FillColor = Colors.Black;
            ParticleColor = Colors.Red;
        }

        private void Rect_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            IsBeingDragged = true;
            _rect.CapturePointer(e.Pointer);

            //Get position of pointer relative to shape movement center for smoother pickups
            Point pointerCoord = e.GetCurrentPoint(Scene.MainScene).Position;

            //Get position of pointer relative to shape movement center for smoother pickups
            PointerDragPoint = new Coord(pointerCoord.X - Position.X, pointerCoord.Y - Position.Y);

            //Rotate mode on if user hold shift
            if (Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down))
            {
                IsMouseRotateMode = true;
            }
            //Drag mode on if user hold control
            else if (Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down))
            {
                IsMouseDragMode = true;
                _rect.Opacity = 0.6;
            }
        }
        private void Rect_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IsBeingDragged = false;
            _rect.ReleasePointerCapture(e.Pointer);
            IsMouseRotateMode = false;
            IsMouseDragMode = false;
            _rect.Opacity = 1.0;
        }
        private void Rect_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!IsBeingDragged) return;

            //Rotate mode on if user hold shift
            if (Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down))
            {
                IsMouseRotateMode = true;
            } else
            {
                IsMouseRotateMode = false;
            }
            //Drag mode on if user hold control
            if (!IsMouseRotateMode && Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down))
            {
                IsMouseDragMode = true;
                _rect.Opacity = 0.6;
            } else
            {
                IsMouseDragMode = false;
                _rect.Opacity = 1.0;
            }

            Point pointerCoord = e.GetCurrentPoint(Scene.MainScene).Position;

            //If rotation mode is active, only rotate
            if (IsMouseRotateMode)
            {
                RotationAngle = -Physics.GetAngle(new Coord(Position.X + EJECTOR_SIZE.Width / 2.0, Position.Y + EJECTOR_SIZE.Height / 2.0), Coord.FromPoint(pointerCoord)) * 180.0 / Math.PI + 180.0;
                return;
            }
            
            double posx = pointerCoord.X - PointerDragPoint.X;
            double posy = pointerCoord.Y - PointerDragPoint.Y;
            if (Scene.IsSnappableGridEnabled && IsMouseDragMode)
            {
                posx = Math.Round(posx / Scene.SnapCellSize) * Scene.SnapCellSize;
                posy = Math.Round(posy / Scene.SnapCellSize) * Scene.SnapCellSize;
            }
            Position = new Coord(posx, posy);
        }
        private void Rect_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            IsPaused = !IsPaused;
        }



        public override Shape GetUIElement() => _rect;

        public override void Update()
        {
            base.Update();

            //If particle limit has not been reached
            if (ParticlesEjected < ParticleLimit && Timer.FPS > 40)
            {
                //if ejector is not paused, add to timer
                if (!IsPaused) ParticleTimer += Timer.DeltaTime;

                //if timer is reached, reset timer and eject particle
                if (ParticleTimer > 1.0 / ParticleRate)
                {
                    ParticleTimer = 0;

                    EjectParticle();
                }
            }
        }

        public void EjectParticle()
        {
            //Get particle radius
            double radius = ParticleRadius;
            if (ParticleRadiusRange != 0.0)
                radius += ParticleRadiusRange * Scene.Rand.NextDouble();

            //Create Particle
            Particle particle = new Particle(new Coord(Position.X + EJECTOR_SIZE.Width / 2.0, Position.Y + EJECTOR_SIZE.Height / 2.0), radius);
            particle.Phys.Elasticity = ParticleElasticity;
            particle.Phys.Friction = ParticleFriction;
            particle.ColorChangeRate = ParticleColorChangeRate;

            //Set Particle Color
            particle.Fill = ParticleColor;
            if (ColorChangeRate != 0.0)
            {
                double[] HsvParticleColor = ColorFunctions.RgbToHsv(ParticleColor.R, ParticleColor.G, ParticleColor.B);
                HsvParticleColor[0] = (HsvParticleColor[0] + ColorChangeRate * Timer.TimeScale) % 360.0;
                double[] newColor = ColorFunctions.HsvToRgb(HsvParticleColor[0], HsvParticleColor[1], HsvParticleColor[2]);
                ParticleColor = Color.FromArgb(255, (byte)newColor[0], (byte)newColor[1], (byte)newColor[2]);
            }
            if (FillColorIsBasedOnParticle)
                FillColor = ParticleColor;


            //Set Eject Velocity
            double ejectAngle = RotationAngle;
            if (ParticleScatterAngle != 0)
                ejectAngle += ParticleScatterAngle * (Scene.Rand.NextDouble() * 2.0 - 1.0);

            double rotationRadians = ejectAngle * Math.PI / 180.0;
            particle.Phys.ApplyForce(new Coord(Math.Cos(rotationRadians) * ParticleVelocity, Math.Sin(rotationRadians) * ParticleVelocity));

            //Create
            Scene.AddLater(particle);

            ParticlesEjected++;
        }
    }
}
