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

        int tRowsTracings = 3,
            tRowsNumerics = 3;
        int tFontsize = 2;
        bool tFullscreen = false;
        Utility.ColorScheme tColorScheme = Utility.ColorScheme.Normal;

        Patient tPatient;
        List<Channel> tChannels = new List<Channel> ();
        List<Controls.Numeric> tNumerics = new List<Controls.Numeric> ();

        // Define WPF UI commands for binding
        private ICommand icPauseDevice, icCloseDevice, icExitProgram;
        public ICommand IC_PauseDevice { get { return icPauseDevice; } }
        public ICommand IC_CloseDevice { get { return icCloseDevice; } }
        public ICommand IC_ExitProgram { get { return icExitProgram; } }



        public class Channel {
            public Strip cStrip;
            public Controls.Tracing cTracing;

            public Channel (Strip s, Controls.Tracing t) {
                cStrip = s;
                cTracing = t;
            }
        }

        public DeviceMonitor () {
            InitializeComponent ();

            InitInterface ();

            /* IMP
            timerTracing.Interval = Waveforms.Draw_Refresh;
            timerVitals.Interval = 5000;

            foreach (Utility.ColorScheme cs in Enum.GetValues (typeof (Utility.ColorScheme)))
                menuItem_ColorScheme.DropDownItems.Add (Utility.UnderscoreToSpace (cs.ToString ()), null, MenuColorScheme_Click);
            */

            OnLayoutChange ();
        }

        private void InitInterface () {
            // Initiate ICommands for KeyBindings
            icPauseDevice = new ActionCommand (() => {
                tPatient.TogglePause ();
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
                            case "tRowsTracings": tRowsTracings = int.Parse (pValue); break;
                            case "tRowsNumerics": tRowsNumerics = int.Parse (pValue); break;
                            case "tFontsize": tFontsize = int.Parse (pValue); break;
                            case "tFullscreen": tFullscreen = bool.Parse (pValue); break;
                            case "tColorScheme": tColorScheme = (Utility.ColorScheme)Enum.Parse (typeof (Utility.ColorScheme), line.Substring (line.IndexOf (':') + 1)); break;
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

            sWrite.AppendLine (String.Format ("{0}:{1}", "tRowsTracings", tRowsTracings));
            sWrite.AppendLine (String.Format ("{0}:{1}", "tRowsNumerics", tRowsNumerics));
            sWrite.AppendLine (String.Format ("{0}:{1}", "tFontsize", tFontsize));
            sWrite.AppendLine (String.Format ("{0}:{1}", "tFullscreen", tFullscreen));
            sWrite.AppendLine (String.Format ("{0}:{1}", "tColorScheme", tColorScheme));

            List<string> numericTypes = new List<string> (),
                         tracingTypes = new List<string> ();

            throw new NotImplementedException();
            //tNumerics.ForEach (o => { numericTypes.Add (o.controlType.Value.ToString ()); });
            tChannels.ForEach (o => { tracingTypes.Add (o.cStrip.Lead.Value.ToString ()); });
            sWrite.AppendLine (String.Format ("{0}:{1}", "numericTypes", string.Join (",", numericTypes)));
            sWrite.AppendLine (String.Format ("{0}:{1}", "tracingTypes", string.Join (",", tracingTypes)));

            return sWrite.ToString ();
        }

        public void SetPatient (Patient iPatient) {
            tPatient = iPatient;
            OnTick_Vitals (null, new EventArgs ());
        }

        private void ApplyFontSize () {
            throw new NotImplementedException ();
            /* IMP
            foreach (II.Controls.Numeric rn in tNumerics)
                rn.SetFontSize (tFontsize * 0.5f);
            */
        }

        private void ApplyFullScreen () {
            throw new NotImplementedException ();
            /* IMP
            menuItem_Fullscreen.Checked = tFullscreen;

            switch (tFullscreen) {
                default:
                case false:
                    this.TopMost = false;
                    this.FormBorderStyle = FormBorderStyle.Sizable;
                    this.WindowState = FormWindowState.Maximized;
                    break;

                case true:
                    this.TopMost = true;
                    this.FormBorderStyle = FormBorderStyle.None;
                    this.WindowState = FormWindowState.Maximized;
                    break;
            }
            */
        }


        private void ApplyPause () {
            throw new NotImplementedException ();
            // IMP
            //menuItem_PauseDevice.Checked = tPatient.Paused;

            if (!tPatient.Paused)
                foreach (Channel c in tChannels)
                    c.cStrip.Unpause ();
        }

        private void MenuClose_Click (object sender, RoutedEventArgs e) {
            this.Close ();
        }

        private void MenuExit_Click (object sender, RoutedEventArgs e) {
            App.Patient_Editor.RequestExit ();
        }

        private void MenuFontSize_Click (object sender, RoutedEventArgs e) {
            switch (((MenuItem)sender).Name) {
                case "menuFontSizeDecrease": tFontsize = Utility.Clamp(tFontsize - 1, 1, 5); break;
                case "menuFontSizeIncrease": tFontsize = Utility.Clamp (tFontsize + 1, 1, 5); break;
            }

            ApplyFontSize ();
        }

        private void MenuAddNumeric_Click (object sender, RoutedEventArgs e) {
            throw new NotImplementedException ();
            OnLayoutChange ();
        }

        private void MenuAddTracing_Click (object sender, RoutedEventArgs e) {
            throw new NotImplementedException ();
            OnLayoutChange ();
        }

        private void MenuTogglePause_Click (object sender, RoutedEventArgs e) {
            tPatient.TogglePause ();
            ApplyPause ();
        }

        private void MenuFullscreen_Click (object sender, RoutedEventArgs e) {
            tFullscreen = !tFullscreen;
            ApplyFullScreen ();
        }

        private void OnTick_Tracing (object sender, RoutedEventArgs e) {
            if (tPatient.Paused)
                return;

            foreach (Channel c in tChannels) {
                c.cStrip.Scroll ();
                throw new NotImplementedException ();
                // IMP
                //c.cTracing.Invalidate ();
            }
        }

        private void OnTick_Vitals (object sender, EventArgs e) {
            if (tPatient.Paused)
                return;
            throw new NotImplementedException ();
            /* IMP
            foreach (II.Controls.Numeric v in tNumerics)
                v.Update (tPatient);
            */
        }

        private void OnLayoutChange (List<string> numericTypes = null, List<string> tracingTypes = null) {
            throw new NotImplementedException ();
            /* IMP
            layoutTracings.Controls.Clear ();
            layoutTracings.RowStyles.Clear ();
            layoutTracings.GrowStyle = TableLayoutPanelGrowStyle.AddRows;

            layoutNumerics.Controls.Clear ();
            layoutNumerics.RowStyles.Clear ();
            layoutNumerics.GrowStyle = TableLayoutPanelGrowStyle.AddRows;

            // If numericTypes or tracingTypes are not null... then we are loading a file; clear tNumerics and tChannels!
            if (numericTypes != null)
                tNumerics.Clear ();
            if (tracingTypes != null)
                tChannels.Clear ();

            // Set default numeric types to populate
            if (numericTypes == null || numericTypes.Count == 0)
                numericTypes = new List<string> (new string [] { "ECG", "NIBP", "SPO2", "CVP", "ABP" });
            else if (numericTypes.Count < tRowsNumerics) {
                List<string> buffer = new List<string> (new string [] { "ECG", "NIBP", "SPO2", "CVP", "ABP" });
                buffer.RemoveRange (0, numericTypes.Count);
                numericTypes.AddRange (buffer);
            }


            for (int i = tNumerics.Count; i < tRowsNumerics && i < numericTypes.Count; i++) {
                II.Controls.Numeric newNum;
                newNum = new II.Controls.Numeric ((II.Controls.Numeric.ControlType.Values)Enum.Parse (typeof (II.Controls.Numeric.ControlType.Values),
                    numericTypes [i]), tColorScheme);
                tNumerics.Add (newNum);
            }

            // Set default tracing types to populate
            if (tracingTypes == null || tracingTypes.Count == 0)
                tracingTypes = new List<string> (new string [] { "ECG_II", "ECG_III", "SPO2", "CVP", "ABP" });
            else if (tracingTypes.Count < tRowsTracings) {
                List<string> buffer = new List<string> (new string [] { "ECG_II", "ECG_III", "SPO2", "CVP", "ABP" });
                buffer.RemoveRange (0, tracingTypes.Count);
                tracingTypes.AddRange (buffer);
            }

            for (int i = tChannels.Count; i < tRowsTracings && i < tracingTypes.Count; i++) {
                Strip newStrip = new Strip (6f, (Leads.Values)Enum.Parse (typeof (Leads.Values), tracingTypes [i]));
                Controls.Tracing newTracing = new Controls.Tracing (newStrip.Lead, tColorScheme);
                Strip.Renderer newRenderer = new Strip.Renderer (newTracing, ref newStrip, tColorScheme, newStrip.Lead.Color);
                newTracing.Paint += delegate (object s, PaintRoutedEventArgs e) { newRenderer.Draw (e); };
                newTracing.TracingEdited += OnTracingEdited;

                tChannels.Add (new Channel (newStrip, newRenderer, newTracing));
            }

            layoutNumerics.RowCount = tRowsNumerics;
            for (int i = 0; i < tRowsNumerics; i++) {
                layoutNumerics.RowStyles.Add (new RowStyle (SizeType.Percent, 100 / tRowsNumerics));
                layoutNumerics.Controls.Add (tNumerics [i], 0, i);
            }

            layoutTracings.RowCount = tRowsTracings;
            for (int i = 0; i < tRowsTracings; i++) {
                layoutTracings.RowStyles.Add (new RowStyle (SizeType.Percent, 100 / tRowsTracings));
                layoutTracings.Controls.Add (tChannels [i].cTracing, 0, i);
            }
            */

        }

        public void OnPatientEvent (object sender, Patient.PatientEvent_Args e) {
            throw new NotImplementedException ();
            switch (e.EventType) {
                default: break;
                /* IMP
                case Patient.PatientEvent_Args.EventTypes.Vitals_Change:
                    tPatient = e.Patient;

                    foreach (Channel c in tChannels) {
                        c.cStrip.ClearFuture ();
                        c.cStrip.Add_Beat__Cardiac_Baseline (tPatient);
                    }
                    foreach (II.Controls.Numeric n in tNumerics)
                        n.Update (tPatient);
                    break;
                */
                case Patient.PatientEvent_Args.EventTypes.Cardiac_Baseline:
                    foreach (Channel c in tChannels)
                        c.cStrip.Add_Beat__Cardiac_Baseline (tPatient);
                    break;

                case Patient.PatientEvent_Args.EventTypes.Cardiac_Atrial:
                    foreach (Channel c in tChannels)
                        c.cStrip.Add_Beat__Cardiac_Atrial (tPatient);
                    break;

                case Patient.PatientEvent_Args.EventTypes.Cardiac_Ventricular:
                    foreach (Channel c in tChannels)
                        c.cStrip.Add_Beat__Cardiac_Ventricular (tPatient);
                    break;

                case Patient.PatientEvent_Args.EventTypes.Respiratory_Baseline:
                    foreach (Channel c in tChannels)
                        c.cStrip.Add_Beat__Respiratory_Baseline (tPatient);
                    break;

                case Patient.PatientEvent_Args.EventTypes.Respiratory_Inspiration:
                    foreach (Channel c in tChannels)
                        c.cStrip.Add_Beat__Respiratory_Inspiration (tPatient);
                    break;

                case Patient.PatientEvent_Args.EventTypes.Respiratory_Expiration:
                    foreach (Channel c in tChannels)
                        c.cStrip.Add_Beat__Respiratory_Expiration (tPatient);
                    break;
            }
        }


        private void OnTracingEdited (object sender, Controls.Tracing.TracingEdited_EventArgs e) {
            throw new NotImplementedException ();
            /* IMP
            foreach (Channel c in tChannels) {
               if (c.cTracing == sender) {
                   c.cTracing.SetLead (e.Lead);
                   c.cRenderer.pcolor = e.Lead.Color;
                   c.cStrip.Lead = e.Lead;
               }

               c.cStrip.Reset ();
               c.cStrip.Add_Beat__Cardiac_Baseline (tPatient);
               c.cStrip.Add_Beat__Respiratory_Baseline (tPatient);
           } */
        }

        private void OnFormResize (object sender, RoutedEventArgs e) {
            OnLayoutChange ();
        }
    }
}