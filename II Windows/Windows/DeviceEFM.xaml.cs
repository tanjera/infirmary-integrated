using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using II;
using II.Rhythm;
using II.Waveform;

namespace II_Windows {

    /// <summary>
    /// Interaction logic for DeviceEFM.xaml
    /// </summary>
    public partial class DeviceEFM : Window {

        private bool isFullscreen = false,
             isPaused = false;

        private List<Controls.EFMTracing> listTracings = new List<Controls.EFMTracing> ();

        private Timer timerTracing = new Timer ();

        // Define WPF UI commands for binding
        private ICommand icToggleFullscreen, icPauseDevice, icCloseDevice, icExitProgram,
            icSaveScreen, icPrintScreen;

        public ICommand IC_ToggleFullscreen { get { return icToggleFullscreen; } }
        public ICommand IC_PauseDevice { get { return icPauseDevice; } }
        public ICommand IC_CloseDevice { get { return icCloseDevice; } }
        public ICommand IC_ExitProgram { get { return icExitProgram; } }
        public ICommand IC_SaveScreen { get { return icSaveScreen; } }
        public ICommand IC_PrintScreen { get { return icPrintScreen; } }

        public DeviceEFM () {
            InitializeComponent ();
            DataContext = this;

            InitTimers ();
            InitInterface ();
        }

        ~DeviceEFM () => Dispose ();

        public void Dispose () {
            /* Clean subscriptions from the Main Timer */
            App.Timer_Main.Tick -= timerTracing.Process;

            /* Dispose of local Timers */
            timerTracing.Dispose ();

            /* Unsubscribe from the main Patient event listing */
            App.Patient.PatientEvent -= OnPatientEvent;
        }

        private void InitTimers () {
            timerTracing.Set (Draw.RefreshTime);
            App.Timer_Main.Tick += timerTracing.Process;
            timerTracing.Tick += OnTick_Tracing;
            timerTracing.Start ();
        }

        private void InitInterface () {
            /* Initiate ICommands for KeyBindings */
            icToggleFullscreen = new ActionCommand (() => ToggleFullscreen ());
            icPauseDevice = new ActionCommand (() => TogglePause ());
            icCloseDevice = new ActionCommand (() => Close ());
            icExitProgram = new ActionCommand (() => App.Patient_Editor.Exit ());
            icSaveScreen = new ActionCommand (() => SaveScreen ());
            icPrintScreen = new ActionCommand (() => PrintScreen ());

            /* Populate UI strings per language selection */
            wdwDeviceEFM.Title = App.Language.Localize ("EFM:WindowTitle");
            menuDevice.Header = App.Language.Localize ("MENU:MenuDeviceOptions");
            menuPauseDevice.Header = App.Language.Localize ("MENU:MenuPauseDevice");
            menuToggleFullscreen.Header = App.Language.Localize ("MENU:MenuToggleFullscreen");
            menuSaveScreen.Header = App.Language.Localize ("MENU:MenuSaveScreen");
            menuPrintScreen.Header = App.Language.Localize ("MENU:MenuPrintScreen");
            menuCloseDevice.Header = App.Language.Localize ("MENU:MenuCloseDevice");
            menuExitProgram.Header = App.Language.Localize ("MENU:MenuExitProgram");

            // Instantiate and add Tracings to UI
            Controls.EFMTracing fhrTracing = new Controls.EFMTracing (new Strip (Lead.Values.FHR, 600f));
            fhrTracing.SetValue (Grid.RowProperty, 0);
            fhrTracing.SetValue (Grid.ColumnProperty, 0);
            fhrTracing.Background = new ImageBrush (new BitmapImage (
                new Uri ("pack://application:,,,/Resources/FHR Grid.png")));
            listTracings.Add (fhrTracing);
            displayGrid.Children.Add (fhrTracing);

            Controls.EFMTracing tocoTracing = new Controls.EFMTracing (new Strip (Lead.Values.TOCO, 600f));
            tocoTracing.SetValue (Grid.RowProperty, 2);
            tocoTracing.SetValue (Grid.ColumnProperty, 0);
            tocoTracing.Background = new ImageBrush (new BitmapImage (
                new Uri ("pack://application:,,,/Resources/Toco Grid.png")));
            listTracings.Add (tocoTracing);
            displayGrid.Children.Add (tocoTracing);
        }

        public void Load_Process (string inc) {
            StringReader sRead = new StringReader (inc);

            try {
                string line;
                while ((line = sRead.ReadLine ()) != null) {
                    if (line.Contains (":")) {
                        string pName = line.Substring (0, line.IndexOf (':')),
                                pValue = line.Substring (line.IndexOf (':') + 1);
                        switch (pName) {
                            default: break;
                            case "isPaused": isPaused = bool.Parse (pValue); break;
                            case "isFullscreen": isFullscreen = bool.Parse (pValue); break;
                        }
                    }
                }
            } catch {
            } finally {
                sRead.Close ();
            }
        }

        public string Save () {
            StringBuilder sWrite = new StringBuilder ();

            sWrite.AppendLine (String.Format ("{0}:{1}", "isPaused", isPaused));
            sWrite.AppendLine (String.Format ("{0}:{1}", "isFullscreen", isFullscreen));

            return sWrite.ToString ();
        }

        private void SaveScreen ()
            => Screenshot.SavePdf (
                Screenshot.SavePng (Screenshot.GetBitmap (displayGrid, 1), II.File.GetCachePath ("png")),
                App.Language.Localize ("EFM:WindowTitle"), "");

        private void PrintScreen ()
            => II.Screenshot.PrintPdf (II.Screenshot.AssemblePdf (
                Screenshot.SavePng (Screenshot.GetBitmap (displayGrid, 1), II.File.GetCachePath ("png")),
                    App.Language.Localize ("EFM:WindowTitle"), ""));

        private void ApplyFullScreen () {
            menuToggleFullscreen.IsChecked = isFullscreen;

            switch (isFullscreen) {
                default:
                case false:
                    wdwDeviceEFM.WindowStyle = WindowStyle.SingleBorderWindow;
                    wdwDeviceEFM.WindowState = WindowState.Normal;
                    wdwDeviceEFM.ResizeMode = ResizeMode.CanResize;
                    break;

                case true:
                    wdwDeviceEFM.WindowStyle = WindowStyle.None;
                    wdwDeviceEFM.WindowState = WindowState.Maximized;
                    wdwDeviceEFM.ResizeMode = ResizeMode.NoResize;
                    break;
            }
        }

        private void TogglePause () {
            isPaused = !isPaused;
            menuPauseDevice.IsChecked = isPaused;

            if (!isPaused)
                listTracings.ForEach (c => c.Strip.Unpause ());
        }

        private void ToggleFullscreen () {
            isFullscreen = !isFullscreen;
            ApplyFullScreen ();
        }

        private void MenuClose_Click (object s, RoutedEventArgs e)
            => this.Close ();

        private void MenuExit_Click (object s, RoutedEventArgs e)
            => App.Patient_Editor.Exit ();

        private void MenuTogglePause_Click (object s, RoutedEventArgs e)
            => TogglePause ();

        private void MenuFullscreen_Click (object sender, RoutedEventArgs e)
            => ToggleFullscreen ();

        private void MenuSaveScreen_Click (object sender, RoutedEventArgs e)
            => SaveScreen ();

        private void MenuPrintScreen_Click (object sender, RoutedEventArgs e)
            => PrintScreen ();

        private void OnClosed (object sender, EventArgs e)
            => this.Dispose ();

        private void OnTick_Tracing (object sender, EventArgs e) {
            if (isPaused)
                return;

            for (int i = 0; i < listTracings.Count; i++) {
                listTracings [i].Strip.Scroll ();
                listTracings [i].DrawTracing ();
            }
        }

        public void OnPatientEvent (object sender, Patient.PatientEventArgs e) {
            switch (e.EventType) {
                default: break;
                case Patient.PatientEventTypes.Obstetric_Baseline:
                    listTracings.ForEach (c => c.Strip.Add_Beat__Obstetric_Baseline (App.Patient));
                    break;

                case Patient.PatientEventTypes.Obstetric_Contraction_Start:
                    listTracings.ForEach (c => c.Strip.ClearFuture (App.Patient));
                    listTracings.ForEach (c => c.Strip.Add_Beat__Obstetric_Contraction_Start (App.Patient));
                    break;

                case Patient.PatientEventTypes.Obstetric_Contraction_End:
                    listTracings.ForEach (c => c.Strip.ClearFuture (App.Patient));
                    listTracings.ForEach (c => c.Strip.Add_Beat__Obstetric_Baseline (App.Patient));
                    break;
            }
        }
    }
}