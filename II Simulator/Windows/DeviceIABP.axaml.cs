using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

using LibVLCSharp.Shared;

namespace IISIM {

    public partial class DeviceIABP : DeviceWindow {

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
        }

        public DeviceIABP (App? app) : base (app) {
            InitializeComponent ();
#if DEBUG
            this.AttachDevTools ();
#endif
            DataContext = this;
            InitInterface ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public override void InitAudio () {
            if (Instance?.AudioLib is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitAudio)}");
                return;
            }

            base.InitAudio ();

            var assets = AvaloniaLocator.Current.GetService<Avalonia.Platform.IAssetLoader> ();
            if (AudioPlayer is not null)
                AudioPlayer.Media = new Media (Instance.AudioLib, new StreamMediaInput (assets.Open (new Uri ("avares://Infirmary Integrated/Resources/Alarm_IABP_Augmentation.wav"))));
        }

        private void InitInterface () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitInterface)}");
                return;
            }

            // Populate UI strings per language selection

            this.FindControl<Window> ("wdwDeviceIABP").Title = Instance.Language.Localize ("IABP:WindowTitle");
            this.FindControl<MenuItem> ("menuDevice").Header = Instance.Language.Localize ("MENU:MenuDeviceOptions");
            this.FindControl<MenuItem> ("menuPauseDevice").Header = Instance.Language.Localize ("MENU:MenuPauseDevice");
            this.FindControl<MenuItem> ("menuCloseDevice").Header = Instance.Language.Localize ("MENU:MenuCloseDevice");
            this.FindControl<MenuItem> ("menuColor").Header = Instance.Language.Localize ("MENU:MenuColorScheme");
            this.FindControl<MenuItem> ("menuColorLight").Header = Instance.Language.Localize ("MENU:MenuColorSchemeLight");
            this.FindControl<MenuItem> ("menuColorDark").Header = Instance.Language.Localize ("MENU:MenuColorSchemeDark");

            this.FindControl<TextBlock> ("buttonModeAuto").Text = Instance.Language.Localize ("IABPMODE:Auto");
            this.FindControl<TextBlock> ("buttonModeSemiAuto").Text = Instance.Language.Localize ("IABPMODE:SemiAuto");
            this.FindControl<TextBlock> ("buttonZero").Text = Utility.WrapString (Instance.Language.Localize ("IABPBUTTON:ZeroPressure"));
            this.FindControl<TextBlock> ("buttonStart").Text = Instance.Language.Localize ("IABPBUTTON:Start");
            this.FindControl<TextBlock> ("buttonPause").Text = Instance.Language.Localize ("IABPBUTTON:Pause");
            this.FindControl<TextBlock> ("btntxtTrigger").Text = Utility.WrapString (Instance.Language.Localize ("IABPBUTTON:Trigger"));
            this.FindControl<TextBlock> ("btntxtFrequency").Text = Utility.WrapString (Instance.Language.Localize ("IABPBUTTON:Frequency"));
            this.FindControl<TextBlock> ("buttonPrimeBalloon").Text = Utility.WrapString (Instance.Language.Localize ("IABPBUTTON:PrimeBalloon"));
            this.FindControl<TextBlock> ("btntxtAugmentationPressure").Text = Utility.WrapString (Instance.Language.Localize ("IABP:AugmentationPressure"));
            this.FindControl<TextBlock> ("btntxtAugmentationAlarm").Text = Utility.WrapString (Instance.Language.Localize ("IABP:AugmentationAlarm"));
            this.FindControl<TextBlock> ("btntxtIncrease").Text = Utility.WrapString (Instance.Language.Localize ("IABPBUTTON:Increase"));
            this.FindControl<TextBlock> ("btntxtDecrease").Text = Utility.WrapString (Instance.Language.Localize ("IABPBUTTON:Decrease"));

            // Random helium tank remaining amount... it's for show!
            this.FindControl<TextBlock> ("lblHelium").Text = String.Format ("{0}: {1:0}%",
                Utility.WrapString (Instance.Language.Localize ("IABP:Helium")),
                II.Math.RandomInt (20, 80));

            Grid displayGrid = this.FindControl<Grid> ("displayGrid");

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
            });
        }

        public async Task Load (string inc) {
            using StringReader sRead = new (inc);

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

        public void SetColorScheme (Color.Schemes scheme) {
            colorScheme = scheme;
            UpdateInterface ();
        }

        public override void TogglePause () {
            base.TogglePause ();

            if (State == States.Running)
                listTracings.ForEach (c => c.Strip?.Unpause ()); ;
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

        private void IterateAutoScale () {
            /* Iterations and trigger for auto-scaling pressure waveform strips */
            autoScale_iter -= 1;

            if (autoScale_iter <= 0) {
                for (int i = 0; i < listTracings.Count; i++) {
                    listTracings [i].Strip?.SetAutoScale (Instance?.Physiology);
                    Dispatcher.UIThread.InvokeAsync (listTracings [i].UpdateScale);
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
                    PauseDevice ();
                    UpdateInterface ();
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
                    PauseDevice ();
                    UpdateInterface ();
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

        public override void OnClosing (object? sender, CancelEventArgs e) {
            base.OnClosing (sender, e);

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

            Dispatcher.UIThread.InvokeAsync (UpdateInterface);
        }

        public override void OnTick_Alarm (object? sender, EventArgs e) {
            if (AudioPlayer is null || Instance?.AudioLib is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (OnTick_Alarm)}");
                return;
            }

            if (Instance?.Settings.AudioEnabled == false) {
                AudioPlayer.Stop ();
                return;
            }

            if (Running && Instance?.Physiology?.IABP_AP < AugmentationAlarm) {
                if (!AudioPlayer.IsPlaying) {
                    AudioPlayer.Play ();
                }
            } else {
                AudioPlayer.Stop ();
                return;
            }
        }

        public override void OnTick_Tracing (object? sender, EventArgs e) {
            if (State != States.Running)
                return;

            for (int i = 0; i < listTracings.Count; i++) {
                listTracings [i].Strip?.Scroll ();
                Dispatcher.UIThread.InvokeAsync (listTracings [i].DrawTracing);
            }
        }

        public override void OnTick_Vitals_Cardiac (object? sender, EventArgs e) {
            if (State != States.Running || Instance?.Physiology is null)
                return;

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

            Dispatcher.UIThread.InvokeAsync (UpdateInterface);

            listNumerics.ForEach (n => Dispatcher.UIThread.InvokeAsync (n.UpdateVitals));
        }

        public override void OnPhysiologyEvent (object? sender, Physiology.PhysiologyEventArgs e) {
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
                        Instance.Physiology.IABP_Trigger = Trigger.Value.ToString ();
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