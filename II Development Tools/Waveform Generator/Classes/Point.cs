using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waveform_Generator {

    public class Point {
        public double X, Y;

        public Point (double x, double y) {
            X = x; Y = y;
        }

        public static Point Lerp (Point a, Point b, double t) {
            return new Point (Utility.Lerp (a.X, b.X, t), Utility.Lerp (a.Y, b.Y, t));
        }

        public static Point operator * (double f, Point p) {
            return new Point (p.X * f, p.Y * f);
        }

        public static Point operator + (Point a, Point b) {
            return new Point (a.X + b.X, a.Y + b.Y);
        }
    }
}