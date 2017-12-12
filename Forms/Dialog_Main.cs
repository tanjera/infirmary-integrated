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
                
                new double[] {
                    (double)numSTE_I.Value, (double)numSTE_II.Value, (double)numSTE_III.Value,
                    (double)numSTE_aVR.Value, (double)numSTE_aVL.Value, (double)numSTE_aVF.Value,
                    (double)numSTE_V1.Value, (double)numSTE_V2.Value, (double)numSTE_V3.Value,
                    (double)numSTE_V4.Value, (double)numSTE_V5.Value, (double)numSTE_V6.Value
                },
                new double[] {
                    (double)numTWE_I.Value, (double)numTWE_II.Value, (double)numTWE_III.Value,
                    (double)numTWE_aVR.Value, (double)numTWE_aVL.Value, (double)numTWE_aVF.Value,
                    (double)numTWE_V1.Value, (double)numTWE_V2.Value, (double)numTWE_V3.Value,
                    (double)numTWE_V4.Value, (double)numTWE_V5.Value, (double)numTWE_V6.Value
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
                
                comboCardiacRhythm.SelectedIndex = (int)e.Patient.Cardiac_Rhythm;
                comboAxisShift.SelectedIndex = (int)e.Patient.Cardiac_Axis_Shift;

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

        private void onRhythmSelected (object sender, EventArgs e) {
            if (checkDefaultVitals.Checked) {
                Rhythms._Rhythm tRhythm = Rhythms.Rhythm_Index.Get_Rhythm (
                    (Rhythms.Cardiac_Rhythm)Enum.Parse (typeof (Rhythms.Cardiac_Rhythm), _.SpaceToUnderscore (comboCardiacRhythm.Text)));
                
                numHR.Value = (decimal)_.Clamp ((double)numHR.Value, tRhythm.Range_HR.Min, tRhythm.Range_HR.Max);
                numNSBP.Value = (decimal)_.Clamp ((double)numNSBP.Value, tRhythm.Range_SBP.Min, tRhythm.Range_SBP.Max);
                numNDBP.Value = (decimal)_.Clamp ((double)numNDBP.Value, tRhythm.Range_DBP.Min, tRhythm.Range_DBP.Max);
                numASBP.Value = (decimal)_.Clamp ((double)numASBP.Value, tRhythm.Range_SBP.Min, tRhythm.Range_SBP.Max);                
                numADBP.Value = (decimal)_.Clamp ((double)numADBP.Value, tRhythm.Range_DBP.Min, tRhythm.Range_DBP.Max);
                numSpO2.Value = (decimal)_.Clamp ((double)numSpO2.Value, tRhythm.Range_SpO2.Min, tRhythm.Range_SpO2.Max);
            }
        }
    }
}
