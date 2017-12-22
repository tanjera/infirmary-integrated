using System;
using System.Collections.Generic;

namespace II {

    public static class _ {

        public const string Version = "0.7";

        public enum ColorScheme {
            Normal, Monochrome
        }

        public class StringPair {
            public string Index, Value;
            public StringPair (string index, string value) {
                Index = index;
                Value = value;
            }
        }

        public static double Time {
            get { return (double)(DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess ().StartTime.ToUniversalTime ()).TotalSeconds; }
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

        public static double Lerp (double min, double max, double t) {
            return min * t + max * (1 - t);
        }

        public static double InverseLerp (double min, double max, double current) {
            return (current - min) / (max - min);
        }

        public static double RandomDouble (double min, double max) {
            Random r = new Random ();
            return (double)r.NextDouble () * (max - min) + min;
        }

        public static double RandomPercentRange (double value, double percent) {
            return RandomDouble((value - (value * percent)), (value + (value * percent)));
        }

        public static string UnderscoreToSpace (string str) {
            return str.Replace('_', ' ');
        }

        public static string SpaceToUnderscore(string str) {
            return str.Replace(' ', '_');
        }
    }
}