using System;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;


namespace II {

    public static class Utility {

        public const string Version = "0.96";

        public static bool IsNewerVersion(string current, string comparison) {
            string[] curSplit = current.Split('.'),
                    compSplit = comparison.Split('.');

            for (int i = 0; i < compSplit.Length; i++)
                if ((i < curSplit.Length ? int.Parse(curSplit[i]) : 0) < int.Parse(compSplit[i]))
                    return true;
            return false;
        }

        public static double Time {
            get { return (double)(DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess ().StartTime.ToUniversalTime ()).TotalSeconds; }
        }

        public static string DateTime_ToString (DateTime dt)
            => dt.ToString ("yyyy/MM/dd HH:mm:ss");
        public static DateTime DateTime_FromString (string str) {
            return new DateTime (
                int.Parse(str.Substring(0, 4)),
                int.Parse (str.Substring(5, 2)),
                int.Parse (str.Substring(8, 2)),
                int.Parse (str.Substring (11, 2)),
                int.Parse (str.Substring (14, 2)),
                int.Parse (str.Substring (17, 2)));
        }

        public static string RandomString (int length) {
            Random r = new Random ();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string (Enumerable.Repeat (chars, length)
              .Select (s => s [r.Next (s.Length)]).ToArray ());
        }

        public static string WrapString (string input) {
            return input.Replace (" ", Environment.NewLine);
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
            return min * (1 - t) + max * t;
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

        public static string EncryptAES (string str) {
            byte [] output;
            using (AesManaged aes = new AesManaged ()) {
                aes.Key = Access.Encryption.Key;
                aes.IV = Access.Encryption.IV;
                ICryptoTransform encryptor = aes.CreateEncryptor (aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream ()) {
                    using (CryptoStream cs = new CryptoStream (ms, encryptor, CryptoStreamMode.Write)) {
                        using (StreamWriter sw = new StreamWriter (cs))
                            sw.Write (str);
                        output = ms.ToArray ();
                    }
                }
            }
            return Convert.ToBase64String(output);
        }

        public static string DecryptAES (string str) {
            string output;
            using (AesManaged aes = new AesManaged ()) {
                aes.Key = Access.Encryption.Key;
                aes.IV = Access.Encryption.IV;
                ICryptoTransform decryptor = aes.CreateDecryptor (aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream (Convert.FromBase64String(str))) {
                    using (CryptoStream cs = new CryptoStream (ms, decryptor, CryptoStreamMode.Read)) {
                        using (StreamReader reader = new StreamReader (cs))
                            output = reader.ReadToEnd ();
                    }
                }
            }
            return output;
        }
    }
}