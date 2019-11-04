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
    /// Interaction logic for DeviceDefib.xaml
    /// </summary>
    public partial class DeviceDefib : Window {

        public enum Modes {
            DEFIB,
            SYNC,
            PACER
        };

        // Device settings
        public Modes Mode = Modes.DEFIB;

        public bool Charging = false,
                    Charged = false,
                    Analyzed = false;

        public int Energy = 200,
                    PacerEnergy = 0,
                    PacerRate = 80;

        private int rowsTracings = 1;
        private int colsNumerics = 4;

        private int autoScale_iter = Strip.DefaultAutoScale_Iterations;

        private bool isFullscreen = false;
        private bool isPaused = false;

        private List<Controls.DefibTracing> listTracings = new List<Controls.DefibTracing> ();
        private List<Controls.DefibNumeric> listNumerics = new List<Controls.DefibNumeric> ();

        private Timer
            timerTracing = new Timer (),
            timerVitals_Cardiac = new Timer (),
            timerVitals_Respiratory = new Timer (),
            timerAncillary_Delay = new Timer ();

        // Define WPF UI commands for binding
        private ICommand icToggleFullscreen, icPauseDevice, icCloseDevice, icExitProgram,
            icSaveScreen, icPrintScreen;

        public ICommand IC_ToggleFullscreen { get { return icToggleFullscreen; } }
        public ICommand IC_PauseDevice { get { return icPauseDevice; } }
        public ICommand IC_CloseDevice { get { return icCloseDevice; } }
        public ICommand IC_ExitProgram { get { return icExitProgram; } }
        public ICommand IC_SaveScreen { get { return icSaveScreen; } }
        public ICommand IC_PrintScreen { get { return icPrintScreen; } }

        public DeviceDefib () {
            InitializeComponent ();
            DataContext = this;

            InitTimers ();
            InitInterface ();

            OnLayoutChange ();
        }

        ~DeviceDefib () => Dispose ();

        public void Dispose () {
            /* Clean subscriptions from the Main Timer */
            App.Timer_Main.Tick -= timerTracing.Process;
            App.Timer_Main.Tick -= timerVitals_Cardiac.Process;
            App.Timer_Main.Tick -= timerVitals_Respiratory.Process;
            App.Timer_Main.Tick -= timerAncillary_Delay.Process;

            /* Dispose of local Timers */
            timerTracing.Dispose ();
            timerVitals_Cardiac.Dispose ();
            timerVitals_Respiratory.Dispose ();
            timerAncillary_Delay.Dispose ();

            /* Unsubscribe from the main Patient event listing */
            App.Patient.PatientEvent -= OnPatientEvent;
        }

        private void InitTimers () {
            App.Timer_Main.Tick += timerTracing.Process;
            App.Timer_Main.Tick += timerVitals_Cardiac.Process;
            App.Timer_Main.Tick += timerVitals_Respiratory.Process;
            App.Timer_Main.Tick += timerAncillary_Delay.Process;

            timerTracing.Tick += OnTick_Tracing;
            timerVitals_Cardiac.Tick += OnTick_Vitals_Cardiac;
            timerVitals_Respiratory.Tick += OnTick_Vitals_Respiratory;

            timerTracing.Set (Draw.RefreshTime);
            timerVitals_Cardiac.Set (II.Math.Clamp ((int)(App.Patient.GetHR_Seconds * 1000 / 2), 2000, 6000));
            timerVitals_Respiratory.Set (II.Math.Clamp ((int)(App.Patient.GetRR_Seconds * 1000 / 2), 2000, 8000));

            timerTracing.Start ();
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
            wdwDeviceDefib.Title = App.Language.Localize ("DEFIB:WindowTitle");
            menuDevice.Header = App.Language.Localize ("MENU:MenuDeviceOptions");
            menuPauseDevice.Header = App.Language.Localize ("MENU:MenuPauseDevice");
            menuAddNumeric.Header = App.Language.Localize ("MENU:MenuAddNumeric");
            menuAddTracing.Header = App.Language.Localize ("MENU:MenuAddTracing");
            menuToggleFullscreen.Header = App.Language.Localize ("MENU:MenuToggleFullscreen");
            menuSaveScreen.Header = App.Language.Localize ("MENU:MenuSaveScreen");
            menuPrintScreen.Header = App.Language.Localize ("MENU:MenuPrintScreen");
            menuCloseDevice.Header = App.Language.Localize ("MENU:MenuCloseDevice");
            menuExitProgram.Header = App.Language.Localize ("MENU:MenuExitProgram");

            btntxtDefib.Text = App.Language.Localize ("DEFIB:Defibrillator");
            txtEnergyAmount.Text = App.Language.Localize ("DEFIB:EnergyAmount");
            btntxtEnergyDecrease.Text = App.Language.Localize ("DEFIB:Decrease");
            btntxtEnergyIncrease.Text = App.Language.Localize ("DEFIB:Increase");
            btntxtCharge.Text = App.Language.Localize ("DEFIB:Charge");
            btntxtShock.Text = App.Language.Localize ("DEFIB:Shock");
            btntxtAnalyze.Text = App.Language.Localize ("DEFIB:Analyze");
            btntxtSync.Text = App.Language.Localize ("DEFIB:Sync");

            btntxtPacer.Text = App.Language.Localize ("DEFIB:Pacer");
            txtPaceRate.Text = App.Language.Localize ("DEFIB:Rate");
            btntxtPaceRateDecrease.Text = App.Language.Localize ("DEFIB:Decrease");
            btntxtPaceRateIncrease.Text = App.Language.Localize ("DEFIB:Increase");
            txtPaceEnergy.Text = App.Language.Localize ("DEFIB:EnergyAmount");
            btntxtPaceEnergyDecrease.Text = App.Language.Localize ("DEFIB:Decrease");
            btntxtPaceEnergyIncrease.Text = App.Language.Localize ("DEFIB:Increase");
            btntxtPacePause.Text = App.Language.Localize ("DEFIB:Pause");
        }

        private void UpdateInterface () {
            listNumerics.Find (o => o.controlType.Value == Controls.DefibNumeric.ControlType.Values.DEFIB).UpdateVitals (this);
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
                            case "colsNumerics": colsNumerics = int.Parse (pValue); break;
                            case "isPaused": isPaused = bool.Parse (pValue); break;
                            case "isFullscreen": isFullscreen = bool.Parse (pValue); break;
                            case "numericTypes": numericTypes.AddRange (pValue.Split (',').Where ((o) => o != "")); break;
                            case "tracingTypes": tracingTypes.AddRange (pValue.Split (',').Where ((o) => o != "")); break;
                            case "Mode": Mode = (Modes)Enum.Parse (typeof (Modes), pValue); break;
                            case "Charged": Charged = bool.Parse (pValue); break;
                            case "Analyzed": Analyzed = bool.Parse (pValue); break;
                            case "Energy": Energy = int.Parse (pValue); break;
                            case "PacerEnergy": PacerEnergy = int.Parse (pValue); break;
                            case "PacerRate": PacerRate = int.Parse (pValue); break;
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
            sWrite.AppendLine (String.Format ("{0}:{1}", "colsNumerics", colsNumerics));
            sWrite.AppendLine (String.Format ("{0}:{1}", "isPaused", isPaused));
            sWrite.AppendLine (String.Format ("{0}:{1}", "isFullscreen", isFullscreen));

            List<string> numericTypes = new List<string> (),
                         tracingTypes = new List<string> ();

            listNumerics.ForEach (o => { numericTypes.Add (o.controlType.Value.ToString ()); });
            listTracings.ForEach (o => { tracingTypes.Add (o.Strip.Lead.Value.ToString ()); });
            sWrite.AppendLine (String.Format ("{0}:{1}", "numericTypes", string.Join (",", numericTypes)));
            sWrite.AppendLine (String.Format ("{0}:{1}", "tracingTypes", string.Join (",", tracingTypes)));

            sWrite.AppendLine (String.Format ("{0}:{1}", "Mode", Mode));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Charged", Charged));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Analyzed", Analyzed));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Energy", Energy));
            sWrite.AppendLine (String.Format ("{0}:{1}", "PacerEnergy", PacerEnergy));
            sWrite.AppendLine (String.Format ("{0}:{1}", "PacerRate", PacerRate));

            return sWrite.ToString ();
        }

        private void SaveScreen ()
            => Screenshot.SavePdf (
                Screenshot.SavePng (Screenshot.GetBitmap (mainGrid, 1), II.File.GetTempFilePath ("png")),
                App.Language.Localize ("DEFIB:WindowTitle"), null);

        private void PrintScreen ()
            => II.Screenshot.PrintPdf (II.Screenshot.AssemblePdf (
                Screenshot.SavePng (Screenshot.GetBitmap (mainGrid, 1), II.File.GetTempFilePath ("png")),
                    App.Language.Localize ("DEFIB:WindowTitle"), null));

        private void ApplyFullScreen () {
            menuToggleFullscreen.IsChecked = isFullscreen;

            switch (isFullscreen) {
                default:
                case false:
                    wdwDeviceDefib.WindowStyle = WindowStyle.SingleBorderWindow;
                    wdwDeviceDefib.WindowState = WindowState.Normal;
                    wdwDeviceDefib.ResizeMode = ResizeMode.CanResize;
                    break;

                case true:
                    wdwDeviceDefib.WindowStyle = WindowStyle.None;
                    wdwDeviceDefib.WindowState = WindowState.Maximized;
                    wdwDeviceDefib.ResizeMode = ResizeMode.NoResize;
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

        public void AddTracing () {
            rowsTracings += 1;
            OnLayoutChange ();
        }

        public void AddNumeric () {
            colsNumerics += 1;
            OnLayoutChange ();
        }

        public void RemoveTracing (Controls.DefibTracing requestSender) {
            rowsTracings -= 1;
            listTracings.Remove (requestSender);
            OnLayoutChange ();
        }

        public void RemoveNumeric (Controls.DefibNumeric requestSender) {
            colsNumerics -= 1;
            listNumerics.Remove (requestSender);
            OnLayoutChange ();
        }

        private void UpdatePacemaker () {
            App.Patient.Pacemaker (Mode == Modes.PACER, PacerRate, PacerEnergy);
        }

        private void ButtonDefib_Click (object s, RoutedEventArgs e) {
            Mode = Modes.DEFIB;
            UpdatePacemaker ();
            UpdateInterface ();
        }

        private void ButtonEnergyDecrease_Click (object s, RoutedEventArgs e) {
            if (Mode != Modes.DEFIB && Mode != Modes.SYNC)
                return;

            Energy = II.Math.Clamp (Energy - 20, 0, 200);
            UpdateInterface ();
        }

        private void ButtonEnergyIncrease_Click (object s, RoutedEventArgs e) {
            if (Mode != Modes.DEFIB && Mode != Modes.SYNC)
                return;

            Energy = II.Math.Clamp (Energy + 20, 0, 200);
            UpdateInterface ();
        }

        private void ButtonCharge_Click (object s, RoutedEventArgs e) {

            // Only charge if in Defib or Sync mode...
            if (Mode != Modes.DEFIB && Mode != Modes.SYNC)
                return;

            Analyzed = false;

            if (timerAncillary_Delay.IsLocked) {
                Charging = false;
                Charged = true;
            } else {
                Charging = true;
                Charged = false;
                timerAncillary_Delay.Lock ();
                timerAncillary_Delay.Tick += OnTick_ChargingComplete;
                timerAncillary_Delay.Set (3000);
                timerAncillary_Delay.Start ();
            }

            UpdateInterface ();
        }

        private void ButtonShock_Click (object s, RoutedEventArgs e) {
            if (!Charged)
                return;

            Charged = false;

            switch (Mode) {
                default: break;
                case Modes.DEFIB: App.Patient.Defibrillate (); break;
                case Modes.SYNC: App.Patient.Cardiovert (); break;
            }

            UpdateInterface ();
        }

        private void ButtonAnalyze_Click (object s, RoutedEventArgs e) {
            Analyzed = true;
            Mode = Modes.DEFIB;
            UpdatePacemaker ();
            UpdateInterface ();
        }

        private void ButtonSync_Click (object s, RoutedEventArgs e) {
            Analyzed = false;
            Mode = (Mode != Modes.SYNC ? Modes.SYNC : Modes.DEFIB);
            UpdatePacemaker ();
            UpdateInterface ();
        }

        private void ButtonPacer_Click (object s, RoutedEventArgs e) {
            Analyzed = false;
            Mode = (Mode != Modes.PACER ? Modes.PACER : Modes.DEFIB);
            UpdatePacemaker ();
            UpdateInterface ();
        }

        private void ButtonPaceRateDecrease_Click (object s, RoutedEventArgs e) {
            if (Mode != Modes.PACER)
                return;

            PacerRate = II.Math.Clamp (PacerRate - 5, 0, 200);
            UpdatePacemaker ();
            UpdateInterface ();
        }

        private void ButtonPaceRateIncrease_Click (object s, RoutedEventArgs e) {
            if (Mode != Modes.PACER)
                return;

            PacerRate = II.Math.Clamp (PacerRate + 5, 0, 200);
            UpdatePacemaker ();
            UpdateInterface ();
        }

        private void ButtonPaceEnergyDecrease_Click (object s, RoutedEventArgs e) {
            if (Mode != Modes.PACER)
                return;

            PacerEnergy = II.Math.Clamp (PacerEnergy - 5, 0, 200);
            UpdatePacemaker ();
            UpdateInterface ();
        }

        private void ButtonPaceEnergyIncrease_Click (object s, RoutedEventArgs e) {
            if (Mode != Modes.PACER)
                return;

            PacerEnergy = II.Math.Clamp (PacerEnergy + 5, 0, 200);
            UpdatePacemaker ();
            UpdateInterface ();
        }

        private void ButtonPacePause_Click (object s, RoutedEventArgs e) => App.Patient.PacemakerPause ();

        private void MenuClose_Click (object s, RoutedEventArgs e) => this.Close ();

        private void MenuExit_Click (object s, RoutedEventArgs e) => App.Patient_Editor.Exit ();

        private void MenuAddNumeric_Click (object s, RoutedEventArgs e) => AddNumeric ();

        private void MenuAddTracing_Click (object s, RoutedEventArgs e) => AddTracing ();

        private void MenuTogglePause_Click (object s, RoutedEventArgs e) => TogglePause ();

        private void MenuFullscreen_Click (object sender, RoutedEventArgs e) {
            isFullscreen = !isFullscreen;
            ApplyFullScreen ();
        }

        private void MenuSaveScreen_Click (object sender, RoutedEventArgs e)
            => SaveScreen ();

        private void MenuPrintScreen_Click (object sender, RoutedEventArgs e)
            => PrintScreen ();

        private void OnTick_ChargingComplete (object sender, EventArgs e) {
            timerAncillary_Delay.Stop ();
            timerAncillary_Delay.Unlock ();
            timerAncillary_Delay.Tick -= OnTick_ChargingComplete;

            Charging = false;
            Charged = true;

            UpdateInterface ();
        }

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
                    => n.controlType.Value != Controls.DefibNumeric.ControlType.Values.ETCO2
                    && n.controlType.Value != Controls.DefibNumeric.ControlType.Values.RR)
                .ToList ()
                .ForEach (n => n.UpdateVitals (this));
        }

        private void OnTick_Vitals_Respiratory (object sender, EventArgs e) {
            if (isPaused)
                return;

            listNumerics
                .Where (n
                    => n.controlType.Value == Controls.DefibNumeric.ControlType.Values.ETCO2
                    || n.controlType.Value == Controls.DefibNumeric.ControlType.Values.RR)
                .ToList ()
                .ForEach (n => n.UpdateVitals (this));
        }

        private void OnLayoutChange (List<string> numericTypes = null, List<string> tracingTypes = null) {

            // If numericTypes or tracingTypes are not null... then we are loading a file; clear lNumerics and lTracings!
            if (numericTypes != null)
                listNumerics.Clear ();
            if (tracingTypes != null)
                listTracings.Clear ();

            // Set default numeric types to populate
            if (numericTypes == null || numericTypes.Count == 0)
                numericTypes = new List<string> (new string [] { "DEFIB", "ECG", "NIBP", "SPO2", "ETCO2", "ABP" });
            else if (numericTypes.Count < colsNumerics) {
                List<string> buffer = new List<string> (new string [] { "DEFIB", "ECG", "NIBP", "SPO2", "ETCO2", "ABP" });
                buffer.RemoveRange (0, numericTypes.Count);
                numericTypes.AddRange (buffer);
            }

            for (int i = listNumerics.Count; i < colsNumerics && i < numericTypes.Count; i++) {
                Controls.DefibNumeric newNum;
                newNum = new Controls.DefibNumeric ((Controls.DefibNumeric.ControlType.Values)Enum.Parse (typeof (Controls.DefibNumeric.ControlType.Values), numericTypes [i]));
                listNumerics.Add (newNum);
            }

            // Set default tracing types to populate
            if (tracingTypes == null || tracingTypes.Count == 0)
                tracingTypes = new List<string> (new string [] { "ECG_II", "SPO2", "ETCO2", "ABP" });
            else if (tracingTypes.Count < rowsTracings) {
                List<string> buffer = new List<string> (new string [] { "ECG_II", "SPO2", "ETCO2", "ABP" });
                buffer.RemoveRange (0, tracingTypes.Count);
                tracingTypes.AddRange (buffer);
            }

            for (int i = listTracings.Count; i < rowsTracings && i < tracingTypes.Count; i++) {
                Strip newStrip = new Strip ((Lead.Values)Enum.Parse (typeof (Lead.Values), tracingTypes [i]), 6f);
                Controls.DefibTracing newTracing = new Controls.DefibTracing (newStrip);
                listTracings.Add (newTracing);
            }

            // Reset the UI container and repopulate with the UI elements
            gridNumerics.Children.Clear ();
            gridNumerics.ColumnDefinitions.Clear ();
            for (int i = 0; i < colsNumerics && i < listNumerics.Count; i++) {
                gridNumerics.ColumnDefinitions.Add (new ColumnDefinition ());
                listNumerics [i].SetValue (Grid.ColumnProperty, i);
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
                    listNumerics.ForEach ((n) => n.UpdateVitals (this));
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