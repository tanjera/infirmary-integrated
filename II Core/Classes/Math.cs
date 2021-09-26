﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace II {

    public static class Math {

        public static double Clamp (double value, double min, double max) {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static int Clamp (int value, int min, int max) {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static double Clamp (double value) {
            return (value < 0) ? 0 : (value > 1) ? 1 : value;
        }

        public static double Lerp (double min, double max, double t) {
            return min * (1 - t) + max * t;
        }

        public static int Lerp (int min, int max, double t) {
            return (int)(min * (1 - t) + max * t);
        }

        public static double InverseLerp (double min, double max, double current) {
            return (current - min) / (max - min);
        }

        public static double InverseLerp (int min, int max, double current) {
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