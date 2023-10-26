using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Shapes;

namespace PhysicsEngine
{
    public class Physics
    {
        public static double GravityAcceleration { get; set; } = 0;

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
                    if (Math.Abs(Velocity.X) < Epsilon) Velocity = new Coord(0, Velocity.Y);
                    if (Math.Abs(Velocity.Y) < Epsilon) Velocity = new Coord(Velocity.X, 0);

                    //Update Friction
                    ApplyForce(new Coord(-Math.Sign(Velocity.X) * (Friction * Timer.DeltaTime), -Math.Sign(Velocity.Y) * (Friction * Timer.DeltaTime)));

                    //Update Velocity & Gravity
                    Velocity = new Coord(Velocity.X + (Acceleration.X * Timer.DeltaTime), Velocity.Y + ((Acceleration.Y + GravityAcceleration) * Timer.DeltaTime) / Timer.TimeScale);

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
            force = new Coord(force.X * 1000.0, force.Y * 1000.0);
            Acceleration = new Coord(Acceleration.X + force.X / (Mass * Timer.TimeScale), Acceleration.Y + force.Y / (Mass * Timer.TimeScale));
        }


        private Coord UpdatePositionOnCollison(Coord newPosition)
        {
            if (!(Parent is Particle)) return newPosition;

            Coord oldPosition = ((Particle)Parent).Position;

            //If position didn't move, no collision
            if (oldPosition.X == newPosition.X && oldPosition.Y == newPosition.Y) return newPosition;

            foreach (Component comp in Scene.Children.Values)
            {
                if (!comp.IsCollisionEnabled)
                    continue;

                Coord intersection = new Coord(0, 0);
                bool IsColliding = false;

                //Particle Collision
                if (comp is Particle)
                {
                    Particle parent = ((Particle)Parent);
                    Particle particle = (Particle)comp;
                    if (particle.Equals(parent)) continue;

                    //Get particle distance
                    double particleDistance = GetLength(new Coord(newPosition.X - particle.Position.X, newPosition.Y - particle.Position.Y));
                    if (particleDistance > parent.Radius + particle.Radius)
                        continue;

                    //Collision is guarenteed at this point
                    IsColliding = true;


                    double p1Velocity = GetLength(Velocity);
                    double p2Velocity = GetLength(Scene.Children[particle.ID].Phys.Velocity);

                    double p1MoveAngle = GetAngle(new Coord(0, 0), Velocity);
                    double p2MoveAngle = GetAngle(new Coord(0, 0), Scene.Children[particle.ID].Phys.Velocity);

                    double contactAngle = GetAngle(newPosition, Scene.Children[particle.ID].Position);


                    //Particle 1 new Velocities
                    double p1NewVelX = ((p1Velocity * Math.Cos(p1MoveAngle - contactAngle) * (Mass - Scene.Children[particle.ID].Phys.Mass) + 2.0 * Scene.Children[particle.ID].Phys.Mass * p2Velocity * Math.Cos(p2MoveAngle - contactAngle)) /
                        (Mass + Scene.Children[particle.ID].Phys.Mass)) * Math.Cos(contactAngle) + p1Velocity * Math.Sin(p1MoveAngle - contactAngle) * Math.Cos(contactAngle + Math.PI / 2.0);

                    double p1NewVelY = ((p1Velocity * Math.Cos(p1MoveAngle - contactAngle) * (Mass - Scene.Children[particle.ID].Phys.Mass) + 2.0 * Scene.Children[particle.ID].Phys.Mass * p2Velocity * Math.Cos(p2MoveAngle - contactAngle)) /
                        (Mass + Scene.Children[particle.ID].Phys.Mass)) * Math.Sin(contactAngle) + p1Velocity * Math.Sin(p1MoveAngle - contactAngle) * Math.Sin(contactAngle + Math.PI / 2.0);

                    //Particle 2 new Velocities
                    double p2NewVelX = ((p2Velocity * Math.Cos(p2MoveAngle - contactAngle) * (Scene.Children[particle.ID].Phys.Mass - Mass) + 2.0 * Mass * p1Velocity * Math.Cos(p1MoveAngle - contactAngle)) /
                        (Scene.Children[particle.ID].Phys.Mass + Mass)) * Math.Cos(contactAngle) + p2Velocity * Math.Sin(p2MoveAngle - contactAngle) * Math.Cos(contactAngle + Math.PI / 2.0);

                    double p2NewVelY = ((p2Velocity * Math.Cos(p2MoveAngle - contactAngle) * (Scene.Children[particle.ID].Phys.Mass - Mass) + 2.0 * Mass * p1Velocity * Math.Cos(p1MoveAngle - contactAngle)) /
                        (Scene.Children[particle.ID].Phys.Mass + Mass)) * Math.Sin(contactAngle) + p2Velocity * Math.Sin(p2MoveAngle - contactAngle) * Math.Sin(contactAngle + Math.PI / 2.0);

                    Velocity = new Coord(p1NewVelX, p1NewVelY);
                    Scene.Children[particle.ID].Phys.Velocity = new Coord(p2NewVelX, p2NewVelY);
                    //Scene.Children[particle.ID].IsCollisionEnabled = false;
                    /*double particleCollisionLength = (parent.Radius + particle.Radius) - particleDistance;

                    double moveDistance = particleCollisionLength / 2.0;
                    double p1Angle = GetAngle(particle.Position, newPosition);
                    Coord p1NewPoint = MovePoint(parent.Position, moveDistance, p1Angle);
                    ApplyForce(new Coord((p1NewPoint.X - newPosition.X) * Elasticity, (p1NewPoint.Y - newPosition.Y) * Elasticity));

                    double p2Angle = p1Angle + Math.PI;
                    Coord p2NewPoint = MovePoint(particle.Position, moveDistance, p2Angle);
                    Scene.Children[particle.ID].Phys.ApplyForce(new Coord((p2NewPoint.X - particle.Position.X) * Scene.Children[particle.ID].Phys.Elasticity, (p2NewPoint.Y - particle.Position.Y) * Scene.Children[particle.ID].Phys.Elasticity));
                    */



                    //Set current particle Velocity
                    /*Velocity = new Coord(
                        (Velocity.X * (parent.Phys.Mass - particle.Phys.Mass) + 2.0 * particle.Phys.Mass * particle.Phys.Velocity.X) / (parent.Phys.Mass + particle.Phys.Mass),
                        (Velocity.Y * (parent.Phys.Mass - particle.Phys.Mass) + 2.0 * particle.Phys.Mass * particle.Phys.Velocity.Y) / (parent.Phys.Mass + particle.Phys.Mass)
                    );

                    //Set other particle velocity
                    Scene.Children[particle.ID].Phys.Velocity = new Coord(
                        (particle.Phys.Velocity.X * (particle.Phys.Mass - parent.Phys.Mass) + 2.0 * parent.Phys.Mass * parent.Phys.Velocity.X) / (parent.Phys.Mass + particle.Phys.Mass),
                        (particle.Phys.Velocity.Y * (particle.Phys.Mass - parent.Phys.Mass) + 2.0 * parent.Phys.Mass * parent.Phys.Velocity.Y) / (parent.Phys.Mass + particle.Phys.Mass)
                    );
                    */
                }
                //Line Collision
                else if (comp is CompLine)
                {
                    CompLine line = (CompLine)comp;

                    //Get continueos lines intersection point
                    //double lineSlope = 0;
                    //if (line.PosA.X < line.PosB.X)
                    //    lineSlope = GetSlope(line.PosA, line.PosB);
                    //else
                    //    lineSlope = GetSlope(line.PosB, line.PosA);
                    //double lineSlope = GetAngle(line.PosA, line.PosB);
                    //double addX = -Math.Sign(Velocity.X) * Math.Cos(Math.Atan(-1 / lineSlope)) * ((Particle)Parent).Radius;
                    //double addY = Math.Sign(Velocity.Y) * Math.Sin(Math.Atan(-1 / lineSlope)) * ((Particle)Parent).Radius;


                    double addX = 0;
                    double addY = 0;
                    Coord pointA = new Coord(line.PosA.X + addX, line.PosA.Y + addY);
                    Coord pointB = new Coord(line.PosB.X + addX, line.PosB.Y + addY);
                    intersection = GetIntersectionPoint(oldPosition, newPosition, pointA, pointB);
                    if (intersection.X is double.NaN || intersection.Y is double.NaN)
                        continue;

                    //Check if intersection point is within both lines
                    if (intersection.X < Math.Min(oldPosition.X, newPosition.X) || intersection.X > Math.Max(oldPosition.X, newPosition.X) ||
                        intersection.X < Math.Min(pointA.X, pointB.X) || intersection.X > Math.Max(pointA.X, pointB.X) ||
                        intersection.Y < Math.Min(oldPosition.Y, newPosition.Y) || intersection.Y > Math.Max(oldPosition.Y, newPosition.Y) ||
                        intersection.Y < Math.Min(pointA.Y, pointB.Y) || intersection.Y > Math.Max(pointA.Y, pointB.Y))
                    {
                        continue; //no collision
                    }

                    //Collision is guarenteed at this point
                    IsColliding = true;

                    //Set new position to the reflected point
                    Coord reflectPos = GetReflectedPosition(oldPosition, newPosition, pointA, pointB, intersection);
                    if (reflectPos.X is double.NaN || reflectPos.Y is double.NaN)
                        continue;

                    newPosition = reflectPos;
                    oldPosition = intersection;
                }
                //Rectangle Collision
                else if (comp is CompRectangle)
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
                            oldPosition = intersection;
                        }
                    }

                }

                //If collision is detected, update velocity
                if (IsColliding && !(comp is Particle))
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
                        Velocity = new Coord(
                            (VelocityDirection.X == 0 ? 1 : VelocityDirection.X) * Math.Abs(Velocity.X),
                            (VelocityDirection.Y == 0 ? 1 : VelocityDirection.Y) * Velocity.Y
                        );
                    }
                    else if (newParticleSlope == double.PositiveInfinity || newParticleSlope == double.NegativeInfinity)
                    {
                        Velocity = new Coord(
                            (VelocityDirection.X == 0 ? 1 : VelocityDirection.X) * Velocity.X,
                            (VelocityDirection.Y == 0 ? 1 : VelocityDirection.Y) * Math.Abs(Velocity.Y)
                        );
                    }
                    else
                    {
                        double newVelocityY = Math.Sqrt((newParticleSlope * newParticleSlope * velocityLength * velocityLength) / (1.0 + newParticleSlope * newParticleSlope));
                        Velocity = new Coord(newVelocityY / newParticleSlope, newVelocityY);

                        //reset velocity direction
                        Velocity = new Coord(
                            VelocityDirection.X * Math.Abs(Velocity.X),
                            VelocityDirection.Y * Math.Abs(Velocity.Y)
                        );
                    }


                    //Add collision dampining due to elasticity
                    Velocity = new Coord(Velocity.X * Elasticity, Velocity.Y * Elasticity);
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

        private double GetPerpendicularSlope(double slope)
        {
            if (slope == 0) return double.NegativeInfinity;
            if (double.IsInfinity(slope)) return 0.0;

            return -1.0 / slope;
        }

        /// <summary>
        /// Gets the slope of a line with two given points
        /// </summary>
        /// <param name="pointA"></param>
        /// <param name="pointB"></param>
        /// <returns>The line slope</returns>
        private double GetAngle(Coord pointA, Coord pointB)
        {
            double slope = GetSlope(pointA, pointB);
            if (slope == 0) return Math.PI;
            if (double.IsPositiveInfinity(slope)) return -Math.PI / 2.0;
            if (double.IsNegativeInfinity(slope)) return Math.PI / 2.0;
            
            double angle = -Math.Atan(slope);
            if (pointB.X < pointA.X)
                angle = angle + Math.PI;

            while (angle < 0.0)
                angle += Math.PI * 2.0;

            while (angle > Math.PI * 2.0)
                angle -= Math.PI * 2.0;

            return angle;
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
        /// Gets the distance between 2 given points
        /// </summary>
        /// <param name="pointA"></param>
        /// <param name="pointB"></param>
        /// <returns>Point distance from each other</returns>
        private double GetDistance(Coord pointA, Coord pointB)
        {
            return Math.Sqrt(Math.Pow(pointB.X - pointA.X, 2.0) + Math.Pow(pointB.Y - pointA.Y, 2.0));
        }

        /// <summary>
        /// Moves a point at a given distance at a given angle
        /// </summary>
        /// <param name="point"></param>
        /// <param name="distance"></param>
        /// <param name="angle"></param>
        /// <returns>Moved point</returns>
        private Coord MovePoint(Coord point, double distance, double angle)
        {
            return new Coord(point.X + Math.Cos(angle) * distance, point.Y + (-Math.Sin(angle)) * distance);
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
            double particleAngle = GetAngle(oldPosition, newPosition);
            double lineAngle = GetAngle(linePointA, linePointB);

            double diffAngle = Math.Abs(particleAngle - lineAngle);

            if (particleAngle > lineAngle) diffAngle = -diffAngle;
            double newParticleAngle = particleAngle + (2.0 * diffAngle);

            double passedIntersectionLength = GetDistance(intersection, newPosition);
            
            return MovePoint(intersection, passedIntersectionLength, newParticleAngle);

            //double xstep = Math.Cos(newParticleAngle);
            //double ystep = -Math.Sin(newParticleAngle);
            //return new Coord(intersection.X + xstep * passedIntersectionLength, intersection.Y + ystep * passedIntersectionLength);

            //Get reflect slope and position
            //Get particle slope
            /*double particleSlope = GetSlope(oldPosition, newPosition);

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
            */
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
