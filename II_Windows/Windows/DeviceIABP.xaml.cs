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
        List<Controls.IABPNumeric> listNumerics = new List<Controls.IABPNumeric> ();

        Timer timerTracing = new Timer (),
              timerVitals = new Timer ();

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

            timerVitals.Interval = 5000;
            App.Timer_Main.Tick += timerVitals.Process;
            timerVitals.Tick += OnTick_Vitals;
            timerVitals.Start ();
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

            buttonModeAuto.Text = Strings.Lookup (l, "IABPMODE:Auto");
            buttonModeSemiAuto.Text = Strings.Lookup (l, "IABPMODE:SemiAuto");
            buttonModeManual.Text = Strings.Lookup (l, "IABPMODE:Manual");
            buttonZero.Text = Strings.Lookup (l, "IABPBUTTON:ZeroPressure");
            buttonStart.Text = Strings.Lookup (l, "IABPBUTTON:Start");
            buttonPause.Text = Strings.Lookup (l, "IABPBUTTON:Pause");

            // Instantiate and add Tracings to UI
            listTracings.Add (new Controls.IABPTracing (new Strip (6f, Leads.Values.ECG_II)));
            listTracings.Add (new Controls.IABPTracing (new Strip (6f, Leads.Values.ABP)));
            listTracings.Add (new Controls.IABPTracing (new Strip (6f, Leads.Values.IABP)));
            for (int i = 0; i < listTracings.Count; i++) {
                listTracings [i].SetValue (Grid.RowProperty, i);
                listTracings [i].SetValue (Grid.ColumnProperty, 0);
                displayGrid.Children.Add (listTracings [i]);
            }

            // Instantiate and add Numerics to UI
            listNumerics.Add (new Controls.IABPNumeric (Controls.IABPNumeric.ControlType.Values.ECG));
            listNumerics.Add (new Controls.IABPNumeric (Controls.IABPNumeric.ControlType.Values.ABP));
            for (int i = 0; i < listNumerics.Count; i++) {
                listNumerics [i].SetValue (Grid.RowProperty, i);
                listNumerics [i].SetValue (Grid.ColumnProperty, 1);
                displayGrid.Children.Add (listNumerics [i]);
            }
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
                    c.Unpause ();
        }

        private void ButtonStart_Click (object s, RoutedEventArgs e) {
            App.Patient.IABPRunning = true;
        }

        private void ButtonPause_Click (object s, RoutedEventArgs e) {
            App.Patient.IABPRunning = false;
        }

        private void ButtonZeroABP_Click (object s, RoutedEventArgs e) {
            App.Patient.TransducerZeroed_ABP = true;
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
                c.Scroll ();
                c.Draw ();
            }
        }

        private void OnTick_Vitals (object sender, EventArgs e) {
            if (isPaused)
                return;

            foreach (Controls.IABPNumeric v in listNumerics)
                v.UpdateVitals ();
        }

        public void OnPatientEvent (object sender, Patient.PatientEvent_Args e) {
            switch (e.EventType) {
                default: break;
                case Patient.PatientEvent_Args.EventTypes.Vitals_Change:
                    foreach (Controls.IABPTracing c in listTracings) {
                        c.ClearFuture ();
                        c.Add_Beat__Cardiac_Baseline (App.Patient);
                    }
                    break;

                case Patient.PatientEvent_Args.EventTypes.Cardiac_Baseline:
                    foreach (Controls.IABPTracing c in listTracings)
                        c.Add_Beat__Cardiac_Baseline (App.Patient);
                    break;

                case Patient.PatientEvent_Args.EventTypes.Cardiac_Atrial:
                    foreach (Controls.IABPTracing c in listTracings)
                        c.Add_Beat__Cardiac_Atrial (App.Patient);
                    break;

                case Patient.PatientEvent_Args.EventTypes.Cardiac_Ventricular:
                    foreach (Controls.IABPTracing c in listTracings)
                        c.Add_Beat__Cardiac_Ventricular (App.Patient);
                    break;
            }
        }
    }
}