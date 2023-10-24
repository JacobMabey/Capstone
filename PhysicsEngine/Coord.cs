using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
