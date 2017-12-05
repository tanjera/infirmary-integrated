﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using II.Rhythms;

namespace II.Forms
{
    public partial class Device_Monitor : Form
    {
        int layoutRows = 3;
        Patient tPatient = new Patient();
        List<Channel> tChannels = new List<Channel> ();
        List<II.Controls.Rhythm_Numerics> tNumerics = new List<II.Controls.Rhythm_Numerics> ();

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

        public Device_Monitor()
        {
            InitializeComponent();

            timerTracing.Interval = _.Draw_Refresh;
            timerVitals.Interval = 5000;
                        
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
                            newStrip = new Strip (5f, Leads.ECG_II);
                            newNum = new Controls.Rhythm_Numerics (II.Controls.Rhythm_Numerics.ControlType.ECG);
                            break;
                        case 1:
                            newStrip = new Strip (5f, Leads.ECG_III);
                            newNum = new Controls.Rhythm_Numerics (II.Controls.Rhythm_Numerics.ControlType.NIBP);
                            break;
                        case 2:
                            newStrip = new Strip (5f, Leads.SpO2);
                            newNum = new Controls.Rhythm_Numerics (II.Controls.Rhythm_Numerics.ControlType.SPO2);
                            break;
                        case 3:
                            newStrip = new Strip (5f, Leads.CVP);
                            newNum = new Controls.Rhythm_Numerics (II.Controls.Rhythm_Numerics.ControlType.CVP);
                            break;
                        case 4:
                            newStrip = new Strip (5f, Leads.ABP);
                            newNum = new Controls.Rhythm_Numerics (II.Controls.Rhythm_Numerics.ControlType.ABP);
                            break;
                    }

                    II.Controls.Rhythm_Tracing newTracing = new II.Controls.Rhythm_Tracing (newStrip.Lead);
                    Strip.Renderer newRenderer = new Strip.Renderer (newTracing, ref newStrip,
                        new Pen (Strip.stripColors(newStrip.Lead), 1f));
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
        
        private void menuItem_NewPatient_Click (object sender, EventArgs e) {            
            tPatient = new Patient ();

            foreach (Channel c in tChannels)
                c.cStrip.Reset();
            foreach (II.Controls.Rhythm_Numerics n in tNumerics)
                n.Update(tPatient);
        }

        private void menuItem_Exit_Click (object sender, EventArgs e) {
            Application.Exit ();
        }

        private void menuItem_EditPatient_Click (object sender, EventArgs e) {
            Forms.Dialog_Patient pv = new Forms.Dialog_Patient (tPatient);
            pv.Show ();
            pv.PatientEdited += onPatientEdited;
        }

        private void onPatientEdited (object sender, Forms.Dialog_Patient.PatientEdited_EventArgs e) {
            tPatient = e.Patient;

            foreach (Channel c in tChannels)
                c.cStrip.clearFuture ();
            foreach (II.Controls.Rhythm_Numerics n in tNumerics)
                n.Update(tPatient);
        }

        private void onTracingEdited (object sender, Controls.Rhythm_Tracing.TracingEdited_EventArgs e) {
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
            Forms.Dialog_About about = new Forms.Dialog_About ();
            about.Show ();
        }
        
        private void menuItem_RowCount_Click (object sender, EventArgs e) {
            layoutRows = int.Parse(((ToolStripMenuItem)sender).Text);
            onLayoutChange();
        }

        private void onFormResize(object sender, EventArgs e) {            
            onLayoutChange();
        }
    }
}