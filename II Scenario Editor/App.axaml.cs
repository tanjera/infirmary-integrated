using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

using II;
using II.Localization;

namespace II_Scenario_Editor {

    public partial class App : Application {
        public static string []? Start_Args;

        public static Language? Language = new Language();

        public static Scenario? Scenario;

        public static Splash? Window_Splash;
        public static Main? Window_Main;

        public override void Initialize () {
            AvaloniaXamlLoader.Load (this);
        }

        public override async void OnFrameworkInitializationCompleted () {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                Window_Splash = new Splash ();
                Window_Main = new Main ();

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
            }

            base.OnFrameworkInitializationCompleted ();
        }

        public static async Task Exit () {
            Window_Splash?.Close ();
            Window_Main?.Close ();
        }
    }
}