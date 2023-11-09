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
        public static double GravityAcceleration { get; set; } = 10;

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
            if (Timer.TimeScale == 0.0) return;

            try
            {
                if (Parent is Particle)
                {

                    //Update Friction
                    ApplyForce(new Coord(-Math.Sign(Velocity.X) * (Friction), -Math.Sign(Velocity.Y) * (Friction)));

                    //Update Velocity & Gravity
                    double grav = GravityAcceleration / 100.0;
                    Acceleration = new Coord(Acceleration.X * Timer.TimeScale, ((Acceleration.Y + grav)) * Timer.TimeScale);
                    Velocity = new Coord(Velocity.X + Acceleration.X, Velocity.Y + Acceleration.Y);

                    //Update Position
                    Coord newPosition = new Coord(((Particle)Parent).Position.X + Velocity.X * Timer.MovementMultiplier, ((Particle)Parent).Position.Y + Velocity.Y * Timer.MovementMultiplier);

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

        public void ApplyForce(Coord force, eForceType forceType = eForceType.IMPULSE)
        {
            switch (forceType)
            {
                default:
                case eForceType.IMPULSE:
                    force = new Coord(force.X, force.Y);
                    Acceleration = new Coord(Acceleration.X + force.X / (Mass * Timer.TimeScale), Acceleration.Y + force.Y / (Mass * Timer.TimeScale));
                    break;
                case eForceType.DIRECT:
                    Velocity = new Coord(Velocity.X + force.X, Velocity.Y + force.Y);
                    break;
            }
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


            //foreach (Component comp in Scene.Children.Values)
            List<Particle> checkList = new List<Particle>();
            checkList.AddRange(Scene.SpacePartitionGrid[Scene.CurrentCell]);

            //Get adjacent positions
            List<Coord> adjacentCells = new List<Coord>()
            {
                new Coord(Scene.CurrentCell.X - 1, Scene.CurrentCell.Y),
                new Coord(Scene.CurrentCell.X - 1, Scene.CurrentCell.Y - 1),
                new Coord(Scene.CurrentCell.X, Scene.CurrentCell.Y - 1),
                new Coord(Scene.CurrentCell.X + 1, Scene.CurrentCell.Y - 1),
                new Coord(Scene.CurrentCell.X + 1, Scene.CurrentCell.Y),
                new Coord(Scene.CurrentCell.X + 1, Scene.CurrentCell.Y + 1),
                new Coord(Scene.CurrentCell.X, Scene.CurrentCell.Y + 1),
                new Coord(Scene.CurrentCell.X - 1, Scene.CurrentCell.Y + 1)
            };
            foreach (Coord cell in adjacentCells)
            {
                if (Scene.SpacePartitionGrid.ContainsKey(cell))
                    checkList.AddRange(Scene.SpacePartitionGrid[cell]);
            }

            //Loop through all particles and Update Position
            foreach (Particle comp in checkList)
            {
                if (!comp.IsCollisionEnabled || double.IsNaN(comp.Phys.Velocity.X))
                    continue;

                Particle particle = (Particle)comp;
                if (particle.Equals(parent))
                    continue;


                //Get particle distance
                double particleDistance = GetDistance(newPosition, particle.Position);
                if (particleDistance > (parent.Radius + particle.Radius))
                    continue;


                double moveDistance = ((parent.Radius + particle.Radius) - particleDistance) / 2.0;
                Coord moveVector = MovePoint(new Coord(0, 0), moveDistance, GetAngle(particle.Position, newPosition));
                newPosition = new Coord(
                    newPosition.X + moveVector.X,
                    newPosition.Y + moveVector.Y
                );
                ApplyForce(moveVector, eForceType.DIRECT);
                /*Velocity = new Coord(
                    Velocity.X + moveVector.X,
                    Velocity.Y + moveVector.Y
                );*/
            }
            


            //Loop through all lines and rectangles and Update Position
            long lastCheckedId = -1;
            while (CheckAgain) {
                CheckAgain = false;
                foreach (Component comp in Scene.Children.Values)
                {
                    if (!comp.IsCollisionEnabled)
                        continue;

                    if (lastCheckedId == comp.ID)
                        continue;

                    Coord intersection = new Coord(0, 0);
                    IsColliding = false;

                    if (comp is Particle)
                        continue;

                    //Line Collision
                    if (comp is CompLine)
                    {
                        CompLine line = (CompLine)comp;

                        //Check for far away particles
                        if (GetDistance(newPosition, line.PosA, line.PosB) > 50)
                            continue;


                        //Check for collision on the end points of the line
                        double particleToLinePointDistance = GetDistance(newPosition, line.PosA);
                        Coord collidingPoint = line.PosA;
                        if (particleToLinePointDistance > (parent.Radius + 2.0))
                        {
                            particleToLinePointDistance = GetDistance(newPosition, line.PosB);
                            collidingPoint = line.PosB;
                        }

                        if (particleToLinePointDistance <= (parent.Radius + 2.0))
                        {
                            double moveDistance = ((parent.Radius + 2.0) - particleToLinePointDistance);
                            Coord moveVector = MovePoint(new Coord(0, 0), moveDistance, GetAngle(collidingPoint, newPosition));
                            newPosition = new Coord(
                                newPosition.X + moveVector.X,
                                newPosition.Y + moveVector.Y
                            );
                            ApplyForce(moveVector, eForceType.DIRECT);
                            /*Velocity = new Coord(
                                Velocity.X + moveVector.X,
                                Velocity.Y + moveVector.Y
                            );*/
                        }

                        //Get new virtual line position based on the particles radius
                        double lineToParticleAngle = GetAngle(line.PosA, line.PosB) + Math.PI / 2.0;
                        Coord pointA = MovePoint(line.PosA, parent.Radius + line.Thickness / 2.0, lineToParticleAngle);
                        Coord pointB = MovePoint(line.PosB, parent.Radius + line.Thickness / 2.0, lineToParticleAngle);
                        if (GetDistance(oldPosition, pointA, pointB) > GetDistance(oldPosition, line.PosA, line.PosB))
                        {
                            pointA = MovePoint(line.PosA, parent.Radius, lineToParticleAngle + Math.PI);
                            pointB = MovePoint(line.PosB, parent.Radius, lineToParticleAngle + Math.PI);
                        }

                        if (GetDistance(line.PosA, pointA) >= GetDistance(oldPosition, line.PosA, line.PosB))
                            continue;

                        //Get continueos lines intersection point
                        intersection = GetIntersectionPoint(oldPosition, newPosition, pointA, pointB);
                        if (intersection.X is double.NaN || intersection.Y is double.NaN)
                            continue;

                        //Coord movedPosition = MovePoint(newPosition, parent.Radius, GetAngle(oldPosition, newPosition));
                        Coord movedPosition = newPosition;
                        //Check if intersection point is within both lines
                        if (intersection.X < GetEpsilonRounded(Math.Min(oldPosition.X, movedPosition.X)) || intersection.X > GetEpsilonRounded(Math.Max(oldPosition.X, movedPosition.X)) ||
                            intersection.X < GetEpsilonRounded(Math.Min(pointA.X, pointB.X)) || intersection.X > GetEpsilonRounded(Math.Max(pointA.X, pointB.X)) ||
                            intersection.Y < GetEpsilonRounded(Math.Min(oldPosition.Y, movedPosition.Y)) || intersection.Y > GetEpsilonRounded(Math.Max(oldPosition.Y, movedPosition.Y)) ||
                            intersection.Y < GetEpsilonRounded(Math.Min(pointA.Y, pointB.Y)) || intersection.Y > GetEpsilonRounded(Math.Max(pointA.Y, pointB.Y)))
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

                        //Get all 4 rect corner points and rotate via ratation angle
                        Coord pointTL = rect.Position;
                        Coord pointTR = RotatePointAroundPoint(new Coord(rect.Position.X + rect.Size.Width, rect.Position.Y), rect.Position, rect.RotationAngle);
                        Coord pointBR = RotatePointAroundPoint(new Coord(rect.Position.X + rect.Size.Width, rect.Position.Y + rect.Size.Height), rect.Position, rect.RotationAngle);
                        Coord pointBL = RotatePointAroundPoint(new Coord(rect.Position.X, rect.Position.Y + rect.Size.Height), rect.Position, rect.RotationAngle);

                        //Check for collision on the corners of the rect
                        double particleToLinePointDistance = GetDistance(newPosition, pointTL);
                        Coord collidingPoint = pointTL;
                        if (particleToLinePointDistance > (parent.Radius + 2.0))
                        {
                            particleToLinePointDistance = GetDistance(newPosition, pointTR);
                            collidingPoint = pointTR;
                            if (particleToLinePointDistance > (parent.Radius + 2.0))
                            {
                                particleToLinePointDistance = GetDistance(newPosition, pointBR);
                                collidingPoint = pointBR;
                                if (particleToLinePointDistance > (parent.Radius + 2.0))
                                {
                                    particleToLinePointDistance = GetDistance(newPosition, pointBL);
                                    collidingPoint = pointBL;
                                }
                            }
                        }
                        if (particleToLinePointDistance <= (parent.Radius + 2.0))
                        {
                            double moveDistance = ((parent.Radius + 2.0) - particleToLinePointDistance);
                            Coord moveVector = MovePoint(new Coord(0, 0), moveDistance, GetAngle(collidingPoint, newPosition));
                            newPosition = new Coord(
                                newPosition.X + moveVector.X,
                                newPosition.Y + moveVector.Y
                            );
                            ApplyForce(moveVector, eForceType.DIRECT);
                            /*Velocity = new Coord(
                                Velocity.X + moveVector.X,
                                Velocity.Y + moveVector.Y
                            );*/
                        }

                        //Will be set to the two points connected to the side of the rect the particle intersects with
                        Coord pointA = new Coord(0, 0);
                        Coord pointB = new Coord(0, 0);

                        //Get length of particle movement
                        double moveLength = GetLength(new Coord(oldPosition.X - newPosition.X, oldPosition.Y - newPosition.Y));

                        //Find intersection point that is at less distance than moveLength to find intersected side
                        bool IntersectFound = false;

                        //Top line intersection
                        double lineToParticleAngle = GetAngle(pointTL, pointTR) + Math.PI / 2.0;
                        Coord movedPointTL = MovePoint(pointTL, parent.Radius, lineToParticleAngle);
                        Coord movedPointTR = MovePoint(pointTR, parent.Radius, lineToParticleAngle);
                        Coord topIntersect = GetIntersectionPoint(oldPosition, newPosition, movedPointTL, movedPointTR);
                        double topDist = GetLength(new Coord(oldPosition.X - topIntersect.X, oldPosition.Y - topIntersect.Y));
                        if (topDist <= moveLength)
                        {
                            if (GetDistance(pointTL, movedPointTL) > GetDistance(oldPosition, pointTL, pointTR))
                                continue;

                            intersection = topIntersect;
                            pointA = movedPointTL;
                            pointB = movedPointTR;
                            IntersectFound = true;
                            goto IntersectionFound;
                        }

                        //Right line intersection
                        lineToParticleAngle = GetAngle(pointTR, pointBR) + Math.PI / 2.0;
                        movedPointTR = MovePoint(pointTR, parent.Radius, lineToParticleAngle);
                        Coord movedPointBR = MovePoint(pointBR, parent.Radius, lineToParticleAngle);
                        Coord rightIntersect = GetIntersectionPoint(oldPosition, newPosition, movedPointTR, movedPointBR);
                        double rightDist = GetLength(new Coord(oldPosition.X - rightIntersect.X, oldPosition.Y - rightIntersect.Y));
                        if (rightDist <= moveLength)
                        {
                            if (GetDistance(pointTR, movedPointTR) > GetDistance(oldPosition, pointTR, pointBR))
                                continue;

                            intersection = rightIntersect;
                            pointA = movedPointTR;
                            pointB = movedPointBR;
                            IntersectFound = true;
                            goto IntersectionFound;
                        }

                        //Bottom line intersection
                        lineToParticleAngle = GetAngle(pointBL, pointBR) - Math.PI / 2.0;
                        Coord movedPointBL = MovePoint(pointBL, parent.Radius, lineToParticleAngle);
                        movedPointBR = MovePoint(pointBR, parent.Radius, lineToParticleAngle);
                        Coord bottomIntersect = GetIntersectionPoint(oldPosition, newPosition, movedPointBL, movedPointBR);
                        double bottomDist = GetLength(new Coord(oldPosition.X - bottomIntersect.X, oldPosition.Y - bottomIntersect.Y));
                        if (bottomDist <= moveLength)
                        {
                            if (GetDistance(pointBL, movedPointBL) > GetDistance(oldPosition, pointBL, pointBR))
                                continue;

                            intersection = bottomIntersect;
                            pointA = movedPointBL;
                            pointB = movedPointBR;
                            IntersectFound = true;
                            goto IntersectionFound;
                        }

                        //Left line intersection
                        lineToParticleAngle = GetAngle(pointTL, pointBL) - Math.PI / 2.0;
                        movedPointTL = MovePoint(pointTL, parent.Radius, lineToParticleAngle);
                        movedPointBL = MovePoint(pointBL, parent.Radius, lineToParticleAngle);
                        Coord leftIntersect = GetIntersectionPoint(oldPosition, newPosition, movedPointTL, movedPointBL);
                        double leftDist = GetLength(new Coord(oldPosition.X - leftIntersect.X, oldPosition.Y - leftIntersect.Y));
                        if (leftDist <= moveLength)
                        {
                            if (GetDistance(pointTL, movedPointTL) > GetDistance(oldPosition, pointTL, pointBL))
                                continue;

                            intersection = leftIntersect;
                            pointA = movedPointTL;
                            pointB = movedPointBL;
                            IntersectFound = true;
                            goto IntersectionFound;
                        }

                    IntersectionFound:
                        if (IntersectFound)
                        {
                            //Check if intersection point is within both lines
                            if (intersection.X < GetEpsilonRounded(Math.Min(oldPosition.X, newPosition.X)) || intersection.X > GetEpsilonRounded(Math.Max(oldPosition.X, newPosition.X)) ||
                                intersection.X < GetEpsilonRounded(Math.Min(pointA.X, pointB.X)) || intersection.X > GetEpsilonRounded(Math.Max(pointA.X, pointB.X)) ||
                                intersection.Y < GetEpsilonRounded(Math.Min(oldPosition.Y, newPosition.Y)) || intersection.Y > GetEpsilonRounded(Math.Max(oldPosition.Y, newPosition.Y)) ||
                                intersection.Y < GetEpsilonRounded(Math.Min(pointA.Y, pointB.Y)) || intersection.Y > GetEpsilonRounded(Math.Max(pointA.Y, pointB.Y)))
                            {
                                continue; //no collision
                            }

                            //Set new position to the reflected point
                            Coord reflectPos = GetReflectedPosition(oldPosition, newPosition, pointA, pointB, intersection);
                            if (reflectPos.X is double.NaN || reflectPos.Y is double.NaN) continue;

                            //Collision is guarenteed at this point
                            IsColliding = true;

                            newPosition = reflectPos;
                            //oldPosition = intersection;
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

                        //if (velocityLength < 1)
                        //{
                        //   Velocity = new Coord(0, 0);
                        //}

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
                        //double grav = (GravityAcceleration * Timer.DeltaTime * Timer.DeltaTime) / Timer.TimeScale;
                        //Velocity = new Coord(Velocity.X * Elasticity, (Velocity.Y - grav) * Elasticity + grav);
                        Velocity = new Coord(Velocity.X * Elasticity, Velocity.Y * Elasticity);

                        CheckAgain = true;
                        lastCheckedId = comp.ID;
                        break;
                    }
                }
            }


            //If scene has a circle border constraint, update position with it first
            if (Scene.IsCircleBorderActive)
                UpdatePositionWithCircleBorderConstraint(oldPosition, newPosition, parent.Radius);


            return newPosition;
        }


        private Coord UpdatePositionWithCircleBorderConstraint(Coord oldPosition, Coord newPosition, double parentRadius)
        {
            Coord WindowCenter = new Coord(MainPage.WindowSize.Width / 2.0, MainPage.WindowSize.Height / 2.0);
            double ConstraintRadius = Scene.GetCircleBorderRadius();
            Coord toCenter = new Coord(newPosition.X - WindowCenter.X, newPosition.Y - WindowCenter.Y);
            double centerDistance = GetLength(toCenter);
            if (centerDistance > ConstraintRadius - parentRadius)
            {
                //double grav = (GravityAcceleration * Timer.DeltaTime * Timer.DeltaTime) / Timer.TimeScale;
                Coord MoveDirection = new Coord(toCenter.X / centerDistance, toCenter.Y / centerDistance);
                /*Velocity = new Coord(
                    Velocity.X * Elasticity + (WindowCenter.X + MoveDirection.X * (ConstraintRadius - parentRadius) - newPosition.X),
                    (Velocity.Y - grav) * Elasticity + grav + (WindowCenter.Y + MoveDirection.Y * (ConstraintRadius - parentRadius) - newPosition.Y)
                );*/
                ApplyForce(new Coord(
                    (WindowCenter.X + MoveDirection.X * (ConstraintRadius - parentRadius) - newPosition.X),
                    (WindowCenter.Y + MoveDirection.Y * (ConstraintRadius - parentRadius) - newPosition.Y)
                ), eForceType.DIRECT);
                newPosition = new Coord(WindowCenter.X + MoveDirection.X * (ConstraintRadius - parentRadius), WindowCenter.Y + MoveDirection.Y * (ConstraintRadius - parentRadius));
            }

            return newPosition;
        }


        /// <summary>
        /// Gets the slope of a line with two given points
        /// </summary>
        /// <param name="pointA"></param>
        /// <param name="pointB"></param>
        /// <returns>The line slope</returns>
        public static double GetSlope(Coord pointA, Coord pointB)
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
        public static double GetAngle(Coord pointA, Coord pointB)
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


        private double GetEpsilonRounded(double num)
        {
            return Math.Round(num * 1000000.0) / 1000000.0;
        }

        /// <summary>
        /// Gets the length of the line from point 0,0 to given point
        /// </summary>
        /// <param name="point"></param>
        /// <returns>Point distance from (0,0)</returns>
        public static double GetLength(Coord point)
        {
            return Math.Sqrt(Math.Pow(point.X, 2.0) + Math.Pow(point.Y, 2.0));
        }
        /// <summary>
        /// Gets the distance between 2 given points
        /// </summary>
        /// <param name="pointA"></param>
        /// <param name="pointB"></param>
        /// <returns>Point distance from each other</returns>
        public static double GetDistance(Coord pointA, Coord pointB)
        {
            return Math.Sqrt(Math.Pow(pointB.X - pointA.X, 2.0) + Math.Pow(pointB.Y - pointA.Y, 2.0));
        }

        public static double GetDistance(Coord point, Coord pointA, Coord pointB)
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
            //return RotatePointAroundPoint(newPosition, intersection, (2.0 * diffAngle) * 180.0 / Math.PI);
            
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
            //I used the math from the following reference to program this
            //https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection

            double denominator = ((pointA.X - pointB.X) * (pointC.Y - pointD.Y) - (pointA.Y - pointB.Y) * (pointC.X - pointD.X));
            Coord intersection = new Coord(
                ((pointA.X * pointB.Y - pointA.Y * pointB.X) * (pointC.X - pointD.X) - (pointA.X - pointB.X) * (pointC.X * pointD.Y - pointC.Y * pointD.X))
                    / denominator,
                ((pointA.X * pointB.Y - pointA.Y * pointB.X) * (pointC.Y - pointD.Y) - (pointA.Y - pointB.Y) * (pointC.X * pointD.Y - pointC.Y * pointD.X))
                    / denominator
            );
            intersection = new Coord(GetEpsilonRounded(intersection.X), GetEpsilonRounded(intersection.Y));
            return intersection;
        }
    }


    public enum eForceType
    {
        IMPULSE,
        DIRECT
    }
}
