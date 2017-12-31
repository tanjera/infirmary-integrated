using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using II;
using II.Localization;

namespace II_Windows {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class PatientEditor : Window {

        Patient tPatient;

        public PatientEditor () {
            InitializeComponent ();

            InitLanguage ();
            InitUIStrings ();
            InitPatient ();

            //InitMonitor ();
        }

        private void InitLanguage () {

            try {
                if (Properties.Settings.Default.Language != null)
                    App.Language.Value = (Languages.Values)Enum.Parse (typeof (Languages.Values), Properties.Settings.Default.Language);
            } catch { }

            if (Properties.Settings.Default.Language == null) {
                App.Dialog_Language = new DialogLanguage ();
                App.Dialog_Language.Activate ();
                App.Dialog_Language.Show ();
            }

            /* IMP
            Properties.Settings.Default.Language = App.Language.Value.ToString();
            Properties.Settings.Default.Save ();
            */
        }

        private void InitUIStrings () {
            // IMP: Get language selection!
            Languages l = new Languages (Languages.Values.EN);

            menuFile.Header = Strings.Lookup (l.Value, "File");
            menuLoad.Header = Strings.Lookup (l.Value, "LoadSimulation");
            menuSave.Header = Strings.Lookup (l.Value, "SaveSimulation");
            menuExit.Header = Strings.Lookup (l.Value, "ExitProgram");
            menuHelp.Header = Strings.Lookup (l.Value, "Help");
            menuAbout.Header = Strings.Lookup (l.Value, "AboutProgram");

            lblGroupDevices.Content = Strings.Lookup (l.Value, "Devices");
            lblDeviceMonitor.Content = Strings.Lookup (l.Value, "CardiacMonitor");
            lblDevice12LeadECG.Content = Strings.Lookup (l.Value, "12LeadECG");
            lblDeviceDefibrillator.Content = Strings.Lookup (l.Value, "Defibrillator");
            lblDeviceVentilator.Content = Strings.Lookup (l.Value, "Ventilator");
            lblDeviceIABP.Content = Strings.Lookup (l.Value, "IABP");
            lblDeviceCardiotocograph.Content = Strings.Lookup (l.Value, "Cardiotocograph");
            lblDeviceIVPump.Content = Strings.Lookup (l.Value, "IVPump");
            lblDeviceLabResults.Content = Strings.Lookup (l.Value, "LabResults");

            lblGroupVitalSigns.Content = Strings.Lookup (l.Value, "VitalSigns");
            lblHR.Content = String.Format ("{0}:", Strings.Lookup (l.Value, "HeartRate"));
            lblNIBP.Content = String.Format ("{0}:", Strings.Lookup (l.Value, "BloodPressure"));
            lblRR.Content = String.Format ("{0}:", Strings.Lookup (l.Value, "RespiratoryRate"));
            lblSPO2.Content = String.Format ("{0}:", Strings.Lookup (l.Value, "PulseOximetry"));
            lblT.Content = String.Format("{0}:", Strings.Lookup (l.Value, "Temperature"));
            lblCardiacRhythm.Content = String.Format ("{0}:", Strings.Lookup (l.Value, "CardiacRhythm"));
            lblRespiratoryRhythm.Content = String.Format ("{0}:", Strings.Lookup (l.Value, "RespiratoryRhythm"));
            checkDefaultVitals.Content = Strings.Lookup (l.Value, "UseDefaultVitalSignRanges");

            lblGroupHemodynamics.Content = Strings.Lookup (l.Value, "AdvancedHemodynamics");
            lblETCO2.Content = String.Format ("{0}:", Strings.Lookup (l.Value, "EndTidalCO2"));
            lblCVP.Content = String.Format ("{0}:", Strings.Lookup (l.Value, "CentralVenousPressure"));
            lblASBP.Content = String.Format ("{0}:", Strings.Lookup (l.Value, "ArterialBloodPressure"));
            lblPSP.Content = String.Format ("{0}:", Strings.Lookup (l.Value, "PulmonaryArteryPressure"));

            lblGroupRespiratoryProfile.Content = Strings.Lookup (l.Value, "RespiratoryProfile");
            lblInspiratoryRatio.Content = String.Format ("{0}:", Strings.Lookup (l.Value, "InspiratoryExpiratoryRatio"));

            lblGroupCardiacProfile.Content = Strings.Lookup (l.Value, "CardiacProfile");
            grpSTSegmentElevation.Header = Strings.Lookup (l.Value, "STSegmentElevation");
            grpTWaveElevation.Header = Strings.Lookup (l.Value, "TWaveElevation");

            lblParametersApply.Content = Strings.Lookup (l.Value, "ApplyChanges");
            lblParametersReset.Content = Strings.Lookup (l.Value, "ResetParameters");

            comboCardiacRhythm.ItemsSource = Cardiac_Rhythms.Descriptions;
            comboRespiratoryRhythm.ItemsSource = Respiratory_Rhythms.Descriptions;
        }

        public bool RequestExit () {
            Application.Current.Shutdown();
            return true;
        }

        public Patient RequestNewPatient () {
            InitPatient ();
            return tPatient;
        }

        private void InitPatient () {
            tPatient = new Patient ();

            DispatcherTimer dt = new DispatcherTimer ();
            dt.Interval = new TimeSpan(100000); // q 10 milliseconds
            dt.Tick += tPatient.Timers_Process;

            tPatient.PatientEvent += FormUpdateFields;
            FormUpdateFields (this, new Patient.PatientEvent_Args (tPatient, Patient.PatientEvent_Args.EventTypes.Vitals_Change));
        }

        private void InitMonitor () {
            if (App.Device_Monitor == null || !App.Device_Monitor.IsLoaded)
                App.Device_Monitor = new DeviceMonitor ();

            App.Device_Monitor.Activate ();
            App.Device_Monitor.Show ();

            App.Device_Monitor.SetPatient (tPatient);
            tPatient.PatientEvent += App.Device_Monitor.OnPatientEvent;
        }

        private void Load_Open (string fileName) {
            if (File.Exists (fileName)) {
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

            string hash = sr.ReadLine ().Trim ();
            string file = Utility.UnobfuscateB64 (sr.ReadToEnd ().Trim ());
            sr.Close ();

            if (hash == Utility.HashMD5 (file))
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
                        App.Device_Monitor.Load_Process (pbuffer.ToString ());
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
                "Unable to Load File", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Save_T1 (Stream s) {
            StringBuilder sb = new StringBuilder ();

            sb.AppendLine ("> Begin: Patient");
            sb.Append (tPatient.Save ());
            sb.AppendLine ("> End: Patient");

            sb.AppendLine ("> Begin: Editor");
            sb.Append (this.Save_Options ());
            sb.AppendLine ("> End: Editor");

            // Imp: Reference cardiac monitor for save data

            if (App.Device_Monitor != null && App.Device_Monitor.IsLoaded) {
                sb.AppendLine ("> Begin: Cardiac Monitor");
                sb.Append (App.Device_Monitor.Save ());
                sb.AppendLine ("> End: Cardiac Monitor");
            }


            StreamWriter sw = new StreamWriter (s);
            sw.WriteLine (".ii:t1");                                // Metadata (type 1 savefile)
            sw.WriteLine (Utility.HashMD5 (sb.ToString ().Trim ()));      // Hash for validation
            sw.Write (Utility.ObfuscateB64 (sb.ToString ().Trim ()));     // Savefile data obfuscated with Base64
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
                            case "checkDefaultVitals": checkDefaultVitals.IsChecked = bool.Parse (pValue); break;
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

            sWrite.AppendLine (String.Format ("{0}:{1}", "checkDefaultVitals", checkDefaultVitals.IsChecked));

            return sWrite.ToString ();
        }

        private void MenuLoadFile_Click (object sender, RoutedEventArgs e) {
            Stream s;
            Microsoft.Win32.OpenFileDialog dlgLoad = new Microsoft.Win32.OpenFileDialog ();

            dlgLoad.Filter = "Infirmary Integrated simulation files (*.ii)|*.ii|All files (*.*)|*.*";
            dlgLoad.FilterIndex = 1;
            dlgLoad.RestoreDirectory = true;

            if (dlgLoad.ShowDialog () == true) {
                if ((s = dlgLoad.OpenFile ()) != null) {
                    Load_Init (s);
                    s.Close ();
                }
            }
        }

        private void MenuSaveFile_Click (object sender, RoutedEventArgs e) {
            Stream s;
            Microsoft.Win32.SaveFileDialog dlgSave = new Microsoft.Win32.SaveFileDialog ();

            dlgSave.Filter = "Infirmary Integrated simulation files (*.ii)|*.ii|All files (*.*)|*.*";
            dlgSave.FilterIndex = 1;
            dlgSave.RestoreDirectory = true;

            if (dlgSave.ShowDialog () == true) {
                if ((s = dlgSave.OpenFile ()) != null) {
                    Save_T1 (s);
                }
            }
        }

        private void MenuExit_Click (object sender, RoutedEventArgs e) {
            RequestExit ();
        }

        private void MenuAbout_Click (object sender, RoutedEventArgs e) {
            App.Dialog_About = new DialogAbout();
            App.Dialog_About.Activate ();
            App.Dialog_About.Show ();
        }

        private void ButtonDeviceMonitor_Click (object sender, RoutedEventArgs e) {
            InitMonitor ();
        }

        private void ButtonResetParameters_Click (object sender, RoutedEventArgs e) {
            RequestNewPatient ();
        }

        private void ButtonApplyParameters_Click (object sender, RoutedEventArgs e) {
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

                new double [] {
                (double)numSTE_I.Value, (double)numSTE_II.Value, (double)numSTE_III.Value,
                (double)numSTE_aVR.Value, (double)numSTE_aVL.Value, (double)numSTE_aVF.Value,
                (double)numSTE_V1.Value, (double)numSTE_V2.Value, (double)numSTE_V3.Value,
                (double)numSTE_V4.Value, (double)numSTE_V5.Value, (double)numSTE_V6.Value
                },
                new double [] {
                (double)numTWE_I.Value, (double)numTWE_II.Value, (double)numTWE_III.Value,
                (double)numTWE_aVR.Value, (double)numTWE_aVL.Value, (double)numTWE_aVF.Value,
                (double)numTWE_V1.Value, (double)numTWE_V2.Value, (double)numTWE_V3.Value,
                (double)numTWE_V4.Value, (double)numTWE_V5.Value, (double)numTWE_V6.Value
                },

                Cardiac_Rhythms.Parse_Description (comboCardiacRhythm.Text),

                Respiratory_Rhythms.Parse_Description (comboRespiratoryRhythm.Text),
                (int)numInspiratoryRatio.Value,
                (int)numExpiratoryRatio.Value
            );
        }

        private void FormUpdateFields (object sender, Patient.PatientEvent_Args e) {
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
                numInspiratoryRatio.Value = e.Patient.Respiratory_IERatio_I;
                numExpiratoryRatio.Value = e.Patient.Respiratory_IERatio_E;


                numSTE_I.Value = (decimal)e.Patient.ST_Elevation [(int)Leads.Values.ECG_I];
                numSTE_II.Value = (decimal)e.Patient.ST_Elevation [(int)Leads.Values.ECG_II];
                numSTE_III.Value = (decimal)e.Patient.ST_Elevation [(int)Leads.Values.ECG_III];
                numSTE_aVR.Value = (decimal)e.Patient.ST_Elevation [(int)Leads.Values.ECG_AVR];
                numSTE_aVL.Value = (decimal)e.Patient.ST_Elevation [(int)Leads.Values.ECG_AVL];
                numSTE_aVF.Value = (decimal)e.Patient.ST_Elevation [(int)Leads.Values.ECG_AVF];
                numSTE_V1.Value = (decimal)e.Patient.ST_Elevation [(int)Leads.Values.ECG_V1];
                numSTE_V2.Value = (decimal)e.Patient.ST_Elevation [(int)Leads.Values.ECG_V2];
                numSTE_V3.Value = (decimal)e.Patient.ST_Elevation [(int)Leads.Values.ECG_V3];
                numSTE_V4.Value = (decimal)e.Patient.ST_Elevation [(int)Leads.Values.ECG_V4];
                numSTE_V5.Value = (decimal)e.Patient.ST_Elevation [(int)Leads.Values.ECG_V5];
                numSTE_V6.Value = (decimal)e.Patient.ST_Elevation [(int)Leads.Values.ECG_V6];

                numTWE_I.Value = (decimal)e.Patient.T_Elevation [(int)Leads.Values.ECG_I];
                numTWE_II.Value = (decimal)e.Patient.T_Elevation [(int)Leads.Values.ECG_II];
                numTWE_III.Value = (decimal)e.Patient.T_Elevation [(int)Leads.Values.ECG_III];
                numTWE_aVR.Value = (decimal)e.Patient.T_Elevation [(int)Leads.Values.ECG_AVR];
                numTWE_aVL.Value = (decimal)e.Patient.T_Elevation [(int)Leads.Values.ECG_AVL];
                numTWE_aVF.Value = (decimal)e.Patient.T_Elevation [(int)Leads.Values.ECG_AVF];
                numTWE_V1.Value = (decimal)e.Patient.T_Elevation [(int)Leads.Values.ECG_V1];
                numTWE_V2.Value = (decimal)e.Patient.T_Elevation [(int)Leads.Values.ECG_V2];
                numTWE_V3.Value = (decimal)e.Patient.T_Elevation [(int)Leads.Values.ECG_V3];
                numTWE_V4.Value = (decimal)e.Patient.T_Elevation [(int)Leads.Values.ECG_V4];
                numTWE_V5.Value = (decimal)e.Patient.T_Elevation [(int)Leads.Values.ECG_V5];
                numTWE_V6.Value = (decimal)e.Patient.T_Elevation [(int)Leads.Values.ECG_V6];
            }
        }

        private void OnRhythmSelected (object sender, SelectionChangedEventArgs e) {
            if ((bool)checkDefaultVitals.IsChecked && tPatient != null) {
                Cardiac_Rhythms.Default_Vitals v = Cardiac_Rhythms.DefaultVitals (
                    Cardiac_Rhythms.Parse_Description (comboCardiacRhythm.Text));

                numHR.Value = (int)Utility.Clamp ((double)numHR.Value, v.HRMin, v.HRMax);
                numSPO2.Value = (int)Utility.Clamp ((double)numSPO2.Value, v.SPO2Min, v.SPO2Max);
                numETCO2.Value = (int)Utility.Clamp ((double)numETCO2.Value, v.ETCO2Min, v.ETCO2Max);
                numNSBP.Value = (int)Utility.Clamp ((double)numNSBP.Value, v.SBPMin, v.SBPMax);
                numNDBP.Value = (int)Utility.Clamp ((double)numNDBP.Value, v.DBPMin, v.DBPMax);
                numASBP.Value = (int)Utility.Clamp ((double)numASBP.Value, v.SBPMin, v.SBPMax);
                numADBP.Value = (int)Utility.Clamp ((double)numADBP.Value, v.DBPMin, v.DBPMax);
                numPSP.Value = (int)Utility.Clamp ((double)numPSP.Value, v.PSPMin, v.PSPMax);
                numPDP.Value = (int)Utility.Clamp ((double)numPDP.Value, v.PDPMin, v.PDPMax);
            }
        }

        private void OnClosed (object sender, EventArgs e) {
            RequestExit ();
        }
    }
}
