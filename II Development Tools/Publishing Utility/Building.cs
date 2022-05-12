using System;
using System.Diagnostics;
using System.IO;

namespace Publishing {

    public class Building {

        public static void Clean (Program.Variables progVar, string dirProject, string dirBin, string dirObj) {
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

            proc.StartInfo.FileName = progVar.pathDotnet;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.WorkingDirectory = dirProject;
            proc.Start ();
            proc.WaitForExit ();
        }

        public static void Build (Program.Variables progVar, string dirProject) {
            Process proc = new Process ();
            string arguments = "";

            // "dotnet build -c Release"

            arguments = $"build -c Release";
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine ($"- Executing dotnet {arguments}");
            Console.WriteLine (Environment.NewLine);
            Console.ResetColor ();

            proc.StartInfo.FileName = progVar.pathDotnet;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.WorkingDirectory = dirProject;
            proc.Start ();
            proc.WaitForExit ();
        }

        public static void Publish (Program.Variables progVar, string dirProject, string release) {
            Process proc = new Process ();
            string arguments = "";

            // "dotnet publish -c Release -r ..."

            arguments = $"publish -c Release -r {release}";

            if (release.StartsWith ("osx"))
                arguments += " -p:UseAppHost=true";

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine ($"- Executing dotnet {arguments}");
            Console.WriteLine (Environment.NewLine);
            Console.ResetColor ();

            proc.StartInfo.FileName = progVar.pathDotnet;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.WorkingDirectory = dirProject;
            proc.Start ();
            proc.WaitForExit ();
        }

        public static void Pack (Program.Variables progVar, string dirBin, string dirRelease, string release, string verNumber) {
            Process proc = new Process ();
            string arguments = "";

            string dirBuild = Path.Combine (new string[] { dirBin, "Release", progVar.versionDotnet, release });
            string dirPublish = Path.Combine (dirBuild, "publish");
            string dirII = Path.Combine (dirBuild, "Infirmary Integrated");

            if (File.Exists (dirII)) {
                File.Delete (dirII);
            }

            Directory.Move (dirPublish, dirII);

            Console.WriteLine (Environment.NewLine);
            Console.ForegroundColor = ConsoleColor.Yellow;

            if (Directory.Exists (Path.Combine (dirII))) {
                Console.WriteLine ($"Packing build: {release}-{verNumber}");
                string tarName = $"_{release}-{verNumber}.zip";
                arguments = $"-c -f {tarName} \"Infirmary Integrated\"";

                Console.WriteLine ($"- Executing tar {arguments}");
                Console.WriteLine (Environment.NewLine);

                proc.StartInfo.FileName = progVar.pathTar;
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

            Directory.Move (dirII, dirPublish);

            Console.ResetColor ();
        }
    }
}