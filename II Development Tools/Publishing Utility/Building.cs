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
            Console.WriteLine ($"- {dirProject}: Executing dotnet {arguments}");
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
            Console.WriteLine ($"- {dirProject}: Executing dotnet {arguments}");
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
            Console.WriteLine ($"- {dirProject}: Executing dotnet {arguments}");
            Console.WriteLine (Environment.NewLine);
            Console.ResetColor ();

            proc.StartInfo.FileName = progVar.pathDotnet;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.WorkingDirectory = dirProject;
            proc.Start ();
            proc.WaitForExit ();
        }

        public static void Pack (
                Program.Variables.PackageType packType,
                Program.Variables progVar,
                string dirSimulatorBin, string dirScenarioEditorBin,
                string dirRelease, string release, string verNumber) {
            Process proc = new Process ();
            string arguments = "";

            // Ensure the directory exists and is empty
            if (!Directory.Exists (progVar.dirTemporary))
                Directory.CreateDirectory (progVar.dirTemporary);

            // Define the directory structure for the packages
            string dirTempRoot = Path.Combine (progVar.dirTemporary, "Infirmary Integrated");
            string dirTempSimulator = Path.Combine (dirTempRoot, "Infirmary Integrated");
            string dirTempScenarioEditor = Path.Combine (dirTempRoot, "Infirmary Integrated Scenario Editor");

            // Create the directory structure for the packages
            Directory.CreateDirectory (dirTempRoot);

            // Move files into place for packaging
            string dirSimulatorPub = Path.Combine (new string [] { dirSimulatorBin, "Release", progVar.versionDotnet, release, "publish" });
            string dirScenarioEditorPub = Path.Combine (new string [] { dirScenarioEditorBin, "Release", progVar.versionDotnet, release, "publish" });

            Directory.Move (dirSimulatorPub, dirTempSimulator);
            Directory.Move (dirScenarioEditorPub, dirTempScenarioEditor);

            Console.WriteLine (Environment.NewLine);
            Console.ForegroundColor = ConsoleColor.Yellow;

            // Package the directories/files into a tarball or zip

            string packName = "", packTitle = "", osName = "";

            Console.WriteLine ($"Packing build: {release}-{verNumber}");

            osName = release switch {
                "win-x64" => "windows",
                "linux-x64" => "linux",
                "osx-x64" => "macos",
                _ => ""
            };

            packTitle = $"infirmary-integrated-{verNumber}-{osName}";

            if (packType == Program.Variables.PackageType.Tar) {
                packName = $"{packTitle}.tar.gz";
                arguments = $"-czf {packName} \"Infirmary Integrated\"";

                Console.WriteLine ($"- Executing tar {arguments}");

                proc.StartInfo.FileName = progVar.pathTar;
                proc.StartInfo.Arguments = arguments;
            } else if (packType == Program.Variables.PackageType.Zip) {
                packName = $"{packTitle}.zip";
                arguments = $"a -tzip {packName} \"Infirmary Integrated\"";

                Console.WriteLine ($"- Executing 7z {arguments}");

                proc.StartInfo.FileName = progVar.path7Zip;
                proc.StartInfo.Arguments = arguments;
            }

            Console.WriteLine (Environment.NewLine);

            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.WorkingDirectory = progVar.dirTemporary;
            proc.Start ();
            proc.WaitForExit ();

            string packFile = Path.Combine (progVar.dirTemporary, packName);
            string packRelease = Path.Combine (dirRelease, packName);

            // Move the package to the Infirmary Integrated/Release folder
            if (File.Exists (packFile)) {
                Console.WriteLine ($"- Moving package file to {dirRelease}");
                File.Move (packFile, packRelease, true);
            } else {
                Console.WriteLine ($"Error: Unable to locate {packFile}");
            }

            // Clean the temporary directory
            Directory.Delete (progVar.dirTemporary, true);

            Console.ResetColor ();
        }
    }
}