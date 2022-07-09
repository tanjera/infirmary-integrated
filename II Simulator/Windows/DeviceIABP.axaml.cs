using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;

using II;
using II.Rhythm;
using II.Waveform;

namespace IISIM {

    public partial class DeviceIABP : Window {

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

        public bool Paused { get; set; }

        private int autoScale_iter = Strip.DefaultAutoScale_Iterations;

        private Color.Schemes colorScheme = Color.Schemes.Dark;

        private List<Controls.IABPTracing> listTracings = new ();
        private List<Controls.IABPNumeric> listNumerics = new ();

        private Timer timerTracing = new ();
        private Timer timerVitals = new ();
        private Timer timerAncillary_Delay = new ();

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
#if DEBUG
            this.AttachDevTools ();
#endif
            DataContext = this;

            InitTimers ();
            InitInterface ();
        }

        ~DeviceIABP () => Dispose ();

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public void Dispose () {
            App.Timer_Main.Elapsed -= timerTracing.Process;
            App.Timer_Main.Elapsed -= timerVitals.Process;
            App.Timer_Main.Elapsed -= timerAncillary_Delay.Process;

            /* Dispose of local Timers */
            timerTracing.Dispose ();
            timerVitals.Dispose ();
            timerAncillary_Delay.Dispose ();

            /* Unsubscribe from the main Patient event listing */
            App.Patient.PatientEvent -= OnPatientEvent;
        }

        private void InitTimers () {
            App.Timer_Main.Elapsed += timerVitals.Process;
            App.Timer_Main.Elapsed += timerTracing.Process;
            App.Timer_Main.Elapsed += timerAncillary_Delay.Process;

            timerTracing.Set (Draw.RefreshTime);
            timerVitals.Set ((int)(App.Patient.GetHR_Seconds * 1000));

            timerTracing.Tick += OnTick_Tracing;
            timerVitals.Tick += OnTick_Vitals;

            timerTracing.Start ();
            timerVitals.Start ();
        }

        private void InitInterface () {
            // Populate UI strings per language selection
            this.FindControl<Window> ("wdwDeviceIABP").Title = App.Language.Localize ("IABP:WindowTitle");
            this.FindControl<MenuItem> ("menuDevice").Header = App.Language.Localize ("MENU:MenuDeviceOptions");
            this.FindControl<MenuItem> ("menuPauseDevice").Header = App.Language.Localize ("MENU:MenuPauseDevice");
            this.FindControl<MenuItem> ("menuCloseDevice").Header = App.Language.Localize ("MENU:MenuCloseDevice");
            this.FindControl<MenuItem> ("menuColor").Header = App.Language.Localize ("MENU:MenuColorScheme");
            this.FindControl<MenuItem> ("menuColorLight").Header = App.Language.Localize ("MENU:MenuColorSchemeLight");
            this.FindControl<MenuItem> ("menuColorDark").Header = App.Language.Localize ("MENU:MenuColorSchemeDark");

            this.FindControl<TextBlock> ("buttonModeAuto").Text = App.Language.Localize ("IABPMODE:Auto");
            this.FindControl<TextBlock> ("buttonModeSemiAuto").Text = App.Language.Localize ("IABPMODE:SemiAuto");
            this.FindControl<TextBlock> ("buttonZero").Text = Utility.WrapString (App.Language.Localize ("IABPBUTTON:ZeroPressure"));
            this.FindControl<TextBlock> ("buttonStart").Text = App.Language.Localize ("IABPBUTTON:Start");
            this.FindControl<TextBlock> ("buttonPause").Text = App.Language.Localize ("IABPBUTTON:Pause");
            this.FindControl<TextBlock> ("btntxtTrigger").Text = Utility.WrapString (App.Language.Localize ("IABPBUTTON:Trigger"));
            this.FindControl<TextBlock> ("btntxtFrequency").Text = Utility.WrapString (App.Language.Localize ("IABPBUTTON:Frequency"));
            this.FindControl<TextBlock> ("buttonPrimeBalloon").Text = Utility.WrapString (App.Language.Localize ("IABPBUTTON:PrimeBalloon"));
            this.FindControl<TextBlock> ("btntxtAugmentationPressure").Text = Utility.WrapString (App.Language.Localize ("IABP:AugmentationPressure"));
            this.FindControl<TextBlock> ("btntxtAugmentationAlarm").Text = Utility.WrapString (App.Language.Localize ("IABP:AugmentationAlarm"));
            this.FindControl<TextBlock> ("btntxtIncrease").Text = Utility.WrapString (App.Language.Localize ("IABPBUTTON:Increase"));
            this.FindControl<TextBlock> ("btntxtDecrease").Text = Utility.WrapString (App.Language.Localize ("IABPBUTTON:Decrease"));

            // Random helium tank remaining amount... it's for show!
            this.FindControl<TextBlock> ("lblHelium").Text = String.Format ("{0}: {1:0}%",
                Utility.WrapString (App.Language.Localize ("IABP:Helium")),
                II.Math.RandomDouble (20, 80));

            Grid displayGrid = this.FindControl<Grid> ("displayGrid");

            // Instantiate and add Tracings to UI
            listTracings.Add (new Controls.IABPTracing (new Strip (Lead.Values.ECG_II, 6f), colorScheme));
            listTracings.Add (new Controls.IABPTracing (new Strip (Lead.Values.ABP, 6f), colorScheme));
            listTracings.Add (new Controls.IABPTracing (new Strip (Lead.Values.IABP, 6f), colorScheme));
            for (int i = 0; i < listTracings.Count; i++) {
                listTracings [i].SetValue (Grid.RowProperty, i);
                listTracings [i].SetValue (Grid.ColumnProperty, 1);
                displayGrid.Children.Add (listTracings [i]);
            }

            // Instantiate and add Numerics to UI
            listNumerics.Add (new Controls.IABPNumeric (Controls.IABPNumeric.ControlType.Values.ECG, colorScheme));
            listNumerics.Add (new Controls.IABPNumeric (Controls.IABPNumeric.ControlType.Values.ABP, colorScheme));
            listNumerics.Add (new Controls.IABPNumeric (Controls.IABPNumeric.ControlType.Values.IABP_AP, colorScheme));
            for (int i = 0; i < listNumerics.Count; i++) {
                listNumerics [i].SetValue (Grid.RowProperty, i);
                listNumerics [i].SetValue (Grid.ColumnProperty, 2);
                displayGrid.Children.Add (listNumerics [i]);
            }

            UpdateInterface ();
        }

        private void UpdateInterface () {
            Dispatcher.UIThread.InvokeAsync (() => {
                for (int i = 0; i < listTracings.Count; i++)
                    listTracings [i].SetColorScheme (colorScheme);

                for (int i = 0; i < listNumerics.Count; i++)
                    listNumerics [i].SetColorScheme (colorScheme);

                Window window = this.FindControl<Window> ("wdwDeviceIABP");
                window.Background = Color.GetBackground (Color.Devices.DeviceIABP, colorScheme);

                Border brdStatusInfo = this.FindControl<Border> ("brdStatusInfo");

                TextBlock lblTriggerSource = this.FindControl<TextBlock> ("lblTriggerSource");
                TextBlock lblOperationMode = this.FindControl<TextBlock> ("lblOperationMode");
                TextBlock lblFrequency = this.FindControl<TextBlock> ("lblFrequency");
                TextBlock lblMachineStatus = this.FindControl<TextBlock> ("lblMachineStatus");
                TextBlock lblTubingStatus = this.FindControl<TextBlock> ("lblTubingStatus");
                TextBlock lblHelium = this.FindControl<TextBlock> ("lblHelium");

                Border brdTriggerSource = this.FindControl<Border> ("brdTriggerSource");
                Border brdOperationMode = this.FindControl<Border> ("brdOperationMode");
                Border brdFrequency = this.FindControl<Border> ("brdFrequency");
                Border brdMachineStatus = this.FindControl<Border> ("brdMachineStatus");
                Border brdTubingStatus = this.FindControl<Border> ("brdTubingStatus");
                Border brdHelium = this.FindControl<Border> ("brdHelium");

                brdStatusInfo.BorderBrush = colorScheme == Color.Schemes.Dark ? Brushes.SkyBlue : Brushes.Black;

                lblHelium.Foreground = colorScheme == Color.Schemes.Dark ? Brushes.MediumPurple : Brushes.Black;
                brdHelium.BorderBrush = colorScheme == Color.Schemes.Dark ? Brushes.MediumPurple : Brushes.Black;

                lblOperationMode.Foreground = colorScheme == Color.Schemes.Dark ? Brushes.Aqua : Brushes.Black;
                brdOperationMode.BorderBrush = colorScheme == Color.Schemes.Dark ? Brushes.Aqua : Brushes.Black;

                lblTriggerSource.Text = App.Language.Localize (Trigger.LookupString ());
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

                lblOperationMode.Text = App.Language.Localize (Mode.LookupString ());

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
                    lblMachineStatus.Text = App.Language.Localize ("IABP:Running");
                    lblMachineStatus.Foreground = colorScheme == Color.Schemes.Dark ? Brushes.LightGreen : Brushes.Black;
                    brdMachineStatus.BorderBrush = colorScheme == Color.Schemes.Dark ? Brushes.LightGreen : Brushes.Black;
                } else {
                    lblMachineStatus.Text = App.Language.Localize ("IABP:Paused");
                    lblMachineStatus.Foreground = colorScheme == Color.Schemes.Dark ? Brushes.Yellow : Brushes.Black;
                    brdMachineStatus.BorderBrush = colorScheme == Color.Schemes.Dark ? Brushes.Yellow : Brushes.Black;
                }

                if (Priming) {
                    lblTubingStatus.Text = App.Language.Localize ("IABP:Priming");
                    lblTubingStatus.Foreground = colorScheme == Color.Schemes.Dark ? Brushes.Yellow : Brushes.Black;
                    brdTubingStatus.BorderBrush = colorScheme == Color.Schemes.Dark ? Brushes.Yellow : Brushes.Black;
                } else if (Primed) {
                    lblTubingStatus.Text = App.Language.Localize ("IABP:Primed");
                    lblTubingStatus.Foreground = colorScheme == Color.Schemes.Dark ? Brushes.LightGreen : Brushes.Black;
                    brdTubingStatus.BorderBrush = colorScheme == Color.Schemes.Dark ? Brushes.LightGreen : Brushes.Black;
                } else {
                    lblTubingStatus.Text = App.Language.Localize ("IABP:NotPrimed");
                    lblTubingStatus.Foreground = colorScheme == Color.Schemes.Dark ? Brushes.OrangeRed : Brushes.Black;
                    brdTubingStatus.BorderBrush = colorScheme == Color.Schemes.Dark ? Brushes.OrangeRed : Brushes.Black;
                }
            });
        }

        public async Task Load_Process (string inc) {
            using StringReader sRead = new (inc);

            try {
                string? line;
                while ((line = await sRead.ReadLineAsync ()) != null) {
                    if (line.Contains (":")) {
                        string pName = line.Substring (0, line.IndexOf (':')),
                                pValue = line.Substring (line.IndexOf (':') + 1);
                        switch (pName) {
                            default: break;
                            case "isPaused": Paused = bool.Parse (pValue); break;

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

            sWrite.AppendLine (String.Format ("{0}:{1}", "isPaused", Paused));

            sWrite.AppendLine (String.Format ("{0}:{1}", "Frequency", Frequency));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Augmentation", Augmentation));
            sWrite.AppendLine (String.Format ("{0}:{1}", "AugmentationAlarm", AugmentationAlarm));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Trigger", Trigger.Value));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Mode", Mode.Value));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Running", Running));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Primed", Primed));

            return sWrite.ToString ();
        }

        public void SetColorScheme (Color.Schemes scheme) {
            colorScheme = scheme;
            UpdateInterface ();
        }

        private void TogglePause () {
            Paused = !Paused;

            if (!Paused)
                listTracings.ForEach (c => c.Strip.Unpause ()); ;
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
            Button buttonTrigger = this.FindControl<Button> ("buttonTrigger");
            Button buttonFrequency = this.FindControl<Button> ("buttonFrequency");
            Button buttonAugmentationPressure = this.FindControl<Button> ("buttonAugmentationPressure");
            Button buttonAugmentationAlarm = this.FindControl<Button> ("buttonAugmentationAlarm");

            buttonTrigger.Background = Brushes.PowderBlue;
            buttonFrequency.Background = Brushes.PowderBlue;
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

                case Settings.AugmentationPressure:
                    buttonAugmentationPressure.Background = Brushes.Yellow;
                    return;

                case Settings.AugmentationAlarm:
                    buttonAugmentationAlarm.Background = Brushes.Yellow;
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

        private void ButtonStart_Click (object s, RoutedEventArgs e)
            => StartDevice ();

        private void ButtonPause_Click (object s, RoutedEventArgs e)
            => PauseDevice ();

        private void ButtonTrigger_Click (object s, RoutedEventArgs e)
            => SelectSetting (Settings.Trigger);

        private void ButtonFrequency_Click (object s, RoutedEventArgs e)
            => SelectSetting (Settings.Frequency);

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

        private void MenuClose_Click (object s, RoutedEventArgs e)
            => this.Close ();

        private void MenuTogglePause_Click (object s, RoutedEventArgs e)
            => TogglePause ();

        private void MenuColorScheme_Light (object sender, RoutedEventArgs e)
            => SetColorScheme (Color.Schemes.Light);

        private void MenuColorScheme_Dark (object sender, RoutedEventArgs e)
            => SetColorScheme (Color.Schemes.Dark);

        private void OnClosed (object sender, EventArgs e)
            => this.Dispose ();

        public void OnClosing (object? sender, CancelEventArgs e) {
            if (sender is not null && sender == this) {
                this.Hide ();
                this.Paused = true;
                e.Cancel = true;
            }
        }

        private void OnTick_PrimingComplete (object? sender, EventArgs e) {
            timerAncillary_Delay.Stop ();
            timerAncillary_Delay.Unlock ();
            timerAncillary_Delay.Tick -= OnTick_PrimingComplete;

            Priming = false;
            Primed = true;

            if (Prime_ThenStart) {
                Prime_ThenStart = false;
                Running = true;
            }

            Dispatcher.UIThread.InvokeAsync (UpdateInterface);
        }

        private void OnTick_Tracing (object? sender, EventArgs e) {
            if (Paused)
                return;

            for (int i = 0; i < listTracings.Count; i++) {
                listTracings [i].Strip.Scroll ();
                Dispatcher.UIThread.InvokeAsync (listTracings [i].DrawTracing);
            }
        }

        private void OnTick_Vitals (object? sender, EventArgs e) {
            if (Paused)
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

            Dispatcher.UIThread.InvokeAsync (UpdateInterface);

            listNumerics.ForEach (n => Dispatcher.UIThread.InvokeAsync (n.UpdateVitals));
        }

        public void OnPatientEvent (object? sender, Patient.PatientEventArgs e) {
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
                            Dispatcher.UIThread.InvokeAsync (listTracings [i].UpdateScale);
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