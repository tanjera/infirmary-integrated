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
using Avalonia.Threading;

using II;
using II.Rhythm;
using II.Waveform;

using LibVLCSharp.Shared;

namespace IISIM {

    public partial class DeviceMonitor : DeviceWindow {
        /* Device variables */
        private int rowsTracings = 3;
        private int rowsNumerics = 3;

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

            var assets = AvaloniaLocator.Current.GetService<Avalonia.Platform.IAssetLoader> ();
            AlarmMedia = new () {
                new (assets.Open (new Uri ("avares://Infirmary Integrated/Resources/Alarm_Low_Priority.wav"))),
                new (assets.Open (new Uri ("avares://Infirmary Integrated/Resources/Alarm_Medium_Priority.wav"))),
                new (assets.Open (new Uri ("avares://Infirmary Integrated/Resources/Alarm_High_Priority.wav")))
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
            this.FindControl<Window> ("wdwDeviceMonitor").Title = Instance.Language.Localize ("CM:WindowTitle");
            this.FindControl<MenuItem> ("menuDevice").Header = Instance.Language.Localize ("MENU:MenuDeviceOptions");
            this.FindControl<MenuItem> ("menuPauseDevice").Header = Instance.Language.Localize ("MENU:MenuPauseDevice");
            this.FindControl<MenuItem> ("menuAddNumeric").Header = Instance.Language.Localize ("MENU:MenuAddNumeric");
            this.FindControl<MenuItem> ("menuAddTracing").Header = Instance.Language.Localize ("MENU:MenuAddTracing");
            this.FindControl<MenuItem> ("menuCloseDevice").Header = Instance.Language.Localize ("MENU:MenuCloseDevice");

            this.FindControl<MenuItem> ("menuAlarms").Header = Instance.Language.Localize ("MENU:MenuAlarms");
            this.FindControl<MenuItem> ("menuAlarmsEnable").Header = Instance.Language.Localize ("MENU:MenuAlarmsEnable");
            this.FindControl<MenuItem> ("menuAlarmsDisable").Header = Instance.Language.Localize ("MENU:MenuAlarmsDisable");

            this.FindControl<MenuItem> ("menuAudio").Header = Instance.Language.Localize ("MENU:MenuAudio");
            this.FindControl<MenuItem> ("menuAudioOff").Header = Instance.Language.Localize ("MENU:MenuAudioOff");
            this.FindControl<MenuItem> ("menuAudioECG").Header = Instance.Language.Localize ("MENU:MenuAudioECG");
            this.FindControl<MenuItem> ("menuAlarmsSPO2").Header = Instance.Language.Localize ("MENU:MenuAudioSPO2");

            this.FindControl<MenuItem> ("menuColor").Header = Instance.Language.Localize ("MENU:MenuColorScheme");
            this.FindControl<MenuItem> ("menuColorLight").Header = Instance.Language.Localize ("MENU:MenuColorSchemeLight");
            this.FindControl<MenuItem> ("menuColorDark").Header = Instance.Language.Localize ("MENU:MenuColorSchemeDark");
        }

        private void UpdateInterface () {
            Dispatcher.UIThread.InvokeAsync (() => {
                for (int i = 0; i < listTracings.Count; i++)
                    listTracings [i].SetColorScheme (colorScheme);

                for (int i = 0; i < listNumerics.Count; i++)
                    listNumerics [i].SetColorScheme (colorScheme);

                Window window = this.FindControl<Window> ("wdwDeviceMonitor");
                window.Background = Color.GetBackground (Color.Devices.DeviceMonitor, colorScheme);
            });
        }

        public async Task Load (string inc) {
            using StringReader sRead = new (inc);
            List<string> numericTypes = new (),
                         tracingTypes = new ();

            try {
                string? line;
                while ((line = await sRead.ReadLineAsync ()) != null) {
                    if (line.Contains (":")) {
                        string pName = line.Substring (0, line.IndexOf (':')),
                                pValue = line.Substring (line.IndexOf (':') + 1);
                        switch (pName) {
                            default: break;
                            case "rowsTracings": rowsTracings = int.Parse (pValue); break;
                            case "rowsNumerics": rowsNumerics = int.Parse (pValue); break;
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
            sWrite.AppendLine (String.Format ("{0}:{1}", "rowsNumerics", rowsNumerics));

            List<string> numericTypes = new (),
                         tracingTypes = new ();

            listNumerics.ForEach (o => { numericTypes.Add (o.ControlType?.Value.ToString () ?? ""); });
            listTracings.ForEach (o => { tracingTypes.Add (o.Strip?.Lead?.Value.ToString () ?? ""); });
            sWrite.AppendLine (String.Format ("{0}:{1}", "numericTypes", string.Join (",", numericTypes)));
            sWrite.AppendLine (String.Format ("{0}:{1}", "tracingTypes", string.Join (",", tracingTypes)));

            return sWrite.ToString ();
        }

        public void SetAlarms (bool toEnable) {
            foreach (Alarm a in Instance?.Scenario?.DeviceMonitor.Alarms ?? new List<Alarm> ()) {
                a.Enabled = toEnable;

                if (toEnable == false && a.Alarming == true)
                    a.Alarming = false;
            }

            if (toEnable == false && (AudioPlayer?.IsPlaying ?? false))
                AudioPlayer.Stop ();
        }

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

        public void SetColorScheme (Color.Schemes scheme) {
            colorScheme = scheme;
            UpdateInterface ();
        }

        public override void TogglePause () {
            base.TogglePause ();

            if (State == States.Running)
                listTracings.ForEach (c => c.Strip?.Unpause ());
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

        public void AddNumeric () {
            rowsNumerics += 1;
            OnLayoutChange ();
        }

        public void RemoveNumeric (Controls.MonitorNumeric requestSender) {
            rowsNumerics -= 1;
            listNumerics.Remove (requestSender);
            OnLayoutChange ();
        }

        private void MenuClose_Click (object s, RoutedEventArgs e)
            => this.Close ();

        private void MenuAddNumeric_Click (object s, RoutedEventArgs e)
            => AddNumeric ();

        private void MenuAddTracing_Click (object s, RoutedEventArgs e)
            => AddTracing ();

        private void MenuTogglePause_Click (object s, RoutedEventArgs e)
            => TogglePause ();

        private void MenuColorScheme_Light (object sender, RoutedEventArgs e)
            => SetColorScheme (Color.Schemes.Light);

        private void MenuEnableAlarms (object sender, RoutedEventArgs e)
            => SetAlarms (true);

        private void MenuDisableAlarms (object sender, RoutedEventArgs e)
            => SetAlarms (false);

        private void MenuAudioOff (object sender, RoutedEventArgs e)
            => _ = SetAudioTone (ToneSources.None);

        private void MenuAudioECG (object sender, RoutedEventArgs e)
            => _ = SetAudioTone (ToneSources.ECG);

        private void MenuAudioSPO2 (object sender, RoutedEventArgs e)
            => _ = SetAudioTone (ToneSources.SPO2);

        private void MenuColorScheme_Dark (object sender, RoutedEventArgs e)
            => SetColorScheme (Color.Schemes.Dark);

        public override void OnClosing (object? sender, CancelEventArgs e) {
            base.OnClosing (sender, e);

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
                if (!AudioPlayer.IsPlaying) {
                    AudioPlayer.Media = new Media (Instance.AudioLib, AlarmMedia [alarm.Priority.GetHashCode ()]);
                    AudioPlayer.Play ();
                }

                return;
            } else if (alarm.Priority != AlarmActive) {
                AudioPlayer.Stop ();
                AudioPlayer.Media = new Media (Instance.AudioLib, AlarmMedia [alarm.Priority.GetHashCode ()]);
                AudioPlayer.Play ();

                AlarmActive = alarm.Priority;
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
            if (State != States.Running)
                return;

            listNumerics
                .Where (n
                    => n.ControlType?.Value != Controls.MonitorNumeric.ControlTypes.Values.ETCO2
                    && n.ControlType?.Value != Controls.MonitorNumeric.ControlTypes.Values.RR)
                .ToList ()
                .ForEach (n => Dispatcher.UIThread.InvokeAsync (n.UpdateVitals));
        }

        public override void OnTick_Vitals_Respiratory (object? sender, EventArgs e) {
            if (State != States.Running)
                return;

            listNumerics
                .Where (n
                    => n.ControlType?.Value == Controls.MonitorNumeric.ControlTypes.Values.ETCO2
                    || n.ControlType?.Value == Controls.MonitorNumeric.ControlTypes.Values.RR)
                .ToList ()
                .ForEach (n => Dispatcher.UIThread.InvokeAsync (n.UpdateVitals));
        }

        private void OnLayoutChange (List<string>? numericTypes = null, List<string>? tracingTypes = null) {
            // If numericTypes or tracingTypes are not null... then we are loading a file; clear lNumerics and lTracings!
            if (numericTypes != null)
                listNumerics.Clear ();
            if (tracingTypes != null)
                listTracings.Clear ();

            // Set default numeric types to populate
            if (numericTypes == null || numericTypes.Count == 0)
                numericTypes = new List<string> (new string [] { "ECG", "NIBP", "SPO2", "RR", "ETCO2", "ABP", "CVP", "T", "PA", "ICP", "IAP" });
            else if (numericTypes.Count < rowsNumerics) {
                List<string> buffer = new (new string [] { "ECG", "NIBP", "SPO2", "RR", "ETCO2", "ABP", "CVP", "T", "PA", "ICP", "IAP" });
                buffer.RemoveRange (0, numericTypes.Count);
                numericTypes.AddRange (buffer);
            }

            // Cap available amount of numerics
            rowsNumerics = II.Math.Clamp (rowsNumerics, 1, numericTypes.Count);
            for (int i = listNumerics.Count; i < rowsNumerics && i < numericTypes.Count; i++) {
                Controls.MonitorNumeric newNum;
                newNum = new Controls.MonitorNumeric (
                    Instance,
                    (Controls.MonitorNumeric.ControlTypes.Values)Enum.Parse (typeof (Controls.MonitorNumeric.ControlTypes.Values),
                    numericTypes [i]), colorScheme);
                listNumerics.Add (newNum);
            }

            // Set default tracing types to populate
            if (tracingTypes == null || tracingTypes.Count == 0)
                tracingTypes = new List<string> (new string [] { "ECG_II", "ECG_III", "SPO2", "RR", "ETCO2", "ABP", "CVP", "PA", "ICP" });
            else if (tracingTypes.Count < rowsTracings) {
                List<string> buffer = new (new string [] { "ECG_II", "ECG_III", "SPO2", "RR", "ETCO2", "ABP", "CVP", "PA", "ICP" });
                buffer.RemoveRange (0, tracingTypes.Count);
                tracingTypes.AddRange (buffer);
            }

            // Cap available amount of tracings
            rowsTracings = II.Math.Clamp (rowsTracings, 1, tracingTypes.Count);
            for (int i = listTracings.Count; i < rowsTracings && i < tracingTypes.Count; i++) {
                Strip newStrip = new ((Lead.Values)Enum.Parse (typeof (Lead.Values), tracingTypes [i]), 6f);
                Controls.MonitorTracing newTracing = new (Instance, newStrip, colorScheme);
                listTracings.Add (newTracing);
            }

            // Reset the UI container and repopulate with the UI elements
            Grid gridNumerics = this.FindControl<Grid> ("gridNumerics");
            Grid gridTracings = this.FindControl<Grid> ("gridTracings");

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

        public override void OnPhysiologyEvent (object? sender, Physiology.PhysiologyEventArgs e) {
            switch (e.EventType) {
                default: break;
                case Physiology.PhysiologyEventTypes.Vitals_Change:
                    listTracings.ForEach (c => {
                        c.Strip?.ClearFuture (Instance?.Physiology);
                        c.Strip?.Add_Beat__Cardiac_Baseline (Instance?.Physiology);
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

                    /* Iterations and trigger for auto-scaling pressure waveform strips */

                    autoScale_iter -= 1;
                    if (autoScale_iter <= 0) {
                        for (int i = 0; i < listTracings.Count; i++) {
                            listTracings [i].Strip?.SetAutoScale (Instance?.Physiology);
                            Dispatcher.UIThread.InvokeAsync (listTracings [i].UpdateScale);
                        }

                        autoScale_iter = Strip.DefaultAutoScale_Iterations;
                    }
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