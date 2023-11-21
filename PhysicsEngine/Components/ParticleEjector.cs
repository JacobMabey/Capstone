using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Radios;
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

        private SolidColorBrush PausedStrokeBrush { get; set; }

        private bool isPaused = false;
        public bool IsPaused
        {
            get => isPaused;
            set
            {
                isPaused = value;
                if (PausedStrokeBrush == null)
                    PausedStrokeBrush = new SolidColorBrush(Colors.Black);

                if (isPaused)
                    PausedStrokeBrush.Color = Color.FromArgb(255, 125, 20, 12);
                else
                    PausedStrokeBrush.Color = Colors.Black;
                if (_rect.Stroke != PausedStrokeBrush)
                    _rect.Stroke = PausedStrokeBrush;
            }
        }

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

        public static Size EJECTOR_SIZE { get; private set; } = new Size(20, 50);

        private Color fill;
        public override Color Fill
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
                    Fill = ParticleColor;
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

                if (Scene.CompMenu.IsMenuExpanded && Scene.CompMenu.ParentComponent.ID == this.ID)
                {
                    double roundedAngle = rotationAngle;
                    while (roundedAngle < 0) roundedAngle += 360;
                    while (roundedAngle > 359) roundedAngle -= 359;
                    Scene.CompMenu.RotateInput.Text = ((int)(roundedAngle * 1000.0) / 1000.0) + "";
                }
            }
        }

        public double ParticleScatterAngle { get; set; }
        public double ParticleRate { get; set; }
        public double ParticleVelocity { get; set; }
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

        private int particlesEjected = 0;
        public int ParticlesEjected
        {
            get => particlesEjected;
            private set
            {
                particlesEjected = value;

                if (Scene.CompMenu.IsMenuExpanded && Scene.CompMenu.ParentComponent.ID == this.ID)
                {
                    Scene.CompMenu.ParticlesEjectedValue.Text = particlesEjected + "";
                }
            }
        }
        public int ParticleLimit { get; set; }
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

                if (FillColorIsBasedOnParticle)
                    Fill = particleFill;

                if (Scene.CompMenu.IsMenuExpanded && Scene.CompMenu.ParentComponent.ID == this.ID)
                {
                    Scene.CompMenu.EjectorParticleColorPicker.SetColor(particleFill);
                }
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
            _rect.Tapped += Rect_Tapped;
            _rect.RightTapped += Rect_RightTapped;
            RotationTransform = new RotateTransform();
            RotationTransform.CenterX = EJECTOR_SIZE.Width / 2.0;
            RotationTransform.CenterY = EJECTOR_SIZE.Height / 2.0;
            _rect.RenderTransform = RotationTransform;
            PausedStrokeBrush = new SolidColorBrush(Colors.Black);
            _rect.Stroke = PausedStrokeBrush;
            _rect.StrokeThickness = 3.0;
        }

        public ParticleEjector(Coord position, double rotationAngle, int particleLimit, double ratePerSecond = 3.0, double particleVelocity = 10.0)
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
            ParticleElasticity = 0.8;
            ParticleFriction = 0.0;
            ParticleRadius = 5.0;
            ParticleRadiusRange = 0.0;
            ParticlesEjected = 0;
            ParticleTimer = 0;
            FillColorIsBasedOnParticle = true;
            Fill = Colors.Black;
            ParticleColor = Colors.Red;
        }

        public void ResetParticlesEjected() { ParticlesEjected = 0; }

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
            if (IsBeingAdded)
                OpenCompMenu();

            IsBeingDragged = false;
            IsBeingAdded = false;
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
        private void Rect_Tapped(object sender, TappedRoutedEventArgs e)
        {
            IsMouseDragMode = false;
            IsBeingAdded = false;
            IsBeingDragged = false;
            _rect.Opacity = 1.0;
            _rect.ReleasePointerCaptures();

            OpenCompMenu();
        }
        private void Rect_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            IsPaused = !IsPaused;
        }



        public override Shape GetUIElement() => _rect;

        public override Component Clone()
        {
            ParticleEjector clone = new ParticleEjector(Position, RotationAngle, ParticleLimit, ParticleRate, ParticleVelocity);
            clone.ParticleElasticity = ParticleElasticity;
            clone.ParticleFriction = ParticleFriction;
            clone.ColorChangeRate = ColorChangeRate;
            clone.ParticleColorChangeRate = ParticleColorChangeRate;
            clone.Fill = Fill;
            clone.ParticleColor = ParticleColor;
            clone.FillColorIsBasedOnParticle = FillColorIsBasedOnParticle;
            clone.ParticleRadius = ParticleRadius;
            clone.ParticleRadiusRange = ParticleRadiusRange;
            clone.ParticleScatterAngle = ParticleScatterAngle;
            clone.ParticlesEjected = ParticlesEjected;
            clone.IsPaused = IsPaused;

            return clone;
        }

        public override string GetSaveText()
        {
            string output = "comp_ejector\n";

            output += "RotationAngle:" + RotationAngle + "\n";
            output += "ParticleLimit:" + ParticleLimit + "\n";
            output += "ParticleRate:" + ParticleRate + "\n";
            output += "ParticleVelocity:" + ParticleVelocity + "\n";
            output += "ParticleElasticity:" + ParticleElasticity + "\n";
            output += "ParticleFriction:" + ParticleFriction + "\n";
            output += "ColorChangeRate:" + ColorChangeRate + "\n";
            output += "ParticleColorChangeRate:" + ParticleColorChangeRate + "\n";
            output += "Fill:" + Fill.A + "," + Fill.R + "," + Fill.G + "," + Fill.B + "\n";
            output += "ParticleColor:" + ParticleColor.A + "," + ParticleColor.R + "," + ParticleColor.G + "," + ParticleColor.B + "\n";
            output += "FillColorIsBasedOnParticle:" + FillColorIsBasedOnParticle + "\n";
            output += "ParticleRadius:" + ParticleRadius + "\n";
            output += "ParticleRadiusRange:" + ParticleRadiusRange + "\n";
            output += "ParticleScatterAngle:" + ParticleScatterAngle + "\n";
            output += "ParticlesEjected:" + ParticlesEjected + "\n";
            output += "Position:" + Position.X + "," + Position.Y + "\n";

            output += "-\n";
            return output;
        }

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
                Fill = ParticleColor;


            //Set Eject Velocity
            double ejectAngle = RotationAngle;
            if (ParticleScatterAngle != 0)
                ejectAngle += ParticleScatterAngle * (Scene.Rand.NextDouble() * 2.0 - 1.0);

            double rotationRadians = ejectAngle * Math.PI / 180.0;
            particle.Phys.ApplyForce(new Coord(Math.Cos(rotationRadians) * ParticleVelocity, Math.Sin(rotationRadians) * ParticleVelocity));

            //Create
            if (Scene.AddLater(particle))
                ParticlesEjected++;
        }
    }
}
