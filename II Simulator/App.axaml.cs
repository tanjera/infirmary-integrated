using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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

using LibVLCSharp.Shared;

namespace IISIM {

    public class App : Application {
        public string []? Start_Args;

        public II.Settings.Simulation Simulation = new ();
        public Server Server = new ();
        public Mirror Mirror = new ();
        public Language Language = new ();

        public Scenario? Scenario;
        public Physiology? Physiology { get => Scenario?.Physiology; }

        public Splash? Window_Splash;
        public Control? Window_Main;

        public DeviceMonitor? Device_Monitor;
        public DeviceECG? Device_ECG;
        public DeviceDefib? Device_Defib;
        public DeviceIABP? Device_IABP;
        public DeviceEFM? Device_EFM;

        public System.Timers.Timer Timer_Main = new ();

        /* For temporary unpacking of files on runtime e.g. audio */
        public string PathTemporary = Path.Combine (Path.GetTempPath (), Guid.NewGuid ().ToString ());

        /* Audio Engine */
        public LibVLC? AudioLib;

        public override void Initialize () {
            AvaloniaXamlLoader.Load (this);

            Timer_Main.Interval = 10; // q 10 milliseconds
            Timer_Main.Start ();

            II.File.Init ();                                            // Init file structure (for config file, temp files)
            Simulation.Load ();                                           // Load config file
            Language = new (Simulation.Language);                         // Load localization dictionary based on settings

            try {                                                       // try/catch in case LibVLC is not installed, fails, etc.
                AudioLib = new ();                                      // Init audio engine library
            } catch {
                AudioLib = null;
            }
        }

        public override async void OnFrameworkInitializationCompleted () {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                Start_Args = desktop.Args;      // Must set *before* new Window_Main(this) for loading file from desktop

                Window_Splash = new ();
                Window_Main = new (this);

                /* Show the splash screen for 2 seconds, then swap out to the main window
                 * Note: Still run through Window_Splash logic for testing, even if Task.Delay(0)
                 */
                desktop.MainWindow = Window_Splash;
                Window_Splash.Show ();

#if RELEASE     // Splash screen for 2 seconds for Release version
                await Task.Delay (2000);
#else           // Otherwise (Debug version) immediately close splash screen
                await Task.Delay (200);
#endif

                Window_Splash.Hide ();
                Window_Main.Show ();

                desktop.MainWindow = Window_Main;

                Window_Splash.Close ();

                if (OperatingSystem.IsMacOS ())
                    _ = MacOSRegisterLaunchServices ();
            }

            base.OnFrameworkInitializationCompleted ();
        }

        public void OnMenuAbout_Click (object sender, EventArgs e) {
            if (Window_Main is not null)
                _ = Window_Main.DialogAbout ();
        }

        public void Exit () {
            Window_Splash?.Close ();

            Device_Monitor?.Close ();
            Device_Defib?.Close ();
            Device_ECG?.Close ();
            Device_IABP?.Close ();
            Device_EFM?.Close ();

            Window_Main?.Close ();

            try {               // If not fully initialized, may throw an exception... since exiting, just abort.
                //AudioLib?.Dispose ();
            } catch { }
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