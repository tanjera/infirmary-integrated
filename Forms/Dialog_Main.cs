using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace II.Forms {
    public partial class Dialog_Main : Form {
        
        Patient tPatient;
        
        public Dialog_Main () {
            InitializeComponent ();
            
            foreach (Rhythms.Cardiac_Rhythm el in Enum.GetValues (typeof (Rhythms.Cardiac_Rhythm)))
                comboCardiacRhythm.Items.Add (_.UnderscoreToSpace(el.ToString ()));
            comboCardiacRhythm.SelectedIndex = 0;

            foreach (Rhythms.Cardiac_Axis_Shifts el in Enum.GetValues(typeof(Rhythms.Cardiac_Axis_Shifts)))
                comboAxisShift.Items.Add(_.UnderscoreToSpace(el.ToString()));
            comboAxisShift.SelectedIndex = 0;

            initPatient ();

            // Debugging: auto-open cardiac monitor on program start
            buttonMonitor_Click (this, new EventArgs ());
        }
        

        public void RequestExit() {
            if (MessageBox.Show ("Are you sure you want to exit Infirmary Integrated? All unsaved work will be lost.", "Exit Infirmary Integrated", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            Application.Exit ();
        }

        public Patient RequestNewPatient() {
            if (MessageBox.Show ("Are you sure you want to reset all patient parameters?", "Reset Patient Parameters", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return tPatient;

            initPatient ();
            return tPatient;
        }

        private void initPatient() {
            tPatient = new Patient ();
            tPatient.PatientEvent += updateFormParameters;
            updateFormParameters (this, new Patient.PatientEvent_Args (tPatient, Patient.PatientEvent_Args.EventTypes.Vitals_Change));
        }

        private void initMonitor() {
            if (Program.Device_Monitor != null && !Program.Device_Monitor.IsDisposed)
                return;

            Program.Device_Monitor = new Device_Monitor ();
            Program.Device_Monitor.SetPatient (tPatient);
            tPatient.PatientEvent += Program.Device_Monitor.onPatientEvent;
        }

        private void buttonMonitor_Click (object sender, EventArgs e) {
            initMonitor ();            
            Program.Device_Monitor.Show ();
            Program.Device_Monitor.BringToFront ();
        }

        private void menuExit_Click(object sender, EventArgs e) {
            RequestExit ();
        }

        private void menuAbout_Click (object sender, EventArgs e) {
            Forms.Dialog_About about = new Forms.Dialog_About ();
            about.Show ();
        }

        private void buttonResetParameters_Click (object sender, EventArgs e) {
            RequestNewPatient ();
        }

        private void buttonApplyParameters_Click (object sender, EventArgs e) {
            tPatient.UpdateVitals (
                (int)numHR.Value, 
                (int)numRR.Value, 
                (int)numSpO2.Value,
                (int)numT.Value,
                (int)numCVP.Value,
                (int)numETCO2.Value,

                (int)numNSBP.Value,
                (int)numNDBP.Value, 
                Patient.CalculateMAP ((int)numNSBP.Value, (int)numNDBP.Value),
                
                (int)numASBP.Value,
                (int)numADBP.Value, 
                Patient.CalculateMAP ((int)numASBP.Value, (int)numADBP.Value),
                
                (int)numPSP.Value,
                (int)numPDP.Value, 
                Patient.CalculateMAP ((int)numPSP.Value, (int)numPDP.Value),
                
                new float[] {
                    (float)numSTE_I.Value, (float)numSTE_II.Value, (float)numSTE_III.Value,
                    (float)numSTE_aVR.Value, (float)numSTE_aVL.Value, (float)numSTE_aVF.Value,
                    (float)numSTE_V1.Value, (float)numSTE_V2.Value, (float)numSTE_V3.Value,
                    (float)numSTE_V4.Value, (float)numSTE_V5.Value, (float)numSTE_V6.Value
                },
                new float[] {
                    (float)numTWE_I.Value, (float)numTWE_II.Value, (float)numTWE_III.Value,
                    (float)numTWE_aVR.Value, (float)numTWE_aVL.Value, (float)numTWE_aVF.Value,
                    (float)numTWE_V1.Value, (float)numTWE_V2.Value, (float)numTWE_V3.Value,
                    (float)numTWE_V4.Value, (float)numTWE_V5.Value, (float)numTWE_V6.Value
                },

                (Rhythms.Cardiac_Rhythm)Enum.Parse (typeof (Rhythms.Cardiac_Rhythm), _.SpaceToUnderscore (comboCardiacRhythm.Text)),
                (Rhythms.Cardiac_Axis_Shifts)Enum.Parse (typeof (Rhythms.Cardiac_Axis_Shifts), _.SpaceToUnderscore (comboAxisShift.Text)),
                
                1);   // TODO: Add IE Ratio to form              
        }

        private void updateFormParameters(object sender, Patient.PatientEvent_Args e) {
            if (e.EventType == Patient.PatientEvent_Args.EventTypes.Vitals_Change) {                
                numHR.Value = e.Patient.HR;
                numRR.Value = e.Patient.RR;
                numSpO2.Value = e.Patient.SpO2;
                numT.Value = (decimal)e.Patient.T;
                numCVP.Value = e.Patient.CVP;
                numETCO2.Value = e.Patient.ETCO2;

                numNSBP.Value = e.Patient.NSBP;
                numNDBP.Value = e.Patient.NDBP;
                numASBP.Value = e.Patient.ASBP;
                numADBP.Value = e.Patient.ADBP;
                numPSP.Value = e.Patient.PSP;
                numPDP.Value = e.Patient.PDP;

                numSTE_I.Value = (decimal)e.Patient.ST_Elevation[(int)Rhythms.Leads.ECG_I];
                numSTE_II.Value = (decimal)e.Patient.ST_Elevation[(int)Rhythms.Leads.ECG_II];
                numSTE_III.Value = (decimal)e.Patient.ST_Elevation[(int)Rhythms.Leads.ECG_III];
                numSTE_aVR.Value = (decimal)e.Patient.ST_Elevation[(int)Rhythms.Leads.ECG_AVR];
                numSTE_aVL.Value = (decimal)e.Patient.ST_Elevation[(int)Rhythms.Leads.ECG_AVL];
                numSTE_aVF.Value = (decimal)e.Patient.ST_Elevation[(int)Rhythms.Leads.ECG_AVF];
                numSTE_V1.Value = (decimal)e.Patient.ST_Elevation[(int)Rhythms.Leads.ECG_V1];
                numSTE_V2.Value = (decimal)e.Patient.ST_Elevation[(int)Rhythms.Leads.ECG_V2];
                numSTE_V3.Value = (decimal)e.Patient.ST_Elevation[(int)Rhythms.Leads.ECG_V3];
                numSTE_V4.Value = (decimal)e.Patient.ST_Elevation[(int)Rhythms.Leads.ECG_V4];
                numSTE_V5.Value = (decimal)e.Patient.ST_Elevation[(int)Rhythms.Leads.ECG_V5];
                numSTE_V6.Value = (decimal)e.Patient.ST_Elevation[(int)Rhythms.Leads.ECG_V6];

                numTWE_I.Value = (decimal)e.Patient.T_Elevation[(int)Rhythms.Leads.ECG_I];
                numTWE_II.Value = (decimal)e.Patient.T_Elevation[(int)Rhythms.Leads.ECG_II];
                numTWE_III.Value = (decimal)e.Patient.T_Elevation[(int)Rhythms.Leads.ECG_III];
                numTWE_aVR.Value = (decimal)e.Patient.T_Elevation[(int)Rhythms.Leads.ECG_AVR];
                numTWE_aVL.Value = (decimal)e.Patient.T_Elevation[(int)Rhythms.Leads.ECG_AVL];
                numTWE_aVF.Value = (decimal)e.Patient.T_Elevation[(int)Rhythms.Leads.ECG_AVF];
                numTWE_V1.Value = (decimal)e.Patient.T_Elevation[(int)Rhythms.Leads.ECG_V1];
                numTWE_V2.Value = (decimal)e.Patient.T_Elevation[(int)Rhythms.Leads.ECG_V2];
                numTWE_V3.Value = (decimal)e.Patient.T_Elevation[(int)Rhythms.Leads.ECG_V3];
                numTWE_V4.Value = (decimal)e.Patient.T_Elevation[(int)Rhythms.Leads.ECG_V4];
                numTWE_V5.Value = (decimal)e.Patient.T_Elevation[(int)Rhythms.Leads.ECG_V5];
                numTWE_V6.Value = (decimal)e.Patient.T_Elevation[(int)Rhythms.Leads.ECG_V6];                
            }
        }
    }
}
