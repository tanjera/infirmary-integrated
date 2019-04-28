using II;
using II.Rhythm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace II_Windows {

    /// <summary>
    /// Interaction logic for DeviceIABP.xaml
    /// </summary>
    public partial class DeviceIABP : Window {

        public enum Settings {
            None,
            Trigger,
            Frequency,
            AugmentationPressure,
            AugmentationAlarm
        }

        public class Triggering {
            public Values Value;

            public enum Values { ECG, Pressure }

            public Triggering (Values v) {
                Value = v;
            }

            public Triggering () {
                Value = Values.ECG;
            }

            public string LookupString () => LookupString (Value);

            public static string LookupString (Values v) {
                return String.Format ("IABPTRIGGER:{0}", Enum.GetValues (typeof (Values)).GetValue ((int)v).ToString ());
            }
        }

        public class Modes {
            public Values Value;

            public enum Values { Auto, SemiAuto }

            public Modes (Values v) {
                Value = v;
            }

            public Modes () {
                Value = Values.Auto;
            }

            public string LookupString () => LookupString (Value);

            public static string LookupString (Values v) {
                return String.Format ("IABPMODE:{0}", Enum.GetValues (typeof (Values)).GetValue ((int)v).ToString ());
            }
        }

        // Device settings
        public int Frequency = 1,
                   Frequency_Iter = 0,              // Buffer value to determine if current beat triggers IABP
                   Augmentation = 100,              // Expressed as % (e.g. 10%, 100%)
                   AugmentationAlarm = 100;         // Expressed as mmHg

        public Triggering Trigger = new Triggering ();
        public Modes Mode = new Modes ();
        public bool Running = false, Primed = false;

        public Settings SelectedSetting = Settings.None;

        private bool isFullscreen = false,
             isPaused = false;

        private List<Controls.IABPTracing> listTracings = new List<Controls.IABPTracing> ();
        private List<Controls.IABPNumeric> listNumerics = new List<Controls.IABPNumeric> ();

        private Timer timerTracing = new Timer (),
              timerVitals = new Timer ();

        // Define WPF UI commands for binding
        private ICommand icToggleFullscreen, icPauseDevice, icCloseDevice, icExitProgram;

        public ICommand IC_ToggleFullscreen { get { return icToggleFullscreen; } }
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
            icToggleFullscreen = new ActionCommand (() => ToggleFullscreen ());
            icPauseDevice = new ActionCommand (() => TogglePause ());
            icCloseDevice = new ActionCommand (() => this.Close ());
            icExitProgram = new ActionCommand (() => App.Patient_Editor.RequestExit ());

            // Populate UI strings per language selection
            var Dictionary = App.Language.Dictionary;
            wdwDeviceIABP.Title = Dictionary ["IABP:WindowTitle"];
            menuDevice.Header = Dictionary ["MENU:MenuDeviceOptions"];
            menuPauseDevice.Header = Dictionary ["MENU:MenuPauseDevice"];
            menuToggleFullscreen.Header = Dictionary ["MENU:MenuToggleFullscreen"];
            menuCloseDevice.Header = Dictionary ["MENU:MenuCloseDevice"];
            menuExitProgram.Header = Dictionary ["MENU:MenuExitProgram"];

            buttonModeAuto.Text = Dictionary ["IABPMODE:Auto"];
            buttonModeSemiAuto.Text = Dictionary ["IABPMODE:SemiAuto"];
            buttonZero.Text = Utility.WrapString (Dictionary ["IABPBUTTON:ZeroPressure"]);
            buttonStart.Text = Dictionary ["IABPBUTTON:Start"];
            buttonPause.Text = Dictionary ["IABPBUTTON:Pause"];
            btntxtTrigger.Text = Utility.WrapString (Dictionary ["IABPBUTTON:Trigger"]);
            btntxtFrequency.Text = Utility.WrapString (Dictionary ["IABPBUTTON:Frequency"]);
            buttonPrimeBalloon.Text = Utility.WrapString (Dictionary ["IABPBUTTON:PrimeBalloon"]);
            btntxtAugmentationPressure.Text = Utility.WrapString (Dictionary ["IABP:AugmentationPressure"]);
            btntxtAugmentationAlarm.Text = Utility.WrapString (Dictionary ["IABP:AugmentationAlarm"]);
            btntxtIncrease.Text = Utility.WrapString (Dictionary ["IABPBUTTON:Increase"]);
            btntxtDecrease.Text = Utility.WrapString (Dictionary ["IABPBUTTON:Decrease"]);

            // Random helium tank remaining amount... it's for show!
            lblHelium.Text = String.Format ("{0}: {1:0}%",
                Utility.WrapString (Dictionary ["IABP:Helium"]),
                Utility.RandomDouble (20, 80));

            // Instantiate and add Tracings to UI
            listTracings.Add (new Controls.IABPTracing (new Strip (6f, Leads.Values.ECG_II)));
            listTracings.Add (new Controls.IABPTracing (new Strip (6f, Leads.Values.ABP)));
            listTracings.Add (new Controls.IABPTracing (new Strip (6f, Leads.Values.IABP)));
            for (int i = 0; i < listTracings.Count; i++) {
                listTracings [i].SetValue (Grid.RowProperty, i);
                listTracings [i].SetValue (Grid.ColumnProperty, 1);
                displayGrid.Children.Add (listTracings [i]);
            }

            // Instantiate and add Numerics to UI
            listNumerics.Add (new Controls.IABPNumeric (Controls.IABPNumeric.ControlType.Values.ECG));
            listNumerics.Add (new Controls.IABPNumeric (Controls.IABPNumeric.ControlType.Values.ABP));
            listNumerics.Add (new Controls.IABPNumeric (Controls.IABPNumeric.ControlType.Values.IABP_AP));
            for (int i = 0; i < listNumerics.Count; i++) {
                listNumerics [i].SetValue (Grid.RowProperty, i);
                listNumerics [i].SetValue (Grid.ColumnProperty, 2);
                displayGrid.Children.Add (listNumerics [i]);
            }
        }

        private void UpdateInterface () {
            var Dictionary = App.Language.Dictionary;

            lblTriggerSource.Text = Dictionary [Trigger.LookupString ()];
            lblOperationMode.Text = Dictionary [Mode.LookupString ()];
            lblFrequency.Text = String.Format ("1 : {0}", Frequency);
            lblMachineStatus.Text = Dictionary [Running ? "IABP:Running" : "IABP:Paused"];
            lblTubingStatus.Text = Dictionary [Primed ? "IABP:Primed" : "IABP:NotPrimed"];
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

                            case "Frequency": Frequency = int.Parse (pValue); break;
                            case "Augmentation": Augmentation = int.Parse (pValue); break;
                            case "AugmentationAlarm": AugmentationAlarm = int.Parse (pValue); break;
                            case "Trigger": Trigger.Value = (Triggering.Values)Enum.Parse (typeof (Triggering.Values), pValue); break;
                            case "Mode": Mode.Value = (Modes.Values)Enum.Parse (typeof (Modes.Values), pValue); break;
                            case "Running": Running = bool.Parse (pValue); break;
                            case "Primed": Primed = bool.Parse (pValue); break;
                        }
                    }
                }
            } catch (Exception e) {
                App.Server.Post_Exception (e);
                throw e;
            } finally {
                sRead.Close ();
            }
        }

        public string Save () {
            StringBuilder sWrite = new StringBuilder ();

            sWrite.AppendLine (String.Format ("{0}:{1}", "isPaused", isPaused));
            sWrite.AppendLine (String.Format ("{0}:{1}", "isFullscreen", isFullscreen));

            sWrite.AppendLine (String.Format ("{0}:{1}", "Frequency", Frequency));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Augmentation", Augmentation));
            sWrite.AppendLine (String.Format ("{0}:{1}", "AugmentationAlarm", AugmentationAlarm));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Trigger", Trigger.Value));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Mode", Mode.Value));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Running", Running));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Primed", Primed));

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
                listTracings.ForEach (c => c.wfStrip.Unpause ()); ;
        }

        private void ToggleFullscreen () {
            isFullscreen = !isFullscreen;
            ApplyFullScreen ();
        }

        private void StartDevice () {
            PrimeBalloon ();
            Running = true;
            UpdateInterface ();
        }

        private void PauseDevice () {
            Running = false;
            UpdateInterface ();
        }

        private void PrimeBalloon () {
            Primed = true;
            UpdateInterface ();
        }

        private void SetOperationMode (Modes.Values value) {
            Mode.Value = value;
            PauseDevice ();
        }

        private void SelectSetting (Settings s) {
            buttonTrigger.Background = System.Windows.Media.Brushes.PowderBlue;
            buttonFrequency.Background = System.Windows.Media.Brushes.PowderBlue;
            buttonAugmentationPressure.Background = System.Windows.Media.Brushes.LightSkyBlue;
            buttonAugmentationAlarm.Background = System.Windows.Media.Brushes.LightSkyBlue;

            if (SelectedSetting == s)
                SelectedSetting = Settings.None;
            else
                SelectedSetting = s;

            switch (SelectedSetting) {
                default: return;
                case Settings.Trigger:
                    buttonTrigger.Background = System.Windows.Media.Brushes.Yellow;
                    return;

                case Settings.Frequency:
                    buttonFrequency.Background = System.Windows.Media.Brushes.Yellow;
                    return;

                case Settings.AugmentationPressure:
                    buttonAugmentationPressure.Background = System.Windows.Media.Brushes.Yellow;
                    return;

                case Settings.AugmentationAlarm:
                    buttonAugmentationAlarm.Background = System.Windows.Media.Brushes.Yellow;
                    return;
            }
        }

        private void ButtonZeroABP_Click (object s, RoutedEventArgs e) {
            App.Patient.TransducerZeroed_ABP = true;
            UpdateInterface ();
        }

        private void ButtonIncrease_Click (object s, RoutedEventArgs e) {
            switch (SelectedSetting) {
                default: return;
                case Settings.Frequency:
                    Frequency = Utility.Clamp (Frequency + 1, 1, 3);
                    UpdateInterface ();
                    return;

                case Settings.Trigger:
                    Array enumValues = Enum.GetValues (typeof (Triggering.Values));
                    Trigger.Value = (Triggering.Values)enumValues.GetValue (Utility.Clamp ((int)Trigger.Value + 1, 0, enumValues.Length - 1));
                    PauseDevice ();
                    UpdateInterface ();
                    return;

                case Settings.AugmentationPressure:
                    Augmentation = Utility.Clamp (Augmentation + 10, 0, 100);
                    listNumerics.Find (o => o.controlType.Value == Controls.IABPNumeric.ControlType.Values.IABP_AP).UpdateVitals ();
                    return;

                case Settings.AugmentationAlarm:
                    AugmentationAlarm = Utility.Clamp (AugmentationAlarm + 5, 0, 300);
                    listNumerics.Find (o => o.controlType.Value == Controls.IABPNumeric.ControlType.Values.IABP_AP).UpdateVitals ();
                    return;
            }
        }

        private void ButtonDecrease_Click (object s, RoutedEventArgs e) {
            switch (SelectedSetting) {
                default: return;
                case Settings.Frequency:
                    Frequency = Utility.Clamp (Frequency - 1, 1, 3);
                    UpdateInterface ();
                    return;

                case Settings.Trigger:
                    Array enumValues = Enum.GetValues (typeof (Triggering.Values));
                    Trigger.Value = (Triggering.Values)enumValues.GetValue (Utility.Clamp ((int)Trigger.Value - 1, 0, enumValues.Length - 1));
                    PauseDevice ();
                    UpdateInterface ();
                    return;

                case Settings.AugmentationPressure:
                    Augmentation = Utility.Clamp (Augmentation - 10, 0, 100);
                    listNumerics.Find (o => o.controlType.Value == Controls.IABPNumeric.ControlType.Values.IABP_AP).UpdateVitals ();
                    return;

                case Settings.AugmentationAlarm:
                    AugmentationAlarm = Utility.Clamp (AugmentationAlarm - 5, 0, 300);
                    listNumerics.Find (o => o.controlType.Value == Controls.IABPNumeric.ControlType.Values.IABP_AP).UpdateVitals ();
                    return;
            }
        }

        private void ButtonStart_Click (object s, RoutedEventArgs e) => StartDevice ();

        private void ButtonPause_Click (object s, RoutedEventArgs e) => PauseDevice ();

        private void ButtonTrigger_Click (object s, RoutedEventArgs e) => SelectSetting (Settings.Trigger);

        private void ButtonFrequency_Click (object s, RoutedEventArgs e) => SelectSetting (Settings.Frequency);

        private void ButtonAugmentationPressure_Click (object s, RoutedEventArgs e) => SelectSetting (Settings.AugmentationPressure);

        private void ButtonAugmentationAlarm_Click (object s, RoutedEventArgs e) => SelectSetting (Settings.AugmentationAlarm);

        private void ButtonModeAuto_Click (object s, RoutedEventArgs e) => SetOperationMode (Modes.Values.Auto);

        private void ButtonModeSemiAuto_Click (object s, RoutedEventArgs e) => SetOperationMode (Modes.Values.SemiAuto);

        private void ButtonPrimeBalloon_Click (object s, RoutedEventArgs e) => PrimeBalloon ();

        private void MenuClose_Click (object s, RoutedEventArgs e) => this.Close ();

        private void MenuExit_Click (object s, RoutedEventArgs e) => App.Patient_Editor.RequestExit ();

        private void MenuTogglePause_Click (object s, RoutedEventArgs e) => TogglePause ();

        private void MenuFullscreen_Click (object sender, RoutedEventArgs e) => ToggleFullscreen ();

        private void OnTick_Tracing (object sender, EventArgs e) {
            if (isPaused)
                return;

            listTracings.ForEach (c => {
                c.wfStrip.Scroll ();
                c.Draw ();
            });
        }

        private void OnTick_Vitals (object sender, EventArgs e) {
            if (isPaused)
                return;

            // Re-calculate IABP-specific vital signs (augmentation pressure and augmentation-assisted MAP)
            if (Running) {
                App.Patient.IABP_DBP = Utility.Clamp (App.Patient.ADBP - 7, 0, 1000);
                App.Patient.IABP_AP = (int)(App.Patient.ASBP + (App.Patient.ASBP * 0.3f * (Augmentation * 0.01f)));
                App.Patient.IABP_MAP = App.Patient.IABP_DBP + ((App.Patient.IABP_AP - App.Patient.IABP_DBP) / 2);
            } else {    // Use arterial line pressures if the balloon isn't actively pumping
                App.Patient.IABP_DBP = App.Patient.ADBP;
                App.Patient.IABP_AP = 0;
                App.Patient.IABP_MAP = App.Patient.AMAP;
            }

            UpdateInterface ();

            listNumerics.ForEach (n => n.UpdateVitals ());
        }

        public void OnPatientEvent (object sender, Patient.PatientEvent_Args e) {
            switch (e.EventType) {
                default: break;
                case Patient.PatientEvent_Args.EventTypes.Vitals_Change:
                    listTracings.ForEach (c => {
                        c.wfStrip.ClearFuture ();
                        c.wfStrip.Add_Beat__Cardiac_Baseline (App.Patient);
                    });
                    break;

                case Patient.PatientEvent_Args.EventTypes.Cardiac_Defibrillation:
                    listTracings.ForEach (c => c.wfStrip.Add_Beat__Cardiac_Defibrillation (App.Patient));
                    break;

                case Patient.PatientEvent_Args.EventTypes.Cardiac_PacerSpike:
                    listTracings.ForEach (c => c.wfStrip.Add_Beat__Cardiac_Pacemaker (App.Patient));
                    break;

                case Patient.PatientEvent_Args.EventTypes.Cardiac_Baseline:
                    App.Patient.IABP_Active = Running && (Frequency_Iter % Frequency == 0)
                        && ((Trigger.Value == Triggering.Values.ECG && App.Patient.CardiacRhythm.HasWaveform_Ventricular)
                        || (Trigger.Value == Triggering.Values.Pressure && App.Patient.CardiacRhythm.HasPulse_Ventricular));
                    App.Patient.IABP_Trigger = Trigger.Value.ToString ();

                    listTracings.ForEach (c => c.wfStrip.Add_Beat__Cardiac_Baseline (App.Patient));
                    break;

                case Patient.PatientEvent_Args.EventTypes.Cardiac_Atrial:
                    listTracings.ForEach (c => c.wfStrip.Add_Beat__Cardiac_Atrial (App.Patient));
                    break;

                case Patient.PatientEvent_Args.EventTypes.Cardiac_Ventricular:
                    if (Running)
                        Frequency_Iter++;

                    listTracings.ForEach (c => c.wfStrip.Add_Beat__Cardiac_Ventricular (App.Patient));
                    break;
            }
        }
    }
}