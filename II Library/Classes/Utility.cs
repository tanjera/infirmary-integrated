﻿/* Utility.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera), (c) 2023
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace II {
    public static class Utility {
        public static bool IsNewerVersion (string current, string comparison) {
            string [] curSplit = current.Split ('.'),
                compSplit = comparison.Split ('.');

            for (int i = 0; i < compSplit.Length; i++) {
                // Sanitize inputs
                if (!int.TryParse (curSplit [i], out _))
                    curSplit [i] = "-1";
                if (!int.TryParse (compSplit [i], out _))
                    compSplit [i] = "-1";

                if ((i < curSplit.Length ? int.Parse (curSplit [i]) : 0) < int.Parse (compSplit [i]))
                    return true;
                else if ((i < curSplit.Length ? int.Parse (curSplit [i]) : 0) > int.Parse (compSplit [i]))
                    return false;
            }

            return false;
        }

        public static double UtcStartTime {
            get { return (double)(DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess ().StartTime.ToUniversalTime ()).TotalSeconds; }
        }

        public static string? DateOnly_ToString (DateOnly? dt)
            => dt?.ToString ("yyyy/MM/dd");

        public static string? DateTime_ToString (DateTime? dt)
            => dt?.ToString ("yyyy/MM/dd HH:mm:ss");

        public static string? DateTime_ToString_FilePath (DateTime? dt)
            => dt?.ToString ("yyyy.MM.dd.HH.mm.ss");

        public static DateOnly DateOnly_FromString (string str) {
            return new DateOnly (
                int.Parse (str.Substring (0, 4)),
                int.Parse (str.Substring (5, 2)),
                int.Parse (str.Substring (8, 2)));
        }

        public static DateTime DateTime_FromString (string str) {
            return new DateTime (
                int.Parse (str.Substring (0, 4)),
                int.Parse (str.Substring (5, 2)),
                int.Parse (str.Substring (8, 2)),
                int.Parse (str.Substring (11, 2)),
                int.Parse (str.Substring (14, 2)),
                int.Parse (str.Substring (17, 2)));
        }

        public static string RandomString (int length) {
            Random r = new ();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string (Enumerable.Repeat (chars, length)
              .Select (s => s [r.Next (s.Length)]).ToArray ());
        }

        public static string WrapString (string? input) {
            input ??= "";
            return input.Replace (" ", Environment.NewLine);
        }

        public static string UnderscoreToSpace (string str) {
            return str.Replace ('_', ' ');
        }

        public static string SpaceToUnderscore (string str) {
            return str.Replace (' ', '_');
        }

        public static string Indent (int indent) {
            string space = "    ";
            string output = "";

            for (int i = 0; i < indent; i++)
                output += space;

            return output;
        }
    }
}