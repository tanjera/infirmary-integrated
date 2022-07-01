using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

using II;
using II.Localization;
using II.Server;

namespace IISIM {

    public class App : Application {
        public static string []? Start_Args;

        public static II.Settings.Simulator Settings = new ();
        public static Server Server = new ();
        public static Mirror Mirror = new ();
        public static Language Language = new ();

        public static Scenario? Scenario;
        public static Patient? Patient;

        public static WindowSplash? Window_Splash;
        public static WindowMain? Window_Main;

        public static DeviceMonitor? Device_Monitor;
        public static DeviceECG? Device_ECG;
        public static DeviceDefib? Device_Defib;
        public static DeviceIABP? Device_IABP;
        public static DeviceEFM? Device_EFM;

        public static System.Timers.Timer Timer_Main = new ();

        public override void Initialize () {
            AvaloniaXamlLoader.Load (this);

            Timer_Main.Interval = 10; // q 10 milliseconds
            Timer_Main.Start ();

            II.File.Init ();                                        // Init file structure (for config file, temp files)
            App.Settings.Load ();                                   // Load config file
            App.Language = new Language (App.Settings.Language);    // Load localization dictionary based on settings
        }

        public override async void OnFrameworkInitializationCompleted () {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                Window_Splash = new ();
                Window_Main = new ();

                // Show the splash screen for 2 seconds, then swap out to the main window
                desktop.MainWindow = Window_Splash;
                Window_Splash.Show ();

#if !DEBUG
                await Task.Delay (2000);
#endif

                Window_Splash.Hide ();
                Window_Main.Show ();

                desktop.MainWindow = Window_Main;

                Window_Splash.Close ();

                Start_Args = desktop.Args;

                if (OperatingSystem.IsMacOS ())
                    _ = MacOSRegisterLaunchServices ();
            }

            base.OnFrameworkInitializationCompleted ();
        }

        public static void Exit () {
            Window_Splash?.Close ();

            Device_Monitor?.Close ();
            Device_Defib?.Close ();
            Device_ECG?.Close ();
            Device_IABP?.Close ();
            Device_EFM?.Close ();

            Window_Main?.Close ();
        }

        public static Task MacOSRegisterLaunchServices () {
            /* Mac OS X Launch Services is a registration service that links the .app's Info.plist w/ file types.
             * By registering with Launch Services, the OS should recognize the file extension (.ii) in the Info.plist
             * and attempt to recognize the files as being associated with this app.
             */

            try {   // This function is a "fire and forget"- hopefully it works, possibly it won't; either way, it won't hang.
                string execAssembly = Assembly.GetExecutingAssembly ().Location;
                string execApp = execAssembly.Substring (0, execAssembly.IndexOf ("Infirmary Integrated.app") + "Infirmary Integrated.app".Length);
                Process proc = new Process ();
                proc.StartInfo.FileName = "/System/Library/Frameworks/CoreServices.framework/Frameworks/LaunchServices.framework/Support/lsregister";
                proc.StartInfo.Arguments = $"-R -f {execApp}";
                proc.StartInfo.UseShellExecute = false;
                proc.Start ();
                proc.WaitForExit ();

                Console.WriteLine ($"Registered {execApp} with Mac OS X Launch Services");

                return Task.CompletedTask;
            } catch {
                return Task.CompletedTask;
            }
        }
    }
}