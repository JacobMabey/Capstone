using Windows.Foundation;

namespace PhysicsEngine
{
    public struct Coord
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Coord(double x, double y)
        {
            X = x; Y = y;
        }

        public static Coord FromPoint(Point point)
        {
            return new Coord(point.X, point.Y);
        }
        public Point ToPoint() { return new Point(X, Y); }
    }
}
