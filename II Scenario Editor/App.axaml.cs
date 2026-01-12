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

        public II.Settings.Instance Settings = new ();
        
        public Language? Language = new Language ();

        public Scenario? Scenario;

        public WindowSplash? WindowSplash;
        public WindowMain? WindowMain;

        public override void Initialize () {
            AvaloniaXamlLoader.Load (this);
            
            Settings.Load ();                                           // Load config file
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

        public override async void OnFrameworkInitializationCompleted () {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                WindowSplash = new ();
                WindowMain = new (this);

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

        public Task Exit () {
            WindowSplash?.Close ();
            
            if (WindowMain?.WindowStatus != WindowMain.WindowStates.Closed)
                WindowMain?.Close ();

            return Task.CompletedTask;
        }
    }
}