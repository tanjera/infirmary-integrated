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

        public enum IABPSettings {
            None,
            Trigger,
            Frequency,
            AugmentationPressure,
            AugmentationAlarm
        }

        public IABPSettings SelectedSetting = IABPSettings.None;

        bool isFullscreen = false,
             isPaused = false;

        List<Controls.IABPTracing> listTracings = new List<Controls.IABPTracing> ();
        List<Controls.IABPNumeric> listNumerics = new List<Controls.IABPNumeric> ();

        Timer timerTracing = new Timer (),
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
            wdwDeviceIABP.Title = Dictionary["IABP:WindowTitle"];
            menuDevice.Header = Dictionary["MENU:MenuDeviceOptions"];
            menuPauseDevice.Header = Dictionary["MENU:MenuPauseDevice"];
            menuToggleFullscreen.Header = Dictionary["MENU:MenuToggleFullscreen"];
            menuCloseDevice.Header = Dictionary["MENU:MenuCloseDevice"];
            menuExitProgram.Header = Dictionary["MENU:MenuExitProgram"];

            buttonModeAuto.Text = Dictionary["IABPMODE:Auto"];
            buttonModeSemiAuto.Text = Dictionary["IABPMODE:SemiAuto"];
            buttonZero.Text = Utility.WrapString(Dictionary["IABPBUTTON:ZeroPressure"]);
            buttonStart.Text = Dictionary["IABPBUTTON:Start"];
            buttonPause.Text = Dictionary["IABPBUTTON:Pause"];
            btntxtTrigger.Text = Utility.WrapString(Dictionary ["IABPBUTTON:Trigger"]);
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

            lblTriggerSource.Text = Dictionary [App.Patient.IABPTrigger.LookupString ()];
            lblOperationMode.Text = Dictionary [App.Patient.IABPMode.LookupString ()];
            lblFrequency.Text = String.Format ("1 : {0}", App.Patient.IABPFrequencyRatio);
            lblMachineStatus.Text = Dictionary [App.Patient.IABPRunning ? "IABP:Running" : "IABP:Paused"];
            lblTubingStatus.Text = Dictionary [App.Patient.IABPPrimed ? "IABP:Primed" : "IABP:NotPrimed"];
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

        private void ToggleFullscreen () {
            isFullscreen = !isFullscreen;
            ApplyFullScreen ();
        }

        private void StartDevice () {
            PrimeBalloon ();
            App.Patient.IABPRunning = true;
            UpdateInterface ();
        }

        private void PauseDevice() {
            App.Patient.IABPRunning = false;
            UpdateInterface ();
        }

        private void PrimeBalloon () {
            App.Patient.IABPPrimed = true;
            UpdateInterface ();
        }

        private void SetOperationMode(Patient.IABPModes.Values value) {
            App.Patient.IABPMode.Value = value;
            PauseDevice ();
        }

        private void SelectSetting (IABPSettings s) {
            buttonTrigger.Background = System.Windows.Media.Brushes.PowderBlue;
            buttonFrequency.Background = System.Windows.Media.Brushes.PowderBlue;
            buttonAugmentationPressure.Background = System.Windows.Media.Brushes.LightSkyBlue;
            buttonAugmentationAlarm.Background = System.Windows.Media.Brushes.LightSkyBlue;

            if (SelectedSetting == s)
                SelectedSetting = IABPSettings.None;
            else
                SelectedSetting = s;

            switch (SelectedSetting) {
                default: return;
                case IABPSettings.Trigger:
                    buttonTrigger.Background = System.Windows.Media.Brushes.Yellow;
                    return;
                case IABPSettings.Frequency:
                    buttonFrequency.Background = System.Windows.Media.Brushes.Yellow;
                    return;
                case IABPSettings.AugmentationPressure:
                    buttonAugmentationPressure.Background = System.Windows.Media.Brushes.Yellow;
                    return;
                case IABPSettings.AugmentationAlarm:
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
                case IABPSettings.Frequency:
                    App.Patient.IABPFrequencyRatio = Utility.Clamp (App.Patient.IABPFrequencyRatio + 1, 1, 3);
                    return;

                case IABPSettings.Trigger:
                    Array enumValues = Enum.GetValues (typeof (Patient.IABPTriggers.Values));
                    App.Patient.IABPTrigger.Value = (Patient.IABPTriggers.Values)enumValues.GetValue (Utility.Clamp((int)App.Patient.IABPTrigger.Value + 1, 0, enumValues.Length - 1));
                    PauseDevice ();
                    UpdateInterface ();
                    return;

                case IABPSettings.AugmentationPressure:
                    App.Patient.IABPAugmentation = Utility.Clamp (App.Patient.IABPAugmentation + 10, 0, 100);
                    listNumerics.Find (o => o.controlType.Value == Controls.IABPNumeric.ControlType.Values.IABP_AP).UpdateVitals ();
                    return;

                case IABPSettings.AugmentationAlarm:
                    App.Patient.IABPAugmentationAlarm = Utility.Clamp (App.Patient.IABPAugmentationAlarm + 5, 0, 300);
                    listNumerics.Find (o => o.controlType.Value == Controls.IABPNumeric.ControlType.Values.IABP_AP).UpdateVitals ();
                    return;
            }
        }

        private void ButtonDecrease_Click (object s, RoutedEventArgs e) {
            switch (SelectedSetting) {
                default: return;
                case IABPSettings.Frequency:
                    App.Patient.IABPFrequencyRatio = Utility.Clamp (App.Patient.IABPFrequencyRatio - 1, 1, 3);
                    UpdateInterface ();
                    return;

                case IABPSettings.Trigger:
                    Array enumValues = Enum.GetValues (typeof (Patient.IABPTriggers.Values));
                    App.Patient.IABPTrigger.Value = (Patient.IABPTriggers.Values)enumValues.GetValue (Utility.Clamp((int)App.Patient.IABPTrigger.Value - 1, 0, enumValues.Length - 1));
                    PauseDevice ();
                    UpdateInterface ();
                    return;

                case IABPSettings.AugmentationPressure:
                    App.Patient.IABPAugmentation = Utility.Clamp (App.Patient.IABPAugmentation - 10, 0, 100);
                    listNumerics.Find (o => o.controlType.Value == Controls.IABPNumeric.ControlType.Values.IABP_AP).UpdateVitals ();
                    return;

                case IABPSettings.AugmentationAlarm:
                    App.Patient.IABPAugmentationAlarm = Utility.Clamp (App.Patient.IABPAugmentationAlarm - 5, 0, 300);
                    listNumerics.Find (o => o.controlType.Value == Controls.IABPNumeric.ControlType.Values.IABP_AP).UpdateVitals ();
                    return;
            }
        }

        private void ButtonStart_Click (object s, RoutedEventArgs e) => StartDevice ();
        private void ButtonPause_Click (object s, RoutedEventArgs e) => PauseDevice ();
        private void ButtonTrigger_Click (object s, RoutedEventArgs e) => SelectSetting (IABPSettings.Trigger);
        private void ButtonFrequency_Click (object s, RoutedEventArgs e) => SelectSetting (IABPSettings.Frequency);
        private void ButtonAugmentationPressure_Click (object s, RoutedEventArgs e) => SelectSetting (IABPSettings.AugmentationPressure);
        private void ButtonAugmentationAlarm_Click (object s, RoutedEventArgs e) => SelectSetting (IABPSettings.AugmentationAlarm);
        private void ButtonModeAuto_Click (object s, RoutedEventArgs e) => SetOperationMode (Patient.IABPModes.Values.Auto);
        private void ButtonModeSemiAuto_Click (object s, RoutedEventArgs e) => SetOperationMode (Patient.IABPModes.Values.SemiAuto);
        private void ButtonPrimeBalloon_Click (object s, RoutedEventArgs e) => PrimeBalloon ();

        private void MenuClose_Click (object s, RoutedEventArgs e) => this.Close ();
        private void MenuExit_Click (object s, RoutedEventArgs e) => App.Patient_Editor.RequestExit ();
        private void MenuTogglePause_Click (object s, RoutedEventArgs e) => TogglePause ();
        private void MenuFullscreen_Click (object sender, RoutedEventArgs e) => ToggleFullscreen ();

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

            // Re-calculate IABP-specific vital signs (augmentation pressure and augmentation-assisted MAP)
            if (App.Patient.IABPRunning) {
                App.Patient.IABP_DBP = Utility.Clamp (App.Patient.ADBP - 7, 0, 1000);
                App.Patient.IABP_AP = (int)(App.Patient.ASBP + (App.Patient.ASBP * 0.3f * (App.Patient.IABPAugmentation * 0.01f)));
                App.Patient.IABP_MAP = App.Patient.IABP_DBP + ((App.Patient.IABP_AP - App.Patient.IABP_DBP) / 2);
            } else {    // Use arterial line pressures if the balloon isn't actively pumping
                App.Patient.IABP_DBP = App.Patient.ADBP;
                App.Patient.IABP_AP = 0;
                App.Patient.IABP_MAP = App.Patient.AMAP;
            }

            UpdateInterface ();

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