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

namespace IISE {

    public partial class App : Application {
        public static string []? Start_Args;

        public static Language? Language = new Language ();

        public static Scenario? Scenario;

        public static WindowSplash? WindowSplash;
        public static WindowMain? WindowMain;

        public override void Initialize () {
            AvaloniaXamlLoader.Load (this);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

        public override async void OnFrameworkInitializationCompleted () {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                WindowSplash = new ();
                WindowMain = new ();

                // Show the splash screen for 2 seconds, then swap out to the main window
                desktop.MainWindow = WindowSplash;
                WindowSplash.Show ();

#if !DEBUG
                await Task.Delay (2000);
#endif

                WindowSplash.Hide ();
                WindowMain.Show ();

                desktop.MainWindow = WindowMain;

                WindowSplash.Close ();

                Start_Args = desktop.Args;
            }

            base.OnFrameworkInitializationCompleted ();
        }

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        public void OnMenuAbout_Click (object sender, EventArgs e) {
            if (WindowMain is not null)
                _ = WindowMain.DialogAbout ();
        }

        public static Task Exit () {
            WindowSplash?.Close ();
            WindowMain?.Close ();

            return Task.CompletedTask;
        }
    }
}