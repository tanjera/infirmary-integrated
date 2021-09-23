using System;
using System.Diagnostics;
using System.IO;

namespace Publishing {

    public class Package_Windows {

        public static void Process (string dirSolution, string dirRelease, string verNumber) {
            string pkgOrig = $"infirmary-integrated-.msi";
            string pkgName = $"infirmary-integrated-{verNumber}.msi";

            MovePackage_Windows (dirSolution, dirRelease, pkgOrig, pkgName);
            SignPackage_Windows (dirRelease, pkgName);
        }

        public static void MovePackage_Windows (string dirSolution, string dirRelease, string pkgOrig, string pkgName) {
            string pathOrig = Path.Combine (dirSolution, @$"Package, Windows\Release\{pkgOrig}");
            string pathTarget = Path.Combine (dirRelease, pkgName);

            if (File.Exists (pathOrig)) {
                File.Move (pathOrig, pathTarget, true);
            }
        }

        public static void SignPackage_Windows (string dirRelease, string pkgName) {
            Process proc = new Process ();
            string arguments = "";

            Console.Write ("Please enter the signing certificate password: ");
            string password = Console.ReadLine ().Trim ();

            // "signtool sign /f sigfile.pfx /p password filename"

            arguments = $"sign /f \"{Program.pathCert}\" /p {password} \"{pkgName}\"";
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine ($"- Executing signtool {arguments}");
            Console.WriteLine (Environment.NewLine);
            Console.ResetColor ();

            proc.StartInfo.FileName = Program.pathSigntool;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.WorkingDirectory = dirRelease;
            proc.Start ();
            proc.WaitForExit ();
        }
    }
}