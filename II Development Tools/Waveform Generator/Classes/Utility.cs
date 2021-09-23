using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waveform_Generator {

    public static class Utility {

        public static double Lerp (double min, double max, double t) {
            return min * (1 - t) + max * t;
        }

        public static double InverseLerp (double min, double max, double current) {
            return (current - min) / (max - min);
        }

        public static double Clamp (double value, double min, double max) {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static int Clamp (int value, int min, int max) {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static double Clamp (double value) {
            return (value < 0) ? 0 : (value > 1) ? 1 : value;
        }
    }
}