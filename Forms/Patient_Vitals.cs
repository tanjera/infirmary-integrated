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

            foreach (Rhythms.Cardiac_Rhythm el in Enum.GetValues (typeof (Rhythms.Cardiac_Rhythm)))
                comboCardiacRhythm.Items.Add (_.UnderscoreToSpace(el.ToString ()));

            foreach (Rhythms.Cardiac_Axis_Shifts el in Enum.GetValues(typeof(Rhythms.Cardiac_Axis_Shifts)))
                comboAxisShift.Items.Add(_.UnderscoreToSpace(el.ToString()));
        }

        private void Vitals_Update (Patient p) {
            updatingVitals = true;

            numHR.Value = p.HR;
            numRR.Value = p.RR;
            numSpO2.Value = p.SpO2;
            numT.Value = (decimal)p.T;
            numCVP.Value = p.CVP;
            numETCO2.Value = p.ETCO2;

            numNSBP.Value = p.NSBP;
            numNDBP.Value = p.NDBP;
            numASBP.Value = p.ASBP;
            numADBP.Value = p.ADBP;
            numPSP.Value = p.PSP;
            numPDP.Value = p.PDP;
            
            updatingVitals = false;
        }

        private void Vitals_ValueChanged (object sender, EventArgs e) {
            if (updatingVitals)
                return;
            
            bufPatient.HR = (int)numHR.Value;
            bufPatient.RR = (int)numRR.Value;
            bufPatient.SpO2 = (int)numSpO2.Value;
            bufPatient.T = (float)numT.Value;
            bufPatient.CVP = (int)numCVP.Value;
            bufPatient.ETCO2 = (int)numETCO2.Value;
            
            bufPatient.NSBP = (int)numNSBP.Value;
            bufPatient.NDBP = (int)numNDBP.Value;
            bufPatient.NMAP = Patient.calcMAP((int)numNSBP.Value, (int)numNDBP.Value);

            bufPatient.ASBP = (int)numASBP.Value;
            bufPatient.ADBP = (int)numADBP.Value;
            bufPatient.AMAP = Patient.calcMAP((int)numASBP.Value, (int)numADBP.Value);

            bufPatient.PSP = (int)numPSP.Value;
            bufPatient.PDP = (int)numPDP.Value;
            bufPatient.PMP = Patient.calcMAP((int)numPSP.Value, (int)numPDP.Value);
            
            bufPatient.Cardiac_Rhythm = (Rhythms.Cardiac_Rhythm)Enum.Parse (typeof (Rhythms.Cardiac_Rhythm), _.SpaceToUnderscore(comboCardiacRhythm.Text));
            bufPatient.Cardiac_Axis_Shift = (Rhythms.Cardiac_Axis_Shifts)Enum.Parse(typeof(Rhythms.Cardiac_Axis_Shifts), _.SpaceToUnderscore(comboAxisShift.Text));

            if (checkDefaultVitals.Checked)
                Rhythms.Rhythm_Index.Get_Rhythm (bufPatient.Cardiac_Rhythm).Vitals (bufPatient);

            Vitals_Update (bufPatient);
        }

        private void buttonApply_Click (object sender, EventArgs e) {
            lPatient = new Patient(bufPatient);
            PatientEdited (this, new PatientEdited_EventArgs (lPatient));
        }
    }
}
