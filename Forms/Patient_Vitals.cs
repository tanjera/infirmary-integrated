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

            updatingVitals = true;

            foreach (Rhythms.Cardiac_Rhythm el in Enum.GetValues (typeof (Rhythms.Cardiac_Rhythm)))
                comboCardiacRhythm.Items.Add (_.UnderscoreToSpace(el.ToString ()));
            comboCardiacRhythm.SelectedIndex = 0;

            foreach (Rhythms.Cardiac_Axis_Shifts el in Enum.GetValues(typeof(Rhythms.Cardiac_Axis_Shifts)))
                comboAxisShift.Items.Add(_.UnderscoreToSpace(el.ToString()));
            comboAxisShift.SelectedIndex = 0;

            updatingVitals = false;
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

            numSTE_I.Value = (decimal)p.ST_Elevation[(int)Rhythms.Leads.ECG_I];
            numSTE_II.Value = (decimal)p.ST_Elevation[(int)Rhythms.Leads.ECG_II]; 
            numSTE_III.Value = (decimal)p.ST_Elevation[(int)Rhythms.Leads.ECG_III];
            numSTE_aVR.Value = (decimal)p.ST_Elevation[(int)Rhythms.Leads.ECG_AVR]; 
            numSTE_aVL.Value = (decimal)p.ST_Elevation[(int)Rhythms.Leads.ECG_AVL]; 
            numSTE_aVF.Value = (decimal)p.ST_Elevation[(int)Rhythms.Leads.ECG_AVF];
            numSTE_V1.Value = (decimal)p.ST_Elevation[(int)Rhythms.Leads.ECG_V1]; 
            numSTE_V2.Value = (decimal)p.ST_Elevation[(int)Rhythms.Leads.ECG_V2]; 
            numSTE_V3.Value = (decimal)p.ST_Elevation[(int)Rhythms.Leads.ECG_V3];
            numSTE_V4.Value = (decimal)p.ST_Elevation[(int)Rhythms.Leads.ECG_V4]; 
            numSTE_V5.Value = (decimal)p.ST_Elevation[(int)Rhythms.Leads.ECG_V5]; 
            numSTE_V6.Value = (decimal)p.ST_Elevation[(int)Rhythms.Leads.ECG_V6];
      
            numTWE_I.Value = (decimal)p.T_Elevation[(int)Rhythms.Leads.ECG_I];
            numTWE_II.Value = (decimal)p.T_Elevation[(int)Rhythms.Leads.ECG_II];
            numTWE_III.Value = (decimal)p.T_Elevation[(int)Rhythms.Leads.ECG_III];
            numTWE_aVR.Value = (decimal)p.T_Elevation[(int)Rhythms.Leads.ECG_AVR];
            numTWE_aVL.Value = (decimal)p.T_Elevation[(int)Rhythms.Leads.ECG_AVL];
            numTWE_aVF.Value = (decimal)p.T_Elevation[(int)Rhythms.Leads.ECG_AVF];
            numTWE_V1.Value = (decimal)p.T_Elevation[(int)Rhythms.Leads.ECG_V1];
            numTWE_V2.Value = (decimal)p.T_Elevation[(int)Rhythms.Leads.ECG_V2];
            numTWE_V3.Value = (decimal)p.T_Elevation[(int)Rhythms.Leads.ECG_V3];
            numTWE_V4.Value = (decimal)p.T_Elevation[(int)Rhythms.Leads.ECG_V4];
            numTWE_V5.Value = (decimal)p.T_Elevation[(int)Rhythms.Leads.ECG_V5];
            numTWE_V6.Value = (decimal)p.T_Elevation[(int)Rhythms.Leads.ECG_V6];

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

            bufPatient.ST_Elevation = new float[] {
                (float)numSTE_I.Value, (float)numSTE_II.Value, (float)numSTE_III.Value,
                (float)numSTE_aVR.Value, (float)numSTE_aVL.Value, (float)numSTE_aVF.Value,
                (float)numSTE_V1.Value, (float)numSTE_V2.Value, (float)numSTE_V3.Value,
                (float)numSTE_V4.Value, (float)numSTE_V5.Value, (float)numSTE_V6.Value
            };

            bufPatient.T_Elevation = new float[] {                
                (float)numTWE_I.Value, (float)numTWE_II.Value, (float)numTWE_III.Value,
                (float)numTWE_aVR.Value, (float)numTWE_aVL.Value, (float)numTWE_aVF.Value,
                (float)numTWE_V1.Value, (float)numTWE_V2.Value, (float)numTWE_V3.Value,
                (float)numTWE_V4.Value, (float)numTWE_V5.Value, (float)numTWE_V6.Value
            };

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
