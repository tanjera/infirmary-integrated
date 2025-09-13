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
    public partial class DeviceMonitor : Window {
        public App? Instance { get; set; }

        public States State;

        private int rowsTracings = 3;
        private int rowsNumerics = 3;

        private int autoScale_iter = Strip.DefaultAutoScale_Iterations;

        private Color.Schemes colorScheme = Color.Schemes.Dark;

        private List<Controls.MonitorTracing> listTracings = new ();
        private List<Controls.MonitorNumeric> listNumerics = new ();

        public II.Timer
            TimerAlarm = new (),
            TimerTracing = new (),
            TimerNumerics_Cardiac = new (),
            TimerNumerics_Respiratory = new (),
            TimerAncillary_Delay = new ();

        /* Variables for audio tones (QRS or SPO2 beeps) */
        public SoundPlayer? MediaPlayer_Tone = new SoundPlayer ();
        private MemoryStream? MediaStream_Tone;

        /* Variables controlling for audio alarms */
        public SoundPlayer? MediaPlayer_Alarm;
        private Alarm.Priorities? AlarmActive;
        private List<Alarm> AlarmsActive = new ();
        private List<Stream>? AlarmMedia;

        public enum States {
            Running,
            Paused,
            Closed
        }

        public DeviceMonitor () {
            InitializeComponent ();
        }

        public DeviceMonitor (App? app) {
            InitializeComponent ();

            Instance = app;

            Closed += this.OnClosed;
            Closing += this.OnClosing;

            InitAlarms ();
            InitTimers ();

            State = States.Running;

            InitInterface ();
            SetColorScheme (colorScheme);
        }

        ~DeviceMonitor () {
            Dispose ();
        }

        public virtual void Dispose () {
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

        public virtual void DisposeAudio () {
            MediaPlayer_Tone?.Dispose ();
            MediaPlayer_Alarm?.Dispose ();
        }

        public virtual void InitAlarms () {
            AlarmMedia = [
                Application.GetResourceStream(new Uri ("pack://application:,,,/Resources/Alarm_Low_Priority.wav")).Stream,
                Application.GetResourceStream(new Uri ("pack://application:,,,/Resources/Alarm_Medium_Priority.wav")).Stream,
                Application.GetResourceStream(new Uri ("pack://application:,,,/Resources/Alarm_High_Priority.wav")).Stream,
            ];
        }

        public virtual void InitTimers () {
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

            wdwDeviceMonitor.Title = Instance.Language.Localize ("CM:WindowTitle");
            menuDevice.Header = Instance.Language.Localize ("MENU:MenuDeviceOptions");
            menuPauseDevice.Header = Instance.Language.Localize ("MENU:MenuPauseDevice");
            menuAddNumeric.Header = Instance.Language.Localize ("MENU:MenuAddNumeric");
            menuAddTracing.Header = Instance.Language.Localize ("MENU:MenuAddTracing");
            menuToggleFullscreen.Header = Instance.Language.Localize ("MENU:MenuToggleFullscreen");
            menuCloseDevice.Header = Instance.Language.Localize ("MENU:MenuCloseDevice");

            menuAlarms.Header = Instance.Language.Localize ("MENU:MenuAlarms");
            menuAlarmsDisable.Header = Instance.Language.Localize ("MENU:MenuAlarmsDisable");
            menuAlarmsEnable.Header = Instance.Language.Localize ("MENU:MenuAlarmsEnable");

            menuAudio.Header = Instance.Language.Localize ("MENU:MenuAudio");
            menuAudioOff.Header = Instance.Language.Localize ("MENU:MenuAudioOff");
            menuAudioECG.Header = Instance.Language.Localize ("MENU:MenuAudioECG");
            menuAudioSPO2.Header = Instance.Language.Localize ("MENU:MenuAudioSPO2");
            menuAudioDisable.Header = Instance.Language.Localize ("MENU:MenuAudioDisable");
            menuAudioEnable.Header = Instance.Language.Localize ("MENU:MenuAudioEnable");

            menuColor.Header = Instance.Language.Localize ("MENU:MenuColorScheme");
            menuColorLight.Header = Instance.Language.Localize ("MENU:MenuColorSchemeLight");
            menuColorDark.Header = Instance.Language.Localize ("MENU:MenuColorSchemeDark");

            /* Init Numeric & Tracing layout */

            OnLayoutChange (
                new (["ECG", "NIBP", "SPO2", "ETCO2", "ABP"]),
                new (["ECG_II", "SPO2", "ETCO2", "ABP"]));

            /* Init Hotkeys (Commands & InputBinding) */

            RoutedCommand
                cmdMenuTogglePause_Click = new (),
                cmdMenuToggleFullscreen_Click = new (),
                cmdMenuColorScheme_Light = new (),
                cmdMenuColorScheme_Dark = new (),
                cmdMenuAddNumeric = new (),
                cmdMenuAddTracing = new (),
                cmdMenuDisableAlarms = new (),
                cmdMenuEnableAlarms = new (),
                cmdMenuAudioTone_Off = new (),
                cmdMenuAudioTone_ECG = new (),
                cmdMenuAudioTone_SPO2 = new (),
                cmdMenuAudio_Disable = new (),
                cmdMenuAudio_Enable = new (),
                cmdSetNumeric_1 = new (),
                cmdSetNumeric_2 = new (),
                cmdSetNumeric_3 = new (),
                cmdSetNumeric_4 = new (),
                cmdSetNumeric_5 = new (),
                cmdSetNumeric_6 = new (),
                cmdSetNumeric_7 = new (),
                cmdSetNumeric_8 = new (),
                cmdSetTracing_1 = new (),
                cmdSetTracing_2 = new (),
                cmdSetTracing_3 = new (),
                cmdSetTracing_4 = new (),
                cmdSetTracing_5 = new (),
                cmdSetTracing_6 = new (),
                cmdSetTracing_7 = new (),
                cmdSetTracing_8 = new ();

            cmdMenuTogglePause_Click.InputGestures.Add (new KeyGesture (Key.Pause));
            CommandBindings.Add (new CommandBinding (cmdMenuTogglePause_Click, MenuTogglePause_Click));

            cmdMenuToggleFullscreen_Click.InputGestures.Add (new KeyGesture (Key.Enter, ModifierKeys.Alt));
            CommandBindings.Add (new CommandBinding (cmdMenuToggleFullscreen_Click, MenuToggleFullscreen_Click));

            cmdMenuColorScheme_Light.InputGestures.Add (new KeyGesture (Key.F1));
            CommandBindings.Add (new CommandBinding (cmdMenuColorScheme_Light, MenuColorScheme_Light));

            cmdMenuColorScheme_Dark.InputGestures.Add (new KeyGesture (Key.F2));
            CommandBindings.Add (new CommandBinding (cmdMenuColorScheme_Dark, MenuColorScheme_Dark));

            cmdMenuAddNumeric.InputGestures.Add (new KeyGesture (Key.N, ModifierKeys.Control));
            CommandBindings.Add (new CommandBinding (cmdMenuAddNumeric, MenuAddNumeric_Click));

            cmdMenuAddTracing.InputGestures.Add (new KeyGesture (Key.T, ModifierKeys.Control));
            CommandBindings.Add (new CommandBinding (cmdMenuAddTracing, MenuAddTracing_Click));

            cmdMenuDisableAlarms.InputGestures.Add (new KeyGesture (Key.OemMinus));
            CommandBindings.Add (new CommandBinding (cmdMenuDisableAlarms, MenuDisableAlarms));

            cmdMenuEnableAlarms.InputGestures.Add (new KeyGesture (Key.OemPlus));
            CommandBindings.Add (new CommandBinding (cmdMenuEnableAlarms, MenuEnableAlarms));

            cmdMenuAudioTone_Off.InputGestures.Add (new KeyGesture (Key.P, ModifierKeys.Control));
            CommandBindings.Add (new CommandBinding (cmdMenuAudioTone_Off, MenuAudioOff));

            cmdMenuAudioTone_ECG.InputGestures.Add (new KeyGesture (Key.OemOpenBrackets, ModifierKeys.Control));
            CommandBindings.Add (new CommandBinding (cmdMenuAudioTone_ECG, MenuAudioECG));

            cmdMenuAudioTone_SPO2.InputGestures.Add (new KeyGesture (Key.OemCloseBrackets, ModifierKeys.Control));
            CommandBindings.Add (new CommandBinding (cmdMenuAudioTone_SPO2, MenuAudioSPO2));

            cmdMenuAudio_Disable.InputGestures.Add (new KeyGesture (Key.OemMinus, ModifierKeys.Control));
            CommandBindings.Add (new CommandBinding (cmdMenuAudio_Disable, MenuDisableAudio));

            cmdMenuAudio_Enable.InputGestures.Add (new KeyGesture (Key.OemPlus, ModifierKeys.Control));
            CommandBindings.Add (new CommandBinding (cmdMenuAudio_Enable, MenuEnableAudio));

            cmdSetNumeric_1.InputGestures.Add (new KeyGesture (Key.D1, ModifierKeys.Control));
            CommandBindings.Add (new CommandBinding (cmdSetNumeric_1, SetNumeric_1));

            cmdSetNumeric_2.InputGestures.Add (new KeyGesture (Key.D2, ModifierKeys.Control));
            CommandBindings.Add (new CommandBinding (cmdSetNumeric_2, SetNumeric_2));

            cmdSetNumeric_3.InputGestures.Add (new KeyGesture (Key.D3, ModifierKeys.Control));
            CommandBindings.Add (new CommandBinding (cmdSetNumeric_3, SetNumeric_3));

            cmdSetNumeric_4.InputGestures.Add (new KeyGesture (Key.D4, ModifierKeys.Control));
            CommandBindings.Add (new CommandBinding (cmdSetNumeric_4, SetNumeric_4));

            cmdSetNumeric_5.InputGestures.Add (new KeyGesture (Key.D5, ModifierKeys.Control));
            CommandBindings.Add (new CommandBinding (cmdSetNumeric_5, SetNumeric_5));

            cmdSetNumeric_6.InputGestures.Add (new KeyGesture (Key.D6, ModifierKeys.Control));
            CommandBindings.Add (new CommandBinding (cmdSetNumeric_6, SetNumeric_6));

            cmdSetNumeric_7.InputGestures.Add (new KeyGesture (Key.D7, ModifierKeys.Control));
            CommandBindings.Add (new CommandBinding (cmdSetNumeric_7, SetNumeric_7));

            cmdSetNumeric_8.InputGestures.Add (new KeyGesture (Key.D8, ModifierKeys.Control));
            CommandBindings.Add (new CommandBinding (cmdSetNumeric_8, SetNumeric_8));

            cmdSetTracing_1.InputGestures.Add (new KeyGesture (Key.D1, ModifierKeys.Alt));
            CommandBindings.Add (new CommandBinding (cmdSetTracing_1, SetTracing_1));

            cmdSetTracing_2.InputGestures.Add (new KeyGesture (Key.D2, ModifierKeys.Alt));
            CommandBindings.Add (new CommandBinding (cmdSetTracing_2, SetTracing_2));

            cmdSetTracing_3.InputGestures.Add (new KeyGesture (Key.D3, ModifierKeys.Alt));
            CommandBindings.Add (new CommandBinding (cmdSetTracing_3, SetTracing_3));

            cmdSetTracing_4.InputGestures.Add (new KeyGesture (Key.D4, ModifierKeys.Alt));
            CommandBindings.Add (new CommandBinding (cmdSetTracing_4, SetTracing_4));

            cmdSetTracing_5.InputGestures.Add (new KeyGesture (Key.D5, ModifierKeys.Alt));
            CommandBindings.Add (new CommandBinding (cmdSetTracing_5, SetTracing_5));

            cmdSetTracing_6.InputGestures.Add (new KeyGesture (Key.D6, ModifierKeys.Alt));
            CommandBindings.Add (new CommandBinding (cmdSetTracing_6, SetTracing_6));

            cmdSetTracing_7.InputGestures.Add (new KeyGesture (Key.D7, ModifierKeys.Alt));
            CommandBindings.Add (new CommandBinding (cmdSetTracing_7, SetTracing_7));

            cmdSetTracing_8.InputGestures.Add (new KeyGesture (Key.D8, ModifierKeys.Alt));
            CommandBindings.Add (new CommandBinding (cmdSetTracing_8, SetTracing_8));
        }

        private void UpdateInterface () {
            for (int i = 0; i < listTracings.Count; i++)
                listTracings [i].SetColorScheme (colorScheme);

            for (int i = 0; i < listNumerics.Count; i++)
                listNumerics [i].SetColorScheme (colorScheme);

            App.Current.Dispatcher.InvokeAsync ((Action)(() => {
                wdwDeviceMonitor.Background = Color.GetBackground (Color.Devices.DeviceMonitor, colorScheme);
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
                numericTypes = new (new string [] { "ECG", "NIBP", "SPO2", "RR", "ETCO2", "ABP", "CVP", "T", "PA", "CO", "ICP", "IAP" });
            else if (numericTypes.Count < rowsNumerics) {
                List<string> buffer = new (new string [] { "ECG", "NIBP", "SPO2", "RR", "ETCO2", "ABP", "CVP", "T", "PA", "CO", "ICP", "IAP" });
                buffer.RemoveRange (0, numericTypes.Count);
                numericTypes.AddRange (buffer);
            }

            for (int i = listNumerics.Count; i < rowsNumerics && i < numericTypes.Count; i++) {
                Controls.MonitorNumeric newNum;
                newNum = new Controls.MonitorNumeric (
                    Instance, this,
                    (Controls.MonitorNumeric.ControlTypes.Values)Enum.Parse (typeof (Controls.MonitorNumeric.ControlTypes.Values), numericTypes [i]),
                    colorScheme);
                listNumerics.Add (newNum);
            }

            // Set default tracing types to populate
            if (tracingTypes == null || tracingTypes.Count == 0)
                tracingTypes = new (new string [] { "ECG_II", "ECG_III", "SPO2", "RR", "ETCO2", "ABP", "CVP", "PA", "ICP" });
            else if (tracingTypes.Count < rowsTracings) {
                List<string> buffer = new (new string [] { "ECG_II", "ECG_III", "SPO2", "RR", "ETCO2", "ABP", "CVP", "PA", "ICP" });
                buffer.RemoveRange (0, tracingTypes.Count);
                tracingTypes.AddRange (buffer);
            }

            for (int i = listTracings.Count; i < rowsTracings && i < tracingTypes.Count; i++) {
                Strip newStrip = new ((Lead.Values)Enum.Parse (typeof (Lead.Values), tracingTypes [i]), 6f);
                Controls.MonitorTracing newTracing = new (Instance, newStrip, colorScheme);
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
                            case "colsNumerics": rowsNumerics = int.Parse (pValue); break;

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
            StringBuilder sWrite = new ();

            sWrite.AppendLine (String.Format ("{0}:{1}", "rowsTracings", rowsTracings));
            sWrite.AppendLine (String.Format ("{0}:{1}", "colsNumerics", rowsNumerics));

            List<string> numericTypes = new (),
                         tracingTypes = new ();

            listNumerics.ForEach (o => { numericTypes.Add (o.ControlType?.Value.ToString () ?? ""); });
            listTracings.ForEach (o => { tracingTypes.Add (o.Strip?.Lead?.Value.ToString () ?? ""); });
            sWrite.AppendLine (String.Format ("{0}:{1}", "numericTypes", string.Join (",", numericTypes)));
            sWrite.AppendLine (String.Format ("{0}:{1}", "tracingTypes", string.Join (",", tracingTypes)));

            return sWrite.ToString ();
        }

        public void SetAlarms_On () => SetAlarms (true);

        public void SetAlarms_Off () => SetAlarms (false);

        public void SetAlarms (bool toEnable) {
            foreach (Alarm a in Instance?.Scenario?.DeviceMonitor.Alarms ?? new List<Alarm> ()) {
                a.Enabled = toEnable;

                if (toEnable == false && a.Alarming == true)
                    a.Alarming = false;
            }

            if (toEnable == false)
                MediaPlayer_Alarm?.Stop ();
        }

        public void SetAudio_On () => _ = Instance?.Window_Control?.SetAudio (true);

        public void SetAudio_Off () => _ = Instance?.Window_Control?.SetAudio (false);

        public void SetAudioTone_Off () => _ = SetAudioTone (II.Settings.Simulator.ToneSources.Mute);

        public void SetAudioTone_ECG () => _ = SetAudioTone (II.Settings.Simulator.ToneSources.ECG);

        public void SetAudioTone_SPO2 () => _ = SetAudioTone (II.Settings.Simulator.ToneSources.SPO2);

        public async Task SetAudioTone (II.Settings.Simulator.ToneSources source) {
            if (Instance?.Settings is null)
                return;

            Instance.Settings.MonitorAudioSource = source;
            Instance.Settings.Save ();

            if (MediaPlayer_Tone is not null) {
                switch (Instance.Settings.MonitorAudioSource) {
                    default:
                    case Simulator.ToneSources.SPO2:
                        await ReleaseAudioTone ();

                        break;

                    case Simulator.ToneSources.ECG:
                        await ReleaseAudioTone ();
                        MediaStream_Tone = await Audio.ToneGenerator (0.15, 660, true);
                        MediaPlayer_Tone = new SoundPlayer (MediaStream_Tone);
                        break;
                }
            }

            return;
        }

        public async Task PlayAudioTone (Simulator.ToneSources trigger, Physiology? p) {
            if (Instance is null || MediaPlayer_Tone is null) {
                App.Current.Dispatcher.Invoke (() => Debug.WriteLine ($"Null return at {this.Name}.{nameof (PlayAudioTone)}"));
                return;
            }

            if (Instance.Settings.MonitorAudioSource == trigger && (Instance?.Settings.AudioEnabled ?? false)) {
                switch (Instance.Settings.MonitorAudioSource) {
                    default: break;

                    case Simulator.ToneSources.ECG:           // Plays a fixed tone each QRS complex
                        App.Current.Dispatcher.Invoke (() => {
                            MediaPlayer_Tone.Stop ();
                            MediaPlayer_Tone.Play ();
                        });
                        break;

                    case Simulator.ToneSources.SPO2:          // Plays a variable tone depending on SpO2
                        await App.Current.Dispatcher.InvokeAsync (async () => {
                            MediaPlayer_Tone.Stop ();
                            await ReleaseAudioTone ();

                            MediaStream_Tone = await Audio.ToneGenerator (0.15, II.Math.Lerp (110, 330, (double)(p?.SPO2 ?? 100) / 100), true);
                            MediaPlayer_Tone = new SoundPlayer (MediaStream_Tone);
                            MediaPlayer_Tone.Play ();
                        });
                        break;
                }
            }
        }

        private Task ReleaseAudioTone () {
            MediaPlayer_Tone?.Stop ();
            MediaPlayer_Tone?.Dispose ();

            return Task.CompletedTask;
        }

        public void SetColorScheme_Light () => SetColorScheme (Color.Schemes.Light);

        public void SetColorScheme_Dark () => SetColorScheme (Color.Schemes.Dark);

        public void SetColorScheme (Color.Schemes scheme) {
            colorScheme = scheme;
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
            if (wdwDeviceMonitor.WindowState == System.Windows.WindowState.Maximized)
                wdwDeviceMonitor.WindowState = System.Windows.WindowState.Normal;
            else
                wdwDeviceMonitor.WindowState = System.Windows.WindowState.Maximized;
        }

        public void SetTracing_1 (object s, RoutedEventArgs e) => SetTracing (1);

        public void SetTracing_2 (object s, RoutedEventArgs e) => SetTracing (2);

        public void SetTracing_3 (object s, RoutedEventArgs e) => SetTracing (3);

        public void SetTracing_4 (object s, RoutedEventArgs e) => SetTracing (4);

        public void SetTracing_5 (object s, RoutedEventArgs e) => SetTracing (5);

        public void SetTracing_6 (object s, RoutedEventArgs e) => SetTracing (6);

        public void SetTracing_7 (object s, RoutedEventArgs e) => SetTracing (7);

        public void SetTracing_8 (object s, RoutedEventArgs e) => SetTracing (8);

        public void SetTracing (int amount) {
            rowsTracings = amount;
            OnLayoutChange ();
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

        public void SetNumeric_1 (object s, RoutedEventArgs e) => SetNumeric (1);

        public void SetNumeric_2 (object s, RoutedEventArgs e) => SetNumeric (2);

        public void SetNumeric_3 (object s, RoutedEventArgs e) => SetNumeric (3);

        public void SetNumeric_4 (object s, RoutedEventArgs e) => SetNumeric (4);

        public void SetNumeric_5 (object s, RoutedEventArgs e) => SetNumeric (5);

        public void SetNumeric_6 (object s, RoutedEventArgs e) => SetNumeric (6);

        public void SetNumeric_7 (object s, RoutedEventArgs e) => SetNumeric (7);

        public void SetNumeric_8 (object s, RoutedEventArgs e) => SetNumeric (8);

        public void SetNumeric (int amount) {
            rowsNumerics = amount;
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

        private void MenuToggleFullscreen_Click (object s, RoutedEventArgs e)
            => ToggleFullscreen ();

        private void MenuClose_Click (object s, RoutedEventArgs e) {
            wdwDeviceMonitor.Close ();
        }

        private void MenuAddNumeric_Click (object s, RoutedEventArgs e)
            => AddNumeric ();

        private void MenuAddTracing_Click (object s, RoutedEventArgs e)
            => AddTracing ();

        private void MenuTogglePause_Click (object s, RoutedEventArgs e)
            => TogglePause ();

        private void MenuEnableAlarms (object sender, RoutedEventArgs e)
            => SetAlarms (true);

        private void MenuDisableAlarms (object sender, RoutedEventArgs e)
            => SetAlarms (false);

        private void MenuEnableAudio (object sender, RoutedEventArgs e)
            => SetAudio_On ();

        private void MenuDisableAudio (object sender, RoutedEventArgs e)
            => SetAudio_Off ();

        private void MenuAudioOff (object sender, RoutedEventArgs e)
            => _ = SetAudioTone (Simulator.ToneSources.Mute);

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
            if (Instance?.Settings.AudioEnabled == false) {
                MediaPlayer_Alarm?.Stop ();
                AlarmActive = null;
                return;
            }

            AlarmsActive.Clear ();

            foreach (Controls.MonitorNumeric n in listNumerics) {
                if (n.AlarmActive is not null)
                    AlarmsActive.Add (n.AlarmActive);
            }

            Alarm? alarm = AlarmsActive.Find (a => a.Alarming ?? false && a.Priority == Alarm.Priorities.High);
            alarm ??= AlarmsActive.Find (a => a.Alarming ?? false && a.Priority == Alarm.Priorities.Medium);
            alarm ??= AlarmsActive.Find (a => a.Alarming ?? false && a.Priority == Alarm.Priorities.Low);

            if (alarm is null) {
                MediaPlayer_Alarm?.Stop ();
                MediaPlayer_Alarm = null;
                AlarmActive = null;
                return;
            } else if (alarm.Priority == AlarmActive) {
                if (MediaPlayer_Alarm is null && AlarmMedia?.Count () == Enum.GetValues (typeof (Alarm.Priorities)).Length) {
                    MediaPlayer_Alarm = new SoundPlayer (AlarmMedia [alarm.Priority.GetHashCode ()]);
                    MediaPlayer_Alarm.PlayLooping ();
                }
                return;
            } else if (alarm.Priority != AlarmActive) {
                MediaPlayer_Alarm?.Stop ();
                if (AlarmMedia?.Count () == Enum.GetValues (typeof (Alarm.Priorities)).Length) {
                    MediaPlayer_Alarm = new SoundPlayer (AlarmMedia [alarm.Priority.GetHashCode ()]);
                    MediaPlayer_Alarm.PlayLooping ();

                    AlarmActive = alarm.Priority;
                }
                return;
            }
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
                    => n.ControlType?.Value != Controls.MonitorNumeric.ControlTypes.Values.ETCO2
                    && n.ControlType?.Value != Controls.MonitorNumeric.ControlTypes.Values.RR)
                .ToList ()
                .ForEach (n => App.Current.Dispatcher.InvokeAsync (n.UpdateVitals));
        }

        public void OnTick_Vitals_Respiratory (object? sender, EventArgs e) {
            if (State != States.Running)
                return;

            listNumerics
                .Where (n
                    => n.ControlType?.Value == Controls.MonitorNumeric.ControlTypes.Values.ETCO2
                    || n.ControlType?.Value == Controls.MonitorNumeric.ControlTypes.Values.RR)
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