using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicsEngine
{
    public static class ColorFunctions
    {
        public static double[] RgbToHsv(double r, double g, double b)
        {
            //I used the math from the following link to program this
            //https://www.rapidtables.com/convert/color/rgb-to-hsv.html

            r = r / 255.0;
            g = g / 255.0;
            b = b / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            double hue = 0;
            if (delta == 0)
                hue = 0;
            else if (max == r)
                hue = 60.0 * (((g - b) / delta) % 6.0);
            else if (max == g)
                hue = 60.0 * ((b - r) / delta + 2);
            else if (max == b)
                hue = 60.0 * ((r - g) / delta + 4);


            double saturation = 0;
            if (max != 0)
                saturation = delta / max;


            double value = max;

            while (hue < 0) hue += 360;

            return new double[] { hue, saturation, value };
        }

        public static double[] HsvToRgb(double h, double s, double v)
        {
            //I used the math from the following link to program this
            //https://www.rapidtables.com/convert/color/hsv-to-rgb.html

            double col = v * s;
            double sec = col * (1 - Math.Abs((h / 60.0) % 2.0 - 1));
            double dif = v - col;

            double r = 0;
            double g = 0;
            double b = 0;
            if (h >= 0 && h < 60)
            {
                r = col;
                g = sec;
            }
            else if (h >= 60 && h < 120)
            {
                r = sec;
                g = col;
            }
            else if (h >= 120 && h < 180)
            {
                g = col;
                b = sec;
            }
            else if (h >= 180 && h < 240)
            {
                g = sec;
                b = col;
            }
            else if (h >= 240 && h < 300)
            {
                r = sec;
                b = col;
            }
            else if (h >= 300 && h < 360)
            {
                r = col;
                b = sec;
            }

            return new double[] { r * 255, g * 255, b * 255 };
        }
    }
}
