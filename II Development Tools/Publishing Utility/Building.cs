using System;
using System.Diagnostics;
using System.IO;

namespace Publishing {

    public class Building {

        public static void Clean (string dirProject, string dirBin, string dirObj) {
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

            proc.StartInfo.FileName = Program.pathDotnet;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.WorkingDirectory = dirProject;
            proc.Start ();
            proc.WaitForExit ();
        }

        public static void Build (string dirProject, string [] listReleases) {
            Process proc = new Process ();
            string arguments = "";

            // "dotnet build -c Release"

            arguments = $"build -c Release";
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine ($"- Executing dotnet {arguments}");
            Console.WriteLine (Environment.NewLine);
            Console.ResetColor ();

            proc.StartInfo.FileName = Program.pathDotnet;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.WorkingDirectory = dirProject;
            proc.Start ();
            proc.WaitForExit ();
        }

        public static void Publish (string dirProject, string release) {
            Process proc = new Process ();
            string arguments = "";

            // "dotnet publish -c Release -r ..."

            arguments = $"publish -c Release -r {release}";
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine ($"- Executing dotnet {arguments}");
            Console.WriteLine (Environment.NewLine);
            Console.ResetColor ();

            proc.StartInfo.FileName = Program.pathDotnet;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.WorkingDirectory = dirProject;
            proc.Start ();
            proc.WaitForExit ();
        }

        public static void Pack (string dirBin, string dirRelease, string release, string verNumber) {
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
                Console.WriteLine ($"Packing build: {release}-{verNumber}");
                string tarName = $"_{release}-{verNumber}.zip";
                arguments = $"-c -f {tarName} \"Infirmary Integrated\"";

                Console.WriteLine ($"- Executing tar {arguments}");
                Console.WriteLine (Environment.NewLine);

                proc.StartInfo.FileName = Program.pathTar;
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