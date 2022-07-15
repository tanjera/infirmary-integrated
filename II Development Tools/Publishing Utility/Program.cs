using System;
using System.Diagnostics;
using System.IO;

namespace Publishing {

    public class Program {
        // Parameters to be set for runtime environment

        public class Variables {
            public string versionDotnet;
            public string pathDotnet;
            public string pathTar;
            public string path7Zip;
            public string pathNSIS;
            public string pathSigntool;
            public string pathCert;
            public string dirSolution;
            public string dirTemporary;

            public Variables () {
                versionDotnet = "net6.0";
                dirTemporary = Path.Combine (Path.GetTempPath (), Guid.NewGuid ().ToString ());

                if (OperatingSystem.IsWindows ()) {
                    pathDotnet = @"C:\Program Files\dotnet\dotnet.exe";
                    pathTar = @"C:\Windows\System32\tar.exe";
                    path7Zip = @"C:\Program Files\7-Zip\7z.exe";
                    pathNSIS = @"C:\Program Files (x86)\NSIS\makensis.exe";
                    pathSigntool = @"C:\Program Files (x86)\Windows Kits\10\App Certification Kit\signtool.exe";
                    pathCert = @"C:\Users\Ibi\Documents\Code Signing Certificate, Sectigo.pfx";
                    //dirSolution = @"Z:\Infirmary Integrated";
                    dirSolution = @"C:\Users\Ibi\Documents\Infirmary Integrated";
                } else if (OperatingSystem.IsLinux ()) {
                    pathDotnet = "dotnet";
                    pathTar = "tar";
                    dirSolution = @"/home/ibi/Documents/Infirmary Integrated";
                }
            }

            public enum PackageType {
                Tar,
                Zip
            }
        }

        private static void Main (string [] args) {
            Program p = new Program ();
            p.Init (args);
        }

        public void Init (string [] args) {
            Variables progVar = new ();

            // Get base directory for solution and project
            string dirRelease = Path.Combine (progVar.dirSolution, "Release");

            string dirSimulator = Path.Combine (progVar.dirSolution, "II Simulator");
            string dirSimulatorBin = Path.Combine (dirSimulator, "bin");
            string dirSimulatorObj = Path.Combine (dirSimulator, "obj");

            string dirScenarioEditor = Path.Combine (progVar.dirSolution, "II Scenario Editor");
            string dirScenarioEditorBin = Path.Combine (dirScenarioEditor, "bin");
            string dirScenarioEditorObj = Path.Combine (dirScenarioEditor, "obj");

            string [] listReleases = {
                "win-x64",
                "linux-x64",
                "osx-x64"
                };

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
                Building.Clean (progVar, dirSimulator, dirSimulatorBin, dirSimulatorObj);
                Building.Clean (progVar, dirScenarioEditor, dirScenarioEditorBin, dirScenarioEditorObj);

                Building.Build (progVar, dirSimulator);
                Building.Build (progVar, dirScenarioEditor);

                foreach (string release in listReleases) {
                    Building.Publish (progVar, dirSimulator, release);
                    Building.Publish (progVar, dirScenarioEditor, release);

                    if (release.StartsWith ("win"))
                        Building.Pack (Variables.PackageType.Zip, progVar, dirSimulatorBin, dirScenarioEditorBin, dirRelease, release, verNumber);
                    else
                        Building.Pack (Variables.PackageType.Tar, progVar, dirSimulatorBin, dirScenarioEditorBin, dirRelease, release, verNumber);
                }
            }

            if (OperatingSystem.IsWindows ()) {                     // Process Windows packages
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine (Environment.NewLine);
                Console.Write ("Would you like to create and sign the Windows package? [y/N] ");
                Console.ResetColor ();

                if (Console.ReadLine ().Trim ().ToLower () == "y") {
                    // Re-publish for Windows target because previous Pack() moved/deleted the Publish directory
                    Building.Publish (progVar, dirSimulator, "win-x64");
                    Building.Publish (progVar, dirScenarioEditor, "win-x64");

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