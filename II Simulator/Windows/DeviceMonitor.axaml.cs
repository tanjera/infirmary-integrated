/* Infirmary Integrated Simulator
 * By Ibi Keller (Tanjera), (c) 2023
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Threading;

using II;
using II.Rhythm;
using II.Settings;
using II.Waveform;

using LibVLCSharp.Shared;

namespace IISIM {

    public partial class DeviceMonitor : DeviceWindow {
        /* Device variables */
        private int rowsTracings = 0;
        private int rowsNumerics = 0;

        private int autoScale_iter = Strip.DefaultAutoScale_Iterations;

        private Color.Schemes colorScheme = Color.Schemes.Dark;

        private List<Controls.MonitorTracing> listTracings = new ();
        private List<Controls.MonitorNumeric> listNumerics = new ();

        /* Variables for audio tones (QRS or SPO2 beeps)*/
        private ToneSources ToneSource = ToneSources.None;
        public MediaPlayer? TonePlayer;
        private MemoryStream? ToneMedia;

        /* Variables controlling for audio alarms */
        private Alarm.Priorities? AlarmActive;
        private List<Alarm> AlarmRefs = new ();
        private List<StreamMediaInput>? AlarmMedia;

        private List<II.Settings.Device.Numeric>? Numerics {
            set { 
                if (Instance?.Scenario?.DeviceMonitor is not null)
                    Instance.Scenario.DeviceMonitor.Numerics = value ?? new List<Device.Numeric> (); 
            }
            get { return Instance?.Scenario?.DeviceMonitor.Numerics; }
        }
        
        private List<II.Settings.Device.Numeric>? Transducers_Zeroed {
            set { 
                if (Instance?.Scenario?.DeviceMonitor is not null)
                    Instance.Scenario.DeviceMonitor.Transducers_Zeroed = value ?? new List<Device.Numeric> (); 
            }
            get { return Instance?.Scenario?.DeviceMonitor.Transducers_Zeroed; }
        }
        
        private List<II.Settings.Device.Tracing>? Tracings {
            set { 
                if (Instance?.Scenario?.DeviceMonitor is not null)
                    Instance.Scenario.DeviceMonitor.Tracings = value ?? new List<Device.Tracing> (); 
            }
            get { return Instance?.Scenario?.DeviceMonitor.Tracings; }
        }
        
        public enum ToneSources {
            None,
            ECG,
            SPO2
        }

        public DeviceMonitor () {
            InitializeComponent ();
        }

        public DeviceMonitor (App? app) : base (app) {
            InitializeComponent ();
#if DEBUG
            this.AttachDevTools ();
#endif

            DataContext = this;

            InitInterface ();
            OnLayoutChange ();
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

            TonePlayer = new MediaPlayer (Instance.AudioLib);
            
            AlarmMedia = new () {
                new (AssetLoader.Open (new Uri ("avares://infirmary-integrated/Resources/Alarm_Low_Priority.wav"))),
                new (AssetLoader.Open (new Uri ("avares://infirmary-integrated/Resources/Alarm_Medium_Priority.wav"))),
                new (AssetLoader.Open (new Uri ("avares://infirmary-integrated/Resources/Alarm_High_Priority.wav")))
            };
        }

        public override void DisposeAudio () {
            /* Note: It's important to nullify objects after Disposing them because this function may
             * be triggered multiple times (e.g. on Window.Close() and on Application.Exit()).
             * Since LibVLC wraps a C++ library, nullifying and null checking prevents accessing
             * released/reassigned memory blocks (a Memory Exception)
             */

            base.DisposeAudio ();

            if (TonePlayer is not null) {
                TonePlayer.Stop ();
                TonePlayer.Dispose ();
                TonePlayer = null;
            }

            if (AlarmMedia is not null) {
                foreach (StreamMediaInput smi in AlarmMedia) {
                    smi.Close ();
                    smi.Dispose ();
                }

                AlarmMedia = null;
            }
        }

        private void InitInterface () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitInterface)}");
                return;
            }
            
            // Populate UI strings per language selection
            this.GetControl<Avalonia.Controls.Window> ("wdwDeviceMonitor").Title = Instance.Language.Localize ("CM:WindowTitle");
            this.GetControl<MenuItem> ("menuDevice").Header = Instance.Language.Localize ("MENU:MenuDeviceOptions");
            this.GetControl<MenuItem> ("menuPauseSimulation").Header = Instance.Language.Localize ("MENU:PauseSimulation");
            this.GetControl<MenuItem> ("menuAddNumeric").Header = Instance.Language.Localize ("MENU:MenuAddNumeric");
            this.GetControl<MenuItem> ("menuAddTracing").Header = Instance.Language.Localize ("MENU:MenuAddTracing");
            this.GetControl<MenuItem> ("menuToggleFullscreen").Header = Instance.Language.Localize ("MENU:MenuToggleFullscreen");
            this.GetControl<MenuItem> ("menuCloseDevice").Header = Instance.Language.Localize ("MENU:MenuCloseDevice");

            this.GetControl<MenuItem> ("menuAlarms").Header = Instance.Language.Localize ("MENU:MenuAlarms");
            this.GetControl<MenuItem> ("menuAlarmsEnable").Header = Instance.Language.Localize ("MENU:MenuAlarmsEnable");
            this.GetControl<MenuItem> ("menuAlarmsDisable").Header = Instance.Language.Localize ("MENU:MenuAlarmsDisable");

            this.GetControl<MenuItem> ("menuAudio").Header = Instance.Language.Localize ("MENU:MenuAudio");
            this.GetControl<MenuItem> ("menuAudioOff").Header = Instance.Language.Localize ("MENU:MenuAudioOff");
            this.GetControl<MenuItem> ("menuAudioECG").Header = Instance.Language.Localize ("MENU:MenuAudioECG");
            this.GetControl<MenuItem> ("menuAudioSPO2").Header = Instance.Language.Localize ("MENU:MenuAudioSPO2");
            this.GetControl<MenuItem> ("menuAudioDisable").Header = Instance.Language.Localize ("MENU:MenuAudioDisable");
            this.GetControl<MenuItem> ("menuAudioEnable").Header = Instance.Language.Localize ("MENU:MenuAudioEnable");

            this.GetControl<MenuItem> ("menuColor").Header = Instance.Language.Localize ("MENU:MenuColorScheme");
            this.GetControl<MenuItem> ("menuColorLight").Header = Instance.Language.Localize ("MENU:MenuColorSchemeLight");
            this.GetControl<MenuItem> ("menuColorDark").Header = Instance.Language.Localize ("MENU:MenuColorSchemeDark");
        }

        private void UpdateInterface () {
            Dispatcher.UIThread.InvokeAsync (() => {
                for (int i = 0; i < listTracings.Count; i++)
                    listTracings [i].SetColorScheme (colorScheme);

                for (int i = 0; i < listNumerics.Count; i++)
                    listNumerics [i].SetColorScheme (colorScheme);

                Avalonia.Controls.Window window = this.GetControl<Avalonia.Controls.Window> ("wdwDeviceMonitor");
                window.Background = Color.GetBackground (Color.Devices.DeviceMonitor, colorScheme);
            });
        }

        public async Task Load (string inc) {
            using StringReader sRead = new (inc);

            // For backwards-compatibility!
            int rNumerics = 0,
                rTracings = 0;
            List<string> tNumerics = new (),
                         tTracings = new ();

            try {
                string? line;
                while ((line = await sRead.ReadLineAsync ()) != null) {
                    if (line.Contains (":")) {
                        string pName = line.Substring (0, line.IndexOf (':')),
                                pValue = line.Substring (line.IndexOf (':') + 1);
                        switch (pName) {
                            default: break;
                            case "rowsTracings": rTracings = int.Parse (pValue); break;
                            case "rowsNumerics": rNumerics = int.Parse (pValue); break;
                            case "numericTypes": tNumerics.AddRange (pValue.Split (',').Where ((o) => o != "")); break;
                            case "tracingTypes": tTracings.AddRange (pValue.Split (',').Where ((o) => o != "")); break;
                            
                            case "Numerics":
                                if (Instance?.Scenario is not null) {
                                    Numerics = new();
                                    foreach (string s in pValue.Split (',')) {
                                        if (Enum.TryParse (s, true, out II.Settings.Device.Numeric res)) {
                                            Numerics.Add (res);
                                        }
                                    }
                                    SetNumerics (Instance.Scenario.DeviceMonitor);
                                }
                                break;
                            
                            case "Numerics_Zeroed":
                                Transducers_Zeroed = new();
                                foreach (string s in pValue.Split (',')) {
                                    if (Enum.TryParse (s, true, out II.Settings.Device.Numeric res)) {
                                        Transducers_Zeroed.Add (res);
                                    }
                                }
                                break;
                            
                            case "Tracings":
                                if (Instance?.Scenario is not null) {
                                    Tracings = new();
                                    foreach (string s in pValue.Split (',')) {
                                        if (Enum.TryParse (s, true,
                                                out II.Settings.Device.Tracing res)) {
                                            Tracings.Add (res);
                                        }
                                    }
                                    SetTracings (Instance.Scenario.DeviceMonitor);
                                }
                                break;
                        }
                    }
                }

                if (rNumerics > 0 && tNumerics.Count > 0 && Instance?.Scenario is not null) {
                    for (int i = 0; i < rNumerics && i < tNumerics.Count; i++) {
                        if (Enum.TryParse (tNumerics[i], true, out II.Settings.Device.Numeric res)) {
                            Numerics?.Add (res);
                        }
                    } 
                }
                
                if (rTracings > 0 && tTracings.Count > 0 && Instance?.Scenario is not null) {
                    for (int i = 0; i < rTracings && i < tTracings.Count; i++) {
                        if (Enum.TryParse (tTracings[i], true, out II.Settings.Device.Tracing res)) {
                            Tracings?.Add (res);
                        }
                    } 
                }
                
            } finally {
                sRead.Close ();
                OnLayoutChange ();
            }
        }

        public string Save () {
            UpdateSettings();
            
            StringBuilder sw = new ();

            if (Numerics?.Count > 0) {
                sw.AppendLine (String.Format ("{0}:{1}", 
                    "Numerics", 
                    string.Join (',', Numerics ?? new List<Device.Numeric> ())));
            }
            
            if (Transducers_Zeroed?.Count > 0) {
                sw.AppendLine (String.Format ("{0}:{1}", 
                    "Numerics_Zeroed", 
                    string.Join (',', Transducers_Zeroed)));
            }
            
            if (Tracings?.Count > 0) {
                sw.AppendLine (String.Format ("{0}:{1}", 
                    "Tracings", 
                    string.Join (',', Tracings ?? new List<Device.Tracing> ())));
            }
            
            return sw.ToString ();
        }

        public void SetAlarms_On () => SetAlarms (true);

        public void SetAlarms_Off () => SetAlarms (false);

        public void SetAlarms (bool toEnable) {
            foreach (Alarm a in Instance?.Scenario?.DeviceMonitor.Alarms ?? new List<Alarm> ()) {
                a.Enabled = toEnable;

                if (toEnable == false && a.Alarming == true)
                    a.Alarming = false;
            }

            if (toEnable == false && (AudioPlayer?.IsPlaying ?? false))
                AudioPlayer.Stop ();
        }

        public void SetAudio_On () => _ = Instance?.Window_Main?.SetAudio (true);

        public void SetAudio_Off () => _ = Instance?.Window_Main?.SetAudio (false);

        public void SetAudioTone_Off () => _ = SetAudioTone (ToneSources.None);

        public void SetAudioTone_ECG () => _ = SetAudioTone (ToneSources.ECG);

        public void SetAudioTone_SPO2 () => _ = SetAudioTone (ToneSources.SPO2);

        public async Task SetAudioTone (ToneSources source) {
            ToneSource = source;

            if (TonePlayer is not null && Instance?.AudioLib is not null) {
                switch (ToneSource) {
                    default:
                    case ToneSources.SPO2:
                        await ReleaseAudioTone ();
                        break;

                    case ToneSources.ECG:
                        await ReleaseAudioTone ();
                        ToneMedia = await Audio.ToneGenerator (0.15, 330, true);
                        TonePlayer.Media = new Media (Instance.AudioLib, new StreamMediaInput (ToneMedia));
                        break;
                }
            }

            return;
        }

        public async Task PlayAudioTone (ToneSources trigger, Physiology? p) {
            if (TonePlayer is null || Instance?.AudioLib is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (PlayAudioTone)}");
                return;
            }

            if (ToneSource == trigger && (Instance?.Settings.AudioEnabled ?? false)) {
                switch (ToneSource) {
                    default: break;

                    case ToneSources.ECG:           // Plays a fixed tone each QRS complex
                        TonePlayer.Stop ();
                        TonePlayer.Play ();
                        break;

                    case ToneSources.SPO2:          // Plays a variable tone depending on SpO2
                        TonePlayer.Stop ();
                        await ReleaseAudioTone ();
                        ToneMedia = await Audio.ToneGenerator (0.15, II.Math.Lerp (110, 330, (double)(p?.SPO2 ?? 100) / 100), true);
                        TonePlayer.Media = new Media (Instance.AudioLib, new StreamMediaInput (ToneMedia));
                        TonePlayer.Play ();
                        break;
                }
            }
        }

        private Task ReleaseAudioTone () {
            TonePlayer?.Stop ();
            if (TonePlayer is not null)
                TonePlayer.Media = null;

            ToneMedia?.Close ();
            ToneMedia?.Dispose ();

            return Task.CompletedTask;
        }

        private void SetColorScheme_Light () => SetColorScheme (Color.Schemes.Light);

        private void SetColorScheme_Dark () => SetColorScheme (Color.Schemes.Dark);

        public void SetColorScheme (Color.Schemes scheme) {
            colorScheme = scheme;
            UpdateInterface ();
        }

        public void ToggleFullscreen () {
            if (WindowState == WindowState.FullScreen)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.FullScreen;
        }

        public void SetTracing_1 (object s, RoutedEventArgs e) => SetTracing (1);

        public void SetTracing_2 (object s, RoutedEventArgs e) => SetTracing (2);

        public void SetTracing_3 (object s, RoutedEventArgs e) => SetTracing (3);

        public void SetTracing_4 (object s, RoutedEventArgs e) => SetTracing (4);

        public void SetTracing_5 (object s, RoutedEventArgs e) => SetTracing (5);

        public void SetTracing_6 (object s, RoutedEventArgs e) => SetTracing (6);

        public void SetTracing_7 (object s, RoutedEventArgs e) => SetTracing (7);

        public void SetTracing_8 (object s, RoutedEventArgs e) => SetTracing (8);

        
        public void SetTracings (II.Settings.Device settings) {
            rowsTracings = 0;           // Reset for re-instantiation
            listTracings.Clear ();
        }
        
        public void SetTracing (int amount) {
            rowsTracings = amount;
            OnLayoutChange ();
        }

        public void AddTracing () {
            rowsTracings += 1;
            OnLayoutChange ();
        }

        public void RemoveTracing (Controls.MonitorTracing requestSender) {
            if (rowsTracings == 1) // Don't remove the last Control!
                return;
            
            rowsTracings -= 1;

            Dispatcher.UIThread.InvokeAsync (() => {
                listTracings.Remove (requestSender);
                OnLayoutChange ();
            });
        }
        
        public void MoveTracing (Controls.MonitorTracing req, int delta) {
            int i = listTracings.FindIndex (o => o == req);

            if (i + delta >= 0 && i + delta < listTracings.Count) {    // Ensure the proposed move is sane
                if (i + delta < rowsTracings) {                        // Ensure the proposed move is valid in the existing visuals
                    listTracings.RemoveAt (i);
                    listTracings.Insert (i + delta, req);
                    OnLayoutChange ();
                }
            }
        }

        public void MoveTracing_Up (Controls.MonitorTracing req)
            => MoveTracing (req, -1);

        public void MoveTracing_Down (Controls.MonitorTracing req) 
            => MoveTracing (req, 1);

        public void SetNumeric_1 (object s, RoutedEventArgs e) => SetNumeric (1);

        public void SetNumeric_2 (object s, RoutedEventArgs e) => SetNumeric (2);

        public void SetNumeric_3 (object s, RoutedEventArgs e) => SetNumeric (3);

        public void SetNumeric_4 (object s, RoutedEventArgs e) => SetNumeric (4);

        public void SetNumeric_5 (object s, RoutedEventArgs e) => SetNumeric (5);

        public void SetNumeric_6 (object s, RoutedEventArgs e) => SetNumeric (6);

        public void SetNumeric_7 (object s, RoutedEventArgs e) => SetNumeric (7);

        public void SetNumeric_8 (object s, RoutedEventArgs e) => SetNumeric (8);

        public void SetNumerics (II.Settings.Device settings) {
            rowsNumerics = 0;           // Reset for re-instantiation
            listNumerics.Clear ();
        }
        
        public void SetNumeric (int amount) {
            rowsNumerics = amount;
            OnLayoutChange ();
        }

        public void AddNumeric () {
            rowsNumerics += 1;
            OnLayoutChange ();
        }

        public void RemoveNumeric (Controls.MonitorNumeric requestSender) {
            if (rowsNumerics == 1) // Don't remove the last Control!
                return;
            
            rowsNumerics -= 1;

            Dispatcher.UIThread.InvokeAsync (() => {
                listNumerics.Remove (requestSender);
                OnLayoutChange ();
            });
        }

        public void MoveNumeric (Controls.MonitorNumeric req, int delta) {
            int i = listNumerics.FindIndex (o => o == req);

            if (i + delta >= 0 && i + delta < listNumerics.Count) {    // Ensure the proposed move is sane
                if (i + delta < rowsNumerics) {
                    // Ensure the proposed move is valid in the existing visuals
                    listNumerics.RemoveAt (i);
                    listNumerics.Insert (i + delta, req);
                    OnLayoutChange ();
                }
            }
        }

        public void MoveNumeric_Up (Controls.MonitorNumeric req)
            => MoveNumeric (req, -1);

        public void MoveNumeric_Down (Controls.MonitorNumeric req) 
            => MoveNumeric (req, 1);
        
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

        public void RefreshLayout ()
            => Dispatcher.UIThread.InvokeAsync (OnLayoutChange);
        
        private void UpdateSettings () {
            // Carry current visually present and set Numerics & Tracings into Device.Settings
            if (Numerics is not null) {
                for (int i = 0; i < rowsNumerics && i < listNumerics.Count; i++) {
                    if (Enum.TryParse (listNumerics [i]?.ControlType?.Value.ToString (),
                            true, out Device.Numeric res)) {
                        if (Numerics.Count <= i) {
                            Numerics.Add (res);
                        } else {
                            Numerics [i] = res;
                        }
                    }
                }
            }
            
            if (Tracings is not null) {
                for (int i = 0; i < rowsTracings && i < listTracings.Count; i++) {
                    if (Enum.TryParse (listTracings [i].Lead?.Value.ToString (),
                            true, out Device.Tracing res)) {
                        if (Tracings.Count <= i) {
                            Tracings.Add (res);
                        } else {
                            Tracings [i] = res;
                        }
                    }
                }
            }
        }
        
        private void MenuToggleFullscreen_Click (object s, RoutedEventArgs e)
            => ToggleFullscreen ();

        private void MenuClose_Click (object s, RoutedEventArgs e)
            => this.Close ();

        private void MenuAddNumeric_Click (object s, RoutedEventArgs e)
            => AddNumeric ();

        private void MenuAddTracing_Click (object s, RoutedEventArgs e)
            => AddTracing ();

        private void MenuPauseSimulation_Click (object s, RoutedEventArgs e)
            => PauseSimulation();

        private void MenuColorScheme_Light (object sender, RoutedEventArgs e)
            => SetColorScheme (Color.Schemes.Light);

        private void MenuColorScheme_Dark (object sender, RoutedEventArgs e)
            => SetColorScheme (Color.Schemes.Dark);

        private void MenuEnableAlarms (object sender, RoutedEventArgs e)
            => SetAlarms (true);

        private void MenuDisableAlarms (object sender, RoutedEventArgs e)
            => SetAlarms (false);

        private void MenuEnableAudio (object sender, RoutedEventArgs e)
            => SetAudio_On ();

        private void MenuDisableAudio (object sender, RoutedEventArgs e)
            => SetAudio_Off ();

        private void MenuAudioOff (object sender, RoutedEventArgs e)
            => _ = SetAudioTone (ToneSources.None);

        private void MenuAudioECG (object sender, RoutedEventArgs e)
            => _ = SetAudioTone (ToneSources.ECG);

        private void MenuAudioSPO2 (object sender, RoutedEventArgs e)
            => _ = SetAudioTone (ToneSources.SPO2);

        protected override void OnLoaded (RoutedEventArgs e) {
            base.OnLoaded(e);
            
            if (Instance?.Settings.UI?.DeviceMonitor is not null) {
                var pos = new PixelPoint (
                    Instance.Settings.UI.DeviceMonitor.X < 0 ? 0 : Instance.Settings.UI.DeviceMonitor.X ?? Position.X,
                    Instance.Settings.UI.DeviceMonitor.Y < 0 ? 0 : Instance.Settings.UI.DeviceMonitor.Y ?? Position.Y);
                
                if (Screens.All.Any(o => o.WorkingArea.Contains (pos))) { 
                    Position = pos;
                }

                // Set Width, Height, and/or WindowState
                if (Instance.Settings.UI.DeviceMonitor.WindowState == WindowState.Normal) {
                    SizeToContent = SizeToContent.Manual;
                    Width = Instance.Settings.UI.DeviceMonitor.Width ?? Width;
                    Height = Instance.Settings.UI.DeviceMonitor.Height ?? Height;
                } else {
                    WindowState = Instance.Settings.UI.DeviceMonitor.WindowState ?? WindowState;
                }
            }
        }

        protected override void OnClosing (object? sender, CancelEventArgs e) {
            base.OnClosing (sender, e);

            if (Instance?.Settings.UI is not null && WindowStatus == WindowStates.Active) {
                Instance.Settings.UI.DeviceMonitor = new() {
                    X = Position.X,
                    Y = Position.Y,
                    Width = Width,
                    Height = Height,
                    WindowState = WindowState
                };
                
                Instance.Settings.Save();
            }
            
            if (Instance?.Physiology is not null)
                Instance.Physiology.PhysiologyEvent -= OnPhysiologyEvent;
        }

        public override void OnTick_Alarm (object? sender, EventArgs e) {
            if (AudioPlayer is null || Instance?.AudioLib is null || AlarmMedia is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (OnTick_Alarm)}");
                return;
            }

            if (Instance?.Settings.AudioEnabled == false) {
                AudioPlayer.Stop ();
                AlarmActive = null;
                return;
            }

            AlarmRefs.Clear ();

            foreach (Controls.MonitorNumeric n in listNumerics) {
                if (n.AlarmRef is not null)
                    AlarmRefs.Add (n.AlarmRef);
            }

            Alarm? alarm = AlarmRefs.Find (a => a.Alarming ?? false && a.Priority == Alarm.Priorities.High);
            alarm ??= AlarmRefs.Find (a => a.Alarming ?? false && a.Priority == Alarm.Priorities.Medium);
            alarm ??= AlarmRefs.Find (a => a.Alarming ?? false && a.Priority == Alarm.Priorities.Low);

            if (alarm is null) {
                AudioPlayer.Stop ();
                AlarmActive = null;
                return;
            } else if (alarm.Priority == AlarmActive) {
                if (!AudioPlayer.IsPlaying && Instance is not null) {
                    AudioPlayer.Media = new Media (Instance.AudioLib, AlarmMedia [alarm.Priority.GetHashCode ()]);
                    AudioPlayer.Play ();
                }

                return;
            } else if (alarm.Priority != AlarmActive && Instance is not null) {
                AudioPlayer.Stop ();
                AudioPlayer.Media = new Media (Instance.AudioLib, AlarmMedia [alarm.Priority.GetHashCode ()]);
                AudioPlayer.Play ();

                AlarmActive = alarm.Priority;
                return;
            }
        }

        protected override void OnTick_Tracing (object? sender, EventArgs e) {
            if (Instance?.Settings.State != II.Settings.Instance.States.Running)
                return;

            for (int i = 0; i < listTracings.Count; i++) {
                listTracings [i].Strip?.Scroll ();
                Dispatcher.UIThread.InvokeAsync (listTracings [i].DrawTracing);
            }
        }

        protected override void OnTick_Vitals_Cardiac (object? sender, EventArgs e) {
            if (Instance?.Settings.State != II.Settings.Instance.States.Running)
                return;

            listNumerics
                .Where (n
                    => n.ControlType?.Value != Controls.MonitorNumeric.ControlTypes.Values.ETCO2
                    && n.ControlType?.Value != Controls.MonitorNumeric.ControlTypes.Values.RR)
                .ToList ()
                .ForEach (n => Dispatcher.UIThread.InvokeAsync (n.UpdateVitals));
        }

        protected override void OnTick_Vitals_Respiratory (object? sender, EventArgs e) {
            if (Instance?.Settings.State != II.Settings.Instance.States.Running)
                return;

            listNumerics
                .Where (n
                    => n.ControlType?.Value == Controls.MonitorNumeric.ControlTypes.Values.ETCO2
                    || n.ControlType?.Value == Controls.MonitorNumeric.ControlTypes.Values.RR)
                .ToList ()
                .ForEach (n => Dispatcher.UIThread.InvokeAsync (n.UpdateVitals));
        }

        private void OnLayoutChange () {
            List<string>? numericTypes = new List<string> ();
            List<string>? tracingTypes = new List<string> ();

            List<string> defaultNumerics =
                ["ECG", "NIBP", "SPO2", "RR", "ETCO2", "ABP", "CVP", "T", "PA", "ICP", "IAP"];
            List<string> defaultTracings = ["ECG_II", "ECG_III", "SPO2", "RR", "ETCO2", "ABP", "CVP", "PA", "ICP"];
            
            /* Process all Numerics; Load settings, instantiate Controls, and populate Grid */
            
            if (rowsNumerics == 0) {    // On instantiation only, use pre-defined or Load()ed settings!
                // If Device settings are present, then a Scenario is loaded
                if (Numerics is not null && Numerics.Count > 0) {
                    foreach (var n in Numerics)
                        numericTypes.Add (n.ToString ());
                    
                    rowsNumerics = Numerics.Count;
                } else {
                    rowsNumerics = 3;
                    numericTypes.AddRange(defaultNumerics.Slice (0, rowsNumerics));
                }
            }

            // Cap available amount of numerics
            rowsNumerics = II.Math.Clamp (rowsNumerics, 1, defaultNumerics.Count);
            
            // Rebuild the Numeric visual controls; auto-rectifies as it iterates i vs. rowNumerics 
            for (int i = 0; i < rowsNumerics; i++) {
                if (listNumerics.Count <= i) {      // Create new MonitorNumerics only; we don't want to modify existing ones
                                                    // Because they may have been modified by the user through Add/Remove/Select Input
                    Controls.MonitorNumeric n = new (
                        Instance,
                        (Controls.MonitorNumeric.ControlTypes.Values)Enum.Parse (
                            typeof(Controls.MonitorNumeric.ControlTypes.Values),
                            numericTypes.Count > i ? numericTypes [i] : defaultNumerics[i]), colorScheme);
                    listNumerics.Add (n);
                }
            }
            
            // Reset the UI container and repopulate with the UI elements
            Grid gridNumerics = this.GetControl<Grid> ("gridNumerics");
            
            gridNumerics.Children.Clear ();         // Avalonia Controls do *not* need Dispose()/Destroy() called
            gridNumerics.RowDefinitions.Clear ();
            
            for (int i = 0; i < rowsNumerics && i < listNumerics.Count; i++) {
                gridNumerics.RowDefinitions.Add (new RowDefinition ());
                listNumerics [i].SetValue (Grid.RowProperty, i);
                gridNumerics.Children.Add (listNumerics [i]);
            }

            
            /* Process all Tracings; Load settings, instantiate Controls, and populate Grid */
            
            if (rowsTracings == 0) {    // On instantiation only, use pre-defined or Load()ed settings!
                // If Device settings are present, then a Scenario is loaded
                if (Tracings is not null && Tracings.Count > 0) {
                    foreach (var n in Tracings)
                        tracingTypes.Add (n.ToString ());
                    
                    rowsTracings = Tracings.Count;
                } else {
                    rowsTracings = 3;
                    tracingTypes.AddRange(defaultTracings.Slice (0, rowsTracings));
                }
            }

            // Cap available amount of tracings
            rowsTracings = II.Math.Clamp (rowsTracings, 1, defaultTracings.Count);
            
            // Rebuild the Tracing visual controls; auto-rectifies as it iterates i vs. rowTracings 
            for (int i = 0; i < rowsTracings; i++) {
                if (listTracings.Count <= i) {      // Create new MonitorTracings only; we don't want to modify existing ones
                                                    // Because they may have been modified by the user through Add/Remove/Select Input
                    Strip s = new ((Lead.Values)Enum.Parse (typeof (Lead.Values), 
                        tracingTypes.Count > i ? tracingTypes [i] : defaultTracings[i]), 
                        6f);
                    Controls.MonitorTracing t = new (Instance, s, colorScheme);
                    listTracings.Add (t);
                }
            }

            // Reset the UI container and repopulate with the UI elements
            Grid gridTracings = this.GetControl<Grid> ("gridTracings");

            gridTracings.Children.Clear ();         // Avalonia Controls do *not* need Dispose()/Destroy() called
            gridTracings.RowDefinitions.Clear ();
            for (int i = 0; i < rowsTracings && i < listTracings.Count; i++) {
                gridTracings.RowDefinitions.Add (new RowDefinition ());
                listTracings [i].SetValue (Grid.RowProperty, i);
                gridTracings.Children.Add (listTracings [i]);
            }
            
            UpdateSettings();
        }

        public override void OnPhysiologyEvent (object? sender, Physiology.PhysiologyEventArgs e) {
            switch (e.EventType) {
                default: break;
                case Physiology.PhysiologyEventTypes.Vitals_Change:
                    listTracings.ForEach (c => {
                        c.Strip?.ClearFuture (Instance?.Physiology);
                        c.Strip?.Add_Baseline (Instance?.Physiology);
                    });

                    listNumerics.ForEach (n => Dispatcher.UIThread.InvokeAsync (n.UpdateVitals));
                    break;

                case Physiology.PhysiologyEventTypes.Defibrillation:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Defibrillation (Instance?.Physiology));
                    break;

                case Physiology.PhysiologyEventTypes.Pacermaker_Spike:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Pacemaker (Instance?.Physiology));
                    break;

                case Physiology.PhysiologyEventTypes.Cardiac_Baseline:
                    
                    Debug.WriteLine($"{DateTime.Now} Cardiac_Baseline");
                    
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Baseline (Instance?.Physiology));
                    break;

                case Physiology.PhysiologyEventTypes.Cardiac_Atrial_Electric:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Atrial_Electrical (Instance?.Physiology));
                    break;

                case Physiology.PhysiologyEventTypes.Cardiac_Ventricular_Electric:
                    // QRS audio tone is only triggered by rhythms w/ a ventricular electrical (QRS complex) action
                    _ = PlayAudioTone (ToneSources.ECG, e.Physiology);

                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Ventricular_Electrical (Instance?.Physiology));
                    break;

                case Physiology.PhysiologyEventTypes.Cardiac_Atrial_Mechanical:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Atrial_Mechanical (Instance?.Physiology));
                    break;

                case Physiology.PhysiologyEventTypes.Cardiac_Ventricular_Mechanical:
                    // SPO2 audio tone is only triggered  by rhythms w/ a ventricular mechanical action (systole)
                    _ = PlayAudioTone (ToneSources.SPO2, e.Physiology);

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