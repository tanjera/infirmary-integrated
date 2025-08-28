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

namespace IISIM.Windows {

    /// <summary>
    /// Interaction logic for DeviceMonitor.xaml
    /// </summary>
    public partial class DeviceDefib : Window {
        public App? Instance { get; set; }

        public States State;

        /* Device settings */
        public Modes Mode = Modes.DEFIB;

        public ChargeStates Charge;
        public AnalyzeStates Analyze;

        public int DefibEnergy = 200,
                    PacerEnergy = 0,
                    PacerRate = 80;

        private int rowsTracings = 1;
        private int colsNumerics = 4;

        private int autoScale_iter = Strip.DefaultAutoScale_Iterations;

        private Color.Schemes colorScheme = Color.Schemes.Dark;

        private List<Controls.DefibTracing> listTracings = new ();
        private List<Controls.DefibNumeric> listNumerics = new ();

        public II.Timer
            TimerAlarm = new (),
            TimerTracing = new (),
            TimerNumerics_Cardiac = new (),
            TimerNumerics_Respiratory = new (),
            TimerAncillary_Delay = new ();

        /* Variables controlling for audio alarms */
        public SoundPlayer? AudioPlayer;

        /* Variables for audio tones (QRS or SPO2 beeps) and defibrillator charger */

        public MediaPlayer?
            TonePlayer = new MediaPlayer (),
            ChargePlayer = new MediaPlayer ();

        private string?
            ToneMedia,
            ChargeMedia;

        public enum States {
            Running,
            Paused,
            Closed
        }

        public enum AnalyzeStates {
            Inactive,
            Analyzing,
            Analyzed
        }

        public enum ChargeStates {
            Discharged,
            Charging,
            Charged
        }

        public enum Modes {
            DEFIB,
            SYNC,
            PACER
        };

        public DeviceDefib () {
            InitializeComponent ();
        }

        public DeviceDefib (App? app) {
            InitializeComponent ();

            Instance = app;

            Closed += this.OnClosed;
            Closing += this.OnClosing;

            InitTimers ();

            State = States.Running;

            InitInterface ();
            SetColorScheme (colorScheme);
        }

        ~DeviceDefib () {
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
            AudioPlayer?.Dispose ();
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

            wdwDeviceDefib.Title = Instance.Language.Localize ("DEFIB:WindowTitle");
            menuDevice.Header = Instance.Language.Localize ("MENU:MenuDeviceOptions");
            menuPauseDevice.Header = Instance.Language.Localize ("MENU:MenuPauseDevice");
            menuAddNumeric.Header = Instance.Language.Localize ("MENU:MenuAddNumeric");
            menuAddTracing.Header = Instance.Language.Localize ("MENU:MenuAddTracing");
            menuDefibEnergy.Header = Instance.Language.Localize ("DEFIB:DefibrillationEnergy");
            menuDefibMaximum.Header = Instance.Language.Localize ("DEFIB:Maximum");
            menuDefibMaximum_200.Header = $"200 {Instance.Language.Localize ("DEFIB:Joules")}";
            menuDefibMaximum_360.Header = $"360 {Instance.Language.Localize ("DEFIB:Joules")}";
            menuDefibIncrement.Header = Instance.Language.Localize ("DEFIB:Increment");
            menuDefibIncrement_10.Header = $"10 {Instance.Language.Localize ("DEFIB:Joules")}";
            menuDefibIncrement_20.Header = $"20 {Instance.Language.Localize ("DEFIB:Joules")}";
            menuToggleFullscreen.Header = Instance.Language.Localize ("MENU:MenuToggleFullscreen");
            menuCloseDevice.Header = Instance.Language.Localize ("MENU:MenuCloseDevice");

            menuAudio.Header = Instance.Language.Localize ("MENU:MenuAudio");
            menuAudioOff.Header = Instance.Language.Localize ("MENU:MenuAudioOff");
            menuAudioDefib.Header = Instance.Language.Localize ("MENU:MenuAudioDefib");
            menuAudioECG.Header = Instance.Language.Localize ("MENU:MenuAudioECG");
            menuAudioSPO2.Header = Instance.Language.Localize ("MENU:MenuAudioSPO2");
            menuAudioDisable.Header = Instance.Language.Localize ("MENU:MenuAudioDisable");
            menuAudioEnable.Header = Instance.Language.Localize ("MENU:MenuAudioEnable");

            menuColor.Header = Instance.Language.Localize ("MENU:MenuColorScheme");
            menuColorLight.Header = Instance.Language.Localize ("MENU:MenuColorSchemeLight");
            menuColorDark.Header = Instance.Language.Localize ("MENU:MenuColorSchemeDark");

            btntxtDefib.Text = Instance.Language.Localize ("DEFIB:Defibrillator");
            txtDefibEnergy.Text = Instance.Language.Localize ("DEFIB:Energy");
            btntxtDefibEnergyDecrease.Text = Instance.Language.Localize ("DEFIB:Decrease");
            btntxtDefibEnergyIncrease.Text = Instance.Language.Localize ("DEFIB:Increase");
            btntxtCharge.Text = Instance.Language.Localize ("DEFIB:Charge");
            btntxtShock.Text = Instance.Language.Localize ("DEFIB:Shock");
            btntxtAnalyze.Text = Instance.Language.Localize ("DEFIB:Analyze");
            btntxtSync.Text = Instance.Language.Localize ("DEFIB:Sync");

            btntxtPacer.Text = Instance.Language.Localize ("DEFIB:Pacer");
            txtPaceRate.Text = Instance.Language.Localize ("DEFIB:Rate");
            btntxtPaceRateDecrease.Text = Instance.Language.Localize ("DEFIB:Decrease");
            btntxtPaceRateIncrease.Text = Instance.Language.Localize ("DEFIB:Increase");
            txtPaceEnergy.Text = Instance.Language.Localize ("DEFIB:Energy");
            btntxtPaceEnergyDecrease.Text = Instance.Language.Localize ("DEFIB:Decrease");
            btntxtPaceEnergyIncrease.Text = Instance.Language.Localize ("DEFIB:Increase");
            btntxtPacePause.Text = Instance.Language.Localize ("DEFIB:Pause");

            /* Init Numeric & Tracing layout */

            OnLayoutChange (
                new (["DEFIB", "ECG", "NIBP", "SPO2", "ETCO2", "ABP"]),
                new (["ECG_II", "SPO2", "ETCO2", "ABP"]));

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

            // TODO: Implement remaining keybindings!!
        }

        private void UpdateInterface () {
            App.Current.Dispatcher.InvokeAsync ((Action)(() => {
                listNumerics
                    .Where<Controls.DefibNumeric> ((Func<Controls.DefibNumeric, bool>)(n => n.ControlType?.Value == Controls.DefibNumeric.ControlTypes.Values.DEFIB))
                    .ToList<Controls.DefibNumeric> ()
                    .ForEach (n => n.UpdateVitals ());

                for (int i = 0; i < listTracings.Count; i++)
                    listTracings [i].SetColorScheme (colorScheme);

                for (int i = 0; i < listNumerics.Count; i++)
                    listNumerics [i].SetColorScheme (colorScheme);

                wdwDeviceDefib.Background = Color.GetBackground (Color.Devices.DeviceDefib, colorScheme);
            }));
        }

        private void OnLayoutChange (List<string>? numericTypes = null, List<string>? tracingTypes = null) {
            // If numericTypes or tracingTypes are not null... then we are loading a file; clear lNumerics and lTracings!
            if (numericTypes != null)
                listNumerics.Clear ();
            if (tracingTypes != null)
                listTracings.Clear ();

            // Set default numeric types to populate
            if (numericTypes == null || numericTypes.Count == 0)
                numericTypes = new (new string [] { "DEFIB", "ECG", "NIBP", "SPO2", "ETCO2", "ABP" });
            else if (numericTypes.Count < colsNumerics) {
                List<string> buffer = new (new string [] { "DEFIB", "ECG", "NIBP", "SPO2", "ETCO2", "ABP" });
                buffer.RemoveRange (0, numericTypes.Count);
                numericTypes.AddRange (buffer);
            }

            for (int i = listNumerics.Count; i < colsNumerics && i < numericTypes.Count; i++) {
                Controls.DefibNumeric newNum;
                newNum = new Controls.DefibNumeric (
                    Instance, this,
                    (Controls.DefibNumeric.ControlTypes.Values)Enum.Parse (typeof (Controls.DefibNumeric.ControlTypes.Values), numericTypes [i]),
                    colorScheme);
                listNumerics.Add (newNum);
            }

            // Set default tracing types to populate
            if (tracingTypes == null || tracingTypes.Count == 0)
                tracingTypes = new (new string [] { "ECG_II", "SPO2", "ETCO2", "ABP" });
            else if (tracingTypes.Count < rowsTracings) {
                List<string> buffer = new (new string [] { "ECG_II", "SPO2", "ETCO2", "ABP" });
                buffer.RemoveRange (0, tracingTypes.Count);
                tracingTypes.AddRange (buffer);
            }

            for (int i = listTracings.Count; i < rowsTracings && i < tracingTypes.Count; i++) {
                Strip newStrip = new ((Lead.Values)Enum.Parse (typeof (Lead.Values), tracingTypes [i]), 6f);
                Controls.DefibTracing newTracing = new (Instance, newStrip, colorScheme);
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
                            case "rowsTracings": rowsTracings = int.Parse (pValue); break;
                            case "colsNumerics": colsNumerics = int.Parse (pValue); break;

                            case "numericTypes": numericTypes.AddRange (pValue.Split (',').Where ((o) => o != "")); break;
                            case "tracingTypes": tracingTypes.AddRange (pValue.Split (',').Where ((o) => o != "")); break;
                            case "Mode": Mode = (Modes)Enum.Parse (typeof (Modes), pValue); break;
                            case "Charge": Charge = (ChargeStates)Enum.Parse (typeof (ChargeStates), pValue); break;
                            case "Analyze": Analyze = (AnalyzeStates)Enum.Parse (typeof (AnalyzeStates), pValue); break;
                            case "DefibEnergy": DefibEnergy = int.Parse (pValue); break;
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
            StringBuilder sWrite = new ();

            sWrite.AppendLine (String.Format ("{0}:{1}", "rowsTracings", rowsTracings));
            sWrite.AppendLine (String.Format ("{0}:{1}", "colsNumerics", colsNumerics));

            List<string> numericTypes = new (),
                         tracingTypes = new ();

            listNumerics.ForEach ((Action<Controls.DefibNumeric>)(o => {
                if (o?.ControlType?.Value is not null)
                    numericTypes.Add ((string)o.ControlType.Value.ToString ());
            }));
            listTracings.ForEach (o => { tracingTypes.Add (o.Strip?.Lead?.Value.ToString () ?? ""); });
            sWrite.AppendLine (String.Format ("{0}:{1}", "numericTypes", string.Join (",", numericTypes)));
            sWrite.AppendLine (String.Format ("{0}:{1}", "tracingTypes", string.Join (",", tracingTypes)));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Mode", Mode));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Charge", Charge));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Analyze", Analyze));
            sWrite.AppendLine (String.Format ("{0}:{1}", "DefibEnergy", DefibEnergy));
            sWrite.AppendLine (String.Format ("{0}:{1}", "PacerEnergy", PacerEnergy));
            sWrite.AppendLine (String.Format ("{0}:{1}", "PacerRate", PacerRate));

            return sWrite.ToString ();
        }

        public Task SetDefibEnergyMaximum (int joules) {
            if (Instance?.Settings is null)
                return Task.CompletedTask;

            Instance.Settings.DefibEnergyMaximum = joules;
            Instance.Settings.Save ();

            DefibEnergy = II.Math.Clamp (DefibEnergy, 0, Instance?.Settings?.DefibEnergyMaximum ?? joules);
            UpdateInterface ();

            return Task.CompletedTask;
        }

        public Task SetDefibEnergyIncrement (int joules) {
            if (Instance?.Settings is null)
                return Task.CompletedTask;

            Instance.Settings.DefibEnergyIncrement = joules;
            Instance.Settings.Save ();

            return Task.CompletedTask;
        }

        public async Task SetChargeState (ChargeStates charge) {
            Charge = charge;
            await PlayAudioCharge ();
        }

        public async Task PlayAudioCharge () {
            if (ChargePlayer is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (PlayAudioCharge)}");
                return;
            }

            if (!(Instance?.Settings?.AudioEnabled ?? false) || ((Instance?.Settings?.DefibAudioSource ?? Simulator.ToneSources.Mute) == Simulator.ToneSources.Mute)) {
                ChargePlayer.Stop ();
                await ReleaseAudioCharge ();
            } else {
                switch (Charge) {
                    default:
                        ChargePlayer.Stop ();
                        break;

                    case ChargeStates.Charging:
                        ChargePlayer.Stop ();
                        await ReleaseAudioCharge ();
                        // TODO: Open correct audio Uri from Resources/Audio
                        ChargePlayer.Open (new Uri (af_charging));
                        ChargePlayer.Play ();

                        break;

                    case ChargeStates.Charged:
                        ChargePlayer.Stop ();
                        await ReleaseAudioCharge ();
                        // TODO: Open correct audio Uri from Resources/Audio
                        ChargePlayer.Open (new Uri (af_charged));
                        ChargePlayer.Play ();

                        break;
                }
            }
        }

        public void SetAudio_On () => _ = Instance?.Window_Control?.SetAudio (true);

        public void SetAudio_Off () => _ = Instance?.Window_Control?.SetAudio (false);

        public void SetAudioTone_Off () => _ = SetAudioTone (II.Settings.Simulator.ToneSources.Mute);

        public void SetAudioTone_Defib () => _ = SetAudioTone (II.Settings.Simulator.ToneSources.Defibrillator);

        public void SetAudioTone_ECG () => _ = SetAudioTone (II.Settings.Simulator.ToneSources.ECG);

        public void SetAudioTone_SPO2 () => _ = SetAudioTone (II.Settings.Simulator.ToneSources.SPO2);

        public async Task SetAudioTone (II.Settings.Simulator.ToneSources source) {
            if (Instance?.Settings is null)
                return;

            Instance.Settings.DefibAudioSource = source;
            Instance.Settings.Save ();

            if (TonePlayer is not null) {
                switch (Instance.Settings.DefibAudioSource) {
                    default:
                    case Simulator.ToneSources.SPO2:
                        await ReleaseAudioTone ();

                        break;

                    case Simulator.ToneSources.ECG:
                        await ReleaseAudioTone ();
                        // TODO: Open correct audio Uri from Resources/Audio
                        TonePlayer.Open (new Uri (af_ecg));
                        break;
                }
            }

            return;
        }

        public async Task PlayAudioTone (Simulator.ToneSources trigger, Physiology? p) {
            if (Instance is null || TonePlayer is null) {
                App.Current.Dispatcher.Invoke (() => Debug.WriteLine ($"Null return at {this.Name}.{nameof (PlayAudioTone)}"));
                return;
            }

            if (Instance.Settings.DefibAudioSource == trigger && (Instance?.Settings.AudioEnabled ?? false)) {
                switch (Instance.Settings.DefibAudioSource) {
                    default: break;

                    case Simulator.ToneSources.ECG:           // Plays a fixed tone each QRS complex
                        App.Current.Dispatcher.Invoke (() => {
                            TonePlayer.Stop ();
                            TonePlayer.Play ();
                        });
                        break;

                    case Simulator.ToneSources.SPO2:          // Plays a variable tone depending on SpO2
                        await App.Current.Dispatcher.InvokeAsync (async () => {
                            TonePlayer.Stop ();
                            await ReleaseAudioTone ();
                            // TODO: Open correct audio Uri from Resources/Audio
                            TonePlayer.Open (new Uri (af_spo2));
                            TonePlayer.Play ();
                        });
                        break;
                }
            }
        }

        private Task ReleaseAudioCharge () {
            ChargePlayer?.Stop ();
            ChargePlayer?.Close ();

            return Task.CompletedTask;
        }

        private Task ReleaseAudioTone () {
            TonePlayer?.Stop ();
            TonePlayer?.Close ();

            if (!String.IsNullOrEmpty (ToneMedia)) {
                ToneMedia = null;
            }

            return Task.CompletedTask;
        }

        public void SetColorScheme_Light () => SetColorScheme (Color.Schemes.Light);

        public void SetColorScheme_Dark () => SetColorScheme (Color.Schemes.Dark);

        public void SetColorScheme (Color.Schemes scheme) {
            colorScheme = scheme;
            UpdateInterface ();
        }

        private void OnTick_AnalyzingComplete (object? sender, EventArgs e) {
            // TODO: Implement

            UpdateInterface ();
        }

        private void OnTick_ChargingComplete (object? sender, EventArgs e) {
            // TODO: Implement

            UpdateInterface ();
        }

        public void TogglePause () {
            if (State == States.Running)
                State = States.Paused;
            else if (State == States.Paused)
                State = States.Running;
        }

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

        public void ToggleFullscreen () {
            if (wdwDeviceDefib.WindowState == System.Windows.WindowState.Maximized)
                wdwDeviceDefib.WindowState = System.Windows.WindowState.Normal;
            else
                wdwDeviceDefib.WindowState = System.Windows.WindowState.Maximized;
        }

        public void SetTracing_1 () => SetTracing (1);

        public void SetTracing_2 () => SetTracing (2);

        public void SetTracing_3 () => SetTracing (3);

        public void SetTracing_4 () => SetTracing (4);

        public void SetTracing (int amount) {
            // TODO: Implement
        }

        public void AddTracing () {
            // TODO: Implement
        }

        public void RemoveTracing (Controls.DefibTracing requestSender) {
            // TODO: Implement
        }

        public void SetNumeric_1 () => SetNumeric (1);

        public void SetNumeric_2 () => SetNumeric (2);

        public void SetNumeric_3 () => SetNumeric (3);

        public void SetNumeric_4 () => SetNumeric (4);

        public void SetNumeric_5 () => SetNumeric (5);

        public void SetNumeric_6 () => SetNumeric (6);

        public void SetNumeric (int amount) {
            // TODO: Implement
        }

        public void AddNumeric () {
            // TODO: Implement
        }

        public void RemoveNumeric (Controls.DefibNumeric requestSender) {
            // TODO: Implement
        }

        private void UpdatePacemaker () {
            // TODO: Implement
        }

        private void IterateAutoScale () {
            // TODO: Implement
        }

        private void ButtonDefib_Click (object s, RoutedEventArgs e) {
            // TODO: Implement
        }

        private void ButtonDefibEnergyDecrease_Click (object s, RoutedEventArgs e) {
            // TODO: Implement
        }

        private void ButtonDefibEnergyIncrease_Click (object s, RoutedEventArgs e) {
            // TODO: Implement
        }

        private void ButtonCharge_Click (object s, RoutedEventArgs e) {
            // TODO: Implement
        }

        private void ButtonShock_Click (object s, RoutedEventArgs e) {
            // TODO: Implement
        }

        private void ButtonAnalyze_Click (object s, RoutedEventArgs e) {
            // TODO: Implement
        }

        private void ButtonSync_Click (object s, RoutedEventArgs e) {
            // TODO: Implement
        }

        private void ButtonPacer_Click (object s, RoutedEventArgs e) {
            // TODO: Implement
        }

        private void ButtonPaceRateDecrease_Click (object s, RoutedEventArgs e) {
            // TODO: Implement
        }

        private void ButtonPaceRateIncrease_Click (object s, RoutedEventArgs e) {
            // TODO: Implement
        }

        private void ButtonPaceEnergyDecrease_Click (object s, RoutedEventArgs e) {
            // TODO: Implement
        }

        private void ButtonPaceEnergyIncrease_Click (object s, RoutedEventArgs e) {
            // TODO: Implement
        }

        private void ButtonPacePause_Click (object s, RoutedEventArgs e)
            => _ = Instance?.Physiology?.PacemakerPause ();

        private void MenuToggleFullscreen_Click (object s, RoutedEventArgs e)
            => ToggleFullscreen ();

        private void MenuClose_Click (object s, RoutedEventArgs e) {
            wdwDeviceDefib.Close ();
        }

        private void MenuAddNumeric_Click (object s, RoutedEventArgs e)
    => AddNumeric ();

        private void MenuAddTracing_Click (object s, RoutedEventArgs e)
            => AddTracing ();

        private void MenuTogglePause_Click (object s, RoutedEventArgs e)
            => TogglePause ();

        private void MenuDefibEnergyMaximum_200 (object s, RoutedEventArgs e)
            => _ = SetDefibEnergyMaximum (200);

        private void MenuDefibEnergyMaximum_360 (object s, RoutedEventArgs e)
            => _ = SetDefibEnergyMaximum (360);

        private void MenuDefibEnergyIncrement_10 (object s, RoutedEventArgs e)
            => _ = SetDefibEnergyIncrement (10);

        private void MenuDefibEnergyIncrement_20 (object s, RoutedEventArgs e)
             => _ = SetDefibEnergyIncrement (20);

        private void MenuEnableAudio (object sender, RoutedEventArgs e)
            => SetAudio_On ();

        private void MenuDisableAudio (object sender, RoutedEventArgs e)
            => SetAudio_Off ();

        private void MenuAudioOff (object sender, RoutedEventArgs e)
            => _ = SetAudioTone (Simulator.ToneSources.Mute);

        private void MenuAudioDefib (object sender, RoutedEventArgs e)
            => _ = SetAudioTone (Simulator.ToneSources.Defibrillator);

        private void MenuAudioECG (object sender, RoutedEventArgs e)
            => _ = SetAudioTone (Simulator.ToneSources.ECG);

        private void MenuAudioSPO2 (object sender, RoutedEventArgs e)
            => _ = SetAudioTone (Simulator.ToneSources.SPO2);

        private void MenuColorGrid_Click (object sender, RoutedEventArgs e)
            => SetColorScheme (Color.Schemes.Grid);

        private void MenuColorScheme_Light (object sender, RoutedEventArgs e)
            => SetColorScheme (Color.Schemes.Light);

        private void MenuColorScheme_Dark (object sender, RoutedEventArgs e)
            => SetColorScheme (Color.Schemes.Dark);

        public void OnTick_Alarm (object? sender, EventArgs e) {
        }

        public void OnTick_Tracing (object? sender, EventArgs e) {
            if (State != States.Running)
                return;

            for (int i = 0; i < listTracings.Count; i++) {
                listTracings [i].Strip?.Scroll ();
                App.Current.Dispatcher.InvokeAsync (listTracings [i].DrawTracing);
            }
        }

        public void OnTick_Vitals_Cardiac (object? sender, EventArgs e) {
            if (State != States.Running)
                return;

            listNumerics
                .Where (n
                    => n.ControlType?.Value != Controls.DefibNumeric.ControlTypes.Values.ETCO2
                    && n.ControlType?.Value != Controls.DefibNumeric.ControlTypes.Values.RR)
                .ToList ()
                .ForEach (n => App.Current.Dispatcher.InvokeAsync (n.UpdateVitals));
        }

        public void OnTick_Vitals_Respiratory (object? sender, EventArgs e) {
            if (State != States.Running)
                return;

            listNumerics
                .Where (n
                    => n.ControlType?.Value == Controls.DefibNumeric.ControlTypes.Values.ETCO2
                    || n.ControlType?.Value == Controls.DefibNumeric.ControlTypes.Values.RR)
                .ToList ()
                .ForEach (n => App.Current.Dispatcher.InvokeAsync (n.UpdateVitals));
        }

        public void OnPhysiologyEvent (object? sender, Physiology.PhysiologyEventArgs e) {
            switch (e.EventType) {
                default: break;

                case Physiology.PhysiologyEventTypes.Vitals_Change:
                    listTracings.ForEach (c => {
                        c.Strip?.ClearFuture (Instance?.Physiology);
                        c.Strip?.Add_Baseline (Instance?.Physiology);
                    });

                    listNumerics.ForEach ((n) => App.Current.Dispatcher.InvokeAsync (n.UpdateVitals));
                    break;

                case Physiology.PhysiologyEventTypes.Defibrillation:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Defibrillation (Instance?.Physiology));
                    break;

                case Physiology.PhysiologyEventTypes.Pacermaker_Spike:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Pacemaker (Instance?.Physiology));
                    break;

                case Physiology.PhysiologyEventTypes.Cardiac_Baseline:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Baseline (Instance?.Physiology));
                    break;

                case Physiology.PhysiologyEventTypes.Cardiac_Atrial_Electric:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Atrial_Electrical (Instance?.Physiology));
                    break;

                case Physiology.PhysiologyEventTypes.Cardiac_Ventricular_Electric:
                    // QRS audio tone is only triggered by rhythms w/ a ventricular electrical (QRS complex) action
                    _ = PlayAudioTone (Simulator.ToneSources.ECG, e.Physiology);

                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Ventricular_Electrical (Instance?.Physiology));
                    break;

                case Physiology.PhysiologyEventTypes.Cardiac_Atrial_Mechanical:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Atrial_Mechanical (Instance?.Physiology));
                    break;

                case Physiology.PhysiologyEventTypes.Cardiac_Ventricular_Mechanical:
                    // SPO2 audio tone is only triggered  by rhythms w/ a ventricular mechanical action (systole)
                    _ = PlayAudioTone (Simulator.ToneSources.SPO2, e.Physiology);

                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Ventricular_Mechanical (Instance?.Physiology));
                    IterateAutoScale ();
                    break;

                case Physiology.PhysiologyEventTypes.IABP_Balloon_Inflation:
                    // Note: May draw either IABP or ABP waveforms (e.g. ABP with augmentation in non-pulsatile rhythm cases)
                    listTracings.ForEach (c => c.Strip?.Add_Beat__IABP_Balloon (Instance?.Physiology));

                    if (Instance?.Physiology is not null && !Instance.Physiology.Cardiac_Rhythm.HasPulse_Ventricular)
                        IterateAutoScale ();
                    break;

                case Physiology.PhysiologyEventTypes.Respiratory_Baseline:
                    listTracings.ForEach (c => c.Strip?.Add_Breath__Respiratory_Baseline (Instance?.Physiology));
                    break;

                case Physiology.PhysiologyEventTypes.Respiratory_Inspiration:
                    listTracings.ForEach (c => c.Strip?.Add_Breath__Respiratory_Inspiration (Instance?.Physiology));
                    break;

                case Physiology.PhysiologyEventTypes.Respiratory_Expiration:
                    listTracings.ForEach (c => c.Strip?.Add_Breath__Respiratory_Expiration (Instance?.Physiology));
                    break;
            }
        }
    }
}