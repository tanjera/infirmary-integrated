using System;
using System.Drawing;
using System.Windows.Forms;

using II.Rhythms;

namespace II.Forms
{
    public partial class Device_Monitor : Form
    {
        Strip ecgStrip;
        Strip.Renderer ecgRender;

        Strip ecgT1;
        Strip.Renderer ecgT1Render;

        Patient tPatient = new Patient();


        public Device_Monitor()
        {
            InitializeComponent();

            timerTracing.Interval = _.Draw_Refresh;
            timerVitals.Interval = 1000;

            ecgStrip = new Strip (Leads.ECG_L2);
            ecgRender = new Strip.Renderer (ecgTracing, ref ecgStrip, new Pen (Color.Green, 1f));

            ecgT1 = new Strip (Leads.ECG_L1);
            ecgT1Render = new Strip.Renderer (tracing1, ref ecgT1, new Pen (Color.Green, 1f));

            // Populate vital signs
            onTick_Vitals (null, new EventArgs ());
        }

        private void onTick_Tracing (object sender, EventArgs e) {
            ecgStrip.Scroll ();
            ecgTracing.Invalidate ();
            
            ecgStrip.Add_Beat (tPatient);


            ecgT1.Scroll ();
            tracing1.Invalidate ();

            ecgT1.Add_Beat (tPatient);
        }

        private void onTick_Vitals (object sender, EventArgs e) {
            ecgValues.Update (tPatient, II.Controls.Values_HR.ControlType.HR);
            spO2Values.Update (tPatient, II.Controls.Values_HR.ControlType.SpO2);
            bpValues.Update (tPatient, II.Controls.Values_BP.ControlType.NIBP);
        }
        
        private void menuItem_NewPatient_Click (object sender, EventArgs e) {
            ecgStrip.Reset ();
            ecgT1.Reset ();
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
            ecgStrip.Clear_Future ();
            ecgT1.Clear_Future ();
        }

        private void menuItem_About_Click (object sender, EventArgs e) {
            Forms.About about = new Forms.About ();
            about.Show ();
        }
        
        private void ECGTracing_Paint (object sender, PaintEventArgs e) {
            ecgRender.Draw (e);            
        }

        private void tracing1_Paint (object sender, PaintEventArgs e) {
            ecgT1Render.Draw (e);
        }

        private void numericUpDown1_ValueChanged (object sender, EventArgs e) {
            ecgT1.Lead = (Leads)numericUpDown1.Value;
            ecgT1.Clear_Future ();
        }
    }
}