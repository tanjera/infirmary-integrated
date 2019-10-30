using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace II {
    public static class Encryption {
        private static string keyString = "!8x/A?b(G+KbPe$hVkYpEs6V9y$B&E)H";
        public static byte [] Key { get { return Encoding.UTF8.GetBytes (keyString); } }
        public static byte [] IV { get { return Encoding.UTF8.GetBytes (keyString).Take (16).ToArray (); } }

        public static string HashSHA256 (string str) {
            SHA256 sha256 = SHA256.Create ();
            byte [] bytes = Encoding.ASCII.GetBytes (str);
            byte [] hash = sha256.ComputeHash (bytes);

            StringBuilder sb = new StringBuilder ();
            foreach (byte b in hash)
                sb.Append (b.ToString ("X2"));

            return sb.ToString ();
        }

        public static string EncryptAES (string str) {
            byte [] output;
            using (AesManaged aes = new AesManaged ()) {
                aes.Key = Encryption.Key;
                aes.IV = Encryption.IV;
                ICryptoTransform encryptor = aes.CreateEncryptor (aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream ()) {
                    using (CryptoStream cs = new CryptoStream (ms, encryptor, CryptoStreamMode.Write)) {
                        using (StreamWriter sw = new StreamWriter (cs))
                            sw.Write (str);
                        output = ms.ToArray ();
                    }
                }
            }
            return Convert.ToBase64String (output);
        }

        public static string DecryptAES (string str) {
            string output;
            using (AesManaged aes = new AesManaged ()) {
                aes.Key = Encryption.Key;
                aes.IV = Encryption.IV;
                ICryptoTransform decryptor = aes.CreateDecryptor (aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream (Convert.FromBase64String (str))) {
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