using System;
using System.Diagnostics;
using System.IO;

namespace Publishing {

    internal class Program {
        // Parameters to be set for runtime environment

        private const string pathDotnet = @"C:\Program Files\dotnet\dotnet.exe";
        private const string pathTar = @"C:\Windows\System32\tar.exe";
        private const string dirSolution = @"Y:\Infirmary Integrated, Avalonia";

        private static void Main (string [] args) {
            // Get base directory for solution and project

            string dirRelease = Path.Combine (dirSolution, "Releasing");
            string dirProject = Path.Combine (dirSolution, "II Avalonia");
            string pathProject = Path.Combine (dirProject, "II Avalonia.csproj");
            string dirBin = Path.Combine (dirProject, "bin");
            string dirObj = Path.Combine (dirProject, "obj");

            string [] listReleases = {
                "win-x64",
                "linux-x64",
                "osx-x64"
                };

            Clean (dirProject, dirBin, dirObj);
            Build (dirProject, listReleases);

            foreach (string release in listReleases) {
                Publish (dirProject, release);
                Pack (dirBin, dirRelease, release);
            }

            MoveInstaller_Windows (dirSolution, dirRelease);

            Console.WriteLine (Environment.NewLine);
        }

        private static void Clean (string dirProject, string dirBin, string dirObj) {
            Process proc = new Process ();
            string arguments = "";

            Console.WriteLine (Environment.NewLine);
            Console.ForegroundColor = ConsoleColor.Yellow;

            if (Directory.Exists (dirBin)) {
                Console.WriteLine ($"- Deleting {dirBin}");
                Directory.Delete (dirBin, true);
            }

            if (Directory.Exists (dirObj)) {
                Console.WriteLine ($"- Deleting {dirObj}");
                Directory.Delete (dirObj, true);
            }

            // "dotnet clean"

            arguments = $"clean";
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine ($"- Executing dotnet {arguments}");
            Console.WriteLine (Environment.NewLine);
            Console.ResetColor ();

            proc.StartInfo.FileName = pathDotnet;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.WorkingDirectory = dirProject;
            proc.Start ();
            proc.WaitForExit ();
        }

        private static void Build (string dirProject, string [] listReleases) {
            Process proc = new Process ();
            string arguments = "";

            // "dotnet build -c Release"

            arguments = $"build -c Release";
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine ($"- Executing dotnet {arguments}");
            Console.WriteLine (Environment.NewLine);
            Console.ResetColor ();

            proc.StartInfo.FileName = pathDotnet;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.WorkingDirectory = dirProject;
            proc.Start ();
            proc.WaitForExit ();
        }

        private static void Publish (string dirProject, string release) {
            Process proc = new Process ();
            string arguments = "";

            // "dotnet publish -c Release -r ..."

            arguments = $"publish -c Release -r {release}";
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine ($"- Executing dotnet {arguments}");
            Console.WriteLine (Environment.NewLine);
            Console.ResetColor ();

            proc.StartInfo.FileName = pathDotnet;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.WorkingDirectory = dirProject;
            proc.Start ();
            proc.WaitForExit ();
        }

        private static void Pack (string dirBin, string dirRelease, string release) {
            Process proc = new Process ();
            string arguments = "";

            string dirBuild = Path.Combine (dirBin, $"Release\\net5.0\\{release}");
            string dirPublish = Path.Combine (dirBuild, "publish");
            string dirII = Path.Combine (dirBuild, "Infirmary Integrated");

            if (File.Exists (dirII)) {
                File.Delete (dirII);
            }

            Directory.Move (dirPublish, dirII);

            Console.WriteLine (Environment.NewLine);
            Console.ForegroundColor = ConsoleColor.Yellow;

            if (Directory.Exists (Path.Combine (dirII))) {
                Console.WriteLine ($"Packing build: {release}");
                string tarName = $"_{release}.zip";
                arguments = $"-c -f {tarName} \"Infirmary Integrated\"";

                Console.WriteLine ($"- Executing tar {arguments}");
                Console.WriteLine (Environment.NewLine);

                proc.StartInfo.FileName = pathTar;
                proc.StartInfo.Arguments = arguments;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.WorkingDirectory = dirBuild;
                proc.Start ();
                proc.WaitForExit ();

                string tarFile = Path.Combine (dirBuild, tarName);
                string tarRelease = Path.Combine (dirRelease, tarName);

                if (File.Exists (tarFile)) {
                    Console.WriteLine ($"- Moving tar file to {dirRelease}");
                    File.Move (tarFile, tarRelease, true);
                } else {
                    Console.WriteLine ($"Error: Unable to locate {tarFile}");
                }
            } else {
                Console.WriteLine ($"Error: Unable to locate {dirII}");
            }

            Console.ResetColor ();
        }

        private static void MoveInstaller_Windows (string dirSolution, string dirRelease) {
            string insName = "infirmary-integrated-.msi";
            string insFile = Path.Combine (dirSolution, @$"Installer, Windows\Release\{insName}");
            string insTarget = Path.Combine (dirRelease, insName);

            if (File.Exists (insFile)) {
                File.Move (insFile, insTarget, true);
            }
        }
    }
}