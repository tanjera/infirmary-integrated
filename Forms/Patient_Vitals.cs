using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace II.Forms {
    public partial class Patient_Vitals : Form {

        bool updatingVitals;
        Patient lPatient, bufPatient;
        
        public event EventHandler<PatientEdited_EventArgs> PatientEdited;
        public class PatientEdited_EventArgs : EventArgs {
            public Patient Patient { get; set; }
            public PatientEdited_EventArgs (Patient patient) { Patient = patient; }
        }

        public Patient_Vitals (Patient p) {
            InitializeComponent ();

            lPatient = p;
            bufPatient = new Patient (p);

            Vitals_Update (lPatient);
        }

        private void Vitals_Update (Patient p) {
            updatingVitals = true;

            numHR.Value = p.HR;
            numSBP.Value = p.SBP;
            numDBP.Value = p.DBP;
            numSpO2.Value = p.SpO2;

            updatingVitals = false;
        }

        private void Vitals_ValueChanged (object sender, EventArgs e) {
            if (updatingVitals)
                return;
            
            bufPatient.HR = (int)numHR.Value;
            bufPatient.SBP = (int)numSBP.Value;
            bufPatient.DBP = (int)numDBP.Value;
            bufPatient.calcMAP ();
            bufPatient.SpO2 = (int)numSpO2.Value;

            bufPatient.Heart_Rhythm = (Rhythms.Cardiac_Rhythm)Enum.Parse (typeof (Rhythms.Cardiac_Rhythm), comboRhythm.Text);

            if (checkNormalRange.Checked)
                Rhythms.Rhythm_Index.Get_Rhythm (bufPatient.Heart_Rhythm).Vitals (bufPatient);

            Vitals_Update (bufPatient);
        }

        private void buttonApply_Click (object sender, EventArgs e) {
            lPatient = new Patient(bufPatient);
            PatientEdited (this, new PatientEdited_EventArgs (lPatient));
        }
    }
}
