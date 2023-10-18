using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Shapes;

namespace PhysicsEngine
{
    public class Physics
    {
        public static double GravityAcceleration { get; set; } = 0.02;

        private static readonly double Epsilon = 0.01;

        public static bool IsBorderEnclosed { get; set; } = true;

        public Component Parent { get; private set; }

        private Coord Acceleration { get; set; } = new Coord(0, 0);
        public Coord Velocity { get; set; } = new Coord(0, 0);

        /// <summary>
        /// Sets dampening of forces
        /// </summary>
        private double friction = 0.1;
        public double Friction
        {
            get => friction;
            set
            {
                friction = value < 0.0 ? 0.0 : value;
            }
        }

        /// <summary>
        /// 0 to 1 Sets "bounce amount"
        /// 0 will not bounce, while 1 will lose no kenetic energy
        /// </summary>
        private double elasticity = 0.0;
        public double Elasticity
        {
            get => elasticity;
            set
            {
                elasticity = value < 0.0 ? 0.0 : value > 1.0 ? 1.0 : value;
            }
        }

        /// <summary>
        /// Will effect forces on the object.
        /// </summary>
        private double mass = 1.0;
        public double Mass
        {
            get => mass;
            set
            {
                mass = value < 1.0 ? 1.0 : value;
            }
        }

        public Physics(Component parent)
        {
            Parent = parent;
        }

        public void Update()
        {
            try
            {
                if (Parent is Particle)
                {
                    if (Math.Abs(Velocity.X) < Epsilon) Velocity.X = 0;
                    if (Math.Abs(Velocity.Y) < Epsilon) Velocity.Y = 0;

                    //Update Friction
                    ApplyForce(new Coord(-Math.Sign(Velocity.X) * (Friction / 100.0), -Math.Sign(Velocity.Y) * (Friction / 100.0)));

                    //Update Velocity & Gravity
                    Velocity = new Coord(Velocity.X + Acceleration.X, Velocity.Y + Acceleration.Y + GravityAcceleration / Timer.TimeScale);

                    //Update Position
                    Coord step = new Coord(Velocity.X * Timer.DeltaTime, Velocity.Y * Timer.DeltaTime);
                    Coord newPosition = new Coord(((Particle)Parent).Position.X + step.X, ((Particle)Parent).Position.Y + step.Y);

                    //Check for collision in next position
                    newPosition = UpdatePositionOnCollison(newPosition);

                    //Set new Position
                    ((Particle)Parent).Position = newPosition;


                    //Update Acceleration
                    Acceleration = new Coord(0, 0);
                }
            }
            catch {}
            
        }

        public void ApplyForce(Coord force)
        {
            Acceleration = new Coord(Acceleration.X + force.X / (Mass * Timer.TimeScale), Acceleration.Y + force.Y / (Mass * Timer.TimeScale));
        }


        private Coord UpdatePositionOnCollison(Coord newPosition)
        {
            foreach (UIElement element in MainPage.MainScene.Children)
            {
                if (!(element is Shape)) continue;
                object comp = ((Shape)element).Tag;
                if (!(Parent is Particle)) continue;
                if (comp is CompLine)
                {
                    CompLine line = (CompLine)comp;
                    Coord oldPosition = ((Particle)Parent).Position;

                    //If position didn't move, lines are parallel and do not collide
                    if (oldPosition.X == newPosition.X && oldPosition.Y == newPosition.Y) continue;

                    //Get particle slope
                    double particleSlope = (newPosition.Y - oldPosition.Y) / (newPosition.X - oldPosition.X);
                    if (particleSlope == double.PositiveInfinity || particleSlope == double.NegativeInfinity)
                        particleSlope = 0;
                    //get line slope
                    double lineSlope = (line.PosB.Y - line.PosA.Y) / (line.PosB.X - line.PosA.X);
                    if (lineSlope == double.PositiveInfinity || lineSlope == double.NegativeInfinity)
                        lineSlope = 0;

                    //If slopes equal, lines are parallel and do not collide
                    if (particleSlope == lineSlope) continue;


                    //get particle y-intercept
                    double particleYInt = newPosition.Y - particleSlope * newPosition.X;
                    //get line y-intercept
                    double lineYInt = line.PosB.Y - lineSlope * line.PosB.X;


                    //Get continuous lines intersection point
                    Coord intersection = new Coord(0, 0);
                    intersection.X = (particleYInt - lineYInt) / (lineSlope - particleSlope);
                    intersection.Y = particleSlope * intersection.X + particleYInt;

                    //Check if intersection point is within lines end points
                    if (intersection.X < Math.Min(oldPosition.X, newPosition.X) || intersection.X > Math.Max(oldPosition.X, newPosition.X) ||
                        intersection.X < Math.Min(line.PosA.X, line.PosB.X) || intersection.X > Math.Max(line.PosA.X, line.PosB.X))
                    {
                        continue; //no collision
                    }
                    if (intersection.Y < Math.Min(oldPosition.Y, newPosition.Y) || intersection.Y > Math.Max(oldPosition.Y, newPosition.Y) ||
                        intersection.Y < Math.Min(line.PosA.Y, line.PosB.Y) || intersection.Y > Math.Max(line.PosA.Y, line.PosB.Y))
                    {
                        continue; //no collision
                    }

                    //Get reflection slope
                    double reflectSlope = particleSlope == 0 ? 0 : -1 / particleSlope;

                    //Get reflection y-intercept
                    double reflectYInt = intersection.Y - reflectSlope * intersection.X;

                    //Get reflection intersection point
                    Coord reflectIntersection = new Coord(0, 0);
                    reflectIntersection.X = (reflectYInt - lineYInt) / (lineSlope - reflectSlope);
                    reflectIntersection.Y = reflectSlope * reflectIntersection.X + reflectYInt;

                    //Get reflect move distance
                    Coord reflectMoveDistance = new Coord(reflectIntersection.X + (reflectIntersection.X - newPosition.X), reflectIntersection.Y + (reflectIntersection.Y - newPosition.Y));

                    //Set new position
                    newPosition = reflectMoveDistance;

                    //Get new particle slope
                    double newParticleSlope = (reflectMoveDistance.Y - intersection.Y) / (reflectMoveDistance.X - intersection.X);

                    //Reset Velocity
                    //Get velocity length
                    double velocityLength = Math.Sqrt(Math.Pow(Velocity.X, 2) + Math.Pow(Velocity.Y, 2));
                    double newAngle = Math.Atan(newParticleSlope);
                    Velocity = new Coord(Math.Cos(newAngle) * velocityLength, Math.Sin(newAngle) * velocityLength);
                }
                else if (comp is CompRectangle)
                {

                }
            }
            return newPosition;
        }
    }
}
