using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Publishing {

    public class Package_Windows {

        public static void Process (Program.Variables progVar, string dirRelease, string verNumber, string release) {
            string pkgOrig = $"infirmary-integrated-{release}.exe";
            string pkgName = $"infirmary-integrated-{verNumber}-{release}.exe";
            string dirSimulator = Path.Combine (progVar.dirSolution, @$"II Simulator\bin\Release\{progVar.versionDotnet}\{release}\publish");
            string dirScenarioEditor = Path.Combine (progVar.dirSolution, @$"II Scenario Editor\bin\Release\{progVar.versionDotnet}\{release}\publish");

            // Ensure the directory exists and is empty
            if (!Directory.Exists (progVar.dirTemporary))
                Directory.CreateDirectory (progVar.dirTemporary);

            Directory.Move (dirSimulator, Path.Combine (progVar.dirTemporary, "Infirmary Integrated"));
            Directory.Move (dirScenarioEditor, Path.Combine (progVar.dirTemporary, "Infirmary Integrated Scenario Editor"));

            CreatePackage_Windows (progVar, progVar.dirTemporary, verNumber, release);
            MovePackage_Windows (progVar.dirTemporary, dirRelease, pkgOrig, pkgName);
            SignPackage_Windows (progVar, dirRelease, pkgName);

            // Clean the temporary directory
            Directory.Delete (progVar.dirTemporary, true);
        }

        public static void CreatePackage_Windows (Program.Variables progVar, string dirPackage, string verNumber, string release) {
            string nsiOriginal = Path.Combine (progVar.dirSolution, @$"Package, Windows\package-{release}.nsi");
            string nsiBuffer = Path.Combine (dirPackage, "package.nsi");

            if (File.Exists (nsiOriginal)) {
                List<string> nsiTextFile = new List<string> (File.ReadAllLines (nsiOriginal));
                int editIndex = nsiTextFile.FindIndex (s => s.Trim () == ("\"DisplayName\" \"Infirmary Integrated\" ; <-- Package_Windows.cs EDIT <--"));
                nsiTextFile [editIndex] = $"\"DisplayName\" \"Infirmary Integrated {verNumber}\"";
                File.WriteAllLines (nsiBuffer, nsiTextFile.ToArray ());

                Process proc = new ();

                //makensis.exe {dir}\package.nsi

                string arguments = $"package.nsi";

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

            // "signtool sign /fd digest /tr timeserver /td timeserver-digest /f sigfile.pfx /p password filename"

            arguments = $"sign /fd SHA256 /tr http://timestamp.globalsign.com/tsa/r6advanced1 /td SHA256 /f \"{progVar.pathCert}\" /p {password} \"{pkgName}\"";

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

            // "signtool verify /v /pa filepath"

            arguments = $"verify /v /pa \"{pkgName}\"";

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