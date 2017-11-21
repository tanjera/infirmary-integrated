using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Infirmary_Integrated.Forms {
    public partial class Patient_Vitals : Form {

        Patient tPatient;

        public event EventHandler<PatientEdited_EventArgs> PatientEdited;

        public class PatientEdited_EventArgs : EventArgs {
            public Patient Patient { get; set; }
            public PatientEdited_EventArgs (Patient patient) { Patient = patient; }
        }

        public Patient_Vitals (Patient p) {
            InitializeComponent ();

            tPatient = p;

            numHR.Value = tPatient.HR;
            numSBP.Value = tPatient.SBP;
            numDBP.Value = tPatient.DBP;
            numSpO2.Value = tPatient.SpO2;
        }

        private void Vitals_ValueChanged (object sender, EventArgs e) {
            tPatient.HR = (int)numHR.Value;
            tPatient.SBP = (int)numSBP.Value;
            tPatient.DBP = (int)numDBP.Value;
            tPatient.MAP_Calculate ();
            tPatient.SpO2 = (int)numSpO2.Value;        
            tPatient.Heart_Rhythm = (Rhythms.Cardiac_Rhythms)Enum.Parse (typeof (Rhythms.Cardiac_Rhythms), comboRhythm.Text);
        }

        private void buttonApply_Click (object sender, EventArgs e) {
            PatientEdited (this, new PatientEdited_EventArgs (tPatient));
        }
        
    }
}
