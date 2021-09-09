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

        public static SplashScreen? Splash_Screen;
        public static PatientEditor? Patient_Editor;

        public static DeviceMonitor? Device_Monitor;
        public static DeviceECG? Device_ECG;
        public static DeviceDefib? Device_Defib;
        public static DeviceIABP? Device_IABP;
        public static DeviceEFM? Device_EFM;

        public static DialogAbout? Dialog_About;
        public static DialogInitial? Dialog_Language;
        public static DialogUpgrade? Dialog_Upgrade;

        public static System.Timers.Timer Timer_Main = new System.Timers.Timer ();

        public override void Initialize () {
            AvaloniaXamlLoader.Load (this);

            Timer_Main.Interval = 10; // q 10 milliseconds
            Timer_Main.Start ();

            II.File.Init ();
            App.Settings.Load ();
        }

        public async override void OnFrameworkInitializationCompleted () {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                Splash_Screen = new SplashScreen ();
                Patient_Editor = new PatientEditor ();

                // Show the splash screen for 2 seconds, then swap out to the main window
                desktop.MainWindow = Splash_Screen;

#if !DEBUG
                await Task.Delay (2000);
#endif

                desktop.MainWindow = Patient_Editor;
                Splash_Screen.Hide ();
                Patient_Editor.Show ();
                Splash_Screen.Close ();

                Start_Args = desktop.Args;
            }

            base.OnFrameworkInitializationCompleted ();
        }
    }
}