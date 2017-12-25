using System;
using System.Security.Cryptography;
using System.Text;


namespace II {

    public static class _ {

        public const string Version = "0.8";

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

        public static string HashMD5 (string str) {
            MD5 md5 = MD5.Create ();
            byte [] bytes = System.Text.Encoding.ASCII.GetBytes (str);
            byte [] hash = md5.ComputeHash (bytes);

            StringBuilder sb = new StringBuilder ();
            for (int i = 0; i < hash.Length; i++)
                sb.Append (hash [i].ToString ("X2"));

            return sb.ToString ();
        }

        public static string ObfuscateB64 (string str) {
            return Convert.ToBase64String (Encoding.UTF8.GetBytes (str));
        }

        public static string UnobfuscateB64 (string str) {
            return Encoding.UTF8.GetString (Convert.FromBase64String (str));
        }
    }
}