using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using II.Rhythm;

namespace II.Forms
{
    public partial class Device_Monitor : Form
    {
        int rowsTracings = 3,
            rowsNumerics = 3;
        bool tFullscreen = false;
        int tFontsize = 2;

        Patient tPatient;
        List<Channel> tChannels = new List<Channel> ();
        List<II.Controls.Numeric> tNumerics = new List<II.Controls.Numeric> ();
        _.ColorScheme tColorScheme = _.ColorScheme.Normal;

        public class Channel {
            public Strip cStrip;
            public Strip.Renderer cRenderer;
            public Controls.Tracing cTracing;

            public Channel(Strip s, Strip.Renderer r, Controls.Tracing t) {
                cStrip = s;
                cRenderer = r;
                cTracing = t;
            }
        }


        public Device_Monitor() {
            InitializeComponent();

            timerTracing.Interval = Waveforms.Draw_Refresh;
            timerVitals.Interval = 5000;

            foreach (_.ColorScheme cs in Enum.GetValues (typeof (_.ColorScheme)))
                menuItem_ColorScheme.DropDownItems.Add (_.UnderscoreToSpace (cs.ToString ()), null, menuColorScheme_Click);

            onLayoutChange ();
        }


        public void SetPatient (Patient iPatient) {
            tPatient = iPatient;
            onTick_Vitals (null, new EventArgs ());
        }


        private void onTick_Tracing (object sender, EventArgs e) {
            if (tPatient.Paused)
                return;

            foreach(Channel c in tChannels) {
                c.cStrip.Scroll ();
                c.cTracing.Invalidate ();
            }
        }

        private void onTick_Vitals (object sender, EventArgs e) {
            if (tPatient.Paused)
                return;

            foreach (II.Controls.Numeric v in tNumerics)
                v.Update (tPatient);
        }


        private void onLayoutChange() {
            layoutTracings.Controls.Clear();
            layoutTracings.RowStyles.Clear();
            layoutTracings.GrowStyle = TableLayoutPanelGrowStyle.AddRows;

            layoutNumerics.Controls.Clear ();
            layoutNumerics.RowStyles.Clear ();
            layoutNumerics.GrowStyle = TableLayoutPanelGrowStyle.AddRows;

            for (int i = 0; i < rowsNumerics; i++) {
                II.Controls.Numeric newNum;
                switch (i) {
                    default:
                    case 0: newNum = new Controls.Numeric (II.Controls.Numeric.ControlType.Values.ECG);    break;
                    case 1: newNum = new Controls.Numeric (II.Controls.Numeric.ControlType.Values.NIBP);   break;
                    case 2: newNum = new Controls.Numeric (II.Controls.Numeric.ControlType.Values.SPO2);   break;
                    case 3: newNum = new Controls.Numeric (II.Controls.Numeric.ControlType.Values.CVP);    break;
                    case 4: newNum = new Controls.Numeric (II.Controls.Numeric.ControlType.Values.ABP);    break;
                }
                tNumerics.Add (newNum);
            }

            for (int i = 0; i < rowsTracings; i++) {
                if (tChannels.Count <= i) {
                    Strip newStrip;
                    switch (i) {
                        default:
                        case 0: newStrip = new Strip (6f, Leads.Values.ECG_II);    break;
                        case 1: newStrip = new Strip (6f, Leads.Values.ECG_III);   break;
                        case 2: newStrip = new Strip (6f, Leads.Values.SpO2);      break;
                        case 3: newStrip = new Strip (6f, Leads.Values.CVP);       break;
                        case 4: newStrip = new Strip (6f, Leads.Values.ABP);       break;
                    }

                    Controls.Tracing newTracing = new Controls.Tracing (newStrip.Lead);
                    Strip.Renderer newRenderer = new Strip.Renderer (newTracing, ref newStrip, newStrip.Lead.Color);
                    newTracing.Paint += delegate (object s, PaintEventArgs e) { newRenderer.Draw (e); };
                    newTracing.TracingEdited += onTracingEdited;

                    tChannels.Add (new Channel (newStrip, newRenderer, newTracing));
                }
            }

            layoutNumerics.RowCount = rowsNumerics;
            for (int i = 0; i < rowsNumerics; i++) {
                layoutNumerics.RowStyles.Add (new RowStyle (SizeType.Percent, 100 / rowsNumerics));
                layoutNumerics.Controls.Add (tNumerics[i], 0, i);
            }

            layoutTracings.RowCount = rowsTracings;
            for (int i = 0; i < rowsTracings; i++) {
                layoutTracings.RowStyles.Add(new RowStyle(SizeType.Percent, 100 / rowsTracings));
                layoutTracings.Controls.Add(tChannels[i].cTracing, 0, i);
            }
        }

        public void onPatientEvent (object sender, Patient.PatientEvent_Args e) {
            switch (e.EventType) {
                default: break;

                case Patient.PatientEvent_Args.EventTypes.Vitals_Change:
                    tPatient = e.Patient;

                    foreach (Channel c in tChannels) {
                        c.cStrip.clearFuture ();
                        c.cStrip.Add_Beat__Cardiac_Baseline (tPatient);
                    }
                    foreach (II.Controls.Numeric n in tNumerics)
                        n.Update (tPatient);
                    break;

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

        private void onTracingEdited (object sender, Controls.Tracing.TracingEdited_EventArgs e) {
            foreach (Channel c in tChannels) {
                if (c.cTracing == sender) {
                    c.cTracing.setLead (e.Lead);
                    c.cRenderer.pcolor = e.Lead.Color;
                    c.cStrip.Lead = e.Lead;
                }

                c.cStrip.Reset ();
                c.cStrip.Add_Beat__Cardiac_Baseline (tPatient);
                c.cStrip.Add_Beat__Respiratory_Baseline (tPatient);
            }
        }

        private void onFormResize (object sender, EventArgs e) {
            onLayoutChange ();
        }


        private void menuClose_Click (object sender, EventArgs e) {
            this.Close ();
        }

        private void menuExit_Click (object sender, EventArgs e) {
            Program.Dialog_Main.RequestExit ();
        }

        private void menuNewPatient_Click (object sender, EventArgs e) {
            tPatient = Program.Dialog_Main.RequestNewPatient ();

            foreach (Channel c in tChannels)
                c.cStrip.Reset ();
            foreach (II.Controls.Numeric n in tNumerics)
                n.Update (tPatient);
        }

        private void menuEditPatient_Click (object sender, EventArgs e) {
            Program.Dialog_Main.Show ();
            Program.Dialog_Main.BringToFront ();
        }

        private void menuFontSize_Click(object sender, EventArgs e) {
            switch (((ToolStripMenuItem)sender).Text) {
                case "Small": tFontsize = 1; break;
                case "Medium": tFontsize = 2; break;
                case "Large": tFontsize = 3; break;
                case "Extra Large": tFontsize = 4; break;
            }

            foreach (II.Controls.Numeric rn in tNumerics)
                rn.SetFontSize (tFontsize * 0.5f);
        }

        private void menuColorScheme_Click (object sender, EventArgs e) {
            tColorScheme = (_.ColorScheme)Enum.Parse (typeof (_.ColorScheme), _.SpaceToUnderscore (((ToolStripMenuItem)sender).Text));

            foreach (Channel c in tChannels) {
                c.cTracing.setColorScheme (tColorScheme);
                c.cRenderer.setColorScheme (tColorScheme);
            }

            foreach (II.Controls.Numeric n in tNumerics) {
                n.SetColorScheme (tColorScheme);
            }

            switch (tColorScheme) {
                default:
                case _.ColorScheme.Normal:
                    layoutNumerics.BackColor = Color.Black;
                    layoutTracings.BackColor = Color.Black;
                    break;
                case _.ColorScheme.Monochrome:
                    layoutNumerics.BackColor = Color.White;
                    layoutTracings.BackColor = Color.White;
                    break;
            }
        }

        private void menuNumericRowCount_Click (object sender, EventArgs e) {
            rowsNumerics = int.Parse (((ToolStripMenuItem)sender).Text.Replace ('&', ' ').Trim ());
            onLayoutChange ();
        }

        private void menuTracingRowCount_Click (object sender, EventArgs e) {
            rowsTracings = int.Parse (((ToolStripMenuItem)sender).Text.Replace ('&', ' ').Trim ());
            onLayoutChange ();
        }

        private void menuTogglePause_Click(object sender, EventArgs e) {
            tPatient.TogglePause ();
            menuItem_PauseDevice.Checked = tPatient.Paused;

            if (!tPatient.Paused)
                foreach (Channel c in tChannels)
                    c.cStrip.Unpause ();
        }

        private void menuFullscreen_Click (object sender, EventArgs e) {
            tFullscreen = !tFullscreen;
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
        }
    }
}