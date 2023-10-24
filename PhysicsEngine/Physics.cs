using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
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
            IEnumerable<object> components = MainPage.MainScene.Children.Where(e => e is Shape).Select(e => ((Shape)e).Tag).Where(c => !(c is Particle));
            foreach (object comp in components)
            {
                if (!(Parent is Particle)) return newPosition;

                Coord oldPosition = ((Particle)Parent).Position;

                //If position didn't move, no collision
                if (oldPosition.X == newPosition.X && oldPosition.Y == newPosition.Y) return newPosition;


                Coord intersection = new Coord(0, 0);
                bool IsColliding = false;

                //Line Collision
                if (comp is CompLine && ((CompLine)comp).IsCollisionEnabled)
                {
                    CompLine line = (CompLine)comp;

                    //Get continueos lines intersection point
                    intersection = GetIntersectionPoint(oldPosition, newPosition, line.PosA, line.PosB);
                    if (intersection.X is double.NaN || intersection.Y is double.NaN)
                        continue;

                    //Check if intersection point is within both lines
                    if (intersection.X < Math.Min(oldPosition.X, newPosition.X) || intersection.X > Math.Max(oldPosition.X, newPosition.X) ||
                        intersection.X < Math.Min(line.PosA.X, line.PosB.X) || intersection.X > Math.Max(line.PosA.X, line.PosB.X) ||
                        intersection.Y < Math.Min(oldPosition.Y, newPosition.Y) || intersection.Y > Math.Max(oldPosition.Y, newPosition.Y) ||
                        intersection.Y < Math.Min(line.PosA.Y, line.PosB.Y) || intersection.Y > Math.Max(line.PosA.Y, line.PosB.Y))
                    {
                        continue; //no collision
                    }

                    //Collision is guarenteed at this point
                    IsColliding = true;

                    //Set new position to the reflected point
                    Coord reflectPos = GetReflectedPosition(oldPosition, newPosition, line.PosA, line.PosB, intersection);
                    if (reflectPos.X is double.NaN || reflectPos.Y is double.NaN) continue;

                    newPosition = reflectPos;
                }
                //Rectangle Collision
                else if (comp is CompRectangle && ((CompRectangle)comp).IsCollisionEnabled)
                {
                    CompRectangle rect = (CompRectangle)comp;

                    //check if new position is inside rect
                    //rotate point by rect origin by the amount the rect is rotated
                    Coord rotatedPosition = newPosition;
                    if (rect.RotationAngle != 0)
                        rotatedPosition = RotatePointAroundPoint(newPosition, rect.Position, -rect.RotationAngle);

                    //Check if new position is within the rectangle
                    if (new Rect(new Point(rect.Position.X, rect.Position.Y), rect.Size).Contains(new Point(rotatedPosition.X, rotatedPosition.Y)))
                    {

                        //Get all 4 rect corner points and rotate via ratation angle
                        Coord pointTL = rect.Position;
                        Coord pointTR = RotatePointAroundPoint(new Coord(rect.Position.X + rect.Size.Width, rect.Position.Y), rect.Position, rect.RotationAngle);
                        Coord pointBR = RotatePointAroundPoint(new Coord(rect.Position.X + rect.Size.Width, rect.Position.Y + rect.Size.Height), rect.Position, rect.RotationAngle);
                        Coord pointBL = RotatePointAroundPoint(new Coord(rect.Position.X, rect.Position.Y + rect.Size.Height), rect.Position, rect.RotationAngle);

                        //Will be set to the two points connected to the side of the rect the particle intersects with
                        Coord pointA = new Coord(0, 0);
                        Coord pointB = new Coord(0, 0);

                        //Get length of particle movement
                        double moveLength = GetLength(new Coord(oldPosition.X - newPosition.X, oldPosition.Y - newPosition.Y));

                        //Find intersection point that is at less distance than moveLength to find intersected side
                        bool IntersectFound = false;
                        Coord topIntersect = GetIntersectionPoint(oldPosition, newPosition, pointTL, pointTR);
                        double topDist = GetLength(new Coord(oldPosition.X - topIntersect.X, oldPosition.Y - topIntersect.Y));
                        if (topDist <= moveLength)
                        {
                            intersection = topIntersect;
                            pointA = pointTL;
                            pointB = pointTR;
                            IntersectFound = true;
                            goto IntersectionFound;
                        }

                        Coord rightIntersect = GetIntersectionPoint(oldPosition, newPosition, pointTR, pointBR);
                        double rightDist = GetLength(new Coord(oldPosition.X - rightIntersect.X, oldPosition.Y - rightIntersect.Y));
                        if (rightDist <= moveLength)
                        {
                            intersection = rightIntersect;
                            pointA = pointTR;
                            pointB = pointBR;
                            IntersectFound = true;
                            goto IntersectionFound;
                        }

                        Coord bottomIntersect = GetIntersectionPoint(oldPosition, newPosition, pointBL, pointBR);
                        double bottomDist = GetLength(new Coord(oldPosition.X - bottomIntersect.X, oldPosition.Y - bottomIntersect.Y));
                        if (bottomDist <= moveLength)
                        {
                            intersection = bottomIntersect;
                            pointA = pointBL;
                            pointB = pointBR;
                            IntersectFound = true;
                            goto IntersectionFound;
                        }

                        Coord leftIntersect = GetIntersectionPoint(oldPosition, newPosition, pointTL, pointBL);
                        double leftDist = GetLength(new Coord(oldPosition.X - leftIntersect.X, oldPosition.Y - leftIntersect.Y));
                        if (leftDist <= moveLength)
                        {
                            intersection = leftIntersect;
                            pointA = pointTL;
                            pointB = pointBL;
                            IntersectFound = true;
                            goto IntersectionFound;
                        }
                    IntersectionFound:

                        //Set new position to the reflected point
                        Coord reflectPos = GetReflectedPosition(oldPosition, newPosition, pointA, pointB, intersection);
                        if (reflectPos.X is double.NaN || reflectPos.Y is double.NaN) continue;

                        if (IntersectFound)
                        {
                            //Collision is guarenteed at this point
                            IsColliding = true;

                            newPosition = reflectPos;
                        }
                    }

                }

                //If collision is detected, update velocity
                if (IsColliding)
                {
                    //Get new particle slope
                    double newParticleSlope = GetSlope(intersection, newPosition);

                    //Reset Velocity
                    //Get velocity length
                    double velocityLength = GetLength(Velocity);
                    Coord VelocityDirection = new Coord(Math.Sign(newPosition.X - intersection.X), Math.Sign(newPosition.Y - intersection.Y));

                    //Check for perfectly verticle or horizontal lines
                    if (newParticleSlope == 0)
                    {
                        Velocity.X = (VelocityDirection.X == 0 ? 1 : VelocityDirection.X) * Math.Abs(Velocity.X);
                        Velocity.Y = (VelocityDirection.Y == 0 ? 1 : VelocityDirection.Y) * Velocity.Y;
                    }
                    else if (newParticleSlope == double.PositiveInfinity || newParticleSlope == double.NegativeInfinity)
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
            }
            return newPosition;
        }


        /// <summary>
        /// Gets the slope of a line with two given points
        /// </summary>
        /// <param name="pointA"></param>
        /// <param name="pointB"></param>
        /// <returns>The line slope</returns>
        private double GetSlope(Coord pointA, Coord pointB)
        {
            return (pointB.Y - pointA.Y) / (pointB.X - pointA.X);
        }

        /// <summary>
        /// Gets the length of the line from point 0,0 to given point
        /// </summary>
        /// <param name="point"></param>
        /// <returns>Point distance from (0,0)</returns>
        private double GetLength(Coord point)
        {
            return Math.Sqrt(Math.Pow(point.X, 2.0) + Math.Pow(point.Y, 2.0));
        }

        /// <summary>
        /// Rotates a point around another given point with a given angle
        /// </summary>
        /// <param name="point"></param>
        /// <param name="origin"></param>
        /// <param name="angle"></param>
        /// <returns>New rotated point</returns>
        private Coord RotatePointAroundPoint(Coord point, Coord origin, double angle)
        {
            double angleRad = angle * Math.PI / 180.0;
            return new Coord(
                (point.X - origin.X) * Math.Cos(angleRad) - (point.Y - origin.Y) * Math.Sin(angleRad) + origin.X,
                (point.Y - origin.Y) * Math.Cos(-angleRad) - (point.X - origin.X) * Math.Sin(-angleRad) + origin.Y
            );
        }


        /// <summary>
        /// Given a positional movement, collision line, and intersection point 
        /// between the two, gets a new position that is "newPosition" reflected 
        /// off the collision line
        /// </summary>
        /// <param name="oldPosition"></param>
        /// <param name="newPosition"></param>
        /// <param name="linePointA"></param>
        /// <param name="linePointB"></param>
        /// <param name="intersection"></param>
        /// <returns>position reflected off the given line</returns>
        private Coord GetReflectedPosition(Coord oldPosition, Coord newPosition, Coord linePointA, Coord linePointB, Coord intersection)
        {
            //Get reflect slope and position
            //Get particle slope
            double particleSlope = GetSlope(oldPosition, newPosition);

            //get line slope
            double lineSlope = 0;
            if (linePointA.X < linePointB.X)
                lineSlope = GetSlope(linePointA, linePointB);
            else
                lineSlope = GetSlope(linePointB, linePointA);

            //If slopes equal, lines are parallel and do not collide
            if (particleSlope == lineSlope)
                return new Coord(double.NaN, double.NaN);

            //get line y-intercept
            double lineYInt = linePointB.Y - lineSlope * linePointB.X;


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
            return new Coord(reflectIntersection.X + (reflectIntersection.X - newPosition.X), reflectIntersection.Y + (reflectIntersection.Y - newPosition.Y));
        }


        /// <summary>
        /// Gets the intersection point between two lines
        /// </summary>
        /// <param name="pointA"></param>
        /// <param name="pointB"></param>
        /// <param name="pointC"></param>
        /// <param name="pointD"></param>
        /// <returns>Intersection point</returns>
        private Coord GetIntersectionPoint(Coord pointA, Coord pointB, Coord pointC, Coord pointD)
        {
            //Get line A slope
            double lineASlope = GetSlope(pointA, pointB);

            //get line B slope
            double lineBSlope;
            if (pointC.X < pointD.X)
                lineBSlope = GetSlope(pointC, pointD);
            else
                lineBSlope = GetSlope(pointD, pointC);

            //If slopes equal, lines are parallel and do not collide
            if (lineASlope == lineBSlope)
                return new Coord(double.NaN, double.NaN);

            //get particle y-intercept
            double lineAYInt = pointB.Y - lineASlope * pointB.X;
            //get line y-intercept
            double lineBYInt = pointD.Y - lineBSlope * pointD.X;


            //Get continuous lines intersection point
            Coord intersection = new Coord(0, 0);
            if (lineBSlope == 0)
            {
                if (lineASlope == double.PositiveInfinity || lineASlope == double.NegativeInfinity)
                    intersection.X = pointB.X;
                else
                    intersection.X = (lineAYInt - lineBYInt) / (lineBSlope - lineASlope);

                intersection.Y = pointC.Y;
            }
            else if (lineBSlope == double.PositiveInfinity || lineBSlope == double.NegativeInfinity)
            {
                intersection.X = pointC.X;

                if (lineASlope == 0)
                    intersection.Y = pointB.Y;
                else
                    intersection.Y = lineASlope * intersection.X + lineAYInt;
            }
            else
            {
                if (lineASlope == double.PositiveInfinity || lineASlope == double.NegativeInfinity)
                {
                    intersection.X = pointB.X;
                    intersection.Y = lineBSlope * intersection.X + lineBYInt;
                }
                else if (lineASlope == 0)
                {
                    intersection.X = (lineAYInt - lineBYInt) / (lineBSlope - lineASlope);
                    intersection.Y = pointB.Y;
                }
                else
                {
                    intersection.X = (lineAYInt - lineBYInt) / (lineBSlope - lineASlope);
                    intersection.Y = lineASlope * intersection.X + lineAYInt;
                }
            }

            return intersection;
        }
    }
}
