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
        }

        public override void OnFrameworkInitializationCompleted () {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                desktop.MainWindow = new PatientEditor ();
                Start_Args = desktop.Args;
            }

            base.OnFrameworkInitializationCompleted ();
        }
    }
}