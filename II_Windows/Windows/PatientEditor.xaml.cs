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

using II;
using II.Localization;

namespace II_Windows {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class PatientEditor : Window {

        // Define WPF UI commands for binding
        private ICommand icLoadFile, icSaveFile;
        public ICommand IC_LoadFile { get { return icLoadFile; } }
        public ICommand IC_SaveFile { get { return icSaveFile; } }

        public PatientEditor () {
            InitializeComponent ();
            DataContext = this;
            App.Patient_Editor = this;

            InitLanguage ();
            InitInterface ();
            InitPatient ();

            if (App.Start_Args.Length > 0)
                LoadOpen (App.Start_Args [0]);

            // For debugging. Automatically open window being worked on, hide patient editor.
            //InitDeviceIABP();
            //WindowState = WindowState.Minimized;
        }

        private void InitLanguage () {
            string setLang = Properties.Settings.Default.Language;

            if (setLang == null || setLang == ""
                || !Enum.TryParse<Languages.Values>(setLang, out App.Language.Value)) {
                App.Language = new Languages ();
                DialogLanguage ();
            }
        }

        private void InitInterface () {
            // Initiate ICommands for KeyBindings
            icLoadFile = new ActionCommand (() => LoadFile ());
            icSaveFile = new ActionCommand (() => SaveFile ());

            // Populate UI strings per language selection
            wdwPatientEditor.Title = App.Language.Dictionary["PE:WindowTitle"];
            menuFile.Header = App.Language.Dictionary["PE:MenuFile"];
            menuLoad.Header = App.Language.Dictionary["PE:MenuLoadSimulation"];
            menuSave.Header = App.Language.Dictionary["PE:MenuSaveSimulation"];
            menuExit.Header = App.Language.Dictionary["PE:MenuExitProgram"];

            menuSettings.Header = App.Language.Dictionary["PE:MenuSettings"];
            menuSetLanguage.Header = App.Language.Dictionary["PE:MenuSetLanguage"];

            menuHelp.Header = App.Language.Dictionary["PE:MenuHelp"];
            menuAbout.Header = App.Language.Dictionary["PE:MenuAboutProgram"];

            lblGroupDevices.Content = App.Language.Dictionary["PE:Devices"];
            lblDeviceMonitor.Content = App.Language.Dictionary["PE:CardiacMonitor"];
            lblDevice12LeadECG.Content = App.Language.Dictionary["PE:12LeadECG"];
            lblDeviceDefibrillator.Content = App.Language.Dictionary["PE:Defibrillator"];
            //lblDeviceVentilator.Content = App.Language.Dictionary["PE:Ventilator"];
            lblDeviceIABP.Content = App.Language.Dictionary["PE:IABP"];
            //lblDeviceEFM.Content = App.Language.Dictionary["PE:EFM"];
            //lblDeviceIVPump.Content = App.Language.Dictionary["PE:IVPump"];
            //lblDeviceLabResults.Content = App.Language.Dictionary["PE:LabResults"];

            lblGroupVitalSigns.Content = App.Language.Dictionary["PE:VitalSigns"];
            lblHR.Content = String.Format ("{0}:", App.Language.Dictionary["PE:HeartRate"]);
            lblNIBP.Content = String.Format ("{0}:", App.Language.Dictionary["PE:BloodPressure"]);
            lblRR.Content = String.Format ("{0}:", App.Language.Dictionary["PE:RespiratoryRate"]);
            lblSPO2.Content = String.Format ("{0}:", App.Language.Dictionary["PE:PulseOximetry"]);
            lblT.Content = String.Format("{0}:", App.Language.Dictionary["PE:Temperature"]);
            lblCardiacRhythm.Content = String.Format ("{0}:", App.Language.Dictionary["PE:CardiacRhythm"]);
            lblRespiratoryRhythm.Content = String.Format ("{0}:", App.Language.Dictionary["PE:RespiratoryRhythm"]);
            checkDefaultVitals.Content = App.Language.Dictionary["PE:UseDefaultVitalSignRanges"];

            lblGroupHemodynamics.Content = App.Language.Dictionary["PE:AdvancedHemodynamics"];
            lblETCO2.Content = String.Format ("{0}:", App.Language.Dictionary["PE:EndTidalCO2"]);
            lblCVP.Content = String.Format ("{0}:", App.Language.Dictionary["PE:CentralVenousPressure"]);
            lblASBP.Content = String.Format ("{0}:", App.Language.Dictionary["PE:ArterialBloodPressure"]);
            lblPSP.Content = String.Format ("{0}:", App.Language.Dictionary["PE:PulmonaryArteryPressure"]);
            lblICP.Content = String.Format("{0}:", App.Language.Dictionary["PE:IntracranialPressure"]);
            lblIAP.Content = String.Format("{0}:", App.Language.Dictionary["PE:IntraabdominalPressure"]);

            lblGroupRespiratoryProfile.Content = App.Language.Dictionary["PE:RespiratoryProfile"];
            lblInspiratoryRatio.Content = String.Format ("{0}:", App.Language.Dictionary["PE:InspiratoryExpiratoryRatio"]);

            lblGroupCardiacProfile.Content = App.Language.Dictionary["PE:CardiacProfile"];
            grpSTSegmentElevation.Header = App.Language.Dictionary["PE:STSegmentElevation"];
            grpTWaveElevation.Header = App.Language.Dictionary["PE:TWaveElevation"];

            lblGroupObstetricProfile.Content = App.Language.Dictionary["PE:ObstetricProfile"];
            lblFHR.Content = String.Format ("{0}:", App.Language.Dictionary["PE:FetalHeartRate"]);
            lblFHRRhythms.Content = String.Format ("{0}:", App.Language.Dictionary["PE:FetalHeartRhythms"]);
            lblFHRVariability.Content = String.Format ("{0}:", App.Language.Dictionary["PE:FetalHeartVariability"]);
            lblUCFrequency.Content = String.Format ("{0}:", App.Language.Dictionary["PE:UterineContractionFrequency"]);
            lblUCDuration.Content = String.Format ("{0}:", App.Language.Dictionary["PE:UterineContractionDuration"]);
            lblUCIntensity.Content = String.Format ("{0}:", App.Language.Dictionary["PE:UterineContractionIntensity"]);

            lblParametersApply.Content = App.Language.Dictionary["BUTTON:ApplyChanges"];
            lblParametersReset.Content = App.Language.Dictionary["BUTTON:ResetParameters"];

            List<string>    cardiacRhythms = new List<string> (),
                            respiratoryRhythms = new List<string> (),
                            intensityScale = new List<string> (),
                            fetalHeartRhythms = new List<string> ();

            foreach (CardiacRhythms.Values v in Enum.GetValues (typeof (CardiacRhythms.Values)))
                cardiacRhythms.Add (App.Language.Dictionary[CardiacRhythms.LookupString (v)]);
            comboCardiacRhythm.ItemsSource = cardiacRhythms;

            foreach (RespiratoryRhythms.Values v in Enum.GetValues (typeof (RespiratoryRhythms.Values)))
                respiratoryRhythms.Add (App.Language.Dictionary[RespiratoryRhythms.LookupString (v)]);
            comboRespiratoryRhythm.ItemsSource = respiratoryRhythms;

            foreach (Patient.Intensity.Values v in Enum.GetValues (typeof (Patient.Intensity.Values)))
                intensityScale.Add (App.Language.Dictionary[Patient.Intensity.LookupString (v)]);
            comboFHRVariability.ItemsSource = intensityScale;
            comboUCIntensity.ItemsSource = intensityScale;

            foreach (FetalHeartDecelerations.Values v in Enum.GetValues (typeof (FetalHeartDecelerations.Values)))
                fetalHeartRhythms.Add (App.Language.Dictionary[FetalHeartDecelerations.LookupString (v)]);
            listFHRRhythms.ItemsSource = fetalHeartRhythms;
        }

        private void InitPatient () {
            App.Patient = new Patient ();

            App.Timer_Main.Tick += App.Patient.Timers_Process;
            App.Patient.PatientEvent += FormUpdateFields;
            FormUpdateFields (this, new Patient.PatientEvent_Args (App.Patient, Patient.PatientEvent_Args.EventTypes.Vitals_Change));
        }

        private void InitDeviceMonitor () {
            if (App.Device_Monitor == null || !App.Device_Monitor.IsLoaded)
                App.Device_Monitor = new DeviceMonitor ();

            App.Device_Monitor.Activate ();
            App.Device_Monitor.Show ();

            App.Patient.PatientEvent += App.Device_Monitor.OnPatientEvent;
        }

        private void InitDeviceECG () {
            if (App.Device_ECG == null || !App.Device_ECG.IsLoaded)
                App.Device_ECG = new DeviceECG ();

            App.Device_ECG.Activate ();
            App.Device_ECG.Show ();

            App.Patient.PatientEvent += App.Device_ECG.OnPatientEvent;
        }

        private void InitDeviceDefib () {
            if (App.Device_Defib == null || !App.Device_Defib.IsLoaded)
                App.Device_Defib = new DeviceDefib ();

            App.Device_Defib.Activate ();
            App.Device_Defib.Show ();

            App.Patient.PatientEvent += App.Device_Defib.OnPatientEvent;
        }

        private void InitDeviceIABP () {
            if (App.Device_IABP == null || !App.Device_IABP.IsLoaded)
                App.Device_IABP = new DeviceIABP ();

            App.Device_IABP.Activate ();
            App.Device_IABP.Show ();

            App.Patient.PatientEvent += App.Device_IABP.OnPatientEvent;
        }
        private void DialogLanguage(bool reloadUI = false) {
            App.Dialog_Language = new DialogLanguage ();
            App.Dialog_Language.Activate ();
            App.Dialog_Language.ShowDialog ();

            if (reloadUI)
                InitInterface ();
        }

        private void DialogAbout (bool reloadUI = false) {
            App.Dialog_About = new DialogAbout();
            App.Dialog_About.Activate ();
            App.Dialog_About.ShowDialog ();
        }

        private void LoadOpen (string fileName) {
            if (File.Exists (fileName)) {
                Stream s = new FileStream (fileName, FileMode.Open);
                LoadInit (s);
            } else {
                LoadFail ();
            }
        }

        private void LoadInit (Stream incFile) {
            StreamReader sr = new StreamReader (incFile);

            /* Read savefile metadata indicating data formatting
                * Multiple data formats for forward compatibility
                */
            string metadata = sr.ReadLine ();
            if (metadata.StartsWith (".ii:t1"))
                LoadValidateT1 (sr);
            else
                LoadFail ();
        }

        private void LoadValidateT1 (StreamReader sr) {
            /* Savefile type 1: validated and obfuscated, not encrypted or data protected
                * Line 1 is metadata (.ii:t1)
                * Line 2 is hash for validation (hash taken of raw string data, unobfuscated)
                * Line 3 is savefile data obfuscated by Base64 encoding
                */

            string hash = sr.ReadLine ().Trim ();
            string file = Utility.UnobfuscateB64 (sr.ReadToEnd ().Trim ());
            sr.Close ();

            if (hash == Utility.HashMD5 (file))
                LoadProcess (file);
            else
                LoadFail ();
        }

        private void LoadProcess (string incFile) {
            StringReader sRead = new StringReader (incFile);
            string line, pline;
            StringBuilder pbuffer;

            try {
                while ((line = sRead.ReadLine ()) != null) {
                    if (line == "> Begin: Patient") {
                        pbuffer = new StringBuilder ();
                        while ((pline = sRead.ReadLine ()) != null && pline != "> End: Patient")
                            pbuffer.AppendLine (pline);
                        App.Patient.Load_Process (pbuffer.ToString ());

                    } else if (line == "> Begin: Editor") {
                        pbuffer = new StringBuilder ();
                        while ((pline = sRead.ReadLine ()) != null && pline != "> End: Editor")
                            pbuffer.AppendLine (pline);
                        this.LoadOptions (pbuffer.ToString ());

                    } else if (line == "> Begin: Cardiac Monitor") {
                        pbuffer = new StringBuilder ();
                        while ((pline = sRead.ReadLine ()) != null && pline != "> End: Cardiac Monitor")
                            pbuffer.AppendLine (pline);

                        InitDeviceMonitor ();
                        App.Device_Monitor.Load_Process (pbuffer.ToString ());
                    }
                }
            } catch {
                LoadFail ();
            }
            sRead.Close ();
        }

        private void LoadFail () {
            MessageBox.Show (
                "The selected file was unable to be loaded. Perhaps the file was damaged or edited outside of Infirmary Integrated.",
                "Unable to Load File", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void SaveT1 (Stream s) {
            StringBuilder sb = new StringBuilder ();

            sb.AppendLine ("> Begin: Patient");
            sb.Append (App.Patient.Save ());
            sb.AppendLine ("> End: Patient");

            sb.AppendLine ("> Begin: Editor");
            sb.Append (this.SaveOptions ());
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

        private void LoadOptions (string inc) {
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

        private string SaveOptions () {
            StringBuilder sWrite = new StringBuilder ();

            sWrite.AppendLine (String.Format ("{0}:{1}", "checkDefaultVitals", checkDefaultVitals.IsChecked));

            return sWrite.ToString ();
        }

        private void LoadFile () {
            Stream s;
            Microsoft.Win32.OpenFileDialog dlgLoad = new Microsoft.Win32.OpenFileDialog ();

            dlgLoad.Filter = "Infirmary Integrated simulation files (*.ii)|*.ii|All files (*.*)|*.*";
            dlgLoad.FilterIndex = 1;
            dlgLoad.RestoreDirectory = true;

            if (dlgLoad.ShowDialog () == true) {
                if ((s = dlgLoad.OpenFile ()) != null) {
                    LoadInit (s);
                    s.Close ();
                }
            }
        }

        private void SaveFile () {
            Stream s;
            Microsoft.Win32.SaveFileDialog dlgSave = new Microsoft.Win32.SaveFileDialog ();

            dlgSave.Filter = "Infirmary Integrated simulation files (*.ii)|*.ii|All files (*.*)|*.*";
            dlgSave.FilterIndex = 1;
            dlgSave.RestoreDirectory = true;

            if (dlgSave.ShowDialog () == true) {
                if ((s = dlgSave.OpenFile ()) != null) {
                    SaveT1 (s);
                }
            }
        }

        public bool RequestExit () {
            Application.Current.Shutdown ();
            return true;
        }

        public Patient RequestNewPatient () {
            InitPatient ();
            return App.Patient;
        }

        private void MenuLoadFile_Click (object s, RoutedEventArgs e) => LoadFile ();
        private void MenuSaveFile_Click (object s, RoutedEventArgs e) => SaveFile ();
        private void MenuExit_Click (object s, RoutedEventArgs e) => RequestExit ();
        private void MenuSetLanguage_Click (object s, RoutedEventArgs e) => DialogLanguage (true);
        private void MenuAbout_Click (object s, RoutedEventArgs e) => DialogAbout ();
        private void ButtonDeviceMonitor_Click (object s, RoutedEventArgs e) => InitDeviceMonitor ();
        private void ButtonDeviceECG_Click (object s, RoutedEventArgs e) => InitDeviceECG ();
        private void ButtonDeviceIABP_Click (object s, RoutedEventArgs e) => InitDeviceIABP ();
        private void ButtonDeviceDefib_Click (object s, RoutedEventArgs e) => InitDeviceDefib ();
        private void ButtonResetParameters_Click (object s, RoutedEventArgs e) => RequestNewPatient ();

        private void ButtonApplyParameters_Click (object sender, RoutedEventArgs e) {
            List<FetalHeartDecelerations.Values> FHRRhythms = new List<FetalHeartDecelerations.Values> ();
            foreach (object o in listFHRRhythms.SelectedItems)
                FHRRhythms.Add ((FetalHeartDecelerations.Values)Enum.GetValues (typeof (FetalHeartDecelerations.Values)).GetValue (listFHRRhythms.Items.IndexOf(o)));

            App.Patient.UpdateParameters (
                (int)numHR.Value,
                (int)numSPO2.Value,
                (int)numRR.Value,
                (int)numETCO2.Value,

                (double)numT.Value,
                (int)numCVP.Value,

                (int)numNSBP.Value,
                (int)numNDBP.Value,
                Patient.CalculateMAP ((int)numNSBP.Value, (int)numNDBP.Value),

                (int)numASBP.Value,
                (int)numADBP.Value,
                Patient.CalculateMAP ((int)numASBP.Value, (int)numADBP.Value),

                (int)numPSP.Value,
                (int)numPDP.Value,
                Patient.CalculateMAP ((int)numPSP.Value, (int)numPDP.Value),

                (int)numICP.Value,
                (int)numIAP.Value,

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

                (CardiacRhythms.Values)Enum.GetValues(typeof(CardiacRhythms.Values)).GetValue(comboCardiacRhythm.SelectedIndex),
                (RespiratoryRhythms.Values)Enum.GetValues (typeof (RespiratoryRhythms.Values)).GetValue (comboRespiratoryRhythm.SelectedIndex),

                (int)numInspiratoryRatio.Value,
                (int)numExpiratoryRatio.Value,

                (int)numFHR.Value,
                (Patient.Intensity.Values)Enum.GetValues(typeof(Patient.Intensity.Values)).GetValue(comboFHRVariability.SelectedIndex),
                FHRRhythms,
                (int)numUCFrequency.Value,
                (int)numUCDuration.Value,
                (Patient.Intensity.Values)Enum.GetValues (typeof (Patient.Intensity.Values)).GetValue (comboUCIntensity.SelectedIndex)
            );
        }

        private void FormUpdateFields (object sender, Patient.PatientEvent_Args e) {
            if (e.EventType == Patient.PatientEvent_Args.EventTypes.Vitals_Change) {
                numHR.Value = e.Patient.HR;
                numSPO2.Value = e.Patient.SPO2;
                numRR.Value = e.Patient.RR;
                numETCO2.Value = e.Patient.ETCO2;
                numT.Value = (decimal)e.Patient.T;
                numCVP.Value = e.Patient.CVP;

                numNSBP.Value = e.Patient.NSBP;
                numNDBP.Value = e.Patient.NDBP;
                numASBP.Value = e.Patient.ASBP;
                numADBP.Value = e.Patient.ADBP;
                numPSP.Value = e.Patient.PSP;
                numPDP.Value = e.Patient.PDP;

                numICP.Value = e.Patient.ICP;
                numIAP.Value = e.Patient.IAP;

                comboCardiacRhythm.SelectedIndex = (int)e.Patient.CardiacRhythm.Value;

                comboRespiratoryRhythm.SelectedIndex = (int)e.Patient.Respiratory_Rhythm.Value;
                numInspiratoryRatio.Value = e.Patient.Respiratory_IERatio_I;
                numExpiratoryRatio.Value = e.Patient.Respiratory_IERatio_E;

                numSTE_I.Value = (decimal)e.Patient.STElevation [(int)Leads.Values.ECG_I];
                numSTE_II.Value = (decimal)e.Patient.STElevation [(int)Leads.Values.ECG_II];
                numSTE_III.Value = (decimal)e.Patient.STElevation [(int)Leads.Values.ECG_III];
                numSTE_aVR.Value = (decimal)e.Patient.STElevation [(int)Leads.Values.ECG_AVR];
                numSTE_aVL.Value = (decimal)e.Patient.STElevation [(int)Leads.Values.ECG_AVL];
                numSTE_aVF.Value = (decimal)e.Patient.STElevation [(int)Leads.Values.ECG_AVF];
                numSTE_V1.Value = (decimal)e.Patient.STElevation [(int)Leads.Values.ECG_V1];
                numSTE_V2.Value = (decimal)e.Patient.STElevation [(int)Leads.Values.ECG_V2];
                numSTE_V3.Value = (decimal)e.Patient.STElevation [(int)Leads.Values.ECG_V3];
                numSTE_V4.Value = (decimal)e.Patient.STElevation [(int)Leads.Values.ECG_V4];
                numSTE_V5.Value = (decimal)e.Patient.STElevation [(int)Leads.Values.ECG_V5];
                numSTE_V6.Value = (decimal)e.Patient.STElevation [(int)Leads.Values.ECG_V6];

                numTWE_I.Value = (decimal)e.Patient.TElevation [(int)Leads.Values.ECG_I];
                numTWE_II.Value = (decimal)e.Patient.TElevation [(int)Leads.Values.ECG_II];
                numTWE_III.Value = (decimal)e.Patient.TElevation [(int)Leads.Values.ECG_III];
                numTWE_aVR.Value = (decimal)e.Patient.TElevation [(int)Leads.Values.ECG_AVR];
                numTWE_aVL.Value = (decimal)e.Patient.TElevation [(int)Leads.Values.ECG_AVL];
                numTWE_aVF.Value = (decimal)e.Patient.TElevation [(int)Leads.Values.ECG_AVF];
                numTWE_V1.Value = (decimal)e.Patient.TElevation [(int)Leads.Values.ECG_V1];
                numTWE_V2.Value = (decimal)e.Patient.TElevation [(int)Leads.Values.ECG_V2];
                numTWE_V3.Value = (decimal)e.Patient.TElevation [(int)Leads.Values.ECG_V3];
                numTWE_V4.Value = (decimal)e.Patient.TElevation [(int)Leads.Values.ECG_V4];
                numTWE_V5.Value = (decimal)e.Patient.TElevation [(int)Leads.Values.ECG_V5];
                numTWE_V6.Value = (decimal)e.Patient.TElevation [(int)Leads.Values.ECG_V6];

                numFHR.Value = e.Patient.FHR;
                numUCFrequency.Value = e.Patient.UCFrequency;
                numUCDuration.Value = e.Patient.UCDuration;
                comboFHRVariability.SelectedIndex = (int)e.Patient.FHRVariability.Value;
                comboUCIntensity.SelectedIndex = (int)e.Patient.UCIntensity.Value;

                listFHRRhythms.SelectedItems.Clear ();
                foreach (FetalHeartDecelerations.Values fhr_rhythm in e.Patient.FHRDecelerations.ValueList)
                    listFHRRhythms.SelectedItems.Add(listFHRRhythms.Items.GetItemAt ((int)fhr_rhythm));
            }
        }

        private void OnRhythmSelected (object sender, SelectionChangedEventArgs e) {
            if ((bool)checkDefaultVitals.IsChecked && App.Patient != null) {

                int si = comboCardiacRhythm.SelectedIndex;
                Array ev = Enum.GetValues (typeof (CardiacRhythms.Values));
                if (si < 0 || si > ev.Length - 1)
                    return;

                CardiacRhythms.Default_Vitals v = CardiacRhythms.DefaultVitals (
                    (CardiacRhythms.Values)ev.GetValue (si));

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

        private void OnClosed (object sender, EventArgs e) => RequestExit ();

    }
}
