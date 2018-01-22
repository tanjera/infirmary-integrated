using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using II;
using II.Rhythm;
using II.Localization;

namespace II_Windows {
    /// <summary>
    /// Interaction logic for DeviceIABP.xaml
    /// </summary>
    public partial class DeviceIABP : Window {

        bool isFullscreen = false,
             isPaused = false;

        List<Controls.IABPTracing> listTracings = new List<Controls.IABPTracing> ();

        Timer timerTracing = new Timer ();

        // Define WPF UI commands for binding
        private ICommand icPauseDevice, icCloseDevice, icExitProgram;
        public ICommand IC_PauseDevice { get { return icPauseDevice; } }
        public ICommand IC_CloseDevice { get { return icCloseDevice; } }
        public ICommand IC_ExitProgram { get { return icExitProgram; } }


        public DeviceIABP () {
            InitializeComponent ();
            DataContext = this;

            InitInterface ();

            timerTracing.Interval = Waveforms.Draw_Refresh;
            App.Timer_Main.Tick += timerTracing.Process;
            timerTracing.Tick += OnTick_Tracing;
            timerTracing.Start ();
        }

        private void InitInterface () {
            // Initiate ICommands for KeyBindings
            icPauseDevice = new ActionCommand (() => TogglePause ());
            icCloseDevice = new ActionCommand (() => this.Close ());
            icExitProgram = new ActionCommand (() => App.Patient_Editor.RequestExit ());

            // Populate UI strings per language selection
            Languages.Values l = App.Language.Value;

            wdwDeviceIABP.Title = Strings.Lookup (l, "IABP:WindowTitle");
            menuDevice.Header = Strings.Lookup (l, "MENU:MenuDeviceOptions");
            menuPauseDevice.Header = Strings.Lookup (l, "MENU:MenuPauseDevice");
            menuToggleFullscreen.Header = Strings.Lookup (l, "MENU:MenuToggleFullscreen");
            menuCloseDevice.Header = Strings.Lookup (l, "MENU:MenuCloseDevice");
            menuExitProgram.Header = Strings.Lookup (l, "MENU:MenuExitProgram");
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
                sRead.Close ();
                return;
            }

            sRead.Close ();
        }

        public string Save () {
            StringBuilder sWrite = new StringBuilder ();

            sWrite.AppendLine (String.Format ("{0}:{1}", "isPaused", isPaused));
            sWrite.AppendLine (String.Format ("{0}:{1}", "isFullscreen", isFullscreen));

            return sWrite.ToString ();
        }

        private void ApplyFullScreen () {
            menuToggleFullscreen.IsChecked = isFullscreen;

            switch (isFullscreen) {
                default:
                case false:
                    wdwDeviceIABP.WindowStyle = WindowStyle.SingleBorderWindow;
                    wdwDeviceIABP.WindowState = WindowState.Normal;
                    wdwDeviceIABP.ResizeMode = ResizeMode.CanResize;
                    break;

                case true:
                    wdwDeviceIABP.WindowStyle = WindowStyle.None;
                    wdwDeviceIABP.WindowState = WindowState.Maximized;
                    wdwDeviceIABP.ResizeMode = ResizeMode.NoResize;
                    break;
            }
        }

        private void TogglePause () {
            isPaused = !isPaused;
            menuPauseDevice.IsChecked = isPaused;

            if (!isPaused)
                foreach (Controls.IABPTracing c in listTracings)
                    c.wfStrip.Unpause ();
        }

        private void MenuClose_Click (object s, RoutedEventArgs e) => this.Close ();
        private void MenuExit_Click (object s, RoutedEventArgs e) => App.Patient_Editor.RequestExit ();
        private void MenuTogglePause_Click (object s, RoutedEventArgs e) => TogglePause ();

        private void MenuFullscreen_Click (object sender, RoutedEventArgs e) {
            isFullscreen = !isFullscreen;
            ApplyFullScreen ();
        }

        private void OnTick_Tracing (object sender, EventArgs e) {
            if (isPaused)
                return;

            foreach (Controls.IABPTracing c in listTracings) {
                c.wfStrip.Scroll ();
                c.Draw ();
            }
        }

        public void OnPatientEvent (object sender, Patient.PatientEvent_Args e) {
            switch (e.EventType) {
                default: break;
                case Patient.PatientEvent_Args.EventTypes.Vitals_Change:
                    foreach (Controls.IABPTracing c in listTracings) {
                        c.wfStrip.ClearFuture ();
                        c.wfStrip.Add_Beat__Cardiac_Baseline (App.Patient);
                    }
                    break;

                case Patient.PatientEvent_Args.EventTypes.Cardiac_Baseline:
                    foreach (Controls.IABPTracing c in listTracings)
                        c.wfStrip.Add_Beat__Cardiac_Baseline (App.Patient);
                    break;

                case Patient.PatientEvent_Args.EventTypes.Cardiac_Atrial:
                    foreach (Controls.IABPTracing c in listTracings)
                        c.wfStrip.Add_Beat__Cardiac_Atrial (App.Patient);
                    break;

                case Patient.PatientEvent_Args.EventTypes.Cardiac_Ventricular:
                    foreach (Controls.IABPTracing c in listTracings)
                        c.wfStrip.Add_Beat__Cardiac_Ventricular (App.Patient);
                    break;
            }
        }
    }
}