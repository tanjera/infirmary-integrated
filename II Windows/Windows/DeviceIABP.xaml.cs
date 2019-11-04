using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using II;
using II.Rhythm;
using II.Waveform;

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

        public bool Running = false;
        public bool Priming = false;
        public bool Prime_ThenStart = false;
        public bool Primed = false;

        public class PrimingEventArgs : EventArgs { public bool StartWhenComplete = false; };

        public Settings SelectedSetting = Settings.None;

        private int autoScale_iter = Strip.DefaultAutoScale_Iterations;

        private bool isFullscreen = false;
        private bool isPaused = false;

        private List<Controls.IABPTracing> listTracings = new List<Controls.IABPTracing> ();
        private List<Controls.IABPNumeric> listNumerics = new List<Controls.IABPNumeric> ();

        private Timer timerTracing = new Timer ();
        private Timer timerVitals = new Timer ();
        private Timer timerAncillary_Delay = new Timer ();

        // Define WPF UI commands for binding
        private ICommand icToggleFullscreen, icPauseDevice, icCloseDevice, icExitProgram,
            icSaveScreen, icPrintScreen;

        public ICommand IC_ToggleFullscreen { get { return icToggleFullscreen; } }
        public ICommand IC_PauseDevice { get { return icPauseDevice; } }
        public ICommand IC_CloseDevice { get { return icCloseDevice; } }
        public ICommand IC_ExitProgram { get { return icExitProgram; } }
        public ICommand IC_SaveScreen { get { return icSaveScreen; } }
        public ICommand IC_PrintScreen { get { return icPrintScreen; } }

        public DeviceIABP () {
            InitializeComponent ();
            DataContext = this;

            InitTimers ();
            InitInterface ();
        }

        ~DeviceIABP () => Dispose ();

        public void Dispose () {
            /* Clean subscriptions from the Main Timer */
            App.Timer_Main.Tick -= timerTracing.Process;
            App.Timer_Main.Tick -= timerVitals.Process;
            App.Timer_Main.Tick -= timerAncillary_Delay.Process;

            /* Dispose of local Timers */
            timerTracing.Dispose ();
            timerVitals.Dispose ();
            timerAncillary_Delay.Dispose ();

            /* Unsubscribe from the main Patient event listing */
            App.Patient.PatientEvent -= OnPatientEvent;
        }

        private void InitTimers () {
            App.Timer_Main.Tick += timerVitals.Process;
            App.Timer_Main.Tick += timerTracing.Process;
            App.Timer_Main.Tick += timerAncillary_Delay.Process;

            timerTracing.Set (Draw.RefreshTime);
            timerVitals.Set ((int)(App.Patient.GetHR_Seconds * 1000));

            timerTracing.Tick += OnTick_Tracing;
            timerVitals.Tick += OnTick_Vitals;

            timerTracing.Start ();
            timerVitals.Start ();
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
            wdwDeviceIABP.Title = App.Language.Localize ("IABP:WindowTitle");
            menuDevice.Header = App.Language.Localize ("MENU:MenuDeviceOptions");
            menuPauseDevice.Header = App.Language.Localize ("MENU:MenuPauseDevice");
            menuToggleFullscreen.Header = App.Language.Localize ("MENU:MenuToggleFullscreen");
            menuSaveScreen.Header = App.Language.Localize ("MENU:MenuSaveScreen");
            menuPrintScreen.Header = App.Language.Localize ("MENU:MenuPrintScreen");
            menuCloseDevice.Header = App.Language.Localize ("MENU:MenuCloseDevice");
            menuExitProgram.Header = App.Language.Localize ("MENU:MenuExitProgram");

            buttonModeAuto.Text = App.Language.Localize ("IABPMODE:Auto");
            buttonModeSemiAuto.Text = App.Language.Localize ("IABPMODE:SemiAuto");
            buttonZero.Text = Utility.WrapString (App.Language.Localize ("IABPBUTTON:ZeroPressure"));
            buttonStart.Text = App.Language.Localize ("IABPBUTTON:Start");
            buttonPause.Text = App.Language.Localize ("IABPBUTTON:Pause");
            btntxtTrigger.Text = Utility.WrapString (App.Language.Localize ("IABPBUTTON:Trigger"));
            btntxtFrequency.Text = Utility.WrapString (App.Language.Localize ("IABPBUTTON:Frequency"));
            buttonPrimeBalloon.Text = Utility.WrapString (App.Language.Localize ("IABPBUTTON:PrimeBalloon"));
            btntxtAugmentationPressure.Text = Utility.WrapString (App.Language.Localize ("IABP:AugmentationPressure"));
            btntxtAugmentationAlarm.Text = Utility.WrapString (App.Language.Localize ("IABP:AugmentationAlarm"));
            btntxtIncrease.Text = Utility.WrapString (App.Language.Localize ("IABPBUTTON:Increase"));
            btntxtDecrease.Text = Utility.WrapString (App.Language.Localize ("IABPBUTTON:Decrease"));

            // Random helium tank remaining amount... it's for show!
            lblHelium.Text = String.Format ("{0}: {1:0}%",
                Utility.WrapString (App.Language.Localize ("IABP:Helium")),
                II.Math.RandomDouble (20, 80));

            // Instantiate and add Tracings to UI
            listTracings.Add (new Controls.IABPTracing (new Strip (Lead.Values.ECG_II, 6f)));
            listTracings.Add (new Controls.IABPTracing (new Strip (Lead.Values.ABP, 6f)));
            listTracings.Add (new Controls.IABPTracing (new Strip (Lead.Values.IABP, 6f)));
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

            lblTriggerSource.Text = App.Language.Localize (Trigger.LookupString ());
            switch (Trigger.Value) {
                default:
                case Triggering.Values.ECG: lblTriggerSource.Foreground = Brushes.Green; break;
                case Triggering.Values.Pressure: lblTriggerSource.Foreground = Brushes.Red; break;
            }

            lblOperationMode.Text = App.Language.Localize (Mode.LookupString ());

            lblFrequency.Text = String.Format ("1 : {0}", Frequency);
            switch (Frequency) {
                default:
                case 1: lblFrequency.Foreground = Brushes.LightGreen; break;
                case 2: lblFrequency.Foreground = Brushes.Yellow; break;
                case 3: lblFrequency.Foreground = Brushes.OrangeRed; break;
            }

            if (Running) {
                lblMachineStatus.Text = App.Language.Localize ("IABP:Running");
                lblMachineStatus.Foreground = Brushes.LightGreen;
            } else {
                lblMachineStatus.Text = App.Language.Localize ("IABP:Paused");
                lblMachineStatus.Foreground = Brushes.Yellow;
            }

            if (Priming) {
                lblTubingStatus.Text = App.Language.Localize ("IABP:Priming");
                lblTubingStatus.Foreground = Brushes.Yellow;
            } else if (Primed) {
                lblTubingStatus.Text = App.Language.Localize ("IABP:Primed");
                lblTubingStatus.Foreground = Brushes.LightGreen;
            } else {
                lblTubingStatus.Text = App.Language.Localize ("IABP:NotPrimed");
                lblTubingStatus.Foreground = Brushes.OrangeRed;
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
            } catch {
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

        private void SaveScreen ()
            => Screenshot.SavePdf (
                Screenshot.SavePng (Screenshot.GetBitmap (mainGrid, 1), II.File.GetTempFilePath ("png")),
                App.Language.Localize ("IABP:WindowTitle"), null);

        private void PrintScreen ()
            => II.Screenshot.PrintPdf (II.Screenshot.AssemblePdf (
                Screenshot.SavePng (Screenshot.GetBitmap (mainGrid, 1), II.File.GetTempFilePath ("png")),
                    App.Language.Localize ("IABP:WindowTitle"), null));

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
                listTracings.ForEach (c => c.Strip.Unpause ()); ;
        }

        private void ToggleFullscreen () {
            isFullscreen = !isFullscreen;
            ApplyFullScreen ();
        }

        private void StartDevice () {
            if (!Primed) {
                Prime_ThenStart = true;
                PrimeBalloon ();
            } else {
                Prime_ThenStart = false;
                Running = true;
                UpdateInterface ();
            }
        }

        private void PauseDevice () {
            Running = false;
            UpdateInterface ();
        }

        private void PrimeBalloon () {
            if (timerAncillary_Delay.IsLocked) {
                Priming = false;
                Primed = true;
                if (Prime_ThenStart) {
                    Running = true;
                    Prime_ThenStart = false;
                }
            } else {
                Priming = true;
                Primed = false;

                timerAncillary_Delay.Lock ();
                timerAncillary_Delay.Tick += OnTick_PrimingComplete;
                timerAncillary_Delay.Set (5000);
                timerAncillary_Delay.Start ();
            }

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
                    Frequency = II.Math.Clamp (Frequency + 1, 1, 3);
                    UpdateInterface ();
                    return;

                case Settings.Trigger:
                    Array enumValues = Enum.GetValues (typeof (Triggering.Values));
                    Trigger.Value = (Triggering.Values)enumValues.GetValue (II.Math.Clamp ((int)Trigger.Value + 1, 0, enumValues.Length - 1));
                    PauseDevice ();
                    UpdateInterface ();
                    return;

                case Settings.AugmentationPressure:
                    Augmentation = II.Math.Clamp (Augmentation + 10, 0, 100);
                    listNumerics.Find (o => o.controlType.Value == Controls.IABPNumeric.ControlType.Values.IABP_AP).UpdateVitals ();
                    return;

                case Settings.AugmentationAlarm:
                    AugmentationAlarm = II.Math.Clamp (AugmentationAlarm + 5, 0, 300);
                    listNumerics.Find (o => o.controlType.Value == Controls.IABPNumeric.ControlType.Values.IABP_AP).UpdateVitals ();
                    return;
            }
        }

        private void ButtonDecrease_Click (object s, RoutedEventArgs e) {
            switch (SelectedSetting) {
                default: return;
                case Settings.Frequency:
                    Frequency = II.Math.Clamp (Frequency - 1, 1, 3);
                    UpdateInterface ();
                    return;

                case Settings.Trigger:
                    Array enumValues = Enum.GetValues (typeof (Triggering.Values));
                    Trigger.Value = (Triggering.Values)enumValues.GetValue (II.Math.Clamp ((int)Trigger.Value - 1, 0, enumValues.Length - 1));
                    PauseDevice ();
                    UpdateInterface ();
                    return;

                case Settings.AugmentationPressure:
                    Augmentation = II.Math.Clamp (Augmentation - 10, 0, 100);
                    listNumerics.Find (o => o.controlType.Value == Controls.IABPNumeric.ControlType.Values.IABP_AP).UpdateVitals ();
                    return;

                case Settings.AugmentationAlarm:
                    AugmentationAlarm = II.Math.Clamp (AugmentationAlarm - 5, 0, 300);
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

        private void MenuExit_Click (object s, RoutedEventArgs e) => App.Patient_Editor.Exit ();

        private void MenuTogglePause_Click (object s, RoutedEventArgs e) => TogglePause ();

        private void MenuFullscreen_Click (object sender, RoutedEventArgs e) => ToggleFullscreen ();

        private void MenuSaveScreen_Click (object sender, RoutedEventArgs e)
            => SaveScreen ();

        private void MenuPrintScreen_Click (object sender, RoutedEventArgs e)
            => PrintScreen ();

        private void OnClosed (object sender, EventArgs e)
            => this.Dispose ();

        private void OnTick_PrimingComplete (object sender, EventArgs e) {
            timerAncillary_Delay.Stop ();
            timerAncillary_Delay.Unlock ();
            timerAncillary_Delay.Tick -= OnTick_PrimingComplete;

            Priming = false;
            Primed = true;

            if (Prime_ThenStart) {
                Prime_ThenStart = false;
                Running = true;
            }

            UpdateInterface ();
        }

        private void OnTick_Tracing (object sender, EventArgs e) {
            if (isPaused)
                return;

            for (int i = 0; i < listTracings.Count; i++) {
                listTracings [i].Strip.Scroll ();
                listTracings [i].DrawTracing ();
            }
        }

        private void OnTick_Vitals (object sender, EventArgs e) {
            if (isPaused)
                return;

            // Re-calculate IABP-specific vital signs (augmentation pressure and augmentation-assisted MAP)
            if (Running) {
                App.Patient.IABP_DBP = II.Math.Clamp (App.Patient.ADBP - 7, 0, 1000);
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

        public void OnPatientEvent (object sender, Patient.PatientEventArgs e) {
            switch (e.EventType) {
                default: break;
                case Patient.PatientEventTypes.Vitals_Change:
                    listTracings.ForEach (c => {
                        c.Strip.ClearFuture (App.Patient);
                        c.Strip.Add_Beat__Cardiac_Baseline (App.Patient);
                    });
                    break;

                case Patient.PatientEventTypes.Defibrillation:
                    listTracings.ForEach (c => c.Strip.Add_Beat__Cardiac_Defibrillation (App.Patient));
                    break;

                case Patient.PatientEventTypes.Pacermaker_Spike:
                    listTracings.ForEach (c => c.Strip.Add_Beat__Cardiac_Pacemaker (App.Patient));
                    break;

                case Patient.PatientEventTypes.Cardiac_Baseline:
                    App.Patient.IABP_Active = Running && (Frequency_Iter % Frequency == 0)
                        && ((Trigger.Value == Triggering.Values.ECG && App.Patient.Cardiac_Rhythm.HasWaveform_Ventricular)
                        || (Trigger.Value == Triggering.Values.Pressure && App.Patient.Cardiac_Rhythm.HasPulse_Ventricular));
                    App.Patient.IABP_Trigger = Trigger.Value.ToString ();

                    listTracings.ForEach (c => c.Strip.Add_Beat__Cardiac_Baseline (App.Patient));
                    break;

                case Patient.PatientEventTypes.Cardiac_Atrial_Electric:
                    listTracings.ForEach (c => c.Strip.Add_Beat__Cardiac_Atrial_Electrical (App.Patient));
                    break;

                case Patient.PatientEventTypes.Cardiac_Ventricular_Electric:
                    if (Running)
                        Frequency_Iter++;

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

                case Patient.PatientEventTypes.IABP_Balloon_Inflation:
                    listTracings.ForEach (c => c.Strip.Add_Beat__IABP_Balloon (App.Patient));
                    break;
            }
        }
    }
}