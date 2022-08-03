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

    public partial class DeviceDefib : DeviceWindow {

        // Device settings
        public Modes Mode = Modes.DEFIB;

        public bool Charging = false,
                    Charged = false,
                    Analyzed = false;

        public int Energy = 200,
                    PacerEnergy = 0,
                    PacerRate = 80;

        private int rowsTracings = 1;
        private int colsNumerics = 4;

        private int autoScale_iter = Strip.DefaultAutoScale_Iterations;

        private Color.Schemes colorScheme = Color.Schemes.Dark;

        private List<Controls.DefibTracing> listTracings = new ();
        private List<Controls.DefibNumeric> listNumerics = new ();

        /* Variables for audio tones (QRS or SPO2 beeps)*/
        private ToneSources ToneSource = ToneSources.None;
        public MediaPlayer? TonePlayer;
        private MemoryStream? ToneMedia;

        public enum Modes {
            DEFIB,
            SYNC,
            PACER
        };

        public enum ToneSources {
            None,
            ECG,
            SPO2
        }

        public DeviceDefib () {
            InitializeComponent ();
        }

        public DeviceDefib (App? app) : base (app) {
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
            if (Instance?.AudioLib is null)
                return;

            base.InitAudio ();

            TonePlayer = new MediaPlayer (Instance.AudioLib);
        }

        public override void DisposeAudio () {
            base.DisposeAudio ();

            if (TonePlayer is not null) {
                TonePlayer.Stop ();
                TonePlayer.Dispose ();
                TonePlayer = null;
            }

            if (ToneMedia is not null) {
                ToneMedia.Close ();
                ToneMedia.Dispose ();
                ToneMedia = null;
            }
        }

        private void InitInterface () {
            // Populate UI strings per language selection
            if (Instance?.Language is not null) {
                this.FindControl<Window> ("wdwDeviceDefib").Title = Instance.Language.Localize ("DEFIB:WindowTitle");
                this.FindControl<MenuItem> ("menuDevice").Header = Instance.Language.Localize ("MENU:MenuDeviceOptions");
                this.FindControl<MenuItem> ("menuPauseDevice").Header = Instance.Language.Localize ("MENU:MenuPauseDevice");
                this.FindControl<MenuItem> ("menuAddNumeric").Header = Instance.Language.Localize ("MENU:MenuAddNumeric");
                this.FindControl<MenuItem> ("menuAddTracing").Header = Instance.Language.Localize ("MENU:MenuAddTracing");
                this.FindControl<MenuItem> ("menuCloseDevice").Header = Instance.Language.Localize ("MENU:MenuCloseDevice");

                this.FindControl<MenuItem> ("menuAudio").Header = Instance.Language.Localize ("MENU:MenuAudio");
                this.FindControl<MenuItem> ("menuAudioOff").Header = Instance.Language.Localize ("MENU:MenuAudioOff");
                this.FindControl<MenuItem> ("menuAudioECG").Header = Instance.Language.Localize ("MENU:MenuAudioECG");
                this.FindControl<MenuItem> ("menuAlarmsSPO2").Header = Instance.Language.Localize ("MENU:MenuAudioSPO2");

                this.FindControl<MenuItem> ("menuColor").Header = Instance.Language.Localize ("MENU:MenuColorScheme");
                this.FindControl<MenuItem> ("menuColorLight").Header = Instance.Language.Localize ("MENU:MenuColorSchemeLight");
                this.FindControl<MenuItem> ("menuColorDark").Header = Instance.Language.Localize ("MENU:MenuColorSchemeDark");

                this.FindControl<TextBlock> ("btntxtDefib").Text = Instance.Language.Localize ("DEFIB:Defibrillator");
                this.FindControl<TextBlock> ("txtEnergyAmount").Text = Instance.Language.Localize ("DEFIB:EnergyAmount");
                this.FindControl<TextBlock> ("btntxtEnergyDecrease").Text = Instance.Language.Localize ("DEFIB:Decrease");
                this.FindControl<TextBlock> ("btntxtEnergyIncrease").Text = Instance.Language.Localize ("DEFIB:Increase");
                this.FindControl<TextBlock> ("btntxtCharge").Text = Instance.Language.Localize ("DEFIB:Charge");
                this.FindControl<TextBlock> ("btntxtShock").Text = Instance.Language.Localize ("DEFIB:Shock");
                this.FindControl<TextBlock> ("btntxtAnalyze").Text = Instance.Language.Localize ("DEFIB:Analyze");
                this.FindControl<TextBlock> ("btntxtSync").Text = Instance.Language.Localize ("DEFIB:Sync");

                this.FindControl<TextBlock> ("btntxtPacer").Text = Instance.Language.Localize ("DEFIB:Pacer");
                this.FindControl<TextBlock> ("txtPaceRate").Text = Instance.Language.Localize ("DEFIB:Rate");
                this.FindControl<TextBlock> ("btntxtPaceRateDecrease").Text = Instance.Language.Localize ("DEFIB:Decrease");
                this.FindControl<TextBlock> ("btntxtPaceRateIncrease").Text = Instance.Language.Localize ("DEFIB:Increase");
                this.FindControl<TextBlock> ("txtPaceEnergy").Text = Instance.Language.Localize ("DEFIB:EnergyAmount");
                this.FindControl<TextBlock> ("btntxtPaceEnergyDecrease").Text = Instance.Language.Localize ("DEFIB:Decrease");
                this.FindControl<TextBlock> ("btntxtPaceEnergyIncrease").Text = Instance.Language.Localize ("DEFIB:Increase");
                this.FindControl<TextBlock> ("btntxtPacePause").Text = Instance.Language.Localize ("DEFIB:Pause");
            }
        }

        private void UpdateInterface () {
            Dispatcher.UIThread.InvokeAsync ((Action)(() => {
                listNumerics
                    .Where<Controls.DefibNumeric> ((Func<Controls.DefibNumeric, bool>)(n => n.ControlType?.Value == Controls.DefibNumeric.ControlTypes.Values.DEFIB))
                    .ToList<Controls.DefibNumeric> ()
                    .ForEach (n => n.UpdateVitals ());

                for (int i = 0; i < listTracings.Count; i++)
                    listTracings [i].SetColorScheme (colorScheme);

                for (int i = 0; i < listNumerics.Count; i++)
                    listNumerics [i].SetColorScheme (colorScheme);

                Window window = this.FindControl<Window> ("wdwDeviceDefib");
                window.Background = Color.GetBackground (Color.Devices.DeviceDefib, colorScheme);
            }));
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
                            case "Charged": Charged = bool.Parse (pValue); break;
                            case "Analyzed": Analyzed = bool.Parse (pValue); break;
                            case "Energy": Energy = int.Parse (pValue); break;
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

            listNumerics.ForEach ((Action<Controls.DefibNumeric>)(o => { numericTypes.Add ((string)o.ControlType.Value.ToString ()); }));
            listTracings.ForEach (o => { tracingTypes.Add (o.Strip?.Lead?.Value.ToString () ?? ""); });
            sWrite.AppendLine (String.Format ("{0}:{1}", "numericTypes", string.Join (",", numericTypes)));
            sWrite.AppendLine (String.Format ("{0}:{1}", "tracingTypes", string.Join (",", tracingTypes)));

            sWrite.AppendLine (String.Format ("{0}:{1}", "Mode", Mode));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Charged", Charged));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Analyzed", Analyzed));
            sWrite.AppendLine (String.Format ("{0}:{1}", "Energy", Energy));
            sWrite.AppendLine (String.Format ("{0}:{1}", "PacerEnergy", PacerEnergy));
            sWrite.AppendLine (String.Format ("{0}:{1}", "PacerRate", PacerRate));

            return sWrite.ToString ();
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

        public async Task PlayAudioTone (ToneSources trigger, Patient? p) {
            if (TonePlayer is null || Instance?.AudioLib is null)
                return;

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
                listTracings.ForEach (c => c?.Strip?.Unpause ());
        }

        public void AddTracing () {
            rowsTracings += 1;
            OnLayoutChange ();
        }

        public void AddNumeric () {
            colsNumerics += 1;
            OnLayoutChange ();
        }

        public void RemoveTracing (Controls.DefibTracing requestSender) {
            rowsTracings -= 1;
            listTracings.Remove (requestSender);
            OnLayoutChange ();
        }

        public void RemoveNumeric (Controls.DefibNumeric requestSender) {
            colsNumerics -= 1;
            listNumerics.Remove (requestSender);
            OnLayoutChange ();
        }

        private void UpdatePacemaker () {
            _ = Instance?.Patient?.Pacemaker (Mode == Modes.PACER, PacerRate, PacerEnergy);
        }

        private void ButtonDefib_Click (object s, RoutedEventArgs e) {
            Mode = Modes.DEFIB;
            UpdatePacemaker ();
            UpdateInterface ();
        }

        private void ButtonEnergyDecrease_Click (object s, RoutedEventArgs e) {
            if (Mode != Modes.DEFIB && Mode != Modes.SYNC)
                return;

            Energy = II.Math.Clamp (Energy - 20, 0, 200);
            UpdateInterface ();
        }

        private void ButtonEnergyIncrease_Click (object s, RoutedEventArgs e) {
            if (Mode != Modes.DEFIB && Mode != Modes.SYNC)
                return;

            Energy = II.Math.Clamp (Energy + 20, 0, 200);
            UpdateInterface ();
        }

        private void ButtonCharge_Click (object s, RoutedEventArgs e) {
            // Only charge if in Defib or Sync mode...
            if (Mode != Modes.DEFIB && Mode != Modes.SYNC)
                return;

            Analyzed = false;

            if (TimerAncillary_Delay.IsLocked) {
                Charging = false;
                Charged = true;
            } else {
                Charging = true;
                Charged = false;
                TimerAncillary_Delay.Lock ();
                TimerAncillary_Delay.Tick += OnTick_ChargingComplete;
                TimerAncillary_Delay.Set (3000);
                TimerAncillary_Delay.Start ();
            }

            UpdateInterface ();
        }

        private void ButtonShock_Click (object s, RoutedEventArgs e) {
            if (!Charged)
                return;

            Charged = false;

            switch (Mode) {
                default: break;
                case Modes.DEFIB: _ = Instance.Patient?.Defibrillate (); break;
                case Modes.SYNC: _ = Instance.Patient?.Cardiovert (); break;
            }

            UpdateInterface ();
        }

        private void ButtonAnalyze_Click (object s, RoutedEventArgs e) {
            Analyzed = true;
            Mode = Modes.DEFIB;
            UpdatePacemaker ();
            UpdateInterface ();
        }

        private void ButtonSync_Click (object s, RoutedEventArgs e) {
            Analyzed = false;
            Mode = (Mode != Modes.SYNC ? Modes.SYNC : Modes.DEFIB);
            UpdatePacemaker ();
            UpdateInterface ();
        }

        private void ButtonPacer_Click (object s, RoutedEventArgs e) {
            Analyzed = false;
            Mode = (Mode != Modes.PACER ? Modes.PACER : Modes.DEFIB);
            UpdatePacemaker ();
            UpdateInterface ();
        }

        private void ButtonPaceRateDecrease_Click (object s, RoutedEventArgs e) {
            if (Mode != Modes.PACER)
                return;

            PacerRate = II.Math.Clamp (PacerRate - 5, 0, 200);
            UpdatePacemaker ();
            UpdateInterface ();
        }

        private void ButtonPaceRateIncrease_Click (object s, RoutedEventArgs e) {
            if (Mode != Modes.PACER)
                return;

            PacerRate = II.Math.Clamp (PacerRate + 5, 0, 200);
            UpdatePacemaker ();
            UpdateInterface ();
        }

        private void ButtonPaceEnergyDecrease_Click (object s, RoutedEventArgs e) {
            if (Mode != Modes.PACER)
                return;

            PacerEnergy = II.Math.Clamp (PacerEnergy - 5, 0, 200);
            UpdatePacemaker ();
            UpdateInterface ();
        }

        private void ButtonPaceEnergyIncrease_Click (object s, RoutedEventArgs e) {
            if (Mode != Modes.PACER)
                return;

            PacerEnergy = II.Math.Clamp (PacerEnergy + 5, 0, 200);
            UpdatePacemaker ();
            UpdateInterface ();
        }

        private void ButtonPacePause_Click (object s, RoutedEventArgs e)
            => _ = Instance?.Patient?.PacemakerPause ();

        private void MenuClose_Click (object s, RoutedEventArgs e)
            => this.Close ();

        private void MenuAddNumeric_Click (object s, RoutedEventArgs e)
            => AddNumeric ();

        private void MenuAddTracing_Click (object s, RoutedEventArgs e)
            => AddTracing ();

        private void MenuTogglePause_Click (object s, RoutedEventArgs e)
            => TogglePause ();

        private void MenuAudioOff (object sender, RoutedEventArgs e)
            => _ = SetAudioTone (ToneSources.None);

        private void MenuAudioECG (object sender, RoutedEventArgs e)
            => _ = SetAudioTone (ToneSources.ECG);

        private void MenuAudioSPO2 (object sender, RoutedEventArgs e)
            => _ = SetAudioTone (ToneSources.SPO2);

        private void MenuColorScheme_Light (object sender, RoutedEventArgs e)
            => SetColorScheme (Color.Schemes.Light);

        private void MenuColorScheme_Dark (object sender, RoutedEventArgs e)
            => SetColorScheme (Color.Schemes.Dark);

        private void OnTick_ChargingComplete (object? sender, EventArgs e) {
            TimerAncillary_Delay.Stop ();
            TimerAncillary_Delay.Unlock ();
            TimerAncillary_Delay.Tick -= OnTick_ChargingComplete;

            Charging = false;
            Charged = true;

            UpdateInterface ();
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
                    => n.ControlType?.Value != Controls.DefibNumeric.ControlTypes.Values.ETCO2
                    && n.ControlType?.Value != Controls.DefibNumeric.ControlTypes.Values.RR)
                .ToList ()
                .ForEach (n => Dispatcher.UIThread.InvokeAsync (n.UpdateVitals));
        }

        public override void OnTick_Vitals_Respiratory (object? sender, EventArgs e) {
            if (State != States.Running)
                return;

            listNumerics
                .Where (n
                    => n.ControlType?.Value == Controls.DefibNumeric.ControlTypes.Values.ETCO2
                    || n.ControlType?.Value == Controls.DefibNumeric.ControlTypes.Values.RR)
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
            Grid gridNumerics = this.FindControl<Grid> ("gridNumerics");

            gridNumerics.Children.Clear ();
            gridNumerics.ColumnDefinitions.Clear ();
            for (int i = 0; i < colsNumerics && i < listNumerics.Count; i++) {
                gridNumerics.ColumnDefinitions.Add (new ColumnDefinition ());
                listNumerics [i].SetValue (Grid.ColumnProperty, i);
                gridNumerics.Children.Add (listNumerics [i]);
            }

            Grid gridTracings = this.FindControl<Grid> ("gridTracings");

            gridTracings.Children.Clear ();
            gridTracings.RowDefinitions.Clear ();
            for (int i = 0; i < rowsTracings && i < listTracings.Count; i++) {
                gridTracings.RowDefinitions.Add (new RowDefinition ());
                listTracings [i].SetValue (Grid.RowProperty, i);
                gridTracings.Children.Add (listTracings [i]);
            }
        }

        public new void OnPatientEvent (object? sender, Patient.PatientEventArgs e) {
            switch (e.EventType) {
                default: break;

                case Patient.PatientEventTypes.Vitals_Change:
                    listTracings.ForEach (c => {
                        c.Strip?.ClearFuture (Instance?.Patient);
                        c.Strip?.Add_Beat__Cardiac_Baseline (Instance?.Patient);
                    });

                    listNumerics.ForEach ((n) => Dispatcher.UIThread.InvokeAsync (n.UpdateVitals));
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
                    // QRS audio tone is only triggered by rhythms w/ a ventricular electrical (QRS complex) action
                    _ = PlayAudioTone (ToneSources.ECG, e.Patient);

                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Ventricular_Electrical (Instance?.Patient));
                    break;

                case Patient.PatientEventTypes.Cardiac_Atrial_Mechanical:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Atrial_Mechanical (Instance?.Patient));
                    break;

                case Patient.PatientEventTypes.Cardiac_Ventricular_Mechanical:
                    // SPO2 audio tone is only triggered  by rhythms w/ a ventricular mechanical action (systole)
                    _ = PlayAudioTone (ToneSources.SPO2, e.Patient);

                    listTracings.ForEach (c => c.Strip?.Add_Beat__Cardiac_Ventricular_Mechanical (Instance?.Patient));

                    /* Iterations and trigger for auto-scaling pressure waveform strips */
                    autoScale_iter -= 1;
                    if (autoScale_iter <= 0) {
                        for (int i = 0; i < listTracings.Count; i++) {
                            listTracings [i].Strip?.SetAutoScale (Instance?.Patient ?? new Patient ());
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