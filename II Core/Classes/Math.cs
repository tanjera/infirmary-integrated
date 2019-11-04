using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace II {
    public static class Math {
        public static double Clamp (double value, double min, double max) {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static float Clamp (float value, float min, float max) {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static int Clamp (int value, int min, int max) {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static double Clamp (double value) {
            return (value < 0) ? 0 : (value > 1) ? 1 : value;
        }
        public static float Clamp (float value) {
            return (value < 0) ? 0 : (value > 1) ? 1 : value;
        }

        public static double Lerp (double min, double max, double t) {
            return min * (1 - t) + max * t;
        }

        public static float Lerp (float min, float max, float t) {
            return min * (1 - t) + max * t;
        }

        public static int Lerp (int min, int max, float t) {
            return (int)(min * (1 - t) + max * t);
        }

        public static Point Lerp (Point a, Point b, float t) {
            return new Point (Lerp (a.X, b.X, t), Lerp (a.Y, b.Y, t));
        }

        public static Point Multiply (Point p, float f) {
            return new Point ((int)(p.X * f), (int)(p.Y * f));
        }

        public static Point Add (Point a, Point b) {
            return new Point (a.X + b.X, a.Y + b.Y);
        }

        public static PointF Lerp (PointF a, PointF b, float t) {
            return new PointF (Lerp (a.X, b.X, t), Lerp (a.Y, b.Y, t));
        }

        public static PointF Multiply (PointF p, float f) {
            return new PointF (p.X * f, p.Y * f);
        }

        public static PointF Add (PointF a, PointF b) {
            return new PointF (a.X + b.X, a.Y + b.Y);
        }

        public static double InverseLerp (double min, double max, double current) {
            return (current - min) / (max - min);
        }

        public static float InverseLerp (float min, float max, float current) {
            return (current - min) / (max - min);
        }

        public static float InverseLerp (int min, int max, float current) {
            return ((current - min) / (max - min));
        }

        public static double RandomDouble (double min, double max) {
            Random r = new Random ();
            return (double)r.NextDouble () * (max - min) + min;
        }

        public static double RandomPercentRange (double value, double percent) {
            return RandomDouble ((value - (value * percent)), (value + (value * percent)));
        }
    }
}