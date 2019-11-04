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
        private int rowsTracings = 3;
        private int rowsNumerics = 3;

        private int autoScale_iter = Strip.DefaultAutoScale_Iterations;

        private bool isFullscreen = false;
        private bool isPaused = false;

        private List<Controls.MonitorTracing> listTracings = new List<Controls.MonitorTracing> ();
        private List<Controls.MonitorNumeric> listNumerics = new List<Controls.MonitorNumeric> ();

        private Timer timerTracing = new Timer ();
        private Timer timerVitals_Cardiac = new Timer ();
        private Timer timerVitals_Respiratory = new Timer ();

        // Define WPF UI commands for binding
        private ICommand icToggleFullscreen, icPauseDevice, icCloseDevice, icExitProgram,
            icSaveScreen, icPrintScreen;

        public ICommand IC_ToggleFullscreen { get { return icToggleFullscreen; } }
        public ICommand IC_PauseDevice { get { return icPauseDevice; } }
        public ICommand IC_CloseDevice { get { return icCloseDevice; } }
        public ICommand IC_ExitProgram { get { return icExitProgram; } }
        public ICommand IC_SaveScreen { get { return icSaveScreen; } }
        public ICommand IC_PrintScreen { get { return icPrintScreen; } }

        public DeviceMonitor () {
            InitializeComponent ();
            DataContext = this;

            InitTimers ();
            InitInterface ();

            OnLayoutChange ();
        }

        ~DeviceMonitor () => Dispose ();

        public void Dispose () {
            /* Clean subscriptions from the Main Timer */
            App.Timer_Main.Tick -= timerTracing.Process;
            App.Timer_Main.Tick -= timerVitals_Cardiac.Process;
            App.Timer_Main.Tick -= timerVitals_Respiratory.Process;

            /* Dispose of local Timers */
            timerTracing.Dispose ();
            timerVitals_Cardiac.Dispose ();
            timerVitals_Respiratory.Dispose ();

            /* Unsubscribe from the main Patient event listing */
            App.Patient.PatientEvent -= OnPatientEvent;
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
            icSaveScreen = new ActionCommand (() => SaveScreen ());
            icPrintScreen = new ActionCommand (() => PrintScreen ());

            // Populate UI strings per language selection
            wdwDeviceMonitor.Title = App.Language.Localize ("CM:WindowTitle");
            menuDevice.Header = App.Language.Localize ("MENU:MenuDeviceOptions");
            menuPauseDevice.Header = App.Language.Localize ("MENU:MenuPauseDevice");
            menuAddNumeric.Header = App.Language.Localize ("MENU:MenuAddNumeric");
            menuAddTracing.Header = App.Language.Localize ("MENU:MenuAddTracing");
            menuToggleFullscreen.Header = App.Language.Localize ("MENU:MenuToggleFullscreen");
            menuSaveScreen.Header = App.Language.Localize ("MENU:MenuSaveScreen");
            menuPrintScreen.Header = App.Language.Localize ("MENU:MenuPrintScreen");
            menuCloseDevice.Header = App.Language.Localize ("MENU:MenuCloseDevice");
            menuExitProgram.Header = App.Language.Localize ("MENU:MenuExitProgram");
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

        private void SaveScreen ()
            => Screenshot.SavePdf (
                Screenshot.SavePng (Screenshot.GetBitmap (layoutGrid, 1), II.File.GetTempFilePath ("png")),
                App.Language.Localize ("CM:WindowTitle"), null);

        private void PrintScreen ()
            => II.Screenshot.PrintPdf (II.Screenshot.AssemblePdf (
                Screenshot.SavePng (Screenshot.GetBitmap (layoutGrid, 1), II.File.GetTempFilePath ("png")),
                    App.Language.Localize ("CM:WindowTitle"), null));

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
            rowsNumerics = II.Math.Clamp (rowsNumerics, 1, numericTypes.Count);
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
            rowsTracings = II.Math.Clamp (rowsTracings, 1, tracingTypes.Count);
            for (int i = listTracings.Count; i < rowsTracings && i < tracingTypes.Count; i++) {
                Strip newStrip = new Strip ((Lead.Values)Enum.Parse (typeof (Lead.Values), tracingTypes [i]), 6f);
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

                case Patient.PatientEventTypes.Defibrillation:
                    listTracings.ForEach (c => c.Strip.Add_Beat__Cardiac_Defibrillation (App.Patient));
                    break;

                case Patient.PatientEventTypes.Pacermaker_Spike:
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

                    /* Iterations and trigger for auto-scaling pressure waveform strips */
                    autoScale_iter -= 1;
                    if (autoScale_iter <= 0) {
                        for (int i = 0; i < listTracings.Count; i++) {
                            listTracings [i].Strip.SetAutoScale (App.Patient);
                            listTracings [i].UpdateScale ();
                        }

                        autoScale_iter = Strip.DefaultAutoScale_Iterations;
                    }
                    break;

                case Patient.PatientEventTypes.Respiratory_Baseline:
                    listTracings.ForEach (c => c.Strip.Add_Breath__Respiratory_Baseline (App.Patient));
                    break;

                case Patient.PatientEventTypes.Respiratory_Inspiration:
                    listTracings.ForEach (c => c.Strip.Add_Breath__Respiratory_Inspiration (App.Patient));
                    break;

                case Patient.PatientEventTypes.Respiratory_Expiration:
                    listTracings.ForEach (c => c.Strip.Add_Breath__Respiratory_Expiration (App.Patient));
                    break;
            }
        }

        private void OnFormResize (object sender, RoutedEventArgs e) => OnLayoutChange ();
    }
}