using System;
using System.Diagnostics;
using System.IO;

namespace Publishing {

    public class Package_Windows {

        public static void Process (Program.Variables progVar, string dirRelease, string verNumber) {
            string pkgOrig = $"infirmary-integrated-.exe";
            string pkgName = $"infirmary-integrated-{verNumber}.exe";
            string dirPackage = Path.Combine (progVar.dirSolution, @$"II Simulator\bin\Release\{progVar.versionDotnet}\win-x64");

            CreatePackage_Windows (progVar, dirPackage);
            MovePackage_Windows (dirPackage, dirRelease, pkgOrig, pkgName);
            SignPackage_Windows (progVar, dirRelease, pkgName);
        }

        public static void CreatePackage_Windows (Program.Variables progVar, string dirPackage) {
            string nsiOriginal = Path.Combine (progVar.dirSolution, @$"Package, Windows\Package.nsi");

            string nsiBuffer = Path.Combine (dirPackage, "Package.nsi");

            if (File.Exists (nsiOriginal)) {
                File.Copy (nsiOriginal, nsiBuffer);

                Process proc = new Process ();

                //makensis.exe {dir}\Package.nsi

                string arguments = $"Package.nsi";

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine (Environment.NewLine);
                Console.WriteLine ($"- Executing makensis {arguments}");
                Console.WriteLine (Environment.NewLine);
                Console.ResetColor ();

                proc.StartInfo.FileName = progVar.pathNSIS;
                proc.StartInfo.Arguments = arguments;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.WorkingDirectory = dirPackage;
                proc.Start ();
                proc.WaitForExit ();

                File.Delete (nsiBuffer);
            } else {
                throw new FileNotFoundException ();
            }
        }

        public static void MovePackage_Windows (string dirPackage, string dirRelease, string pkgOrig, string pkgName) {
            string pathOrig = Path.Combine (dirPackage, pkgOrig);
            string pathTarget = Path.Combine (dirRelease, pkgName);

            if (File.Exists (pathOrig)) {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine (Environment.NewLine);
                Console.WriteLine ($"- Moving package to ${dirRelease}");
                Console.WriteLine (Environment.NewLine);
                Console.ResetColor ();

                File.Move (pathOrig, pathTarget, true);
            } else {
                throw new FileNotFoundException ();
            }
        }

        public static void SignPackage_Windows (Program.Variables progVar, string dirRelease, string pkgName) {
            Process proc = new Process ();
            string arguments = "";

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine (Environment.NewLine);
            Console.WriteLine ($"- Preparing to sign package");
            Console.WriteLine (Environment.NewLine);
            Console.ResetColor ();

            Console.Write ("Please enter the signing certificate password: ");
            string password = Console.ReadLine ().Trim ();

            // "signtool sign /f sigfile.pfx /p password filename"

            arguments = $"sign /f \"{progVar.pathCert}\" /p {password} \"{pkgName}\"";

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine (Environment.NewLine);
            Console.WriteLine ($"- Executing signtool {arguments}");
            Console.WriteLine (Environment.NewLine);
            Console.ResetColor ();

            proc.StartInfo.FileName = progVar.pathSigntool;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.WorkingDirectory = dirRelease;
            proc.Start ();
            proc.WaitForExit ();
        }
    }
}