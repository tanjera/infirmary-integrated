using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace II.Forms {
    public partial class Form_Editor : Form {

        Patient tPatient;

        public Form_Editor (string[] args) {

            InitializeComponent ();

            foreach (string el in Cardiac_Rhythms.Descriptions)
                comboCardiacRhythm.Items.Add (el);
            comboCardiacRhythm.SelectedIndex = 0;

            foreach (string el in Respiratory_Rhythms.Descriptions)
                comboRespiratoryRhythm.Items.Add (el);
            comboRespiratoryRhythm.SelectedIndex = 0;

            InitPatient ();

            if (args.Length > 0)
                Load_Open (args [0]);

            // Debugging: auto-open cardiac monitor on program start
            ButtonMonitor_Click (this, new EventArgs ());
        }

        public bool RequestExit () {
            Application.Exit ();
            return true;
        }

        public Patient RequestNewPatient () {
            InitPatient ();
            return tPatient;
        }

        private void InitPatient () {
            tPatient = new Patient ();
            tPatient.PatientEvent += FormUpdateFields;
            FormUpdateFields (this, new Patient.PatientEvent_Args (tPatient, Patient.PatientEvent_Args.EventTypes.Vitals_Change));
        }

        private void InitMonitor () {
            if (Program.Device_Monitor != null && !Program.Device_Monitor.IsDisposed)
                return;

            Program.Device_Monitor = new Device_Monitor ();
            Program.Device_Monitor.SetPatient (tPatient);
            tPatient.PatientEvent += Program.Device_Monitor.OnPatientEvent;
        }

        private void Load_Open (string fileName) {
            if (File.Exists(fileName)) {
                Stream s = new FileStream (fileName, FileMode.Open);
                Load_Init (s);
            } else {
                Load_Fail ();
            }
        }

        private void Load_Init (Stream incFile) {
            StreamReader sr = new StreamReader (incFile);

            /* Read savefile metadata indicating data formatting
             * Multiple data formats for forward compatibility
             */
            string metadata = sr.ReadLine ();
            if (metadata.StartsWith (".ii:t1"))
                Load_Validate_T1 (sr);
            else
                Load_Fail ();
        }

        private void Load_Validate_T1 (StreamReader sr) {
            /* Savefile type 1: validated and obfuscated, not encrypted or data protected
             * Line 1 is metadata (.ii:t1)
             * Line 2 is hash for validation (hash taken of raw string data, unobfuscated)
             * Line 3 is savefile data obfuscated by Base64 encoding
             */

            string hash = sr.ReadLine ().Trim();
            string file = _.UnobfuscateB64(sr.ReadToEnd ().Trim());
            sr.Close ();

            if (hash == _.HashMD5 (file))
                Load_Process (file);
            else
                Load_Fail ();
        }

        private void Load_Process (string incFile) {
            StringReader sRead = new StringReader (incFile);
            string line, pline;
            StringBuilder pbuffer;

            try {
                while ((line = sRead.ReadLine ()) != null) {
                    if (line == "> Begin: Patient") {
                        pbuffer = new StringBuilder ();
                        while ((pline = sRead.ReadLine ()) != null && pline != "> End: Patient")
                            pbuffer.AppendLine (pline);
                        tPatient.Load_Process (pbuffer.ToString ());

                    } else if (line == "> Begin: Editor") {
                        pbuffer = new StringBuilder ();
                        while ((pline = sRead.ReadLine ()) != null && pline != "> End: Editor")
                            pbuffer.AppendLine (pline);
                        this.Load_Options (pbuffer.ToString ());

                    } else if (line == "> Begin: Cardiac Monitor") {
                        pbuffer = new StringBuilder ();
                        while ((pline = sRead.ReadLine ()) != null && pline != "> End: Cardiac Monitor")
                            pbuffer.AppendLine (pline);
                        InitMonitor ();
                        Program.Device_Monitor.Load_Process (pbuffer.ToString ());
                        Program.Device_Monitor.Show ();
                        Program.Device_Monitor.BringToFront ();
                    }
                }
            } catch {
                Load_Fail ();
            }
            sRead.Close ();
        }

        private void Load_Fail () {
            MessageBox.Show (
                "The selected file was unable to be loaded. Perhaps the file was damaged or edited outside of Infirmary Integrated.",
                "Unable to Load File",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void Save_T1 (Stream s) {
            StringBuilder sb = new StringBuilder ();

            sb.AppendLine ("> Begin: Patient");
            sb.Append (tPatient.Save ());
            sb.AppendLine ("> End: Patient");

            sb.AppendLine ("> Begin: Editor");
            sb.Append (this.Save_Options ());
            sb.AppendLine ("> End: Editor");

            if (Program.Device_Monitor != null && !Program.Device_Monitor.IsDisposed) {
                sb.AppendLine ("> Begin: Cardiac Monitor");
                sb.Append (Program.Device_Monitor.Save ());
                sb.AppendLine ("> End: Cardiac Monitor");
            }

            StreamWriter sw = new StreamWriter (s);
            sw.WriteLine (".ii:t1");                                // Metadata (type 1 savefile)
            sw.WriteLine (_.HashMD5 (sb.ToString ().Trim ()));      // Hash for validation
            sw.Write (_.ObfuscateB64 (sb.ToString ().Trim ()));     // Savefile data obfuscated with Base64
            sw.Close ();
            s.Close ();
        }

        private void Load_Options (string inc) {
            StringReader sRead = new StringReader (inc);

            try {
                string line;
                while ((line = sRead.ReadLine ()) != null) {
                    if (line.Contains (":")) {
                        string pName = line.Substring (0, line.IndexOf (':')),
                                pValue = line.Substring (line.IndexOf (':') + 1);
                        switch (pName) {
                            default: break;
                            case "checkDefaultVitals": checkDefaultVitals.Checked = bool.Parse (pValue); break;
                        }
                    }
                }
            } catch {
                sRead.Close ();
                return;
            }

            sRead.Close ();
        }

        private string Save_Options () {
            StringBuilder sWrite = new StringBuilder ();

            sWrite.AppendLine (String.Format ("{0}:{1}", "checkDefaultVitals", checkDefaultVitals.Checked));

            return sWrite.ToString ();
        }

        private void MenuLoadFile_Click (object sender, EventArgs e) {
            Stream s;
            OpenFileDialog dlgLoad = new OpenFileDialog ();

            dlgLoad.Filter = "Infirmary Integrated simulation files (*.ii)|*.ii|All files (*.*)|*.*";
            dlgLoad.FilterIndex = 1;
            dlgLoad.RestoreDirectory = true;

            if (dlgLoad.ShowDialog () == DialogResult.OK) {
                if ((s = dlgLoad.OpenFile ()) != null) {
                    Load_Init (s);
                    s.Close ();
                }
            }
        }

        private void MenuSaveFile_Click (object sender, EventArgs e) {
            Stream s;
            SaveFileDialog dlgSave = new SaveFileDialog ();

            dlgSave.Filter = "Infirmary Integrated simulation files (*.ii)|*.ii|All files (*.*)|*.*";
            dlgSave.FilterIndex = 1;
            dlgSave.RestoreDirectory = true;

            if (dlgSave.ShowDialog () == DialogResult.OK) {
                if ((s = dlgSave.OpenFile ()) != null) {
                    Save_T1 (s);
                }
            }
        }

        private void MenuExit_Click(object sender, EventArgs e) {
            RequestExit ();
        }

        private void MenuAbout_Click (object sender, EventArgs e) {
            Forms.Dialog_About about = new Forms.Dialog_About ();
            about.Show ();
        }

        private void ButtonMonitor_Click (object sender, EventArgs e)
        {
            InitMonitor ();
            Program.Device_Monitor.Show ();
            Program.Device_Monitor.BringToFront ();
        }

        private void ButtonResetParameters_Click (object sender, EventArgs e) {
            RequestNewPatient ();
        }

        private void ButtonApplyParameters_Click (object sender, EventArgs e) {
            tPatient.UpdateVitals (
                (int)numHR.Value,
                (int)numRR.Value,
                (int)numSPO2.Value,
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

                Cardiac_Rhythms.Parse_Description(comboCardiacRhythm.Text),

                Respiratory_Rhythms.Parse_Description(comboRespiratoryRhythm.Text),
                (int)numInspRatio.Value,
                (int)numExpRatio.Value
            );
        }

        private void FormUpdateFields(object sender, Patient.PatientEvent_Args e) {
            if (e.EventType == Patient.PatientEvent_Args.EventTypes.Vitals_Change) {
                numHR.Value = e.Patient.HR;
                numRR.Value = e.Patient.RR;
                numSPO2.Value = e.Patient.SPO2;
                numT.Value = (decimal)e.Patient.T;
                numCVP.Value = e.Patient.CVP;
                numETCO2.Value = e.Patient.ETCO2;

                numNSBP.Value = e.Patient.NSBP;
                numNDBP.Value = e.Patient.NDBP;
                numASBP.Value = e.Patient.ASBP;
                numADBP.Value = e.Patient.ADBP;
                numPSP.Value = e.Patient.PSP;
                numPDP.Value = e.Patient.PDP;

                comboCardiacRhythm.SelectedIndex = (int)e.Patient.Cardiac_Rhythm.Value;

                comboRespiratoryRhythm.SelectedIndex = (int)e.Patient.Respiratory_Rhythm.Value;
                numInspRatio.Value = (decimal)e.Patient.Respiratory_IERatio_I;
                numExpRatio.Value = (decimal)e.Patient.Respiratory_IERatio_E;


                numSTE_I.Value = (decimal)e.Patient.ST_Elevation[(int)Leads.Values.ECG_I];
                numSTE_II.Value = (decimal)e.Patient.ST_Elevation[(int)Leads.Values.ECG_II];
                numSTE_III.Value = (decimal)e.Patient.ST_Elevation[(int)Leads.Values.ECG_III];
                numSTE_aVR.Value = (decimal)e.Patient.ST_Elevation[(int)Leads.Values.ECG_AVR];
                numSTE_aVL.Value = (decimal)e.Patient.ST_Elevation[(int)Leads.Values.ECG_AVL];
                numSTE_aVF.Value = (decimal)e.Patient.ST_Elevation[(int)Leads.Values.ECG_AVF];
                numSTE_V1.Value = (decimal)e.Patient.ST_Elevation[(int)Leads.Values.ECG_V1];
                numSTE_V2.Value = (decimal)e.Patient.ST_Elevation[(int)Leads.Values.ECG_V2];
                numSTE_V3.Value = (decimal)e.Patient.ST_Elevation[(int)Leads.Values.ECG_V3];
                numSTE_V4.Value = (decimal)e.Patient.ST_Elevation[(int)Leads.Values.ECG_V4];
                numSTE_V5.Value = (decimal)e.Patient.ST_Elevation[(int)Leads.Values.ECG_V5];
                numSTE_V6.Value = (decimal)e.Patient.ST_Elevation[(int)Leads.Values.ECG_V6];

                numTWE_I.Value = (decimal)e.Patient.T_Elevation[(int)Leads.Values.ECG_I];
                numTWE_II.Value = (decimal)e.Patient.T_Elevation[(int)Leads.Values.ECG_II];
                numTWE_III.Value = (decimal)e.Patient.T_Elevation[(int)Leads.Values.ECG_III];
                numTWE_aVR.Value = (decimal)e.Patient.T_Elevation[(int)Leads.Values.ECG_AVR];
                numTWE_aVL.Value = (decimal)e.Patient.T_Elevation[(int)Leads.Values.ECG_AVL];
                numTWE_aVF.Value = (decimal)e.Patient.T_Elevation[(int)Leads.Values.ECG_AVF];
                numTWE_V1.Value = (decimal)e.Patient.T_Elevation[(int)Leads.Values.ECG_V1];
                numTWE_V2.Value = (decimal)e.Patient.T_Elevation[(int)Leads.Values.ECG_V2];
                numTWE_V3.Value = (decimal)e.Patient.T_Elevation[(int)Leads.Values.ECG_V3];
                numTWE_V4.Value = (decimal)e.Patient.T_Elevation[(int)Leads.Values.ECG_V4];
                numTWE_V5.Value = (decimal)e.Patient.T_Elevation[(int)Leads.Values.ECG_V5];
                numTWE_V6.Value = (decimal)e.Patient.T_Elevation[(int)Leads.Values.ECG_V6];
            }
        }

        private void OnRhythmSelected (object sender, EventArgs e) {
            if (checkDefaultVitals.Checked && tPatient != null) {
                Cardiac_Rhythms.Default_Vitals v = Cardiac_Rhythms.DefaultVitals(
                    Cardiac_Rhythms.Parse_Description(comboCardiacRhythm.Text));

                numHR.Value = (decimal)_.Clamp ((double)numHR.Value, v.HRMin, v.HRMax);
                numSPO2.Value = (decimal)_.Clamp ((double)numSPO2.Value, v.SPO2Min, v.SPO2Max);
                numETCO2.Value = (decimal)_.Clamp ((double)numETCO2.Value, v.ETCO2Min, v.ETCO2Max);
                numNSBP.Value = (decimal)_.Clamp ((double)numNSBP.Value, v.SBPMin, v.SBPMax);
                numNDBP.Value = (decimal)_.Clamp ((double)numNDBP.Value, v.DBPMin, v.DBPMax);
                numASBP.Value = (decimal)_.Clamp ((double)numASBP.Value, v.SBPMin, v.SBPMax);
                numADBP.Value = (decimal)_.Clamp ((double)numADBP.Value, v.DBPMin, v.DBPMax);
                numPSP.Value = (decimal)_.Clamp ((double)numPSP.Value, v.PSPMin, v.PSPMax);
                numPDP.Value = (decimal)_.Clamp ((double)numPDP.Value, v.PDPMin, v.PDPMax);
            }
        }

        private void OnNumUpDown_Enter (object sender, EventArgs e) {
            ((NumericUpDown)sender).Select (0, ((NumericUpDown)sender).Text.Length);
        }
    }
}
