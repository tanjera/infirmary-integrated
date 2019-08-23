using II;
using II.Localization;
using II.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

            InitInitialRun ();
            InitInterface ();
            InitScenario ();

            if (App.Start_Args.Length > 0)
                LoadOpen (App.Start_Args [0]);

            App.Mirror.timerUpdate.Tick += delegate { App.Mirror.TimerTick (App.Patient, App.Server); };
            App.Mirror.timerUpdate.ResetAuto (5000);

            /* Debugging and testing code below */
        }

        private void InitInitialRun () {
            string setLang = Properties.Settings.Default.Language;
            if (setLang == null || setLang == ""
                || !Enum.TryParse<Languages.Values> (setLang, out App.Language.Value)) {
                App.Language = new Languages ();
                DialogInitial ();
            }
        }

        private void InitInterface () {
            // Initiate ICommands for KeyBindings
            icLoadFile = new ActionCommand (() => LoadFile ());
            icSaveFile = new ActionCommand (() => SaveFile ());

            // Populate UI strings per language selection
            wdwPatientEditor.Title = App.Language.Dictionary ["PE:WindowTitle"];
            menuFile.Header = App.Language.Dictionary ["PE:MenuFile"];
            menuLoad.Header = App.Language.Dictionary ["PE:MenuLoadSimulation"];
            menuSave.Header = App.Language.Dictionary ["PE:MenuSaveSimulation"];
            menuExit.Header = App.Language.Dictionary ["PE:MenuExitProgram"];

            menuSettings.Header = App.Language.Dictionary ["PE:MenuSettings"];
            menuSetLanguage.Header = App.Language.Dictionary ["PE:MenuSetLanguage"];

            menuHelp.Header = App.Language.Dictionary ["PE:MenuHelp"];
            menuAbout.Header = App.Language.Dictionary ["PE:MenuAboutProgram"];

            lblGroupDevices.Content = App.Language.Dictionary ["PE:Devices"];
            lblDeviceMonitor.Content = App.Language.Dictionary ["PE:CardiacMonitor"];
            lblDevice12LeadECG.Content = App.Language.Dictionary ["PE:12LeadECG"];
            lblDeviceDefibrillator.Content = App.Language.Dictionary ["PE:Defibrillator"];
            lblDeviceIABP.Content = App.Language.Dictionary ["PE:IABP"];
            //lblDeviceVentilator.Content = App.Language.Dictionary["PE:Ventilator"];
            //lblDeviceEFM.Content = App.Language.Dictionary["PE:EFM"];
            //lblDeviceIVPump.Content = App.Language.Dictionary["PE:IVPump"];
            //lblDeviceLabResults.Content = App.Language.Dictionary["PE:LabResults"];

            lblGroupMirroring.Content = App.Language.Dictionary ["PE:MirrorPatientData"];
            lblMirrorStatus.Content = App.Language.Dictionary ["PE:Status"];
            radioInactive.Content = App.Language.Dictionary ["PE:Inactive"];
            radioServer.Content = App.Language.Dictionary ["PE:Server"];
            radioClient.Content = App.Language.Dictionary ["PE:Client"];
            lblAccessionKey.Content = App.Language.Dictionary ["PE:AccessionKey"];
            lblAccessPassword.Content = App.Language.Dictionary ["PE:AccessPassword"];
            lblAdminPassword.Content = App.Language.Dictionary ["PE:AdminPassword"];
            btnApplyMirroring.Content = App.Language.Dictionary ["BUTTON:ApplyChanges"];

            lblGroupScenarioPlayer.Content = App.Language.Dictionary ["PE:ScenarioPlayer"];

            lblGroupVitalSigns.Content = App.Language.Dictionary ["PE:VitalSigns"];
            lblHR.Content = String.Format ("{0}:", App.Language.Dictionary ["PE:HeartRate"]);
            lblNIBP.Content = String.Format ("{0}:", App.Language.Dictionary ["PE:BloodPressure"]);
            lblRR.Content = String.Format ("{0}:", App.Language.Dictionary ["PE:RespiratoryRate"]);
            lblSPO2.Content = String.Format ("{0}:", App.Language.Dictionary ["PE:PulseOximetry"]);
            lblT.Content = String.Format ("{0}:", App.Language.Dictionary ["PE:Temperature"]);
            lblCardiacRhythm.Content = String.Format ("{0}:", App.Language.Dictionary ["PE:CardiacRhythm"]);
            checkDefaultVitals.Content = App.Language.Dictionary ["PE:UseDefaultVitalSignRanges"];

            lblGroupHemodynamics.Content = App.Language.Dictionary ["PE:AdvancedHemodynamics"];
            lblETCO2.Content = String.Format ("{0}:", App.Language.Dictionary ["PE:EndTidalCO2"]);
            lblCVP.Content = String.Format ("{0}:", App.Language.Dictionary ["PE:CentralVenousPressure"]);
            lblASBP.Content = String.Format ("{0}:", App.Language.Dictionary ["PE:ArterialBloodPressure"]);
            lblPSP.Content = String.Format ("{0}:", App.Language.Dictionary ["PE:PulmonaryArteryPressure"]);
            lblICP.Content = String.Format ("{0}:", App.Language.Dictionary ["PE:IntracranialPressure"]);
            lblIAP.Content = String.Format ("{0}:", App.Language.Dictionary ["PE:IntraabdominalPressure"]);

            lblGroupRespiratoryProfile.Content = App.Language.Dictionary ["PE:RespiratoryProfile"];
            lblRespiratoryRhythm.Content = String.Format ("{0}:", App.Language.Dictionary ["PE:RespiratoryRhythm"]);
            lblMechanicallyVentilated.Content = String.Format ("{0}:", App.Language.Dictionary ["PE:MechanicallyVentilated"]);
            lblInspiratoryRatio.Content = String.Format ("{0}:", App.Language.Dictionary ["PE:InspiratoryExpiratoryRatio"]);

            lblGroupCardiacProfile.Content = App.Language.Dictionary ["PE:CardiacProfile"];
            lblPacemakerCaptureThreshold.Content = String.Format ("{0}:", App.Language.Dictionary ["PE:PacemakerCaptureThreshold"]);
            lblPulsusParadoxus.Content = String.Format ("{0}:", App.Language.Dictionary ["PE:PulsusParadoxus"]);
            lblPulsusAlternans.Content = String.Format ("{0}:", App.Language.Dictionary ["PE:PulsusAlternans"]);
            lblCardiacAxis.Content = String.Format ("{0}:", App.Language.Dictionary ["PE:CardiacAxis"]);
            grpSTSegmentElevation.Header = App.Language.Dictionary ["PE:STSegmentElevation"];
            grpTWaveElevation.Header = App.Language.Dictionary ["PE:TWaveElevation"];

            lblGroupObstetricProfile.Content = App.Language.Dictionary ["PE:ObstetricProfile"];
            lblFHR.Content = String.Format ("{0}:", App.Language.Dictionary ["PE:FetalHeartRate"]);
            lblFHRRhythms.Content = String.Format ("{0}:", App.Language.Dictionary ["PE:FetalHeartRhythms"]);
            lblFHRVariability.Content = String.Format ("{0}:", App.Language.Dictionary ["PE:FetalHeartVariability"]);
            lblUCFrequency.Content = String.Format ("{0}:", App.Language.Dictionary ["PE:UterineContractionFrequency"]);
            lblUCDuration.Content = String.Format ("{0}:", App.Language.Dictionary ["PE:UterineContractionDuration"]);
            lblUCIntensity.Content = String.Format ("{0}:", App.Language.Dictionary ["PE:UterineContractionIntensity"]);

            lblParametersApply.Content = App.Language.Dictionary ["BUTTON:ApplyChanges"];
            lblParametersReset.Content = App.Language.Dictionary ["BUTTON:ResetParameters"];

            List<string> cardiacRhythms = new List<string> (),
                cardiacAxes = new List<string> (),
                respiratoryRhythms = new List<string> (),
                intensityScale = new List<string> (),
                fetalHeartRhythms = new List<string> ();

            foreach (Cardiac_Rhythms.Values v in Enum.GetValues (typeof (Cardiac_Rhythms.Values)))
                cardiacRhythms.Add (App.Language.Dictionary [Cardiac_Rhythms.LookupString (v)]);
            comboCardiacRhythm.ItemsSource = cardiacRhythms;

            foreach (CardiacAxes.Values v in Enum.GetValues (typeof (CardiacAxes.Values)))
                cardiacAxes.Add (App.Language.Dictionary [CardiacAxes.LookupString (v)]);
            comboCardiacAxis.ItemsSource = cardiacAxes;

            foreach (Respiratory_Rhythms.Values v in Enum.GetValues (typeof (Respiratory_Rhythms.Values)))
                respiratoryRhythms.Add (App.Language.Dictionary [Respiratory_Rhythms.LookupString (v)]);
            comboRespiratoryRhythm.ItemsSource = respiratoryRhythms;

            foreach (Patient.Intensity.Values v in Enum.GetValues (typeof (Patient.Intensity.Values)))
                intensityScale.Add (App.Language.Dictionary [Patient.Intensity.LookupString (v)]);
            comboFHRVariability.ItemsSource = intensityScale;
            comboUCIntensity.ItemsSource = intensityScale;

            foreach (FetalHeartDecelerations.Values v in Enum.GetValues (typeof (FetalHeartDecelerations.Values)))
                fetalHeartRhythms.Add (App.Language.Dictionary [FetalHeartDecelerations.LookupString (v)]);
            listFHRRhythms.ItemsSource = fetalHeartRhythms;

            // Populate status bar with updated version information and make visible
            BackgroundWorker bgw = new BackgroundWorker ();
            string latestVersion = "";
            bgw.DoWork += delegate { latestVersion = App.Server.Get_LatestVersion (); };
            bgw.RunWorkerCompleted += delegate {
                if (Utility.IsNewerVersion (Utility.Version, latestVersion)) {
                    txtUpdateAvailable.Text = String.Format (App.Language.Dictionary ["STATUS:UpdateAvailable"], latestVersion).Trim ();
                } else {
                    statusUpdateAvailable.Visibility = Visibility.Collapsed;
                }
            };
            bgw.RunWorkerAsync ();
        }

        private void InitScenario () {
            App.Scenario = new Scenario ();
            App.Timer_Main.Tick += App.Scenario.ProcessTimer;
            App.Patient = App.Scenario.Patient;
            InitPatientEvents ();
        }

        private void InitPatientEvents () {
            App.Timer_Main.Tick += App.Patient.ProcessTimers;
            App.Timer_Main.Tick += App.Mirror.ProcessTimer;
            App.Patient.PatientEvent += FormUpdateFields;
            FormUpdateFields (this, new Patient.PatientEventArgs (App.Patient, Patient.PatientEventTypes.Vitals_Change));
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

        private void DialogInitial (bool reloadUI = false) {
            App.Dialog_Language = new DialogInitial ();
            App.Dialog_Language.Activate ();
            App.Dialog_Language.ShowDialog ();

            if (reloadUI)
                InitInterface ();
        }

        private void DialogAbout (bool reloadUI = false) {
            App.Dialog_About = new DialogAbout ();
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

            FormUpdateFields (this, new Patient.PatientEventArgs (App.Patient, Patient.PatientEventTypes.Vitals_Change));
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
                * Line 3 is savefile data encrypted by AES encoding
                */

            string hash = sr.ReadLine ().Trim ();
            string file = Utility.DecryptAES (sr.ReadToEnd ().Trim ());
            sr.Close ();

            if (hash == Utility.HashSHA256 (file))
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
                    } else if (line == "> Begin: Scenario") {
                        pbuffer = new StringBuilder ();
                        while ((pline = sRead.ReadLine ()) != null && pline != "> End: Scenario")
                            pbuffer.AppendLine (pline);
                        App.Scenario.Load_Process (pbuffer.ToString ());
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
                    } else if (line == "> Begin: 12 Lead ECG") {
                        pbuffer = new StringBuilder ();
                        while ((pline = sRead.ReadLine ()) != null && pline != "> End: 12 Lead ECG")
                            pbuffer.AppendLine (pline);

                        InitDeviceECG ();
                        App.Device_ECG.Load_Process (pbuffer.ToString ());
                    } else if (line == "> Begin: Defibrillator") {
                        pbuffer = new StringBuilder ();
                        while ((pline = sRead.ReadLine ()) != null && pline != "> End: Defibrillator")
                            pbuffer.AppendLine (pline);

                        InitDeviceDefib ();
                        App.Device_Defib.Load_Process (pbuffer.ToString ());
                    } else if (line == "> Begin: Intra-aortic Balloon Pump") {
                        pbuffer = new StringBuilder ();
                        while ((pline = sRead.ReadLine ()) != null && pline != "> End: Intra-aortic Balloon Pump")
                            pbuffer.AppendLine (pline);

                        InitDeviceIABP ();
                        App.Device_IABP.Load_Process (pbuffer.ToString ());
                    }
                }
            } catch (Exception e) {
                App.Server.Post_Exception (e);
                LoadFail ();
            } finally {
                sRead.Close ();
            }
        }

        private void LoadFail () {
            MessageBox.Show (
                    "The selected file was unable to be loaded. Perhaps the file was damaged or edited outside of Infirmary Integrated.",
                    "Unable to Load File", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void SaveT1 (Stream s) {
            StringBuilder sb = new StringBuilder ();

            sb.AppendLine ("> Begin: Scenario");
            sb.Append (App.Scenario.Save ());
            sb.AppendLine ("> End: Scenario");

            sb.AppendLine ("> Begin: Editor");
            sb.Append (this.SaveOptions ());
            sb.AppendLine ("> End: Editor");

            if (App.Device_Monitor != null && App.Device_Monitor.IsLoaded) {
                sb.AppendLine ("> Begin: Cardiac Monitor");
                sb.Append (App.Device_Monitor.Save ());
                sb.AppendLine ("> End: Cardiac Monitor");
            }
            if (App.Device_ECG != null && App.Device_ECG.IsLoaded) {
                sb.AppendLine ("> Begin: 12 Lead ECG");
                sb.Append (App.Device_ECG.Save ());
                sb.AppendLine ("> End: 12 Lead ECG");
            }
            if (App.Device_Defib != null && App.Device_Defib.IsLoaded) {
                sb.AppendLine ("> Begin: Defibrillator");
                sb.Append (App.Device_Defib.Save ());
                sb.AppendLine ("> End: Defibrillator");
            }
            if (App.Device_IABP != null && App.Device_IABP.IsLoaded) {
                sb.AppendLine ("> Begin: Intra-aortic Balloon Pump");
                sb.Append (App.Device_IABP.Save ());
                sb.AppendLine ("> End: Intra-aortic Balloon Pump");
            }

            StreamWriter sw = new StreamWriter (s);
            sw.WriteLine (".ii:t1");                                      // Metadata (type 1 savefile)
            sw.WriteLine (Utility.HashSHA256 (sb.ToString ().Trim ()));      // Hash for validation
            sw.Write (Utility.EncryptAES (sb.ToString ().Trim ()));       // Savefile data encrypted with AES
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
            } catch (Exception e) {
                App.Server.Post_Exception (e);
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
            App.Scenario.Patient = new Patient ();
            InitPatientEvents ();
            return App.Patient;
        }

        private void MenuLoadFile_Click (object s, RoutedEventArgs e) => LoadFile ();

        private void MenuSaveFile_Click (object s, RoutedEventArgs e) => SaveFile ();

        private void MenuExit_Click (object s, RoutedEventArgs e) => RequestExit ();

        private void MenuSetLanguage_Click (object s, RoutedEventArgs e) => DialogInitial (true);

        private void MenuAbout_Click (object s, RoutedEventArgs e) => DialogAbout ();

        private void ButtonDeviceMonitor_Click (object s, RoutedEventArgs e) => InitDeviceMonitor ();

        private void ButtonDeviceECG_Click (object s, RoutedEventArgs e) => InitDeviceECG ();

        private void ButtonDeviceIABP_Click (object s, RoutedEventArgs e) => InitDeviceIABP ();

        private void ButtonDeviceDefib_Click (object s, RoutedEventArgs e) => InitDeviceDefib ();

        private void ButtonGenerateAccessionKey_Click (object sender, RoutedEventArgs e)
            => txtAccessionKey.Text = Utility.RandomString (8);

        private void ButtonApplyMirroring_Click (object s, RoutedEventArgs e) {
            App.Mirror.PatientUpdated = new DateTime ();
            App.Mirror.ServerQueried = new DateTime ();

            if (radioInactive.IsChecked ?? true) {
                App.Mirror.Status = Mirrors.Statuses.INACTIVE;
                lblStatusText.Content = App.Language.Dictionary ["PE:StatusMirroringDisabled"];
            } else if (radioClient.IsChecked ?? true) {
                App.Mirror.Status = Mirrors.Statuses.CLIENT;
                App.Mirror.Accession = txtAccessionKey.Text;
                App.Mirror.PasswordAccess = txtAccessPassword.Password;
                lblStatusText.Content = App.Language.Dictionary ["PE:StatusMirroringActivated"];
            } else if (radioServer.IsChecked ?? true) {
                if (txtAccessionKey.Text == "")
                    txtAccessionKey.Text = Utility.RandomString (8);

                App.Mirror.Status = Mirrors.Statuses.HOST;
                App.Mirror.Accession = txtAccessionKey.Text;
                App.Mirror.PasswordAccess = txtAccessPassword.Password;
                App.Mirror.PasswordEdit = txtAdminPassword.Password;
                lblStatusText.Content = App.Language.Dictionary ["PE:StatusMirroringActivated"];
            }
        }

        private void ButtonPreviousStage_Click (object s, RoutedEventArgs e)
            => App.Patient = App.Scenario.LastStage ();

        private void ButtonNextStage_Click (object s, RoutedEventArgs e)
            => App.Patient = App.Scenario.NextStage ();

        private void ButtonPauseStage_Click (object s, RoutedEventArgs e)
            => App.Scenario.PauseStage ();

        private void ButtonPlayStage_Click (object s, RoutedEventArgs e)
            => App.Scenario.PlayStage ();

        private void ButtonResetParameters_Click (object s, RoutedEventArgs e) {
            RequestNewPatient ();
            lblStatusText.Content = App.Language.Dictionary ["PE:StatusPatientReset"];
        }

        private void ButtonApplyParameters_Click (object sender, RoutedEventArgs e) {
            ButtonApplyMirroring_Click (sender, e);

            List<FetalHeartDecelerations.Values> FHRRhythms = new List<FetalHeartDecelerations.Values> ();
            foreach (object o in listFHRRhythms.SelectedItems)
                FHRRhythms.Add ((FetalHeartDecelerations.Values)Enum.GetValues (typeof (FetalHeartDecelerations.Values)).GetValue (listFHRRhythms.Items.IndexOf (o)));

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

                (int)numPacemakerCaptureThreshold.Value,
                chkPulsusParadoxus.IsChecked ?? false,
                chkPulsusAlternans.IsChecked ?? false,

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

                (Cardiac_Rhythms.Values)Enum.GetValues (typeof (Cardiac_Rhythms.Values)).GetValue (
                    comboCardiacRhythm.SelectedIndex < 0 ? 0 : comboCardiacRhythm.SelectedIndex),
                (CardiacAxes.Values)Enum.GetValues (typeof (CardiacAxes.Values)).GetValue (
                    comboCardiacAxis.SelectedIndex < 0 ? 0 : comboCardiacAxis.SelectedIndex),
                (Respiratory_Rhythms.Values)Enum.GetValues (typeof (Respiratory_Rhythms.Values)).GetValue (
                    comboRespiratoryRhythm.SelectedIndex < 0 ? 0 : comboRespiratoryRhythm.SelectedIndex),

                (float)numInspiratoryRatio.Value,
                (float)numExpiratoryRatio.Value,

                chkMechanicallyVentilated.IsChecked ?? false,

                (int)numFHR.Value,
                (Patient.Intensity.Values)Enum.GetValues (typeof (Patient.Intensity.Values)).GetValue (
                    comboFHRVariability.SelectedIndex < 0 ? 0 : comboFHRVariability.SelectedIndex),
                FHRRhythms,
                (int)numUCFrequency.Value,
                (int)numUCDuration.Value,
                (Patient.Intensity.Values)Enum.GetValues (typeof (Patient.Intensity.Values)).GetValue (
                    comboUCIntensity.SelectedIndex < 0 ? 0 : comboUCIntensity.SelectedIndex)
            );

            App.Mirror.PostPatient (App.Patient, App.Server);
            txtAccessionKey.Text = App.Mirror.Accession;

            if (App.Mirror.Status == Mirrors.Statuses.INACTIVE)
                lblStatusText.Content = App.Language.Dictionary ["PE:StatusPatientUpdated"];
            else if (App.Mirror.Status == Mirrors.Statuses.HOST)
                lblStatusText.Content = App.Language.Dictionary ["PE:StatusMirroredPatientUpdated"];
        }

        private void TextBoxAccessionKey_PreviewTextInput (object sender, TextCompositionEventArgs e) {
            Regex regex = new Regex ("^[a-zA-Z0-9]*$");
            e.Handled = !regex.IsMatch (e.Text);
        }

        private void RadioMirrorSelected_Click (object sender, RoutedEventArgs e) {
            if (txtAccessionKey == null || txtAccessPassword == null || txtAdminPassword == null)
                return;

            if ((sender as RadioButton).Name == "radioInactive") {
                txtAccessionKey.IsEnabled = false;
                btnGenerateAccessionKey.IsEnabled = false;
                txtAccessPassword.IsEnabled = false;
                txtAdminPassword.IsEnabled = false;
            } else if ((sender as RadioButton).Name == "radioClient") {
                txtAccessionKey.IsEnabled = true;
                btnGenerateAccessionKey.IsEnabled = false;
                txtAccessPassword.IsEnabled = true;
                txtAdminPassword.IsEnabled = false;
            } else if ((sender as RadioButton).Name == "radioServer") {
                txtAccessionKey.IsEnabled = true;
                btnGenerateAccessionKey.IsEnabled = true;
                txtAccessPassword.IsEnabled = true;
                txtAdminPassword.IsEnabled = true;
            }
        }

        private void Hyperlink_RequestNavigate (object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
            => System.Diagnostics.Process.Start (e.Uri.ToString ());

        private void FormUpdateFields (object sender, Patient.PatientEventArgs e) {
            if (e.EventType == Patient.PatientEventTypes.Vitals_Change) {
                numHR.Value = e.Patient.VS_Settings.HR;
                numSPO2.Value = e.Patient.VS_Settings.SPO2;
                numRR.Value = e.Patient.VS_Settings.RR;
                numETCO2.Value = e.Patient.VS_Settings.ETCO2;
                numT.Value = (decimal)e.Patient.VS_Settings.T;
                numCVP.Value = e.Patient.VS_Settings.CVP;

                numNSBP.Value = e.Patient.VS_Settings.NSBP;
                numNDBP.Value = e.Patient.VS_Settings.NDBP;
                numASBP.Value = e.Patient.VS_Settings.ASBP;
                numADBP.Value = e.Patient.VS_Settings.ADBP;
                numPSP.Value = e.Patient.VS_Settings.PSP;
                numPDP.Value = e.Patient.VS_Settings.PDP;

                numICP.Value = e.Patient.VS_Settings.ICP;
                numIAP.Value = e.Patient.VS_Settings.IAP;

                comboCardiacRhythm.SelectedIndex = (int)e.Patient.Cardiac_Rhythm.Value;
                comboCardiacAxis.SelectedIndex = (int)e.Patient.Cardiac_Axis.Value;

                numInspiratoryRatio.Value = (decimal)e.Patient.VS_Settings.RR_IE_I;
                numExpiratoryRatio.Value = (decimal)e.Patient.VS_Settings.RR_IE_E;
                comboRespiratoryRhythm.SelectedIndex = (int)e.Patient.Respiratory_Rhythm.Value;

                numPacemakerCaptureThreshold.Value = e.Patient.Pacemaker_Threshold;

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

                numFHR.Value = e.Patient.FHR;
                numUCFrequency.Value = e.Patient.UC_Frequency;
                numUCDuration.Value = e.Patient.UC_Duration;
                comboFHRVariability.SelectedIndex = (int)e.Patient.FHR_Variability.Value;
                comboUCIntensity.SelectedIndex = (int)e.Patient.UC_Intensity.Value;

                listFHRRhythms.SelectedItems.Clear ();
                foreach (FetalHeartDecelerations.Values fhr_rhythm in e.Patient.FHR_Decelerations.ValueList)
                    listFHRRhythms.SelectedItems.Add (listFHRRhythms.Items.GetItemAt ((int)fhr_rhythm));
            }
        }

        private void OnCardiacRhythmSelected (object sender, SelectionChangedEventArgs e) {
            if (!(bool)checkDefaultVitals.IsChecked || App.Patient == null)
                return;

            int si = comboCardiacRhythm.SelectedIndex;
            Array ev = Enum.GetValues (typeof (Cardiac_Rhythms.Values));
            if (si < 0 || si > ev.Length - 1)
                return;

            Cardiac_Rhythms.Default_Vitals v = Cardiac_Rhythms.DefaultVitals (
                (Cardiac_Rhythms.Values)ev.GetValue (si));

            numHR.Value = (int)Utility.Clamp ((double)numHR.Value, v.HRMin, v.HRMax);
            numRR.Value = (int)Utility.Clamp ((double)numRR.Value, v.RRMin, v.RRMax);
            numSPO2.Value = (int)Utility.Clamp ((double)numSPO2.Value, v.SPO2Min, v.SPO2Max);
            numETCO2.Value = (int)Utility.Clamp ((double)numETCO2.Value, v.ETCO2Min, v.ETCO2Max);
            numNSBP.Value = (int)Utility.Clamp ((double)numNSBP.Value, v.SBPMin, v.SBPMax);
            numNDBP.Value = (int)Utility.Clamp ((double)numNDBP.Value, v.DBPMin, v.DBPMax);
            numASBP.Value = (int)Utility.Clamp ((double)numASBP.Value, v.SBPMin, v.SBPMax);
            numADBP.Value = (int)Utility.Clamp ((double)numADBP.Value, v.DBPMin, v.DBPMax);
            numPSP.Value = (int)Utility.Clamp ((double)numPSP.Value, v.PSPMin, v.PSPMax);
            numPDP.Value = (int)Utility.Clamp ((double)numPDP.Value, v.PDPMin, v.PDPMax);
        }

        private void OnRespiratoryRhythmSelected (object sender, SelectionChangedEventArgs e) {
            if (!(bool)checkDefaultVitals.IsChecked || App.Patient == null)
                return;

            int si = comboRespiratoryRhythm.SelectedIndex;
            Array ev = Enum.GetValues (typeof (Respiratory_Rhythms.Values));
            if (si < 0 || si > ev.Length - 1)
                return;

            Respiratory_Rhythms.Default_Vitals v = Respiratory_Rhythms.DefaultVitals (
                (Respiratory_Rhythms.Values)ev.GetValue (si));

            numRR.Value = (int)Utility.Clamp ((double)numRR.Value, v.RRMin, v.RRMax);
            numInspiratoryRatio.Value = (int)Utility.Clamp ((double)numInspiratoryRatio.Value, v.RR_IE_I_Min, v.RR_IE_I_Max);
            numExpiratoryRatio.Value = (int)Utility.Clamp ((double)numExpiratoryRatio.Value, v.RR_IE_E_Min, v.RR_IE_E_Max);
        }

        private void OnClosed (object sender, EventArgs e) => RequestExit ();
    }
}