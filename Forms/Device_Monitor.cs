using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using II.Rhythms;

namespace II.Forms
{
    public partial class Device_Monitor : Form
    {
        
        Patient tPatient = new Patient();
        List<Channel> tChannels = new List<Channel> ();
        List<II.Controls.Values> tValues = new List<II.Controls.Values> ();

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

        public Device_Monitor()
        {
            InitializeComponent();

            timerTracing.Interval = _.Draw_Refresh;
            timerVitals.Interval = 1000;

            onLayoutChange ();
            
            // Populate vital signs
            onTick_Vitals (null, new EventArgs ());
        }
        
        private void onTick_Tracing (object sender, EventArgs e) {
            foreach(Channel c in tChannels) {
                c.cStrip.Scroll ();
                c.cStrip.Add_Beat (tPatient);
                c.cTracing.Invalidate ();
            }
        }

        private void onTick_Vitals (object sender, EventArgs e) {
            foreach (II.Controls.Values v in tValues)
                v.Update (tPatient);
        }

        private void onLayoutChange() {
            for (int i = 0; i < mainLayout.RowCount; i++) {
                if (tChannels.Count <= i) {
                    Strip newStrip;
                    II.Controls.Values newValue;
                    switch (i) {
                        default:
                        case 0:
                            newStrip = new Strip (Leads.ECG_L2);
                            newValue = new Controls.Values (II.Controls.Values.ControlType.ECG);
                            break;
                        case 1:
                            newStrip = new Strip (Leads.ECG_L3);
                            newValue = new Controls.Values (II.Controls.Values.ControlType.NIBP);
                            break;
                        case 2:
                            newStrip = new Strip (Leads.SPO2);
                            newValue = new Controls.Values (II.Controls.Values.ControlType.SPO2);
                            break;
                        case 3:
                            newStrip = new Strip (Leads.CVP);
                            newValue = new Controls.Values (II.Controls.Values.ControlType.CVP);
                            break;
                        case 4:
                            newStrip = new Strip (Leads.ABP);
                            newValue = new Controls.Values (II.Controls.Values.ControlType.ABP);
                            break;
                    }

                    II.Controls.Tracing newTracing = new II.Controls.Tracing (newStrip.Lead);
                    Strip.Renderer newRenderer = new Strip.Renderer (newTracing, ref newStrip,
                        new Pen (Strip.stripColors(newStrip.Lead), 1f));
                    newTracing.Paint += delegate (object s, PaintEventArgs e) { newRenderer.Draw (e); };
                    newTracing.TracingEdited += onTracingEdited;

                    tChannels.Add (new Channel (newStrip, newRenderer, newTracing));
                    tValues.Add (newValue);

                    mainLayout.Controls.Add (newValue, 0, i);
                    mainLayout.Controls.Add (newTracing, 1, i);
                }
            }
        }
        
        private void menuItem_NewPatient_Click (object sender, EventArgs e) {
            foreach (Channel c in tChannels)
                c.cStrip.Reset ();

            tPatient = new Patient ();        
        }

        private void menuItem_Exit_Click (object sender, EventArgs e) {
            Application.Exit ();
        }

        private void menuItem_EditPatient_Click (object sender, EventArgs e) {
            Forms.Patient_Vitals pv = new Forms.Patient_Vitals (tPatient);
            pv.Show ();
            pv.PatientEdited += onPatientEdited;
        }

        private void onPatientEdited (object sender, Forms.Patient_Vitals.PatientEdited_EventArgs e) {
            tPatient = e.Patient;

            foreach (Channel c in tChannels)
                c.cStrip.clearFuture ();            
        }

        private void onTracingEdited (object sender, Controls.Tracing.TracingEdited_EventArgs e) {
            foreach (Channel c in tChannels) {
                if (c.cTracing == sender) {
                    c.cTracing.setLead (e.Lead);                    
                    c.cRenderer.pen = new Pen (Strip.stripColors (e.Lead), 1f);
                    c.cStrip.Lead = e.Lead;                    
                }

                c.cStrip.Reset ();
            }
        }

        private void menuItem_About_Click (object sender, EventArgs e) {
            Forms.About about = new Forms.About ();
            about.Show ();
        }        
    }
}