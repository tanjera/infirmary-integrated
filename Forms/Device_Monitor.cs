using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using II.Rhythms;

namespace II.Forms
{
    public partial class Device_Monitor : Form
    {
        int layoutRows = 3;
        bool tPaused = false,
             tFullscreen = false;

        Patient tPatient;
        List<Channel> tChannels = new List<Channel> ();
        List<II.Controls.Rhythm_Numerics> tNumerics = new List<II.Controls.Rhythm_Numerics> ();
        _.ColorScheme tColorScheme = _.ColorScheme.Normal;

        public class Channel {
            public Strip cStrip;
            public Strip.Renderer cRenderer;
            public Controls.Rhythm_Tracing cTracing;

            public Channel(Strip s, Strip.Renderer r, Controls.Rhythm_Tracing t) {
                cStrip = s;
                cRenderer = r;
                cTracing = t;
            }
        }


        public Device_Monitor() {
            InitializeComponent();

            timerTracing.Interval = Rhythm.Draw_Refresh;
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
            if (tPaused)
                return;

            foreach(Channel c in tChannels) {
                c.cStrip.Scroll ();
                c.cTracing.Invalidate ();
            }
        }

        private void onTick_Vitals (object sender, EventArgs e) {
            if (tPaused)
                return;

            foreach (II.Controls.Rhythm_Numerics v in tNumerics)
                v.Update (tPatient);
        }
        

        private void onLayoutChange() {
            mainLayout.Controls.Clear();
            mainLayout.RowStyles.Clear();
            mainLayout.GrowStyle = TableLayoutPanelGrowStyle.AddRows;

            for (int i = 0; i < layoutRows; i++) {
                if (tChannels.Count <= i) {
                    Strip newStrip;
                    II.Controls.Rhythm_Numerics newNum;
                    switch (i) {
                        default:
                        case 0:
                            newStrip = new Strip (6f, Leads.ECG_II);
                            newNum = new Controls.Rhythm_Numerics (II.Controls.Rhythm_Numerics.ControlType.ECG);
                            break;
                        case 1:
                            newStrip = new Strip (6f, Leads.ECG_III);
                            newNum = new Controls.Rhythm_Numerics (II.Controls.Rhythm_Numerics.ControlType.NIBP);
                            break;
                        case 2:
                            newStrip = new Strip (6f, Leads.SpO2);
                            newNum = new Controls.Rhythm_Numerics (II.Controls.Rhythm_Numerics.ControlType.SPO2);
                            break;
                        case 3:
                            newStrip = new Strip (6f, Leads.CVP);
                            newNum = new Controls.Rhythm_Numerics (II.Controls.Rhythm_Numerics.ControlType.CVP);
                            break;
                        case 4:
                            newStrip = new Strip (6f, Leads.ABP);
                            newNum = new Controls.Rhythm_Numerics (II.Controls.Rhythm_Numerics.ControlType.ABP);
                            break;
                    }

                    II.Controls.Rhythm_Tracing newTracing = new II.Controls.Rhythm_Tracing (newStrip.Lead);
                    Strip.Renderer newRenderer = new Strip.Renderer (newTracing, ref newStrip, Strip.stripColors(newStrip.Lead));
                    newTracing.Paint += delegate (object s, PaintEventArgs e) { newRenderer.Draw (e); };
                    newTracing.TracingEdited += onTracingEdited;

                    tChannels.Add (new Channel (newStrip, newRenderer, newTracing));
                    tNumerics.Add (newNum);                    
                }
            }

            mainLayout.RowCount = layoutRows;
            for (int i = 0; i < layoutRows; i++) {
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100 / layoutRows));
                mainLayout.Controls.Add(tNumerics[i], 0, i);
                mainLayout.Controls.Add(tChannels[i].cTracing, 1, i);                
            }            
        }
        
        public void onPatientEvent (object sender, Patient.PatientEvent_Args e) {
            switch (e.EventType) {
                default: break;

                case Patient.PatientEvent_Args.EventTypes.Vitals_Change:
                    tPatient = e.Patient;

                    foreach (Channel c in tChannels) { 
                        c.cStrip.clearFuture ();
                        c.cStrip.Add_Beat__Baseline (tPatient);
                    }
                    foreach (II.Controls.Rhythm_Numerics n in tNumerics)
                        n.Update (tPatient);
                    break;

                case Patient.PatientEvent_Args.EventTypes.Cardiac_Baseline:                    
                    foreach (Channel c in tChannels)
                        c.cStrip.Add_Beat__Baseline (tPatient);
                    break;

                case Patient.PatientEvent_Args.EventTypes.Cardiac_Atrial:
                    foreach (Channel c in tChannels)
                        c.cStrip.Add_Beat__Atrial (tPatient);
                    break;

                case Patient.PatientEvent_Args.EventTypes.Cardiac_Ventricular:
                    foreach (Channel c in tChannels)
                        c.cStrip.Add_Beat__Ventricular (tPatient);                    
                    break;                    
            }            
        }

        private void onTracingEdited (object sender, Controls.Rhythm_Tracing.TracingEdited_EventArgs e) {
            foreach (Channel c in tChannels) {
                if (c.cTracing == sender) {
                    c.cTracing.setLead (e.Lead);
                    c.cRenderer.pcolor = Strip.stripColors (e.Lead);
                    c.cStrip.Lead = e.Lead;
                }

                c.cStrip.Reset ();
                c.cStrip.Add_Beat__Baseline (tPatient);
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
            foreach (II.Controls.Rhythm_Numerics n in tNumerics)
                n.Update (tPatient);
        }

        private void menuEditPatient_Click (object sender, EventArgs e) {
            Program.Dialog_Main.Show ();
            Program.Dialog_Main.BringToFront ();
        }

        private void menuColorScheme_Click (object sender, EventArgs e) {
            tColorScheme = (_.ColorScheme)Enum.Parse (typeof (_.ColorScheme), _.SpaceToUnderscore (((ToolStripMenuItem)sender).Text));

            foreach (Channel c in tChannels) {
                c.cTracing.setColorScheme (tColorScheme);
                c.cRenderer.setColorScheme (tColorScheme);
            }

            foreach (II.Controls.Rhythm_Numerics n in tNumerics) {
                n.setColorScheme (tColorScheme);
            }

            switch (tColorScheme) {
                default:
                case _.ColorScheme.Normal:
                    mainLayout.BackColor = Color.Black;
                    break;
                case _.ColorScheme.Monochrome:
                    mainLayout.BackColor = Color.White;
                    break;
            }
        }

        private void menuRowCount_Click (object sender, EventArgs e) {            
            layoutRows = int.Parse(((ToolStripMenuItem)sender).Text.Replace ('&', ' ').Trim());
            onLayoutChange();
        }
        
        private void menuTogglePause_Click(object sender, EventArgs e) {
            tPaused = !tPaused;
            menuItem_PauseDevice.Checked = tPaused;
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