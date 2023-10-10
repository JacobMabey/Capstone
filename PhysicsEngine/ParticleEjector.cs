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

        private int ParticlesEjected { get; set; }
        public int ParticleLimit { get; private set; }
        private double ParticleTimer { get; set; }


        public ParticleEjector(Coord position, double rotationAngle, int particleLimit, double ratePerSecond = 3.0)
        {
            _rect.Tag = this;
            Position = position;
            Canvas.SetZIndex(_rect, 1);
            _rect.Width = EJECTOR_SIZE.Width;
            _rect.Height = EJECTOR_SIZE.Height;
            RotationTransform = new RotateTransform();
            RotationTransform.CenterX = EJECTOR_SIZE.Width / 2.0;
            RotationTransform.CenterY = EJECTOR_SIZE.Height / 2.0;
            RotationAngle = rotationAngle;
            ParticleLimit = particleLimit;
            ParticleRate = ratePerSecond;
            ParticlesEjected = 0;
            ParticleTimer = 0;
            FillColor = Colors.Black;
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

                    Particle particle = new Particle(new Coord(Position.X + EJECTOR_SIZE.Width / 2.0, Position.Y + EJECTOR_SIZE.Height / 2.0));
                    //Set a force of velocity HERE (including rotational angle)
                    MainPage.MainScene.Children.Add(particle.GetUIElement());

                    ParticlesEjected++;
                }
            }
        }
    }
}
