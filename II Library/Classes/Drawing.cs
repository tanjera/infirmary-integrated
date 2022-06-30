using System;
using System.Collections.Generic;
using System.Text;

namespace II.Drawing {

    public class PointD {
        public double X { get; set; }
        public double Y { get; set; }

        public PointD () {
            X = 0;
            Y = 0;
        }

        public PointD (double x, double y) {
            X = x;
            Y = y;
        }

        public override bool Equals (object? p) {
            if (p != null && p is PointD pt)
                return X == pt.X && Y == pt.Y;
            else
                return false;
        }

        public static PointD Lerp (PointD a, PointD b, double t) {
            return new PointD (Math.Lerp (a.X, b.X, t), Math.Lerp (a.Y, b.Y, t));
        }

        public override int GetHashCode () {
            return HashCode.Combine (X, Y);
        }

        public static bool operator == (PointD lhs, PointD rhs) {
            if (lhs is null && rhs is null)
                return true;
            else if (lhs is null || rhs is null)
                return false;

            return lhs.Equals (rhs);
        }

        public static bool operator != (PointD lhs, PointD rhs) {
            if (lhs is null && rhs is null)
                return false;
            else if (lhs is null || rhs is null)
                return true;

            return !lhs.Equals (rhs);
        }

        public static PointD operator + (PointD a, PointD b)
        => new(a.X + b.X, a.Y + b.Y);

        public static PointD operator - (PointD a, PointD b)
        => new(a.X - b.X, a.Y - b.Y);

        public static PointD operator * (PointD a, PointD b)
        => new(a.X * b.X, a.Y * b.Y);

        public static PointD operator * (PointD a, double b)
        => new(a.X * b, a.Y * b);

        public static PointD operator / (PointD a, PointD b) {
            if (b.X == 0 || b.Y == 0)
                throw new DivideByZeroException ();

            return new PointD (a.X / b.X, a.Y / b.Y);
        }
    }
}