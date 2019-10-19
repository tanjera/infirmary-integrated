using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using II;
using II.Rhythm;
using II.Waveform;

namespace II_Windows {

    /// <summary>
    /// Interaction logic for DeviceMonitor.xaml
    /// </summary>
    public partial class DeviceMonitor : Window {

        private int rowsTracings = 3,
            rowsNumerics = 3;

        private bool isFullscreen = false,
             isPaused = false;

        private List<Controls.MonitorTracing> listTracings = new List<Controls.MonitorTracing> ();
        private List<Controls.MonitorNumeric> listNumerics = new List<Controls.MonitorNumeric> ();

        private Timer timerTracing = new Timer (),
            timerVitals_Cardiac = new Timer (),
            timerVitals_Respiratory = new Timer ();

        // Define WPF UI commands for binding
        private ICommand icToggleFullscreen, icPauseDevice, icCloseDevice, icExitProgram;

        public ICommand IC_ToggleFullscreen { get { return icToggleFullscreen; } }
        public ICommand IC_PauseDevice { get { return icPauseDevice; } }
        public ICommand IC_CloseDevice { get { return icCloseDevice; } }
        public ICommand IC_ExitProgram { get { return icExitProgram; } }

        public DeviceMonitor () {
            InitializeComponent ();
            DataContext = this;

            InitTimers ();
            InitInterface ();

            OnLayoutChange ();
        }

        private void InitTimers () {
            timerTracing.Set (Draw.RefreshTime);
            App.Timer_Main.Tick += timerTracing.Process;
            timerTracing.Tick += OnTick_Tracing;
            timerTracing.Start ();

            timerVitals_Cardiac.Set (3000);
            timerVitals_Respiratory.Set (5000);

            App.Timer_Main.Tick += timerVitals_Cardiac.Process;
            App.Timer_Main.Tick += timerVitals_Respiratory.Process;

            timerVitals_Cardiac.Tick += OnTick_Vitals_Cardiac;
            timerVitals_Respiratory.Tick += OnTick_Vitals_Respiratory;

            timerVitals_Cardiac.Start ();
            timerVitals_Respiratory.Start ();
        }

        private void InitInterface () {

            // Initiate ICommands for KeyBindings
            icToggleFullscreen = new ActionCommand (() => ToggleFullscreen ());
            icPauseDevice = new ActionCommand (() => TogglePause ());
            icCloseDevice = new ActionCommand (() => this.Close ());
            icExitProgram = new ActionCommand (() => App.Patient_Editor.Exit ());

            // Populate UI strings per language selection
            wdwDeviceMonitor.Title = App.Language.Dictionary ["CM:WindowTitle"];
            menuDevice.Header = App.Language.Dictionary ["MENU:MenuDeviceOptions"];
            menuPauseDevice.Header = App.Language.Dictionary ["MENU:MenuPauseDevice"];
            menuAddNumeric.Header = App.Language.Dictionary ["MENU:MenuAddNumeric"];
            menuAddTracing.Header = App.Language.Dictionary ["MENU:MenuAddTracing"];
            menuToggleFullscreen.Header = App.Language.Dictionary ["MENU:MenuToggleFullscreen"];
            menuCloseDevice.Header = App.Language.Dictionary ["MENU:MenuCloseDevice"];
            menuExitProgram.Header = App.Language.Dictionary ["MENU:MenuExitProgram"];
        }

        public void Load_Process (string inc) {
            StringReader sRead = new StringReader (inc);
            List<string> numericTypes = new List<string> (),
                         tracingTypes = new List<string> ();

            try {
                string line;
                while ((line = sRead.ReadLine ()) != null) {
                    if (line.Contains (":")) {
                        string pName = line.Substring (0, line.IndexOf (':')),
                                pValue = line.Substring (line.IndexOf (':') + 1);
                        switch (pName) {
                            default: break;
                            case "rowsTracings": rowsTracings = int.Parse (pValue); break;
                            case "rowsNumerics": rowsNumerics = int.Parse (pValue); break;
                            case "isPaused": isPaused = bool.Parse (pValue); break;
                            case "isFullscreen": isFullscreen = bool.Parse (pValue); break;
                            case "numericTypes": numericTypes.AddRange (pValue.Split (',').Where ((o) => o != "")); break;
                            case "tracingTypes": tracingTypes.AddRange (pValue.Split (',').Where ((o) => o != "")); break;
                        }
                    }
                }
            } catch {
            } finally {
                sRead.Close ();
                OnLayoutChange (numericTypes, tracingTypes);
            }
        }

        public string Save () {
            StringBuilder sWrite = new StringBuilder ();

            sWrite.AppendLine (String.Format ("{0}:{1}", "rowsTracings", rowsTracings));
            sWrite.AppendLine (String.Format ("{0}:{1}", "rowsNumerics", rowsNumerics));
            sWrite.AppendLine (String.Format ("{0}:{1}", "isPaused", isPaused));
            sWrite.AppendLine (String.Format ("{0}:{1}", "isFullscreen", isFullscreen));

            List<string> numericTypes = new List<string> (),
                         tracingTypes = new List<string> ();

            listNumerics.ForEach (o => { numericTypes.Add (o.controlType.Value.ToString ()); });
            listTracings.ForEach (o => { tracingTypes.Add (o.Strip.Lead.Value.ToString ()); });
            sWrite.AppendLine (String.Format ("{0}:{1}", "numericTypes", string.Join (",", numericTypes)));
            sWrite.AppendLine (String.Format ("{0}:{1}", "tracingTypes", string.Join (",", tracingTypes)));

            return sWrite.ToString ();
        }

        private void ApplyFullScreen () {
            menuToggleFullscreen.IsChecked = isFullscreen;

            switch (isFullscreen) {
                default:
                case false:
                    wdwDeviceMonitor.WindowStyle = WindowStyle.SingleBorderWindow;
                    wdwDeviceMonitor.WindowState = WindowState.Normal;
                    wdwDeviceMonitor.ResizeMode = ResizeMode.CanResize;
                    break;

                case true:
                    wdwDeviceMonitor.WindowStyle = WindowStyle.None;
                    wdwDeviceMonitor.WindowState = WindowState.Maximized;
                    wdwDeviceMonitor.ResizeMode = ResizeMode.NoResize;
                    break;
            }
        }

        private void TogglePause () {
            isPaused = !isPaused;
            menuPauseDevice.IsChecked = isPaused;

            if (!isPaused)
                listTracings.ForEach (c => c.Strip.Unpause ());
        }

        public void ToggleFullscreen () {
            isFullscreen = !isFullscreen;
            ApplyFullScreen ();
        }

        public void AddTracing () {
            rowsTracings += 1;
            OnLayoutChange ();
        }

        public void RemoveTracing (Controls.MonitorTracing requestSender) {
            rowsTracings -= 1;
            listTracings.Remove (requestSender);
            OnLayoutChange ();
        }

        public void AddNumeric () {
            rowsNumerics += 1;
            OnLayoutChange ();
        }

        public void RemoveNumeric (Controls.MonitorNumeric requestSender) {
            rowsNumerics -= 1;
            listNumerics.Remove (requestSender);
            OnLayoutChange ();
        }

        private void MenuClose_Click (object s, RoutedEventArgs e) => this.Close ();

        private void MenuExit_Click (object s, RoutedEventArgs e) => App.Patient_Editor.Exit ();

        private void MenuAddNumeric_Click (object s, RoutedEventArgs e) => AddNumeric ();

        private void MenuAddTracing_Click (object s, RoutedEventArgs e) => AddTracing ();

        private void MenuTogglePause_Click (object s, RoutedEventArgs e) => TogglePause ();

        private void MenuFullscreen_Click (object sender, RoutedEventArgs e) => ToggleFullscreen ();

        private void OnTick_Tracing (object sender, EventArgs e) {
            if (isPaused)
                return;

            listTracings.ForEach (c => {
                c.Strip.Scroll ();
                c.Draw ();
            });
        }

        private void OnTick_Vitals_Cardiac (object sender, EventArgs e) {
            if (isPaused)
                return;

            listNumerics
                .Where (n
                    => n.controlType.Value != Controls.MonitorNumeric.ControlType.Values.ETCO2
                    && n.controlType.Value != Controls.MonitorNumeric.ControlType.Values.RR)
                .ToList ()
                .ForEach (n => n.UpdateVitals ());
        }

        private void OnTick_Vitals_Respiratory (object sender, EventArgs e) {
            if (isPaused)
                return;

            listNumerics
                .Where (n
                    => n.controlType.Value == Controls.MonitorNumeric.ControlType.Values.ETCO2
                    || n.controlType.Value == Controls.MonitorNumeric.ControlType.Values.RR)
                .ToList ()
                .ForEach (n => n.UpdateVitals ());
        }

        private void OnLayoutChange (List<string> numericTypes = null, List<string> tracingTypes = null) {

            // If numericTypes or tracingTypes are not null... then we are loading a file; clear lNumerics and lTracings!
            if (numericTypes != null)
                listNumerics.Clear ();
            if (tracingTypes != null)
                listTracings.Clear ();

            // Set default numeric types to populate
            if (numericTypes == null || numericTypes.Count == 0)
                numericTypes = new List<string> (new string [] { "ECG", "NIBP", "SPO2", "RR", "ETCO2", "ABP", "CVP", "T", "PA", "ICP", "IAP" });
            else if (numericTypes.Count < rowsNumerics) {
                List<string> buffer = new List<string> (new string [] { "ECG", "NIBP", "SPO2", "RR", "ETCO2", "ABP", "CVP", "T", "PA", "ICP", "IAP" });
                buffer.RemoveRange (0, numericTypes.Count);
                numericTypes.AddRange (buffer);
            }

            // Cap available amount of numerics
            rowsNumerics = Utility.Clamp (rowsNumerics, 1, numericTypes.Count);
            for (int i = listNumerics.Count; i < rowsNumerics && i < numericTypes.Count; i++) {
                Controls.MonitorNumeric newNum;
                newNum = new Controls.MonitorNumeric ((Controls.MonitorNumeric.ControlType.Values)Enum.Parse (typeof (Controls.MonitorNumeric.ControlType.Values), numericTypes [i]));
                listNumerics.Add (newNum);
            }

            // Set default tracing types to populate
            if (tracingTypes == null || tracingTypes.Count == 0)
                tracingTypes = new List<string> (new string [] { "ECG_II", "ECG_III", "SPO2", "RR", "ETCO2", "ABP", "CVP", "PA", "ICP" });
            else if (tracingTypes.Count < rowsTracings) {
                List<string> buffer = new List<string> (new string [] { "ECG_II", "ECG_III", "SPO2", "RR", "ETCO2", "ABP", "CVP", "PA", "ICP" });
                buffer.RemoveRange (0, tracingTypes.Count);
                tracingTypes.AddRange (buffer);
            }

            // Cap available amount of tracings
            rowsTracings = Utility.Clamp (rowsTracings, 1, tracingTypes.Count);
            for (int i = listTracings.Count; i < rowsTracings && i < tracingTypes.Count; i++) {
                Strip newStrip = new Strip (6f, (Lead.Values)Enum.Parse (typeof (Lead.Values), tracingTypes [i]));
                Controls.MonitorTracing newTracing = new Controls.MonitorTracing (newStrip);
                listTracings.Add (newTracing);
            }

            // Reset the UI container and repopulate with the UI elements
            gridNumerics.Children.Clear ();
            gridNumerics.RowDefinitions.Clear ();
            for (int i = 0; i < rowsNumerics && i < listNumerics.Count; i++) {
                gridNumerics.RowDefinitions.Add (new RowDefinition ());
                listNumerics [i].SetValue (Grid.RowProperty, i);
                gridNumerics.Children.Add (listNumerics [i]);
            }

            gridTracings.Children.Clear ();
            gridTracings.RowDefinitions.Clear ();
            for (int i = 0; i < rowsTracings && i < listTracings.Count; i++) {
                gridTracings.RowDefinitions.Add (new RowDefinition ());
                listTracings [i].SetValue (Grid.RowProperty, i);
                gridTracings.Children.Add (listTracings [i]);
            }
        }

        public void OnPatientEvent (object sender, Patient.PatientEventArgs e) {
            switch (e.EventType) {
                default: break;
                case Patient.PatientEventTypes.Vitals_Change:
                    listTracings.ForEach (c => {
                        c.Strip.ClearFuture (App.Patient);
                        c.Strip.Add_Beat__Cardiac_Baseline (App.Patient);
                    });

                    listNumerics.ForEach (n => n.UpdateVitals ());
                    break;

                case Patient.PatientEventTypes.Cardiac_Defibrillation:
                    listTracings.ForEach (c => c.Strip.Add_Beat__Cardiac_Defibrillation (App.Patient));
                    break;

                case Patient.PatientEventTypes.Cardiac_PacerSpike:
                    listTracings.ForEach (c => c.Strip.Add_Beat__Cardiac_Pacemaker (App.Patient));
                    break;

                case Patient.PatientEventTypes.Cardiac_Baseline:
                    listTracings.ForEach (c => c.Strip.Add_Beat__Cardiac_Baseline (App.Patient));
                    break;

                case Patient.PatientEventTypes.Cardiac_Atrial_Electric:
                    listTracings.ForEach (c => c.Strip.Add_Beat__Cardiac_Atrial_Electrical (App.Patient));
                    break;

                case Patient.PatientEventTypes.Cardiac_Ventricular_Electric:
                    listTracings.ForEach (c => c.Strip.Add_Beat__Cardiac_Ventricular_Electrical (App.Patient));
                    break;

                case Patient.PatientEventTypes.Cardiac_Atrial_Mechanical:
                    listTracings.ForEach (c => c.Strip.Add_Beat__Cardiac_Atrial_Mechanical (App.Patient));
                    break;

                case Patient.PatientEventTypes.Cardiac_Ventricular_Mechanical:
                    listTracings.ForEach (c => c.Strip.Add_Beat__Cardiac_Ventricular_Mechanical (App.Patient));
                    break;

                case Patient.PatientEventTypes.Respiratory_Baseline:
                    listTracings.ForEach (c => c.Strip.Add_Beat__Respiratory_Baseline (App.Patient));
                    break;

                case Patient.PatientEventTypes.Respiratory_Inspiration:
                    listTracings.ForEach (c => c.Strip.Add_Beat__Respiratory_Inspiration (App.Patient));
                    break;

                case Patient.PatientEventTypes.Respiratory_Expiration:
                    listTracings.ForEach (c => c.Strip.Add_Beat__Respiratory_Expiration (App.Patient));
                    break;
            }
        }

        private void OnFormResize (object sender, RoutedEventArgs e) => OnLayoutChange ();
    }
}