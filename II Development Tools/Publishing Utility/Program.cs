using System;
using System.Diagnostics;
using System.IO;

namespace Publishing {

    public class Program {
        // Parameters to be set for runtime environment

        public class Variables {
            public string pathDotnet;
            public string pathTar;
            public string pathSigntool;
            public string pathCert;
            public string dirSolution;

            public Variables() {
                if (OperatingSystem.IsWindows()) {
                    pathDotnet = @"C:\Program Files\dotnet\dotnet.exe";
                    pathTar = @"C:\Windows\System32\tar.exe";
                    pathSigntool = @"C:\Program Files (x86)\Windows Kits\10\App Certification Kit\signtool.exe";
                    pathCert = @"C:\Users\Ibi\Documents\Code Signing Certificate, Sectigo.pfx";
                    dirSolution = @"C:\Users\Ibi\Documents\Infirmary Integrated";
                } else if (OperatingSystem.IsLinux()) {
                    pathDotnet = "dotnet";
                    pathTar = "tar";
                    dirSolution = @"/home/ibi/Documents/Infirmary Integrated";
                }
            }
        }

        private static void Main (string [] args) {
            Program p = new Program();
            p.Init(args);
        }

        public void Init (string [] args) {
            
            Variables progVar = new Variables();

            // Get base directory for solution and project
            string dirRelease = Path.Combine (progVar.dirSolution, "Release");
            string dirProject = Path.Combine (progVar.dirSolution, "II Avalonia");
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
                Building.Clean (progVar, dirProject, dirBin, dirObj);
                Building.Build (progVar, dirProject);

                foreach (string release in listReleases) {
                    Building.Publish (progVar, dirProject, release);
                    Building.Pack (progVar, dirBin, dirRelease, release, verNumber);
                }
            }

            if (OperatingSystem.IsWindows ()) {                     // Process Windows packages
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine (Environment.NewLine);
                Console.Write ("Would you like to process and sign the Windows package? [y/N] ");
                Console.ResetColor ();

                if (Console.ReadLine ().Trim ().ToLower () == "y") {
                    Package_Windows.Process (progVar, dirRelease, verNumber);
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