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
    /// Interaction logic for DeviceMonitor.xaml
    /// </summary>
    public partial class DeviceMonitor : Window {

        int lRowsTracings = 3,
            lRowsNumerics = 3;
        int lFontsize = 2;
        bool lFullscreen = false;

        Patient rPatient;
        List<Controls.Tracing> listTracings = new List<Controls.Tracing> ();
        List<Controls.Numeric> listNumerics = new List<Controls.Numeric> ();

        Timer timerTracing = new Timer (),
              timerVitals = new Timer ();

        // Define WPF UI commands for binding
        private ICommand icPauseDevice, icCloseDevice, icExitProgram;
        public ICommand IC_PauseDevice { get { return icPauseDevice; } }
        public ICommand IC_CloseDevice { get { return icCloseDevice; } }
        public ICommand IC_ExitProgram { get { return icExitProgram; } }


        public DeviceMonitor () {
            InitializeComponent ();

            InitInterface ();

            timerTracing.Interval = Waveforms.Draw_Refresh;
            App.Timer_Main.Tick += timerTracing.Process;
            timerTracing.Tick += OnTick_Tracing;
            timerTracing.Start ();

            timerVitals.Interval = 5000;
            App.Timer_Main.Tick += timerVitals.Process;
            timerVitals.Tick += OnTick_Vitals;
            timerVitals.Start ();

            OnLayoutChange ();
        }

        private void InitInterface () {
            // Initiate ICommands for KeyBindings
            icPauseDevice = new ActionCommand (() => {
                rPatient.TogglePause ();
                ApplyPause ();
            });
            icCloseDevice = new ActionCommand (() => this.Close ());
            icExitProgram = new ActionCommand (() => App.Patient_Editor.RequestExit ());


            // Populate UI strings per language selection
            Languages.Values l = App.Language.Value;

            wdwDeviceMonitor.Title = Strings.Lookup (l, "CM:WindowTitle");
            menuDevice.Header = Strings.Lookup (l, "CM:MenuDeviceOptions");
            menuPauseDevice.Header = Strings.Lookup (l, "CM:MenuPauseDevice");
            menuAddNumeric.Header = Strings.Lookup (l, "CM:MenuAddNumeric");
            menuAddTracing.Header = Strings.Lookup (l, "CM:MenuAddTracing");
            menuFontSize.Header = Strings.Lookup (l, "CM:MenuFontSize");
            menuFontSizeDecrease.Header = Strings.Lookup (l, "CM:MenuFontSizeDecrease");
            menuFontSizeIncrease.Header = Strings.Lookup (l, "CM:MenuFontSizeIncrease");
            menuToggleFullscreen.Header = Strings.Lookup (l, "CM:MenuToggleFullscreen");
            menuCloseDevice.Header = Strings.Lookup (l, "CM:MenuCloseDevice");
            menuExitProgram.Header = Strings.Lookup (l, "CM:MenuExitProgram");
        }

        public void Load_Process (string inc) {
            StringReader sRead = new StringReader (inc);
            List<string> numericTypes = new List<string> (),
                         tracingTypes = new List<string> ();

            try {
                string line;
                while ((line = sRead.ReadLine ()) != null) {
                    if (line.Contains (":")) {
                        string pName = line.Substring (0, line.IndexOf (':')),
                                pValue = line.Substring (line.IndexOf (':') + 1);
                        switch (pName) {
                            default: break;
                            case "tRowsTracings": lRowsTracings = int.Parse (pValue); break;
                            case "tRowsNumerics": lRowsNumerics = int.Parse (pValue); break;
                            case "tFontsize": lFontsize = int.Parse (pValue); break;
                            case "tFullscreen": lFullscreen = bool.Parse (pValue); break;
                            case "numericTypes": numericTypes.AddRange (pValue.Split (',')); break;
                            case "tracingTypes": tracingTypes.AddRange (pValue.Split (',')); break;
                        }
                    }
                }
            } catch {
                sRead.Close ();
                return;
            }

            sRead.Close ();

            OnLayoutChange (numericTypes, tracingTypes);

            ApplyFontSize ();
        }

        public string Save () {
            StringBuilder sWrite = new StringBuilder ();

            sWrite.AppendLine (String.Format ("{0}:{1}", "tRowsTracings", lRowsTracings));
            sWrite.AppendLine (String.Format ("{0}:{1}", "tRowsNumerics", lRowsNumerics));
            sWrite.AppendLine (String.Format ("{0}:{1}", "tFontsize", lFontsize));
            sWrite.AppendLine (String.Format ("{0}:{1}", "tFullscreen", lFullscreen));

            List<string> numericTypes = new List<string> (),
                         tracingTypes = new List<string> ();

            listNumerics.ForEach (o => { numericTypes.Add (o.controlType.Value.ToString ()); });
            listTracings.ForEach (o => { tracingTypes.Add (o.rStrip.Lead.Value.ToString ()); });
            sWrite.AppendLine (String.Format ("{0}:{1}", "numericTypes", string.Join (",", numericTypes)));
            sWrite.AppendLine (String.Format ("{0}:{1}", "tracingTypes", string.Join (",", tracingTypes)));

            return sWrite.ToString ();
        }

        public void SetPatient (Patient iPatient) {
            rPatient = iPatient;
            OnTick_Vitals (null, new EventArgs ());
        }

        private void ApplyFontSize () {
            foreach (Controls.Numeric rn in listNumerics)
                rn.SetFontSize (lFontsize * 0.5f);
        }

        private void ApplyFullScreen () {
            menuToggleFullscreen.IsChecked = lFullscreen;

            switch (lFullscreen) {
                default:
                case false:
                    wdwDeviceMonitor.WindowStyle = WindowStyle.SingleBorderWindow;
                    wdwDeviceMonitor.WindowState = WindowState.Normal;
                    wdwDeviceMonitor.ResizeMode = ResizeMode.CanResize;
                    break;

                case true:
                    wdwDeviceMonitor.WindowStyle = WindowStyle.None;
                    wdwDeviceMonitor.WindowState = WindowState.Maximized;
                    wdwDeviceMonitor.ResizeMode = ResizeMode.NoResize;
                    break;
            }
        }


        private void ApplyPause () {
            menuPauseDevice.IsChecked = rPatient.Paused;

            if (!rPatient.Paused)
                foreach (Controls.Tracing c in listTracings)
                    c.rStrip.Unpause ();
        }

        private void MenuClose_Click (object sender, RoutedEventArgs e) {
            this.Close ();
        }

        private void MenuExit_Click (object sender, RoutedEventArgs e) {
            App.Patient_Editor.RequestExit ();
        }

        private void MenuFontSize_Click (object sender, RoutedEventArgs e) {
            switch (((MenuItem)sender).Name) {
                case "menuFontSizeDecrease": lFontsize = Utility.Clamp (lFontsize - 1, 1, 5); break;
                case "menuFontSizeIncrease": lFontsize = Utility.Clamp (lFontsize + 1, 1, 5); break;
            }

            ApplyFontSize ();
        }

        private void MenuAddNumeric_Click (object sender, RoutedEventArgs e) {
            OnLayoutChange ();
        }

        private void MenuAddTracing_Click (object sender, RoutedEventArgs e) {
            OnLayoutChange ();
        }

        private void MenuTogglePause_Click (object sender, RoutedEventArgs e) {
            rPatient.TogglePause ();
            ApplyPause ();
        }

        private void MenuFullscreen_Click (object sender, RoutedEventArgs e) {
            lFullscreen = !lFullscreen;
            ApplyFullScreen ();
        }

        private void OnTick_Tracing (object sender, EventArgs e) {
            if (rPatient.Paused)
                return;

            foreach (Controls.Tracing c in listTracings) {
                c.rStrip.Scroll ();
                c.Draw ();
            }
        }

        private void OnTick_Vitals (object sender, EventArgs e) {
            if (rPatient.Paused)
                return;

            foreach (Controls.Numeric v in listNumerics)
                v.Update (rPatient);
        }

        private void OnLayoutChange (List<string> numericTypes = null, List<string> tracingTypes = null) {
            // If numericTypes or tracingTypes are not null... then we are loading a file; clear lNumerics and lTracings!
            if (numericTypes != null)
                listNumerics.Clear ();
            if (tracingTypes != null)
                listTracings.Clear ();

            // Set default numeric types to populate
            if (numericTypes == null || numericTypes.Count == 0)
                numericTypes = new List<string> (new string [] { "ECG", "NIBP", "SPO2", "CVP", "ABP" });
            else if (numericTypes.Count < lRowsNumerics) {
                List<string> buffer = new List<string> (new string [] { "ECG", "NIBP", "SPO2", "CVP", "ABP" });
                buffer.RemoveRange (0, numericTypes.Count);
                numericTypes.AddRange (buffer);
            }

            for (int i = listNumerics.Count; i < lRowsNumerics && i < numericTypes.Count; i++) {
                Controls.Numeric newNum;
                newNum = new Controls.Numeric ((Controls.Numeric.ControlType.Values)Enum.Parse (typeof (Controls.Numeric.ControlType.Values), numericTypes [i]));
                listNumerics.Add (newNum);
            }

            // Set default tracing types to populate
            if (tracingTypes == null || tracingTypes.Count == 0)
                tracingTypes = new List<string> (new string [] { "ECG_II", "ECG_III", "SPO2", "CVP", "ABP" });
            else if (tracingTypes.Count < lRowsTracings) {
                List<string> buffer = new List<string> (new string [] { "ECG_II", "ECG_III", "SPO2", "CVP", "ABP" });
                buffer.RemoveRange (0, tracingTypes.Count);
                tracingTypes.AddRange (buffer);
            }

            for (int i = listTracings.Count; i < lRowsTracings && i < tracingTypes.Count; i++) {
                Strip newStrip = new Strip (6f, (Leads.Values)Enum.Parse (typeof (Leads.Values), tracingTypes [i]));
                Controls.Tracing newTracing = new Controls.Tracing (newStrip);
                newTracing.TracingEdited += OnTracingEdited;
                listTracings.Add (newTracing);
            }

            // Reset the UI container and repopulate with the UI elements
            gridNumerics.Children.Clear ();
            for (int i = 0; i < lRowsNumerics && i < listNumerics.Count; i++) {
                gridNumerics.RowDefinitions.Add(new RowDefinition ());
                listNumerics [i].SetValue (Grid.RowProperty, i);
                gridNumerics.Children.Add (listNumerics [i]);
            }

            gridTracings.Children.Clear ();
            for (int i = 0; i < lRowsTracings && i < listTracings.Count; i++) {
                gridTracings.RowDefinitions.Add (new RowDefinition ());
                listTracings [i].SetValue (Grid.RowProperty, i);
                gridTracings.Children.Add (listTracings [i]);
            }
        }

        public void OnPatientEvent (object sender, Patient.PatientEvent_Args e) {
            switch (e.EventType) {
                default: break;
                case Patient.PatientEvent_Args.EventTypes.Vitals_Change:
                    rPatient = e.Patient;

                    foreach (Controls.Tracing c in listTracings) {
                        c.rStrip.ClearFuture ();
                        c.rStrip.Add_Beat__Cardiac_Baseline (rPatient);
                    }
                    foreach (Controls.Numeric n in listNumerics)
                        n.Update (rPatient);
                    break;

                case Patient.PatientEvent_Args.EventTypes.Cardiac_Baseline:
                    foreach (Controls.Tracing c in listTracings)
                        c.rStrip.Add_Beat__Cardiac_Baseline (rPatient);
                    break;

                case Patient.PatientEvent_Args.EventTypes.Cardiac_Atrial:
                    foreach (Controls.Tracing c in listTracings)
                        c.rStrip.Add_Beat__Cardiac_Atrial (rPatient);
                    break;

                case Patient.PatientEvent_Args.EventTypes.Cardiac_Ventricular:
                    foreach (Controls.Tracing c in listTracings)
                        c.rStrip.Add_Beat__Cardiac_Ventricular (rPatient);
                    break;

                case Patient.PatientEvent_Args.EventTypes.Respiratory_Baseline:
                    foreach (Controls.Tracing c in listTracings)
                        c.rStrip.Add_Beat__Respiratory_Baseline (rPatient);
                    break;

                case Patient.PatientEvent_Args.EventTypes.Respiratory_Inspiration:
                    foreach (Controls.Tracing c in listTracings)
                        c.rStrip.Add_Beat__Respiratory_Inspiration (rPatient);
                    break;

                case Patient.PatientEvent_Args.EventTypes.Respiratory_Expiration:
                    foreach (Controls.Tracing c in listTracings)
                        c.rStrip.Add_Beat__Respiratory_Expiration (rPatient);
                    break;
            }
        }


        private void OnTracingEdited (object sender, Controls.Tracing.TracingEdited_EventArgs e) {
            foreach (Controls.Tracing c in listTracings) {
               if (c == sender) {
                   c.SetLead (e.Lead.Value);
               }

               c.rStrip.Reset ();
               c.rStrip.Add_Beat__Cardiac_Baseline (rPatient);
               c.rStrip.Add_Beat__Respiratory_Baseline (rPatient);
           }
        }

        private void OnFormResize (object sender, RoutedEventArgs e) {
            OnLayoutChange ();
        }
    }
}