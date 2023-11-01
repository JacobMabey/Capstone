using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Devices.Geolocation;
using Windows.Devices.Radios;
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
        public static double GravityAcceleration { get; set; } = 300;

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
                    //if (Math.Abs(Velocity.X) < Epsilon) Velocity = new Coord(0, Velocity.Y);
                    //if (Math.Abs(Velocity.Y) < Epsilon) Velocity = new Coord(Velocity.X, 0);

                    //Update Friction
                    ApplyForce(new Coord(-Math.Sign(Velocity.X) * (Friction * Timer.DeltaTime), -Math.Sign(Velocity.Y) * (Friction * Timer.DeltaTime)));

                    //Update Velocity & Gravity
                    Acceleration = new Coord(Acceleration.X * Timer.DeltaTime * Timer.DeltaTime, ((Acceleration.Y + GravityAcceleration) * Timer.DeltaTime * Timer.DeltaTime) / Timer.TimeScale);
                    Velocity = new Coord(Velocity.X + Acceleration.X, Velocity.Y + Acceleration.Y);

                    //Update Position
                    Coord newPosition = new Coord(((Particle)Parent).Position.X + Velocity.X, ((Particle)Parent).Position.Y + Velocity.Y);

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
            if (!(Parent is Particle) || double.IsNaN(Parent.Phys.Velocity.X))
                return newPosition;

            Particle parent = ((Particle)Parent);

            Coord oldPosition = ((Particle)Parent).Position;

            //Used to check for collision with a flat surface (line, rect)
            bool IsColliding = false;

            //Used to check collision again after collision was detected with a line or rect
            //Prevents particles clipping through surface intersections
            bool CheckAgain = true;

            //If position didn't move, no collision
            if (oldPosition.X == newPosition.X && oldPosition.Y == newPosition.Y) return newPosition;



            //If scene has a circle border constraint, update position with it first
            if (Scene.IsCircleBorderActive)
                UpdatePositionWithCircleBorderConstraint(oldPosition, newPosition, parent.Radius);


            //Loop through all particles and Update Position
            foreach (Component comp in Scene.Children.Values)
            {
                if (!comp.IsCollisionEnabled || double.IsNaN(comp.Phys.Velocity.X))
                    continue;

                //Particle Collision
                if (!(comp is Particle))
                    continue;
                Particle particle = (Particle)comp;
                if (particle.Equals(parent))
                    continue;


                //Get particle distance
                double particleDistance = GetDistance(oldPosition, particle.Position);
                if (particleDistance > (parent.Radius + particle.Radius))
                    continue;

                /*
                double p1Velocity = GetLength(Velocity);
                double p2Velocity = GetLength(Scene.Children[particle.ID].Phys.Velocity);

                double p1MoveAngle = GetAngle(new Coord(0, 0), Velocity);
                double p2MoveAngle = GetAngle(new Coord(0, 0), Scene.Children[particle.ID].Phys.Velocity);

                double contactAngle = GetAngle(oldPosition, Scene.Children[particle.ID].Position);


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
                newPosition = new Coord(oldPosition.X + Velocity.X, oldPosition.Y + Velocity.Y);

                //Scene.Children[particle.ID].Phys.Velocity = new Coord(p2NewVelX, p2NewVelY);
                //Scene.Children[particle.ID].IsCollisionEnabled = false;
                    */





                /*double particleCollisionLength = (parent.Radius + particle.Radius) - particleDistance;

                double moveDistance = particleCollisionLength / 2.0;
                double p1Angle = GetAngle(particle.Position, oldPosition);
                Coord p1NewPoint = MovePoint(parent.Position, moveDistance, p1Angle);
                Velocity = new Coord((p1NewPoint.X - oldPosition.X) * Elasticity, (p1NewPoint.Y - oldPosition.Y) * Elasticity);
                newPosition = new Coord(oldPosition.X + Velocity.X, oldPosition.Y + Velocity.Y);

                double p2Angle = p1Angle + Math.PI;
                Coord p2NewPoint = MovePoint(particle.Position, moveDistance, p2Angle);
                Scene.Children[particle.ID].Phys.Velocity = new Coord((p2NewPoint.X - particle.Position.X) * Scene.Children[particle.ID].Phys.Elasticity, (p2NewPoint.Y - particle.Position.Y) * Scene.Children[particle.ID].Phys.Elasticity);
                */



                    
                /*Coord particletoParticle = new Coord(oldPosition.X - particle.Position.X, oldPosition.Y - particle.Position.Y);
                Coord direction = new Coord(particletoParticle.X / particleDistance, particletoParticle.Y / particleDistance);
                double MoveDistance = (parent.Radius + particle.Radius) - particleDistance;
                Velocity = new Coord(
                    (MoveDistance * direction.X / 2.0),
                    (MoveDistance * direction.Y / 2.0) + (GravityAcceleration * Timer.DeltaTime * Timer.DeltaTime / Timer.TimeScale)
                );
                newPosition = new Coord(oldPosition.X + Velocity.X, oldPosition.Y + Velocity.Y);
                //Velocity = new Coord(newPosition.X - oldPosition.X, newPosition.Y - oldPosition.Y);
                */

                /*
                Velocity = new Coord(
                    2.0 * (Mass * Velocity.X + particle.Phys.Mass * particle.Phys.Velocity.X) / (Mass + particle.Phys.Mass),
                    2.0 * (Mass * Velocity.Y + particle.Phys.Mass * particle.Phys.Velocity.Y) / (Mass + particle.Phys.Mass)
                );

                Scene.Children[particle.ID].Phys.Velocity = new Coord(
                    2.0 * (particle.Phys.Mass * particle.Phys.Velocity.X + Mass * Velocity.X) / (Mass + particle.Phys.Mass),
                    2.0 * (particle.Phys.Mass * particle.Phys.Velocity.Y + Mass * Velocity.Y) / (Mass + particle.Phys.Mass)
                );
                */

                    
                double moveDistance = Math.Sqrt(particleDistance);
                //double p1Velocity = GetLength(Velocity);
                //double p2Velocity = GetLength(Scene.Children[particle.ID].Phys.Velocity);
                double p1Radius = parent.Radius;
                double p2Radius = particle.Radius;
                double delta = p1Radius * (1.0 / (p1Radius * p2Radius)) * (p2Radius - moveDistance);

                Velocity = new Coord(
                    (Velocity.X * Elasticity + ((oldPosition.X - particle.Position.X) / moveDistance) * delta),
                    (Velocity.Y * Elasticity + ((oldPosition.Y - particle.Position.Y) / moveDistance) * delta)
                );
                newPosition = new Coord(oldPosition.X + Velocity.X, oldPosition.Y + Velocity.Y);// + (GravityAcceleration * Timer.DeltaTime * Timer.DeltaTime / Timer.TimeScale));
                //Scene.Children[particle.ID].Phys.Velocity = new Coord(
                //    ((oldPosition.X - particle.Position.X) / moveDistance) * delta,
                //    -((oldPosition.Y - particle.Position.Y) / moveDistance) * delta
                //);
                    

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


            //Loop through all lines and rectangles and Update Position
            while (CheckAgain) {
                CheckAgain = false;
                foreach (Component comp in Scene.Children.Values.Where(c => !(c is Particle) && (c is CompLine ? GetDistance(newPosition, ((CompLine)c).PosA, ((CompLine)c).PosB) : GetDistance(newPosition, c.Position)) <= parent.Radius * 2.0)) {
                    Coord intersection = new Coord(0, 0);
                    IsColliding = false;

                    //Line Collision
                    if (comp is CompLine)
                    {
                        CompLine line = (CompLine)comp;

                        //Get continueos lines intersection point
                        Coord pointA = line.PosA;
                        Coord pointB = line.PosB;
                        intersection = GetIntersectionPoint(oldPosition, newPosition, pointA, pointB);
                        if (intersection.X is double.NaN || intersection.Y is double.NaN)
                            continue;

                        //Coord movedPosition = MovePoint(newPosition, parent.Radius, GetAngle(oldPosition, newPosition));
                        Coord movedPosition = newPosition;
                        //Check if intersection point is within both lines
                        if (intersection.X < Math.Min(oldPosition.X, movedPosition.X) || intersection.X > Math.Max(oldPosition.X, movedPosition.X) ||
                            intersection.X < Math.Min(pointA.X, pointB.X) || intersection.X > Math.Max(pointA.X, pointB.X) ||
                            intersection.Y < Math.Min(oldPosition.Y, movedPosition.Y) || intersection.Y > Math.Max(oldPosition.Y, movedPosition.Y) ||
                            intersection.Y < Math.Min(pointA.Y, pointB.Y) || intersection.Y > Math.Max(pointA.Y, pointB.Y))
                        {
                            continue; //no collision
                        }

                        //Collision is guarenteed at this point
                        IsColliding = true;

                        //Set new position to the reflected point
                        //Coord reflectPos = MovePoint(GetReflectedPosition(oldPosition, movedPosition, pointA, pointB, intersection), parent.Radius, GetAngle(newPosition, oldPosition));
                        Coord reflectPos = GetReflectedPosition(oldPosition, movedPosition, pointA, pointB, intersection);
                        if (reflectPos.X is double.NaN || reflectPos.Y is double.NaN)
                            continue;

                        newPosition = reflectPos;
                        //oldPosition = intersection;
                    }
                    //Rectangle Collision
                    else if (comp is CompRectangle)
                    {
                        CompRectangle rect = (CompRectangle)comp;

                        //check if new position is inside rect
                        //rotate point by rect origin by the amount the rect is rotated
                        //Coord movedPosition = MovePoint(newPosition, parent.Radius, GetAngle(oldPosition, newPosition));
                        Coord movedPosition = newPosition;
                        Coord rotatedPosition = movedPosition;
                        if (rect.RotationAngle != 0)
                            rotatedPosition = RotatePointAroundPoint(movedPosition, rect.Position, -rect.RotationAngle);

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
                            double moveLength = GetLength(new Coord(oldPosition.X - movedPosition.X, oldPosition.Y - movedPosition.Y));

                            //Find intersection point that is at less distance than moveLength to find intersected side
                            bool IntersectFound = false;
                            Coord topIntersect = GetIntersectionPoint(oldPosition, movedPosition, pointTL, pointTR);
                            double topDist = GetLength(new Coord(oldPosition.X - topIntersect.X, oldPosition.Y - topIntersect.Y));
                            if (topDist <= moveLength)
                            {
                                intersection = topIntersect;
                                pointA = pointTL;
                                pointB = pointTR;
                                IntersectFound = true;
                                goto IntersectionFound;
                            }

                            Coord rightIntersect = GetIntersectionPoint(oldPosition, movedPosition, pointTR, pointBR);
                            double rightDist = GetLength(new Coord(oldPosition.X - rightIntersect.X, oldPosition.Y - rightIntersect.Y));
                            if (rightDist <= moveLength)
                            {
                                intersection = rightIntersect;
                                pointA = pointTR;
                                pointB = pointBR;
                                IntersectFound = true;
                                goto IntersectionFound;
                            }

                            Coord bottomIntersect = GetIntersectionPoint(oldPosition, movedPosition, pointBL, pointBR);
                            double bottomDist = GetLength(new Coord(oldPosition.X - bottomIntersect.X, oldPosition.Y - bottomIntersect.Y));
                            if (bottomDist <= moveLength)
                            {
                                intersection = bottomIntersect;
                                pointA = pointBL;
                                pointB = pointBR;
                                IntersectFound = true;
                                goto IntersectionFound;
                            }

                            Coord leftIntersect = GetIntersectionPoint(oldPosition, movedPosition, pointTL, pointBL);
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
                            //Coord reflectPos = MovePoint(GetReflectedPosition(oldPosition, movedPosition, pointA, pointB, intersection), parent.Radius, GetAngle(newPosition, oldPosition));
                            Coord reflectPos = GetReflectedPosition(oldPosition, movedPosition, pointA, pointB, intersection);
                            if (reflectPos.X is double.NaN || reflectPos.Y is double.NaN) continue;

                            if (IntersectFound)
                            {
                                //Collision is guarenteed at this point
                                IsColliding = true;

                                newPosition = reflectPos;
                                //oldPosition = intersection;
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

                        if (velocityLength < 1)
                        {
                            Velocity = new Coord(0, 0);
                        }

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
                                VelocityDirection.Y * Math.Abs(Velocity.Y) + (GravityAcceleration * Timer.DeltaTime * Timer.DeltaTime / Timer.TimeScale)
                            );
                        }


                        //Add collision dampining due to elasticity
                        Velocity = new Coord(Velocity.X * Elasticity, Velocity.Y * Elasticity);

                        CheckAgain = true;
                        break;
                    }
                }
            }


            return newPosition;
        }


        private Coord UpdatePositionWithCircleBorderConstraint(Coord oldPosition, Coord newPosition, double parentRadius)
        {
            Coord WindowCenter = new Coord(Scene.MainScene.Width / 2.0, Scene.MainScene.Height / 2.0);
            double ConstraintRadius = Scene.GetCircleBorderRadius();
            Coord toCenter = new Coord(newPosition.X - WindowCenter.X, newPosition.Y - WindowCenter.Y);
            double centerDistance = GetLength(toCenter);
            if (centerDistance > ConstraintRadius - parentRadius)
            {
                Coord MoveDirection = new Coord(toCenter.X / centerDistance, toCenter.Y / centerDistance);
                Velocity = new Coord(
                    Velocity.X * Elasticity + (WindowCenter.X + MoveDirection.X * (ConstraintRadius - parentRadius) - newPosition.X),
                    Velocity.Y * Elasticity + (WindowCenter.Y + MoveDirection.Y * (ConstraintRadius - parentRadius) - newPosition.Y)
                );
                newPosition = new Coord(oldPosition.X + Velocity.X, oldPosition.Y + Velocity.Y);
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

        private double GetDistance(Coord point, Coord pointA, Coord pointB)
        {
            double dist = Math.Abs((pointB.X - pointA.X) * (pointA.Y - point.Y) - (pointA.X - point.X) * (pointB.Y - pointA.Y)) / Math.Sqrt(Math.Pow(pointB.X - pointA.X, 2) + Math.Pow(pointB.Y - pointA.Y, 2));
            return dist;
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
