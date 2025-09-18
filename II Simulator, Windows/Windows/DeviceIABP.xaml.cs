using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
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
using II.Settings;
using II.Waveform;

using IISIM.Classes;
using IISIM.Controls;
using IISIM.Properties;

namespace IISIM.Windows {

    /// <summary>
    /// Interaction logic for DeviceIABP.xaml
    /// </summary>
    public partial class DeviceIABP : Window {
        public App? Instance { get; set; }

        public States State;

        /* Device settings */

        // Device settings
        public int Frequency = 1,
                   Frequency_Iter = 0,              // Buffer value to determine if current beat triggers IABP
                   Augmentation = 100,              // Expressed as % (e.g. 10%, 100%)
                   AugmentationAlarm = 100;         // Expressed as mmHg

        public Triggering Trigger = new ();
        public Modes Mode = new ();

        public bool Running = false;
        public bool Priming = false;
        public bool Prime_ThenStart = false;
        public bool Primed = false;

        public class PrimingEventArgs : EventArgs { public bool StartWhenComplete = false; };

        public Settings SelectedSetting = Settings.None;

        private int autoScale_iter = Strip.DefaultAutoScale_Iterations;

        private Color.Schemes colorScheme = Color.Schemes.Dark;

        private List<Controls.IABPTracing> listTracings = new ();
        private List<Controls.IABPNumeric> listNumerics = new ();

        public II.Timer
            TimerAlarm = new (),
            TimerTracing = new (),
            TimerNumerics_Cardiac = new (),
            TimerNumerics_Respiratory = new (),
            TimerAncillary_Delay = new ();

        /* Variables for audio tones (QRS or SPO2 beeps) and defibrillator charger */

        public enum States {
            Running,
            Paused,
            Closed
        }

        public enum Settings {
            None,
            Trigger,
            Frequency,
            InflationTiming,
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
                return String.Format ("IABPTRIGGER:{0}", Enum.GetValues (typeof (Values)).GetValue ((int)v)?.ToString ());
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
                return String.Format ("IABPMODE:{0}", Enum.GetValues (typeof (Values)).GetValue ((int)v)?.ToString ());
            }
        }

        public DeviceIABP () {
            InitializeComponent ();
        }

        public DeviceIABP (App? app) {
            InitializeComponent ();

            Instance = app;

            Closed += this.OnClosed;
            Closing += this.OnClosing;

            InitTimers ();

            State = States.Running;

            InitInterface ();
            SetColorScheme (colorScheme);
        }

        ~DeviceIABP () {
            Dispose ();
        }

        public void Dispose () {
            /* Clean subscriptions from the Main Timer */
            if (Instance is not null) {
                Instance.Timer_Main.Elapsed -= TimerTracing.Process;
                Instance.Timer_Main.Elapsed -= TimerNumerics_Cardiac.Process;
                Instance.Timer_Main.Elapsed -= TimerNumerics_Respiratory.Process;
                Instance.Timer_Main.Elapsed -= TimerAncillary_Delay.Process;
            }

            /* Dispose of local Timers */
            TimerTracing.Dispose ();
            TimerNumerics_Cardiac.Dispose ();
            TimerNumerics_Respiratory.Dispose ();
            TimerAncillary_Delay.Dispose ();

            /* Unsubscribe from the main Patient event listing */
            if (Instance?.Physiology != null)
                Instance.Physiology.PhysiologyEvent -= OnPhysiologyEvent;
        }

        public void DisposeAudio () {
        }

        public void InitTimers () {
            if (Instance is null)
                return;

            /* TimerAncillary_Delay is attached/detached to events in the Devices for their
             * specific uses (e.g. IABP priming, Defib charging, etc.) ... only want to link it
             * to Timer_Main, otherwise do not set, start, or link to any events here!
             */
            Instance.Timer_Main.Elapsed += TimerAncillary_Delay.Process;

            Instance.Timer_Main.Elapsed += TimerAlarm.Process;
            Instance.Timer_Main.Elapsed += TimerTracing.Process;
            Instance.Timer_Main.Elapsed += TimerNumerics_Cardiac.Process;
            Instance.Timer_Main.Elapsed += TimerNumerics_Respiratory.Process;

            TimerAlarm.Tick += OnTick_Alarm;
            TimerTracing.Tick += OnTick_Tracing;
            TimerNumerics_Cardiac.Tick += OnTick_Vitals_Cardiac;
            TimerNumerics_Respiratory.Tick += OnTick_Vitals_Respiratory;

            TimerAlarm.Set (2500);
            TimerTracing.Set (Draw.RefreshTime);
            TimerNumerics_Cardiac.Set (3000);
            TimerNumerics_Respiratory.Set (5000);

            TimerAlarm.Start ();
            TimerTracing.Start ();
            TimerNumerics_Cardiac.Start ();
            TimerNumerics_Respiratory.Start ();
        }

        private void InitInterface () {
            if (Instance?.Language is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitInterface)}");
                return;
            }

            /* Populate UI strings per language selection */

            wdwDeviceIABP.Title = Instance.Language.Localize ("DEFIB:WindowTitle");
            menuDevice.Header = Instance.Language.Localize ("MENU:MenuDeviceOptions");
            menuPauseDevice.Header = Instance.Language.Localize ("MENU:MenuPauseDevice");
            menuToggleFullscreen.Header = Instance.Language.Localize ("MENU:MenuToggleFullscreen");
            menuCloseDevice.Header = Instance.Language.Localize ("MENU:MenuCloseDevice");

            menuColor.Header = Instance.Language.Localize ("MENU:MenuColorScheme");
            menuColorLight.Header = Instance.Language.Localize ("MENU:MenuColorSchemeLight");
            menuColorDark.Header = Instance.Language.Localize ("MENU:MenuColorSchemeDark");

            buttonModeAuto.Text = Instance.Language.Localize ("IABPMODE:Auto");
            buttonModeSemiAuto.Text = Instance.Language.Localize ("IABPMODE:SemiAuto");
            buttonZero.Text = Utility.WrapString (Instance.Language.Localize ("IABPBUTTON:ZeroPressure"));
            buttonStart.Text = Instance.Language.Localize ("IABPBUTTON:Start");
            buttonPause.Text = Instance.Language.Localize ("IABPBUTTON:Pause");
            btntxtTrigger.Text = Utility.WrapString (Instance.Language.Localize ("IABPBUTTON:Trigger"));
            btntxtFrequency.Text = Utility.WrapString (Instance.Language.Localize ("IABPBUTTON:Frequency"));
            buttonPrimeBalloon.Text = Utility.WrapString (Instance.Language.Localize ("IABPBUTTON:PrimeBalloon"));
            btntxtInflationTiming.Text = Utility.WrapString (Instance.Language.Localize ("IABP:InflationTiming"));
            btntxtAugmentationPressure.Text = Utility.WrapString (Instance.Language.Localize ("IABP:AugmentationPressure"));
            btntxtAugmentationAlarm.Text = Utility.WrapString (Instance.Language.Localize ("IABP:AugmentationAlarm"));
            btntxtIncrease.Text = Utility.WrapString (Instance.Language.Localize ("IABPBUTTON:Increase"));
            btntxtDecrease.Text = Utility.WrapString (Instance.Language.Localize ("IABPBUTTON:Decrease"));

            /* Init Numeric & Tracing layout */

            // Random helium tank remaining amount... it's for show!
            lblHelium.Text = String.Format ("{0}: {1:0}%",
                Utility.WrapString (Instance.Language.Localize ("IABP:Helium")),
                II.Math.RandomInt (20, 80));

            // Instantiate and add Tracings to UI
            listTracings.Add (new Controls.IABPTracing (Instance, new Strip (Lead.Values.ECG_II, 6f), colorScheme));
            listTracings.Add (new Controls.IABPTracing (Instance, new Strip (Lead.Values.ABP, 6f), colorScheme));
            listTracings.Add (new Controls.IABPTracing (Instance, new Strip (Lead.Values.IABP, 6f), colorScheme));
            for (int i = 0; i < listTracings.Count; i++) {
                listTracings [i].SetValue (Grid.RowProperty, i);
                listTracings [i].SetValue (Grid.ColumnProperty, 1);
                displayGrid.Children.Add (listTracings [i]);
            }

            // Instantiate and add Numerics to UI
            listNumerics.Add (new Controls.IABPNumeric (Instance, this, Controls.IABPNumeric.ControlTypes.Values.ECG, colorScheme));
            listNumerics.Add (new Controls.IABPNumeric (Instance, this, Controls.IABPNumeric.ControlTypes.Values.ABP, colorScheme));
            listNumerics.Add (new Controls.IABPNumeric (Instance, this, Controls.IABPNumeric.ControlTypes.Values.IABP_AP, colorScheme));
            for (int i = 0; i < listNumerics.Count; i++) {
                listNumerics [i].SetValue (Grid.RowProperty, i);
                listNumerics [i].SetValue (Grid.ColumnProperty, 2);
                displayGrid.Children.Add (listNumerics [i]);
            }

            /* Init Hotkeys (Commands & InputBinding) */

            RoutedCommand
                cmdMenuTogglePause_Click = new (),
                cmdMenuToggleFullscreen_Click = new (),
                cmdMenuColorScheme_Light = new (),
                cmdMenuColorScheme_Dark = new ();

            cmdMenuTogglePause_Click.InputGestures.Add (new KeyGesture (Key.Pause));
            CommandBindings.Add (new CommandBinding (cmdMenuTogglePause_Click, MenuTogglePause_Click));

            cmdMenuToggleFullscreen_Click.InputGestures.Add (new KeyGesture (Key.Enter, ModifierKeys.Alt));
            CommandBindings.Add (new CommandBinding (cmdMenuToggleFullscreen_Click, MenuToggleFullscreen_Click));

            cmdMenuColorScheme_Light.InputGestures.Add (new KeyGesture (Key.F1));
            CommandBindings.Add (new CommandBinding (cmdMenuColorScheme_Light, MenuColorScheme_Light));

            cmdMenuColorScheme_Dark.InputGestures.Add (new KeyGesture (Key.F2));
            CommandBindings.Add (new CommandBinding (cmdMenuColorScheme_Dark, MenuColorScheme_Dark));
        }

        private void UpdateInterface () {
            for (int i = 0; i < listTracings.Count; i++)
                listTracings [i].SetColorScheme (colorScheme);

            for (int i = 0; i < listNumerics.Count; i++)
                listNumerics [i].SetColorScheme (colorScheme);

            App.Current.Dispatcher.InvokeAsync ((Action)(() => {
                wdwDeviceIABP.Background = Color.GetBackground (Color.Devices.DeviceDefib, colorScheme);
            }));

            brdStatusInfo.BorderBrush = colorScheme == Color.Schemes.Dark ? Brushes.SkyBlue : Brushes.Black;

            lblHelium.Foreground = colorScheme == Color.Schemes.Dark ? Brushes.MediumPurple : Brushes.Black;
            brdHelium.BorderBrush = colorScheme == Color.Schemes.Dark ? Brushes.MediumPurple : Brushes.Black;

            lblOperationMode.Foreground = colorScheme == Color.Schemes.Dark ? Brushes.Aqua : Brushes.Black;
            brdOperationMode.BorderBrush = colorScheme == Color.Schemes.Dark ? Brushes.Aqua : Brushes.Black;

            lblTriggerSource.Text = Instance?.Language.Localize (Trigger.LookupString ());
            switch (Trigger.Value) {
                default:
                case Triggering.Values.ECG:
                    lblTriggerSource.Foreground = colorScheme == Color.Schemes.Dark ? Brushes.Green : Brushes.Black;
                    brdTriggerSource.BorderBrush = colorScheme == Color.Schemes.Dark ? Brushes.Green : Brushes.Black;
                    break;

                case Triggering.Values.Pressure:
                    lblTriggerSource.Foreground = colorScheme == Color.Schemes.Dark ? Brushes.Red : Brushes.Black;
                    brdTriggerSource.BorderBrush = colorScheme == Color.Schemes.Dark ? Brushes.Red : Brushes.Black;
                    break;
            }

            lblOperationMode.Text = Instance?.Language.Localize (Mode.LookupString ());

            lblFrequency.Text = String.Format ("1 : {0}", Frequency);
            switch (Frequency) {
                default:
                case 1:
                    lblFrequency.Foreground = colorScheme == Color.Schemes.Dark ? Brushes.LightGreen : Brushes.Black;
                    brdFrequency.BorderBrush = colorScheme == Color.Schemes.Dark ? Brushes.LightGreen : Brushes.Black;
                    break;

                case 2:
                    lblFrequency.Foreground = colorScheme == Color.Schemes.Dark ? Brushes.Yellow : Brushes.Black;
                    brdFrequency.BorderBrush = colorScheme == Color.Schemes.Dark ? Brushes.Yellow : Brushes.Black;
                    break;

                case 3:
                    lblFrequency.Foreground = colorScheme == Color.Schemes.Dark ? Brushes.OrangeRed : Brushes.Black;
                    brdFrequency.BorderBrush = colorScheme == Color.Schemes.Dark ? Brushes.OrangeRed : Brushes.Black;
                    break;
            }

            if (Running) {
                lblMachineStatus.Text = Instance?.Language.Localize ("IABP:Running");
                lblMachineStatus.Foreground = colorScheme == Color.Schemes.Dark ? Brushes.LightGreen : Brushes.Black;
                brdMachineStatus.BorderBrush = colorScheme == Color.Schemes.Dark ? Brushes.LightGreen : Brushes.Black;
            } else {
                lblMachineStatus.Text = Instance?.Language.Localize ("IABP:Paused");
                lblMachineStatus.Foreground = colorScheme == Color.Schemes.Dark ? Brushes.Yellow : Brushes.Black;
                brdMachineStatus.BorderBrush = colorScheme == Color.Schemes.Dark ? Brushes.Yellow : Brushes.Black;
            }

            if (Priming) {
                lblTubingStatus.Text = Instance?.Language.Localize ("IABP:Priming");
                lblTubingStatus.Foreground = colorScheme == Color.Schemes.Dark ? Brushes.Yellow : Brushes.Black;
                brdTubingStatus.BorderBrush = colorScheme == Color.Schemes.Dark ? Brushes.Yellow : Brushes.Black;
            } else if (Primed) {
                lblTubingStatus.Text = Instance?.Language.Localize ("IABP:Primed");
                lblTubingStatus.Foreground = colorScheme == Color.Schemes.Dark ? Brushes.LightGreen : Brushes.Black;
                brdTubingStatus.BorderBrush = colorScheme == Color.Schemes.Dark ? Brushes.LightGreen : Brushes.Black;
            } else {
                lblTubingStatus.Text = Instance?.Language.Localize ("IABP:NotPrimed");
                lblTubingStatus.Foreground = colorScheme == Color.Schemes.Dark ? Brushes.OrangeRed : Brushes.Black;
                brdTubingStatus.BorderBrush = colorScheme == Color.Schemes.Dark ? Brushes.OrangeRed : Brushes.Black;
            }
        }

        public async Task Load (string inc) {
            using StringReader sRead = new (inc);
            List<string> numericTypes = new (),
                         tracingTypes = new ();

            try {
                string? line;
                while ((line = await sRead.ReadLineAsync ()) != null) {
                    if (line.Contains (':')) {
                        string pName = line.Substring (0, line.IndexOf (':')),
                                pValue = line.Substring (line.IndexOf (':') + 1);
                        switch (pName) {
                            default: break;
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
            StringBuilder sWrite = new ();

            sWrite.AppendLine (String.Format ("{0}:{1}", "Frequency", Frequency));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Augmentation", Augmentation));
            sWrite.AppendLine (String.Format ("{0}:{1}", "AugmentationAlarm", AugmentationAlarm));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Trigger", Trigger.Value));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Mode", Mode.Value));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Running", Running));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Primed", Primed));

            return sWrite.ToString ();
        }

        public void SetColorScheme_Light () => SetColorScheme (Color.Schemes.Light);

        public void SetColorScheme_Dark () => SetColorScheme (Color.Schemes.Dark);

        public void SetColorScheme (Color.Schemes scheme) {
            colorScheme = scheme;
            UpdateInterface ();
        }

        public void PauseDevice (bool toPause) {
            if (toPause) {
                State = States.Paused;

                if (Instance?.Physiology is not null)
                    Instance.Physiology.PhysiologyEvent -= OnPhysiologyEvent;

                TimerAlarm.Stop ();
                TimerTracing.Stop ();
            } else if (toPause == false) {
                State = States.Running;

                /* Trigger an "Unpause" event in each Strip ... otherwise they Scroll() based on DateTime elapsed */
                foreach (var t in listTracings) {
                    t.Strip?.Unpause ();
                }

                TimerAlarm.Start ();
                TimerTracing.Start ();

                if (Instance?.Physiology is not null)
                    Instance.Physiology.PhysiologyEvent += OnPhysiologyEvent;
            }
        }

        public void TogglePause () {
            PauseDevice (State == States.Running);
        }

        private void StartTherapy () {
            if (!Primed) {
                Prime_ThenStart = true;
                PrimeBalloon ();
            } else {
                Prime_ThenStart = false;
                Running = true;
                UpdateInterface ();
            }
        }

        private void PauseTherapy () {
            Running = false;
            UpdateInterface ();
        }

        private void PrimeBalloon () {
            if (TimerAncillary_Delay.IsLocked) {
                Priming = false;
                Primed = true;
                if (Prime_ThenStart) {
                    Running = true;
                    Prime_ThenStart = false;
                }
            } else {
                Priming = true;
                Primed = false;

                TimerAncillary_Delay.Lock ();
                TimerAncillary_Delay.Tick += OnTick_PrimingComplete;
                TimerAncillary_Delay.Set (5000);
                TimerAncillary_Delay.Start ();
            }

            UpdateInterface ();
        }

        private void SetOperationMode (Modes.Values value) {
            Mode.Value = value;
            PauseTherapy ();
        }

        private void SelectSetting (Settings s) {
            buttonTrigger.Background = Brushes.PowderBlue;
            buttonFrequency.Background = Brushes.PowderBlue;
            buttonInflationTiming.Background = Brushes.PowderBlue;
            buttonAugmentationPressure.Background = Brushes.LightSkyBlue;
            buttonAugmentationAlarm.Background = Brushes.LightSkyBlue;

            if (SelectedSetting == s)
                SelectedSetting = Settings.None;
            else
                SelectedSetting = s;

            switch (SelectedSetting) {
                default: return;
                case Settings.Trigger:
                    buttonTrigger.Background = Brushes.Yellow;
                    return;

                case Settings.Frequency:
                    buttonFrequency.Background = Brushes.Yellow;
                    return;

                case Settings.InflationTiming:
                    buttonInflationTiming.Background = Brushes.Yellow;
                    return;

                case Settings.AugmentationPressure:
                    buttonAugmentationPressure.Background = Brushes.Yellow;
                    return;

                case Settings.AugmentationAlarm:
                    buttonAugmentationAlarm.Background = Brushes.Yellow;
                    return;
            }
        }

        public void ToggleFullscreen () {
            if (wdwDeviceIABP.WindowState == System.Windows.WindowState.Maximized)
                wdwDeviceIABP.WindowState = System.Windows.WindowState.Normal;
            else
                wdwDeviceIABP.WindowState = System.Windows.WindowState.Maximized;
        }

        private void IterateAutoScale () {
            /* Iterations and trigger for auto-scaling pressure waveform strips */
            autoScale_iter -= 1;

            if (autoScale_iter <= 0) {
                for (int i = 0; i < listTracings.Count; i++) {
                    listTracings [i].Strip?.SetAutoScale (Instance?.Physiology);
                    App.Current.Dispatcher.InvokeAsync (listTracings [i].UpdateScale);
                }

                autoScale_iter = Strip.DefaultAutoScale_Iterations;
            }
        }

        private void ButtonZeroABP_Click (object s, RoutedEventArgs e) {
            if (Instance?.Physiology is not null)
                Instance.Physiology.TransducerZeroed_ABP = true;
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
                    Trigger.Value = (Triggering.Values)(enumValues.GetValue (II.Math.Clamp ((int)Trigger.Value + 1, 0, enumValues.Length - 1)) ?? Triggering.Values.ECG);

                    if (Instance?.Physiology is not null) {
                        Instance.Physiology.IABP_Trigger = Trigger.Value switch {
                            Triggering.Values.ECG => Physiology.IABP_Triggers.ECG,
                            Triggering.Values.Pressure => Physiology.IABP_Triggers.Pressure,
                            _ => Physiology.IABP_Triggers.ECG
                        };

                        Instance.Physiology.IABP_DelayManual = 0;
                        Instance.Physiology.IABP_DelayDynamic = Physiology.IABP_Delays [Instance.Physiology.IABP_Trigger.GetHashCode ()];
                    }

                    PauseTherapy ();
                    UpdateInterface ();
                    return;

                case Settings.InflationTiming:
                    if (Instance?.Physiology is not null) {
                        Instance.Physiology.IABP_DelayManual += 50;
                    }
                    return;

                case Settings.AugmentationPressure:
                    Augmentation = II.Math.Clamp (Augmentation + 10, 10, 100);
                    listNumerics.Find (o => o.ControlType?.Value == Controls.IABPNumeric.ControlTypes.Values.IABP_AP)?.UpdateVitals ();
                    return;

                case Settings.AugmentationAlarm:
                    AugmentationAlarm = II.Math.Clamp (AugmentationAlarm + 5, 0, 300);
                    listNumerics.Find (o => o.ControlType?.Value == Controls.IABPNumeric.ControlTypes.Values.IABP_AP)?.UpdateVitals ();
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
                    Trigger.Value = (Triggering.Values)(enumValues.GetValue (II.Math.Clamp ((int)Trigger.Value - 1, 0, enumValues.Length - 1)) ?? Triggering.Values.ECG);

                    if (Instance?.Physiology is not null) {
                        Instance.Physiology.IABP_Trigger = Trigger.Value switch {
                            Triggering.Values.ECG => Physiology.IABP_Triggers.ECG,
                            Triggering.Values.Pressure => Physiology.IABP_Triggers.Pressure,
                            _ => Physiology.IABP_Triggers.ECG
                        };

                        Instance.Physiology.IABP_DelayManual = 0;
                        Instance.Physiology.IABP_DelayDynamic = Physiology.IABP_Delays [Instance.Physiology.IABP_Trigger.GetHashCode ()];
                    }

                    PauseTherapy ();
                    UpdateInterface ();
                    return;

                case Settings.InflationTiming:
                    if (Instance?.Physiology is not null) {
                        Instance.Physiology.IABP_DelayManual -= 50;
                    }
                    return;

                case Settings.AugmentationPressure:
                    Augmentation = II.Math.Clamp (Augmentation - 10, 10, 100);
                    listNumerics.Find (o => o.ControlType?.Value == Controls.IABPNumeric.ControlTypes.Values.IABP_AP)?.UpdateVitals ();
                    return;

                case Settings.AugmentationAlarm:
                    AugmentationAlarm = II.Math.Clamp (AugmentationAlarm - 5, 0, 300);
                    listNumerics.Find (o => o.ControlType?.Value == Controls.IABPNumeric.ControlTypes.Values.IABP_AP)?.UpdateVitals ();
                    return;
            }
        }

        private void ButtonStart_Click (object s, RoutedEventArgs e)
            => StartTherapy ();

        private void ButtonPause_Click (object s, RoutedEventArgs e)
            => PauseTherapy ();

        private void ButtonTrigger_Click (object s, RoutedEventArgs e)
            => SelectSetting (Settings.Trigger);

        private void ButtonFrequency_Click (object s, RoutedEventArgs e)
            => SelectSetting (Settings.Frequency);

        private void ButtonInflationTiming_Click (object s, RoutedEventArgs e)
            => SelectSetting (Settings.InflationTiming);

        private void ButtonAugmentationPressure_Click (object s, RoutedEventArgs e)
            => SelectSetting (Settings.AugmentationPressure);

        private void ButtonAugmentationAlarm_Click (object s, RoutedEventArgs e)
            => SelectSetting (Settings.AugmentationAlarm);

        private void ButtonModeAuto_Click (object s, RoutedEventArgs e)
            => SetOperationMode (Modes.Values.Auto);

        private void ButtonModeSemiAuto_Click (object s, RoutedEventArgs e)
            => SetOperationMode (Modes.Values.SemiAuto);

        private void ButtonPrimeBalloon_Click (object s, RoutedEventArgs e)
            => PrimeBalloon ();

        private void MenuToggleFullscreen_Click (object s, RoutedEventArgs e)
            => ToggleFullscreen ();

        private void MenuClose_Click (object s, RoutedEventArgs e) {
            wdwDeviceIABP.Close ();
        }

        private void MenuTogglePause_Click (object s, RoutedEventArgs e)
            => TogglePause ();

        private void MenuColorScheme_Light (object sender, RoutedEventArgs e)
            => SetColorScheme (Color.Schemes.Light);

        private void MenuColorScheme_Dark (object sender, RoutedEventArgs e)
            => SetColorScheme (Color.Schemes.Dark);

        public void OnClosed (object? sender, EventArgs e) {
            State = States.Closed;

            Dispose ();
        }

        public void OnClosing (object? sender, CancelEventArgs e) {
            TimerAlarm?.Dispose ();
            DisposeAudio ();

            if (Instance?.Physiology is not null)
                Instance.Physiology.PhysiologyEvent -= OnPhysiologyEvent;
        }

        private void OnTick_PrimingComplete (object? sender, EventArgs e) {
            TimerAncillary_Delay.Stop ();
            TimerAncillary_Delay.Unlock ();
            TimerAncillary_Delay.Tick -= OnTick_PrimingComplete;

            Priming = false;
            Primed = true;

            if (Prime_ThenStart) {
                Prime_ThenStart = false;
                Running = true;
            }

            App.Current.Dispatcher.InvokeAsync (UpdateInterface);
        }

        public void OnTick_Alarm (object? sender, EventArgs e) {
            // TODO: Implement
        }

        public void OnTick_Tracing (object? sender, EventArgs e) {
            if (State == States.Running) {  // Only pauses advancement of tracing; simulation still active!
                for (int i = 0; i < listTracings.Count; i++) {
                    listTracings [i].Strip?.Scroll (Instance?.Physiology?.Time ?? 0);

                    App.Current.Dispatcher.InvokeAsync (listTracings [i].DrawTracing);
                }
            } else if (State == States.Paused) {
                foreach (var t in listTracings)
                    t.Strip?.Unpause ();
            }
        }

        public void OnTick_Vitals_Cardiac (object? sender, EventArgs e) {
            if (State == States.Running && Instance?.Physiology is not null) {
                // Re-calculate IABP-specific vital signs (augmentation pressure and augmentation-assisted MAP)
                if (Running) {
                    Instance.Physiology.IABP_DBP = II.Math.Clamp (Instance.Physiology.ADBP - 7, 0, 1000);
                    Instance.Physiology.IABP_AP = (int)(Instance.Physiology.ASBP + (Instance.Physiology.ASBP * 0.3f * (Augmentation * 0.01f)));
                    Instance.Physiology.IABP_MAP = Instance.Physiology.IABP_DBP + ((Instance.Physiology.IABP_AP - Instance.Physiology.IABP_DBP) / 2);
                } else {    // Use arterial line pressures if the balloon isn't actively pumping
                    Instance.Physiology.IABP_DBP = Instance.Physiology.ADBP;
                    Instance.Physiology.IABP_AP = 0;
                    Instance.Physiology.IABP_MAP = Instance.Physiology.AMAP;
                }

                App.Current.Dispatcher.InvokeAsync (UpdateInterface);

                listNumerics.ForEach (n => App.Current.Dispatcher.InvokeAsync (n.UpdateVitals));
            }
        }

        public void OnTick_Vitals_Respiratory (object? sender, EventArgs e) {
        }

        public void OnPhysiologyEvent (object? sender, Physiology.PhysiologyEventArgs e) {
            switch (e.EventType) {
                default: break;
                case Physiology.PhysiologyEventTypes.Vitals_Change:
                    listTracings.ForEach (c => {
                        c.Strip?.ClearFuture (Instance?.Physiology);
                        c.Strip?.Add_Baseline (Instance?.Physiology);
                    });
                    break;

                case Physiology.PhysiologyEventTypes.Defibrillation:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Defibrillation (Instance?.Physiology));
                    break;

                case Physiology.PhysiologyEventTypes.Pacermaker_Spike:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Pacemaker (Instance?.Physiology));
                    break;

                case Physiology.PhysiologyEventTypes.Cardiac_Baseline:
                    if (Instance?.Physiology is not null) {
                        Instance.Physiology.IABP_Active = Running && (Frequency_Iter % Frequency == 0)
                            && ((Trigger.Value == Triggering.Values.ECG && Instance.Physiology.Cardiac_Rhythm.HasWaveform_Ventricular)
                            || (Trigger.Value == Triggering.Values.Pressure && Instance.Physiology.Cardiac_Rhythm.HasPulse_Ventricular));
                    }

                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Baseline (Instance?.Physiology));
                    break;

                case Physiology.PhysiologyEventTypes.Cardiac_Atrial_Electric:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Atrial_Electrical (Instance?.Physiology));
                    break;

                case Physiology.PhysiologyEventTypes.Cardiac_Ventricular_Electric:
                    if (Running)
                        Frequency_Iter++;

                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Ventricular_Electrical (Instance?.Physiology));
                    break;

                case Physiology.PhysiologyEventTypes.Cardiac_Atrial_Mechanical:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Atrial_Mechanical (Instance?.Physiology));
                    break;

                case Physiology.PhysiologyEventTypes.Cardiac_Ventricular_Mechanical:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Ventricular_Mechanical (Instance?.Physiology));
                    IterateAutoScale ();
                    break;

                case Physiology.PhysiologyEventTypes.IABP_Balloon_Inflation:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__IABP_Balloon (Instance?.Physiology));

                    if (Instance?.Physiology is not null && !Instance.Physiology.Cardiac_Rhythm.HasPulse_Ventricular)
                        IterateAutoScale ();
                    break;
            }
        }
    }
}