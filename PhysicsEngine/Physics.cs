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
        public static double GravityAcceleration { get; set; } = 0.2;

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
        private double elasticity = 1.0;
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
                    Coord step = new Coord(Velocity.X, Velocity.Y);
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
                if (comp is CompLine && ((CompLine)comp).IsCollisionEnabled)
                {
                    CompLine line = (CompLine)comp;
                    Coord oldPosition = ((Particle)Parent).Position;

                    //If position didn't move, lines are parallel and do not collide
                    if (oldPosition.X == newPosition.X && oldPosition.Y == newPosition.Y) continue;

                    //Get particle slope
                    double particleSlope = GetSlope(oldPosition, newPosition);

                    //get line slope
                    double lineSlope = 0;
                    if (line.PosA.X < line.PosB.X)
                        lineSlope = GetSlope(line.PosA, line.PosB);
                    else
                        lineSlope = GetSlope(line.PosB, line.PosA);

                    //If slopes equal, lines are parallel and do not collide
                    if (particleSlope == lineSlope) continue;


                    //get particle y-intercept
                    double particleYInt = newPosition.Y - particleSlope * newPosition.X;
                    //get line y-intercept
                    double lineYInt = line.PosB.Y - lineSlope * line.PosB.X;

                    //Get continuous lines intersection point
                    Coord intersection = new Coord(0, 0);
                    if (lineSlope == 0)
                    {
                        if (particleSlope == double.PositiveInfinity || particleSlope == double.NegativeInfinity)
                            intersection.X = newPosition.X;
                        else
                            intersection.X = (particleYInt - lineYInt) / (lineSlope - particleSlope);

                        intersection.Y = line.PosA.Y;
                    }
                    else if(lineSlope == double.PositiveInfinity || lineSlope == double.NegativeInfinity)
                    {
                        intersection.X = line.PosA.X;

                        if (particleSlope == 0)
                            intersection.Y = newPosition.Y;
                        else
                            intersection.Y = particleSlope * intersection.X + particleYInt;
                    } else
                    {
                        if (particleSlope == double.PositiveInfinity || particleSlope == double.NegativeInfinity)
                        {
                            intersection.X = newPosition.X;
                            intersection.Y = lineSlope * intersection.X + lineYInt;
                        } else if (particleSlope == 0)
                        {
                            intersection.X = (particleYInt - lineYInt) / (lineSlope - particleSlope);
                            intersection.Y = newPosition.Y;
                        } else
                        {
                            intersection.X = (particleYInt - lineYInt) / (lineSlope - particleSlope);
                            intersection.Y = particleSlope * intersection.X + particleYInt;
                        }
                    }

                    //Check if intersection point is within lines end points
                    if (intersection.X == double.NaN || intersection.Y == double.NaN)
                        continue;

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


                    Coord reflectIntersection = new Coord(0, 0);
                    if (lineSlope == 0)
                    {
                        reflectIntersection = intersection;
                        reflectIntersection.X = newPosition.X;
                    }
                    else if (lineSlope == double.PositiveInfinity || lineSlope == double.NegativeInfinity)
                    {
                        reflectIntersection = intersection;
                        reflectIntersection.Y = newPosition.Y;
                    }
                    else
                    {
                        //Get reflection slope
                        double reflectSlope = -1.0 / lineSlope;

                        //Get reflection y-intercept
                        double reflectYInt = newPosition.Y - reflectSlope * newPosition.X;

                        //Get reflection intersection point
                        reflectIntersection.X = (reflectYInt - lineYInt) / (lineSlope - reflectSlope);
                        reflectIntersection.Y = reflectSlope * reflectIntersection.X + reflectYInt;
                    }

                    //Get reflect move distance
                    Coord reflectMoveDistance = new Coord(reflectIntersection.X + (reflectIntersection.X - newPosition.X), reflectIntersection.Y + (reflectIntersection.Y - newPosition.Y));

                    //Set new position
                    newPosition = reflectMoveDistance;

                    //Get new particle slope
                    double newParticleSlope = GetSlope(intersection, newPosition);

                    //Reset Velocity
                    //Get velocity length
                    double velocityLength = Math.Sqrt(Math.Pow(Velocity.X, 2.0) + Math.Pow(Velocity.Y, 2.0));
                    Coord VelocityDirection = new Coord(Math.Sign(newPosition.X - intersection.X), Math.Sign(newPosition.Y - intersection.Y));

                    //Check for perfectly verticle or horizontal lines
                    if (newParticleSlope == 0)
                    {
                        Velocity.X = (VelocityDirection.X == 0 ? 1 : VelocityDirection.X) * Math.Abs(Velocity.X);
                        Velocity.Y = (VelocityDirection.Y == 0 ? 1 : VelocityDirection.Y) * Velocity.Y;
                    } else if (newParticleSlope == double.PositiveInfinity || newParticleSlope == double.NegativeInfinity)
                    {
                        Velocity.X = (VelocityDirection.X == 0 ? 1 : VelocityDirection.X) * Velocity.X;
                        Velocity.Y = (VelocityDirection.Y == 0 ? 1 : VelocityDirection.Y) * Math.Abs(Velocity.Y);
                    }
                    else
                    {
                        Velocity.Y = Math.Sqrt((newParticleSlope * newParticleSlope * velocityLength * velocityLength) / (1.0 + newParticleSlope * newParticleSlope));
                        Velocity.X = Velocity.Y / newParticleSlope;

                        //reset velocity direction
                        Velocity.X = VelocityDirection.X * Math.Abs(Velocity.X);
                        Velocity.Y = VelocityDirection.Y * Math.Abs(Velocity.Y);
                    }


                    //Add collision dampining due to elasticity
                    Velocity.X *= Elasticity;
                    Velocity.Y *= Elasticity;
                }
                else if (comp is CompRectangle && ((CompRectangle)comp).IsCollisionEnabled)
                {

                }


            }
            return newPosition;
        }


        private double GetSlope(Coord pointA, Coord pointB)
        {
            double slope = (pointB.Y - pointA.Y) / (pointB.X - pointA.X);
            return slope;
        }
    }
}
