using System;
using System.Threading.Tasks;
using System.Timers;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using II;
using II.Localization;
using II.Server;

namespace II_Avalonia {

    public class App : Application {
        public static string []? Start_Args;

        public static Settings Settings = new Settings ();
        public static Server Server = new Server ();
        public static Mirror Mirror = new Mirror ();
        public static Language Language = new Language ();

        public static Scenario? Scenario;
        public static Patient? Patient;

        public static Splash? Window_Splash;
        public static Main? Window_Main;

        public static DeviceMonitor? Device_Monitor;
        public static DeviceECG? Device_ECG;
        public static DeviceDefib? Device_Defib;
        public static DeviceIABP? Device_IABP;
        public static DeviceEFM? Device_EFM;

        public static System.Timers.Timer Timer_Main = new System.Timers.Timer ();

        public override void Initialize () {
            AvaloniaXamlLoader.Load (this);

            Timer_Main.Interval = 10; // q 10 milliseconds
            Timer_Main.Start ();

            II.File.Init ();                                        // Init file structure (for config file, temp files)
            App.Settings.Load ();                                   // Load config file
            App.Language = new Language (App.Settings.Language);    // Load localization dictionary based on settings
        }

        public override void OnFrameworkInitializationCompleted () {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                Window_Splash = new Splash ();
                Window_Main = new Main ();

                // Show the splash screen for 2 seconds, then swap out to the main window
                desktop.MainWindow = Window_Splash;

#if !DEBUG
                await Task.Delay (2000);
#endif

                Window_Splash.Hide ();
                Window_Main.Show ();

                desktop.MainWindow = Window_Main;

                Window_Splash.Close ();

                Start_Args = desktop.Args;
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
    }
}