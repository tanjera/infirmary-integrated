using System;
using System.Diagnostics;
using System.IO;

namespace Publishing {

    internal class Program {
        // Parameters to be set for runtime environment

        public const string pathDotnet = @"C:\Program Files\dotnet\dotnet.exe";
        public const string pathTar = @"C:\Windows\System32\tar.exe";
        public const string pathSigntool = @"C:\Program Files (x86)\Windows Kits\10\App Certification Kit\signtool.exe";
        public const string pathCert = @"C:\Users\Ibi\Documents\Code Signing Certificate, Sectigo.pfx";

        public const string dirSolution = @"Y:\Infirmary Integrated, Avalonia";

        private static void Main (string [] args) {
            // Get base directory for solution and project

            string dirRelease = Path.Combine (dirSolution, "Release");
            string dirProject = Path.Combine (dirSolution, "II Avalonia");
            string dirBin = Path.Combine (dirProject, "bin");
            string dirObj = Path.Combine (dirProject, "obj");

            string [] listReleases = {
                "win-x64",
                "linux-x64",
                "osx-x64" };

            Directory.CreateDirectory (dirRelease);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine (Environment.NewLine);
            Console.Write ("What version should this package be named? ");
            string verNumber = Console.ReadLine ().Trim ();
            Console.ResetColor ();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine (Environment.NewLine);
            Console.Write ($"Would you like to clean and recompile II? [y/N] ");
            Console.ResetColor ();

            if (Console.ReadLine ().Trim ().ToLower () == "y") {
                Building.Clean (dirProject, dirBin, dirObj);
                Building.Build (dirProject);

                foreach (string release in listReleases) {
                    Building.Publish (dirProject, release);
                    Building.Pack (dirBin, dirRelease, release, verNumber);
                }
            }

            if (OperatingSystem.IsWindows ()) {                     // Process Windows packages
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine (Environment.NewLine);
                Console.Write ("Would you like to process and sign the Windows package? [y/N] ");
                Console.ResetColor ();

                if (Console.ReadLine ().Trim ().ToLower () == "y") {
                    Package_Windows.Process (dirSolution, dirRelease, verNumber);
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine (Environment.NewLine);
                Console.WriteLine ("Press any key to exit...");
                Console.WriteLine (Environment.NewLine);
                Console.ReadKey ();
            }
        }
    }
}