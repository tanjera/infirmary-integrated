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

        /* Variables controlling for audio alarms */
        private Timer AlarmTimer = new ();
        private Alarm.Priorities? AlarmActive;
        private List<Alarm> AlarmRefs = new ();
        private MediaPlayer? AlarmPlayer;
        private List<StreamMediaInput> AlarmMedia;

        public DeviceMonitor () {
            InitializeComponent ();
        }

        public DeviceMonitor (App? app) : base (app) {
            InitializeComponent ();
#if DEBUG
            this.AttachDevTools ();
#endif

            DataContext = this;
            InitAudio ();
            InitTimers ();
            InitInterface ();

            OnLayoutChange ();
        }

        ~DeviceMonitor () {
            AlarmTimer?.Dispose ();
            AlarmPlayer?.Dispose ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private void InitAudio () {
            if (Instance?.AudioLib is null)
                return;

            AlarmPlayer = new MediaPlayer (Instance.AudioLib);

            var assets = AvaloniaLocator.Current.GetService<Avalonia.Platform.IAssetLoader> ();
            AlarmMedia = new () {
                new (assets.Open (new Uri ("avares://Infirmary Integrated/Resources/Alarm_Low_Priority.wav"))),
                new (assets.Open (new Uri ("avares://Infirmary Integrated/Resources/Alarm_Medium_Priority.wav"))),
                new (assets.Open (new Uri ("avares://Infirmary Integrated/Resources/Alarm_High_Priority.wav")))
            };
        }

        private void InitTimers () {
            if (Instance is null)
                return;

            Instance.Timer_Main.Elapsed += AlarmTimer.Process;
            Instance.Timer_Main.Elapsed += TimerTracing.Process;
            Instance.Timer_Main.Elapsed += TimerVitals_Cardiac.Process;
            Instance.Timer_Main.Elapsed += TimerVitals_Respiratory.Process;

            AlarmTimer.Set (2500);
            TimerTracing.Set (Draw.RefreshTime);
            TimerVitals_Cardiac.Set (3000);
            TimerVitals_Respiratory.Set (5000);

            AlarmTimer.Tick += OnTick_Alarm;
            TimerTracing.Tick += OnTick_Tracing;
            TimerVitals_Cardiac.Tick += OnTick_Vitals_Cardiac;
            TimerVitals_Respiratory.Tick += OnTick_Vitals_Respiratory;

            AlarmTimer.Start ();
            TimerTracing.Start ();
            TimerVitals_Cardiac.Start ();
            TimerVitals_Respiratory.Start ();
        }

        private void InitInterface () {
            // Populate UI strings per language selection

            if (Instance is not null) {
                this.FindControl<Window> ("wdwDeviceMonitor").Title = Instance.Language.Localize ("CM:WindowTitle");
                this.FindControl<MenuItem> ("menuDevice").Header = Instance.Language.Localize ("MENU:MenuDeviceOptions");
                this.FindControl<MenuItem> ("menuPauseDevice").Header = Instance.Language.Localize ("MENU:MenuPauseDevice");
                this.FindControl<MenuItem> ("menuAddNumeric").Header = Instance.Language.Localize ("MENU:MenuAddNumeric");
                this.FindControl<MenuItem> ("menuAddTracing").Header = Instance.Language.Localize ("MENU:MenuAddTracing");
                this.FindControl<MenuItem> ("menuCloseDevice").Header = Instance.Language.Localize ("MENU:MenuCloseDevice");

                this.FindControl<MenuItem> ("menuAlarms").Header = Instance.Language.Localize ("MENU:MenuAlarms");
                this.FindControl<MenuItem> ("menuAlarmsEnable").Header = Instance.Language.Localize ("MENU:MenuAlarmsEnable");
                this.FindControl<MenuItem> ("menuAlarmsDisable").Header = Instance.Language.Localize ("MENU:MenuAlarmsDisable");

                this.FindControl<MenuItem> ("menuColor").Header = Instance.Language.Localize ("MENU:MenuColorScheme");
                this.FindControl<MenuItem> ("menuColorLight").Header = Instance.Language.Localize ("MENU:MenuColorSchemeLight");
                this.FindControl<MenuItem> ("menuColorDark").Header = Instance.Language.Localize ("MENU:MenuColorSchemeDark");
            }
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

        public async Task Load_Process (string inc) {
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
            foreach (Alarm a in Instance?.Scenario?.DeviceMonitor.Alarms ?? new List<Alarm> ())
                a.Enabled = toEnable;
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

        private void MenuColorScheme_Dark (object sender, RoutedEventArgs e)
            => SetColorScheme (Color.Schemes.Dark);

        private void OnTick_Alarm (object? sender, EventArgs e) {
            if (AlarmPlayer is null || Instance?.AudioLib is null)
                return;

            AlarmRefs.Clear ();

            foreach (Controls.MonitorNumeric n in listNumerics) {
                if (n.AlarmRef is not null)
                    AlarmRefs.Add (n.AlarmRef);
            }

            Alarm? alarm = AlarmRefs.Find (a => a.Alarming ?? false && a.Priority == Alarm.Priorities.High);
            if (alarm is null)
                alarm = AlarmRefs.Find (a => a.Alarming ?? false && a.Priority == Alarm.Priorities.Medium);
            if (alarm is null)
                alarm = AlarmRefs.Find (a => a.Alarming ?? false && a.Priority == Alarm.Priorities.Low);

            if (alarm is null) {
                AlarmPlayer.Stop ();
                AlarmActive = null;
                return;
            } else if (alarm.Priority == AlarmActive) {
                if (!AlarmPlayer.IsPlaying) {
                    AlarmPlayer.Media = new Media (Instance.AudioLib, AlarmMedia [alarm.Priority.GetHashCode ()]);
                    AlarmPlayer.Play ();
                }

                return;
            } else if (alarm.Priority != AlarmActive) {
                AlarmPlayer.Stop ();
                AlarmPlayer.Media = new Media (Instance.AudioLib, AlarmMedia [alarm.Priority.GetHashCode ()]);
                AlarmPlayer.Play ();

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

        public override void OnPatientEvent (object? sender, Patient.PatientEventArgs e) {
            switch (e.EventType) {
                default: break;
                case Patient.PatientEventTypes.Vitals_Change:
                    listTracings.ForEach (c => {
                        c.Strip?.ClearFuture (Instance?.Patient);
                        c.Strip?.Add_Beat__Cardiac_Baseline (Instance?.Patient);
                    });

                    listNumerics.ForEach (n => n.UpdateVitals ());
                    break;

                case Patient.PatientEventTypes.Defibrillation:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Defibrillation (Instance?.Patient));
                    break;

                case Patient.PatientEventTypes.Pacermaker_Spike:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Pacemaker (Instance?.Patient));
                    break;

                case Patient.PatientEventTypes.Cardiac_Baseline:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Baseline (Instance?.Patient));
                    break;

                case Patient.PatientEventTypes.Cardiac_Atrial_Electric:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Atrial_Electrical (Instance?.Patient));
                    break;

                case Patient.PatientEventTypes.Cardiac_Ventricular_Electric:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Ventricular_Electrical (Instance?.Patient));
                    break;

                case Patient.PatientEventTypes.Cardiac_Atrial_Mechanical:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Atrial_Mechanical (Instance?.Patient));
                    break;

                case Patient.PatientEventTypes.Cardiac_Ventricular_Mechanical:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Ventricular_Mechanical (Instance?.Patient));

                    /* Iterations and trigger for auto-scaling pressure waveform strips */

                    autoScale_iter -= 1;
                    if (autoScale_iter <= 0) {
                        for (int i = 0; i < listTracings.Count; i++) {
                            listTracings [i].Strip?.SetAutoScale (Instance?.Patient);
                            Dispatcher.UIThread.InvokeAsync (listTracings [i].UpdateScale);
                        }

                        autoScale_iter = Strip.DefaultAutoScale_Iterations;
                    }
                    break;

                case Patient.PatientEventTypes.Respiratory_Baseline:
                    listTracings.ForEach (c => c.Strip?.Add_Breath__Respiratory_Baseline (Instance?.Patient));
                    break;

                case Patient.PatientEventTypes.Respiratory_Inspiration:
                    listTracings.ForEach (c => c.Strip?.Add_Breath__Respiratory_Inspiration (Instance?.Patient));
                    break;

                case Patient.PatientEventTypes.Respiratory_Expiration:
                    listTracings.ForEach (c => c.Strip?.Add_Breath__Respiratory_Expiration (Instance?.Patient));
                    break;
            }
        }
    }
}