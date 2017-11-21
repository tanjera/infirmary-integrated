using System;
using System.Drawing;
using System.Windows.Forms;

using Infirmary_Integrated.Rhythms;

namespace Infirmary_Integrated
{
    public partial class Device_Monitor : Form
    {
        Strip ecgStrip;
        Strip.Renderer ecgRender;

        Patient tPatient = new Patient();


        public Device_Monitor()
        {
            InitializeComponent();

            timerDraw.Interval = _.Draw_Refresh;

            ecgStrip = new Strip ();
            ecgRender = new Strip.Renderer (ecgTracing, ref ecgStrip, new Pen (Color.Green, 1f));
        }

        private void onTick (object sender, EventArgs e) {
            ecgStrip.Scroll ();
            ecgTracing.Invalidate ();
            
            ecgStrip.Add_Beat (tPatient);
        }

        private void ECGTracing_Paint (object sender, PaintEventArgs e) {
            ecgRender.Draw (e);
        }
        
        private void menuItem_NewPatient_Click (object sender, EventArgs e) {
            ecgStrip.Reset ();
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
            ecgStrip.Reset ();
        }
    }
}