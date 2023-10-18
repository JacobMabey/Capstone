using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsEngine
{
    public class Coord
    {
        public double X { get; set; } = 0.0;
        public double Y { get; set; } = 0.0;

        public Coord(double x, double y)
        {
            X = x; Y = y;
        }
    }
}
