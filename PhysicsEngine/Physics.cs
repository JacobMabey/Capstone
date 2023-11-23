using PhysicsEngine.UI_Menus;
using System;
using System.Collections.Generic;

namespace PhysicsEngine
{
    public class Physics
    {
        public static bool IsGravityEnabled { get; set; } = false;
        public static double MaxGravity => 1000;
        public static double GravityAcceleration { get; set; } = 20;

        private static readonly double Epsilon = 0.01;

        public static bool IsBorderEnclosed { get; set; } = true;

        public Component Parent { get; private set; }

        public Coord Acceleration { get; set; } = new Coord(0, 0);
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
        private double elasticity = 0.8;
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

        public static double MaxVelocity => 30.0;

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
                    ApplyForce(new Coord(-Math.Sign(Velocity.X) * (Friction / 10.0), -Math.Sign(Velocity.Y) * (Friction / 10.0)));

                    //Update Velocity & Gravity
                    double grav = IsGravityEnabled ? GravityAcceleration / 100.0 : 0.0;
                    Acceleration = new Coord(Acceleration.X * Timer.TimeScale, ((Acceleration.Y + grav)) * Timer.TimeScale);
                    Velocity = new Coord(Velocity.X + Acceleration.X, Velocity.Y + Acceleration.Y);

                    double vLength = GetLength(Velocity);
                    if (vLength > MaxVelocity)
                        Velocity = new Coord(Velocity.X / vLength * MaxVelocity, Velocity.Y / vLength * MaxVelocity);

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


            //If particle collision is enabled, react to collisions
            if (parent.IsParticleCollisionEnabled)
            {
                List<Particle> checkList = GetAdjacentCellsParticleList();

                //Loop through all particles and Update Position
                foreach (Particle comp in checkList)
                {
                    if (!comp.IsCollisionEnabled || !comp.IsParticleCollisionEnabled || double.IsNaN(comp.Phys.Velocity.X))
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
                    ApplyForce(new Coord(moveVector.X * (Elasticity + 1.0), moveVector.Y * (Elasticity + 1.0)), eForceType.DIRECT);
                }
            }
            


            //Loop through all lines and rectangles and Update Position
            long lastCheckedId = -1;
            int checkAgainCapCount = 0;
            int checkAgainCap = 5;
            while (CheckAgain && checkAgainCapCount < checkAgainCap) {
                CheckAgain = false;
                checkAgainCapCount++;
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
                        double particleToLineDistance = GetDistance(newPosition, line.PosA, line.PosB);
                        if (particleToLineDistance > Scene.MaxParticleRadius + 1)
                            continue;

                        //Get new virtual line position based on the particles radius
                        double lineToParticleAngle = GetAngle(line.PosA, line.PosB) + Math.PI / 2.0;
                        Coord pointA = MovePoint(line.PosA, parent.Radius + line.Thickness / 2.0, lineToParticleAngle);
                        Coord pointB = MovePoint(line.PosB, parent.Radius + line.Thickness / 2.0, lineToParticleAngle);
                        if (GetDistance(oldPosition, pointA, pointB) > GetDistance(oldPosition, line.PosA, line.PosB))
                        {
                            lineToParticleAngle += Math.PI;
                        }

                        //Get continueos lines intersection point
                        intersection = GetIntersectionPoint(oldPosition, newPosition, line.PosA, line.PosB);
                        if (intersection.X is double.NaN || intersection.Y is double.NaN)
                            continue;

                        //Check if intersection point is within both lines
                        if (intersection.X < GetEpsilonRounded(Math.Min(oldPosition.X, newPosition.X)) || intersection.X > GetEpsilonRounded(Math.Max(oldPosition.X, newPosition.X)) ||
                            intersection.X < GetEpsilonRounded(Math.Min(line.PosA.X, line.PosB.X)) || intersection.X > GetEpsilonRounded(Math.Max(line.PosA.X, line.PosB.X)) ||
                            intersection.Y < GetEpsilonRounded(Math.Min(oldPosition.Y, newPosition.Y)) || intersection.Y > GetEpsilonRounded(Math.Max(oldPosition.Y, newPosition.Y)) ||
                            intersection.Y < GetEpsilonRounded(Math.Min(line.PosA.Y, line.PosB.Y)) || intersection.Y > GetEpsilonRounded(Math.Max(line.PosA.Y, line.PosB.Y)))
                        {
                            //Check for collision on the end points of the line
                            double particleToLinePointDistance = GetDistance(newPosition, line.PosA, line.PosB);
                            Coord collidingPoint = MovePoint(newPosition, particleToLinePointDistance, lineToParticleAngle + Math.PI);
                            if (Math.Max(GetDistance(collidingPoint, line.PosA), GetDistance(collidingPoint, line.PosB)) > GetDistance(line.PosA, line.PosB))
                            {
                                particleToLinePointDistance = GetDistance(newPosition, line.PosA);
                                collidingPoint = line.PosA;
                                double pointBDist = GetDistance(newPosition, line.PosB);
                                if (particleToLinePointDistance > pointBDist)
                                {
                                    particleToLinePointDistance = pointBDist;
                                    collidingPoint = line.PosB;
                                }
                            }

                            if (particleToLinePointDistance < (parent.Radius))
                            {
                                double moveDistance = ((parent.Radius) - particleToLinePointDistance);
                                double angle = GetAngle(collidingPoint, newPosition);
                                Coord moveVector = MovePoint(new Coord(0, 0), moveDistance, GetAngle(collidingPoint, newPosition));
                                newPosition = new Coord(
                                    newPosition.X + moveVector.X,
                                    newPosition.Y + moveVector.Y
                                );
                                ApplyForce(new Coord(moveVector.X * (Elasticity + 1.0), moveVector.Y * (Elasticity + 1.0)), eForceType.DIRECT);
                                continue;
                            }
                            continue;
                        }

                        //Collision is guarenteed at this point
                        IsColliding = true;

                        //Set new position to the reflected point
                        Coord reflectPos = GetReflectedPosition(oldPosition, newPosition, line.PosA, line.PosB, intersection);
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
                        Coord rotateCenter = new Coord(rect.Position.X + rect.RotationCenter.X, rect.Position.Y + rect.RotationCenter.Y);
                        Coord pointTL = RotatePointAroundPoint(new Coord(rect.Position.X, rect.Position.Y), rotateCenter, rect.RotationAngle);
                        Coord pointTR = RotatePointAroundPoint(new Coord(rect.Position.X + rect.Size.Width, rect.Position.Y), rotateCenter, rect.RotationAngle);
                        Coord pointBR = RotatePointAroundPoint(new Coord(rect.Position.X + rect.Size.Width, rect.Position.Y + rect.Size.Height), rotateCenter, rect.RotationAngle);
                        Coord pointBL = RotatePointAroundPoint(new Coord(rect.Position.X, rect.Position.Y + rect.Size.Height), rotateCenter, rect.RotationAngle);


                        //Will be set to the two points connected to the side of the rect the particle intersects with
                        Coord pointA = new Coord(0, 0);
                        Coord pointB = new Coord(0, 0);

                        //Get length of particle movement
                        double moveLength = GetLength(new Coord(oldPosition.X - newPosition.X, oldPosition.Y - newPosition.Y));

                        //Find intersection point that is at less distance than moveLength to find intersected side
                        bool IntersectFound = false;

                        //Get all line to particle angles
                        //Top angle
                        double topLineToParticleAngle = GetAngle(pointTL, pointTR) - Math.PI / 2.0;
                        Coord movedPointTL = MovePoint(pointTL, parent.Radius, topLineToParticleAngle);
                        Coord movedPointTR = MovePoint(pointTR, parent.Radius, topLineToParticleAngle);
                        if (GetDistance(oldPosition, movedPointTL, movedPointTR) > GetDistance(oldPosition, pointTL, pointTR))
                        {
                            topLineToParticleAngle += Math.PI;
                        }
                        //Right angle
                        double rightLineToParticleAngle = GetAngle(pointTR, pointBR) + Math.PI / 2.0;
                        movedPointTR = MovePoint(pointTR, parent.Radius, rightLineToParticleAngle);
                        Coord movedPointBR = MovePoint(pointBR, parent.Radius, rightLineToParticleAngle);
                        if (GetDistance(oldPosition, movedPointTR, movedPointBR) > GetDistance(oldPosition, pointTR, pointBR))
                        {
                            rightLineToParticleAngle += Math.PI;
                        }
                        //Bottom Angle
                        double bottomLineToParticleAngle = GetAngle(pointBL, pointBR) + Math.PI / 2.0;
                        Coord movedPointBL = MovePoint(pointBL, parent.Radius, bottomLineToParticleAngle);
                        movedPointBR = MovePoint(pointBR, parent.Radius, bottomLineToParticleAngle);
                        if (GetDistance(oldPosition, movedPointBL, movedPointBR) > GetDistance(oldPosition, pointBL, pointBR))
                        {
                            bottomLineToParticleAngle += Math.PI;
                        }
                        //Left Angle
                        double leftLineToParticleAngle = GetAngle(pointTL, pointBL) + Math.PI / 2.0;
                        movedPointTL = MovePoint(pointTL, parent.Radius, leftLineToParticleAngle);
                        movedPointBL = MovePoint(pointBL, parent.Radius, leftLineToParticleAngle);
                        if (GetDistance(oldPosition, movedPointTL, movedPointBL) > GetDistance(oldPosition, pointTL, pointBL))
                        {
                            leftLineToParticleAngle += Math.PI;
                        }

                        //Check for real collision other than clipping
                        //Get top closest point and distance
                        double particleToTopPointDistance = GetDistance(newPosition, pointTL, pointTR);
                        Coord topCollidingPoint = MovePoint(newPosition, particleToTopPointDistance, topLineToParticleAngle + Math.PI);
                        if (Math.Max(GetDistance(topCollidingPoint, pointTL), GetDistance(topCollidingPoint, pointTR)) > GetDistance(pointTL, pointTR))
                        {
                            particleToTopPointDistance = GetDistance(newPosition, pointTL);
                            topCollidingPoint = pointTL;
                            double otherPointDist = GetDistance(newPosition, pointTR);
                            if (particleToTopPointDistance > otherPointDist)
                            {
                                particleToTopPointDistance = otherPointDist;
                                topCollidingPoint = pointTR;
                            }
                        }
                        //Get right closest point and distance
                        double particleToRightPointDistance = GetDistance(newPosition, pointTR, pointBR);
                        Coord rightCollidingPoint = MovePoint(newPosition, particleToRightPointDistance, rightLineToParticleAngle + Math.PI);
                        if (Math.Max(GetDistance(rightCollidingPoint, pointTR), GetDistance(rightCollidingPoint, pointBR)) > GetDistance(pointTR, pointBR))
                        {
                            particleToRightPointDistance = GetDistance(newPosition, pointTR);
                            rightCollidingPoint = pointTR;
                            double otherPointDist = GetDistance(newPosition, pointBR);
                            if (particleToRightPointDistance > otherPointDist)
                            {
                                particleToRightPointDistance = otherPointDist;
                                rightCollidingPoint = pointBR;
                            }
                        }
                        //Get bottom closest point and distance
                        double particleToBottomPointDistance = GetDistance(newPosition, pointBL, pointBR);
                        Coord bottomCollidingPoint = MovePoint(newPosition, particleToBottomPointDistance, bottomLineToParticleAngle + Math.PI);
                        if (Math.Max(GetDistance(bottomCollidingPoint, pointBL), GetDistance(bottomCollidingPoint, pointBR)) > GetDistance(pointBL, pointBR))
                        {
                            particleToBottomPointDistance = GetDistance(newPosition, pointBL);
                            bottomCollidingPoint = pointBL;
                            double otherPointDist = GetDistance(newPosition, pointBR);
                            if (particleToBottomPointDistance > otherPointDist)
                            {
                                particleToBottomPointDistance = otherPointDist;
                                bottomCollidingPoint = pointBR;
                            }
                        }

                        //Get left closest point and distance
                        double particleToLeftPointDistance = GetDistance(newPosition, pointTL, pointBL);
                        Coord leftCollidingPoint = MovePoint(newPosition, particleToLeftPointDistance, leftLineToParticleAngle + Math.PI);
                        if (Math.Max(GetDistance(leftCollidingPoint, pointTL), GetDistance(leftCollidingPoint, pointBL)) > GetDistance(pointTL, pointBL))
                        {
                            particleToLeftPointDistance = GetDistance(newPosition, pointTL);
                            leftCollidingPoint = pointTL;
                            double otherPointDist = GetDistance(newPosition, pointBL);
                            if (particleToLeftPointDistance > otherPointDist)
                            {
                                particleToLeftPointDistance = otherPointDist;
                                leftCollidingPoint = pointBL;
                            }
                        }

                        //Get closest colliding point to react with
                        double particleToLinePointDistance = Math.Min(particleToTopPointDistance, Math.Min(particleToRightPointDistance, Math.Min(particleToBottomPointDistance, particleToLeftPointDistance)));
                        Coord collidingPoint = topCollidingPoint;
                        if (particleToLinePointDistance == particleToRightPointDistance)
                            collidingPoint = rightCollidingPoint;
                        if (particleToLinePointDistance == particleToBottomPointDistance)
                            collidingPoint = bottomCollidingPoint;
                        if (particleToLinePointDistance == particleToLeftPointDistance)
                            collidingPoint = leftCollidingPoint;

                        if (particleToLinePointDistance < (parent.Radius))
                        {
                            double moveDistance = ((parent.Radius) - particleToLinePointDistance);
                            Coord moveVector = MovePoint(new Coord(0, 0), moveDistance, GetAngle(collidingPoint, newPosition));
                            newPosition = new Coord(
                                newPosition.X + moveVector.X,
                                newPosition.Y + moveVector.Y
                            );
                            ApplyForce(new Coord(moveVector.X * (Elasticity + 1.0), moveVector.Y * (Elasticity + 1.0)), eForceType.DIRECT);
                        }

                        //Top line intersection
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

                        //Right line intersection
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

                        //Bottom line intersection
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

                        //Left line intersection
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
            Coord WindowCenter = new Coord(Scene.MainScene.Width / 2.0, (Scene.MainScene.Height - Toolbar.ToolbarHeight) / 2.0);
            double ConstraintRadius = Scene.GetCircleBorderRadius();
            Coord toCenter = new Coord(newPosition.X - WindowCenter.X, newPosition.Y - WindowCenter.Y);
            double centerDistance = GetLength(toCenter);
            if (centerDistance > ConstraintRadius - parentRadius)
            {
                Coord MoveDirection = new Coord(toCenter.X / centerDistance, toCenter.Y / centerDistance);
                ApplyForce(new Coord(
                    (WindowCenter.X + MoveDirection.X * (ConstraintRadius - parentRadius) - newPosition.X),
                    (WindowCenter.Y + MoveDirection.Y * (ConstraintRadius - parentRadius) - newPosition.Y)
                ), eForceType.DIRECT);
            }

            return newPosition;
        }



        private List<Particle> GetAdjacentCellsParticleList()
        {
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

            return checkList;
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
            if (slope == 0)
            {
                if (pointA.X > pointB.X)
                    return Math.PI;
                else
                    return 0.0;
            }
            if (double.IsPositiveInfinity(slope)) return -Math.PI / 2.0;
            if (double.IsNegativeInfinity(slope)) return Math.PI / 2.0;
            
            double angle = -Math.Atan(slope);
            if (pointB.X < pointA.X)
                angle = angle + Math.PI;

            while (angle < 0.0)
                angle += Math.PI * 2.0;

            while (angle > Math.PI * 2.0)
                angle -= Math.PI * 2.0;

            if (double.IsNaN(angle))
                angle = 0;
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
        public static Coord MovePoint(Coord point, double distance, double angle)
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
        public static Coord RotatePointAroundPoint(Coord point, Coord origin, double angle)
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

            double passedIntersectionLength = GetDistance(intersection, oldPosition);
            
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


        public Physics Clone(Component parent)
        {
            Physics clone = new Physics(parent);
            clone.Mass = Mass;
            clone.Friction = Friction;
            clone.Elasticity = Elasticity;

            return clone;
        }
    }


    public enum eForceType
    {
        IMPULSE,
        DIRECT
    }
}
