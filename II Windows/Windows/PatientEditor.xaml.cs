using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using II;
using II.Localization;
using II.Server;

using Xceed.Wpf.Toolkit;

namespace II_Windows {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class PatientEditor : Window {
        /* Define WPF UI commands for binding */
        private ICommand icNewFile, icLoadFile, icSaveFile;

        public ICommand IC_NewFile { get { return icNewFile; } }
        public ICommand IC_LoadFile { get { return icLoadFile; } }
        public ICommand IC_SaveFile { get { return icSaveFile; } }

        /* Variables for WPF UI loading */
        private bool uiLoadCompleted = false;

        /* Variables for Auto-Apply functionality */
        private object uiBufferValue;
        private ParameterStatuses ParameterStatus = ParameterStatuses.Loading;

        private enum ParameterStatuses {
            Loading,
            AutoApply,
            ChangesPending,
            ChangesApplied
        }

        public PatientEditor () {
            InitializeComponent ();
            DataContext = this;
            App.Patient_Editor = this;

            InitInitialRun ();
            InitUsageStatistics ();
            InitInterface ();
            InitUpgrade ();
            InitMirroring ();
            InitScenario (true);

            if (App.Start_Args.Length > 0)
                LoadOpen (App.Start_Args [0]);

            SetParameterStatus (Properties.Settings.Default.AutoApplyChanges);

            /* Debugging and testing code below */
        }

        private void InitInitialRun () {
            string setLang = Properties.Settings.Default.Language;
            if (setLang == null || setLang == ""
                || !Enum.TryParse<Language.Values> (setLang, out App.Language.Value)) {
                App.Language = new Language ();
                DialogInitial ();
            }
        }

        private void InitUsageStatistics () {
            /* Send usage statistics to server in background */
            _ = Task.Run (() => App.Server.Post_UsageStatistics (App.Language));
        }

        private void InitInterface () {
            /* Initiate ICommands for KeyBindings */
            icNewFile = new ActionCommand (() => RefreshScenario (true));
            icLoadFile = new ActionCommand (() => LoadFile ());
            icSaveFile = new ActionCommand (() => SaveFile ());

            /* Populate UI strings per language selection */
            wdwPatientEditor.Title = App.Language.Localize ("PE:WindowTitle");
            menuNew.Header = App.Language.Localize ("PE:MenuNewFile");
            menuFile.Header = App.Language.Localize ("PE:MenuFile");
            menuLoad.Header = App.Language.Localize ("PE:MenuLoadSimulation");
            menuSave.Header = App.Language.Localize ("PE:MenuSaveSimulation");
            menuExit.Header = App.Language.Localize ("PE:MenuExitProgram");

            menuSettings.Header = App.Language.Localize ("PE:MenuSettings");
            menuSetLanguage.Header = App.Language.Localize ("PE:MenuSetLanguage");

            menuHelp.Header = App.Language.Localize ("PE:MenuHelp");
            menuAbout.Header = App.Language.Localize ("PE:MenuAboutProgram");

            lblGroupDevices.Content = App.Language.Localize ("PE:Devices");
            lblDeviceMonitor.Content = App.Language.Localize ("PE:CardiacMonitor");
            lblDevice12LeadECG.Content = App.Language.Localize ("PE:12LeadECG");
            lblDeviceDefibrillator.Content = App.Language.Localize ("PE:Defibrillator");
            lblDeviceIABP.Content = App.Language.Localize ("PE:IABP");

            //lblDeviceVentilator.Content = App.Language.Dictionary["PE:Ventilator"];
            //lblDeviceEFM.Content = App.Language.Dictionary["PE:EFM"];
            //lblDeviceIVPump.Content = App.Language.Dictionary["PE:IVPump"];
            //lblDeviceLabResults.Content = App.Language.Dictionary["PE:LabResults"];

            lblGroupMirroring.Content = App.Language.Localize ("PE:MirrorPatientData");
            lblMirrorStatus.Content = App.Language.Localize ("PE:Status");
            radioInactive.Content = App.Language.Localize ("PE:Inactive");
            radioServer.Content = App.Language.Localize ("PE:Server");
            radioClient.Content = App.Language.Localize ("PE:Client");
            lblAccessionKey.Content = App.Language.Localize ("PE:AccessionKey");
            lblAccessPassword.Content = App.Language.Localize ("PE:AccessPassword");
            lblAdminPassword.Content = App.Language.Localize ("PE:AdminPassword");
            btnApplyMirroring.Content = App.Language.Localize ("BUTTON:ApplyChanges");

            lblGroupScenarioPlayer.Content = App.Language.Localize ("PE:ScenarioPlayer");
            lblProgressionOptions.Content = App.Language.Localize ("PE:ProgressionOptions");

            lblGroupVitalSigns.Content = App.Language.Localize ("PE:VitalSigns");
            lblHR.Content = String.Format ("{0}:", App.Language.Localize ("PE:HeartRate"));
            lblNIBP.Content = String.Format ("{0}:", App.Language.Localize ("PE:BloodPressure"));
            lblRR.Content = String.Format ("{0}:", App.Language.Localize ("PE:RespiratoryRate"));
            lblSPO2.Content = String.Format ("{0}:", App.Language.Localize ("PE:PulseOximetry"));
            lblT.Content = String.Format ("{0}:", App.Language.Localize ("PE:Temperature"));
            lblCardiacRhythm.Content = String.Format ("{0}:", App.Language.Localize ("PE:CardiacRhythm"));
            checkDefaultVitals.Content = App.Language.Localize ("PE:UseDefaultVitalSignRanges");

            lblGroupHemodynamics.Content = App.Language.Localize ("PE:AdvancedHemodynamics");
            lblETCO2.Content = String.Format ("{0}:", App.Language.Localize ("PE:EndTidalCO2"));
            lblCVP.Content = String.Format ("{0}:", App.Language.Localize ("PE:CentralVenousPressure"));
            lblASBP.Content = String.Format ("{0}:", App.Language.Localize ("PE:ArterialBloodPressure"));
            lblPACatheterPlacement.Content = String.Format ("{0}:", App.Language.Localize ("PE:PulmonaryArteryCatheterPlacement"));
            lblPSP.Content = String.Format ("{0}:", App.Language.Localize ("PE:PulmonaryArteryPressure"));
            lblICP.Content = String.Format ("{0}:", App.Language.Localize ("PE:IntracranialPressure"));
            lblIAP.Content = String.Format ("{0}:", App.Language.Localize ("PE:IntraabdominalPressure"));

            lblGroupRespiratoryProfile.Content = App.Language.Localize ("PE:RespiratoryProfile");
            lblRespiratoryRhythm.Content = String.Format ("{0}:", App.Language.Localize ("PE:RespiratoryRhythm"));
            lblMechanicallyVentilated.Content = String.Format ("{0}:", App.Language.Localize ("PE:MechanicallyVentilated"));
            lblInspiratoryRatio.Content = String.Format ("{0}:", App.Language.Localize ("PE:InspiratoryExpiratoryRatio"));

            lblGroupCardiacProfile.Content = App.Language.Localize ("PE:CardiacProfile");
            lblPacemakerCaptureThreshold.Content = String.Format ("{0}:", App.Language.Localize ("PE:PacemakerCaptureThreshold"));
            lblPulsusParadoxus.Content = String.Format ("{0}:", App.Language.Localize ("PE:PulsusParadoxus"));
            lblPulsusAlternans.Content = String.Format ("{0}:", App.Language.Localize ("PE:PulsusAlternans"));
            lblCardiacAxis.Content = String.Format ("{0}:", App.Language.Localize ("PE:CardiacAxis"));
            grpSTSegmentElevation.Header = App.Language.Localize ("PE:STSegmentElevation");
            grpTWaveElevation.Header = App.Language.Localize ("PE:TWaveElevation");

            lblGroupObstetricProfile.Content = App.Language.Localize ("PE:ObstetricProfile");
            lblFHR.Content = String.Format ("{0}:", App.Language.Localize ("PE:FetalHeartRate"));
            lblFHRRhythms.Content = String.Format ("{0}:", App.Language.Localize ("PE:FetalHeartRhythms"));
            lblFHRVariability.Content = String.Format ("{0}:", App.Language.Localize ("PE:FetalHeartVariability"));
            lblUCFrequency.Content = String.Format ("{0}:", App.Language.Localize ("PE:UterineContractionFrequency"));
            lblUCDuration.Content = String.Format ("{0}:", App.Language.Localize ("PE:UterineContractionDuration"));
            lblUCIntensity.Content = String.Format ("{0}:", App.Language.Localize ("PE:UterineContractionIntensity"));

            chkAutoApplyChanges.Content = App.Language.Localize ("BUTTON:AutoApplyChanges");
            lblParametersApply.Content = App.Language.Localize ("BUTTON:ApplyChanges");
            lblParametersReset.Content = App.Language.Localize ("BUTTON:ResetParameters");

            chkAutoApplyChanges.IsChecked = Properties.Settings.Default.AutoApplyChanges;

            List<string> cardiacRhythms = new List<string> (),
                respiratoryRhythms = new List<string> (),
                pulmonaryArteryRhythms = new List<string> (),
                cardiacAxes = new List<string> (),
                intensityScale = new List<string> (),
                fetalHeartRhythms = new List<string> ();

            foreach (Cardiac_Rhythms.Values v in Enum.GetValues (typeof (Cardiac_Rhythms.Values)))
                cardiacRhythms.Add (App.Language.Localize (Cardiac_Rhythms.LookupString (v)));
            comboCardiacRhythm.ItemsSource = cardiacRhythms;

            foreach (Respiratory_Rhythms.Values v in Enum.GetValues (typeof (Respiratory_Rhythms.Values)))
                respiratoryRhythms.Add (App.Language.Localize (Respiratory_Rhythms.LookupString (v)));
            comboRespiratoryRhythm.ItemsSource = respiratoryRhythms;

            foreach (PulmonaryArtery_Rhythms.Values v in Enum.GetValues (typeof (PulmonaryArtery_Rhythms.Values)))
                pulmonaryArteryRhythms.Add (App.Language.Localize (PulmonaryArtery_Rhythms.LookupString (v)));
            comboPACatheterPlacement.ItemsSource = pulmonaryArteryRhythms;

            foreach (Cardiac_Axes.Values v in Enum.GetValues (typeof (Cardiac_Axes.Values)))
                cardiacAxes.Add (App.Language.Localize (Cardiac_Axes.LookupString (v)));
            comboCardiacAxis.ItemsSource = cardiacAxes;

            foreach (Scales.Intensity.Values v in Enum.GetValues (typeof (Scales.Intensity.Values)))
                intensityScale.Add (App.Language.Localize (Scales.Intensity.LookupString (v)));
            comboFHRVariability.ItemsSource = intensityScale;
            comboUCIntensity.ItemsSource = intensityScale;

            foreach (FetalHeartDecelerations.Values v in Enum.GetValues (typeof (FetalHeartDecelerations.Values)))
                fetalHeartRhythms.Add (App.Language.Localize (FetalHeartDecelerations.LookupString (v)));
            listFHRRhythms.ItemsSource = fetalHeartRhythms;
        }

        private async void InitUpgrade () {
            /* Newer version available? Check Server, populate status bar, prompt user for upgrade */

            await Task.Run (() => App.Server.Get_LatestVersion_Windows ());

            if (Utility.IsNewerVersion (Utility.Version, App.Server.UpgradeVersion)) {
                txtUpdateAvailable.Text = String.Format (App.Language.Localize ("STATUS:UpdateAvailable"), App.Server.UpgradeVersion).Trim ();
            } else {            // If no update available, no status update
                statusUpdateAvailable.Visibility = Visibility.Collapsed;
                return;
            }

            if (Properties.Settings.Default.MuteUpgrade) {
                if (Utility.IsNewerVersion (Properties.Settings.Default.MuteUpgradeVersion, App.Server.UpgradeVersion)) {
                    Properties.Settings.Default.MuteUpgrade = false;
                    Properties.Settings.Default.Save ();
                } else {        // Mutes update popup notification
                    return;
                }
            }

            DialogUpgrade ();
        }

        private void InitMirroring () {
            App.Timer_Main.Tick += App.Mirror.ProcessTimer;
            App.Mirror.timerUpdate.Tick += OnMirrorTick;
            App.Mirror.timerUpdate.ResetAuto (5000);
        }

        private void InitScenario (bool toInit) {
            App.Scenario = new Scenario (toInit);
            App.Scenario.StepChangeRequest += OnStepChangeRequest;    // Allows unlinking of Timers immediately prior to Step change
            App.Scenario.StepChanged += OnStepChanged;                  // Updates App.Patient, allows PatientEditor UI to update
            App.Timer_Main.Tick += App.Scenario.ProcessTimer;

            if (toInit)         // If toInit is false, Patient is null- InitPatient() will need to be called manually
                InitPatient ();
        }

        private void UnloadScenario () {
            App.Timer_Main.Tick -= App.Scenario.ProcessTimer;   // Unlink Scenario from App/Main Timer
            App.Scenario.Dispose ();        // Disposes Scenario's events and timer, and all Patients' events and timers
        }

        private void RefreshScenario (bool toInit) {
            UnloadScenario ();
            InitScenario (toInit);
        }

        private void InitPatient () {
            App.Patient = App.Scenario.Patient;

            InitPatientEvents ();
            InitStep ();
        }

        private void RefreshPatient () {
            UnloadPatientEvents ();

            App.Patient.Dispose ();
            App.Patient = new Patient ();

            InitPatient ();
        }

        private void InitPatientEvents () {
            /* Tie the Patient's Timer to the Main Timer */
            App.Timer_Main.Tick += App.Patient.ProcessTimers;

            /* Tie PatientEvents to the PatientEditor UI! And trigger. */
            App.Patient.PatientEvent += FormUpdateFields;
            FormUpdateFields (this, new Patient.PatientEventArgs (App.Patient, Patient.PatientEventTypes.Vitals_Change));

            /* Tie PatientEvents to Devices! */
            if (App.Device_Monitor != null && App.Device_Monitor.IsLoaded)
                App.Patient.PatientEvent += App.Device_Monitor.OnPatientEvent;
            if (App.Device_ECG != null && App.Device_ECG.IsLoaded)
                App.Patient.PatientEvent += App.Device_ECG.OnPatientEvent;
            if (App.Device_Defib != null && App.Device_Defib.IsLoaded)
                App.Patient.PatientEvent += App.Device_Defib.OnPatientEvent;
            if (App.Device_IABP != null && App.Device_IABP.IsLoaded)
                App.Patient.PatientEvent += App.Device_IABP.OnPatientEvent;
        }

        private void UnloadPatientEvents () {
            /* Unloading the Patient from the Main Timer also stops all the Patient's Timers
            /* and results in that Patient not triggering PatientEvent's */
            App.Timer_Main.Tick -= App.Patient.ProcessTimers;

            /* But it's still important to clear PatientEvent subscriptions so they're not adding
            /* as duplicates when InitPatientEvents() is called!! */
            App.Patient.UnsubscribePatientEvent ();
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

        private void DialogAbout () {
            App.Dialog_About = new DialogAbout ();
            App.Dialog_About.Activate ();
            App.Dialog_About.ShowDialog ();
        }

        private async void DialogUpgrade () {
            Bootstrap.UpgradeRoute decision = Bootstrap.UpgradeRoute.NULL;

            App.Dialog_Upgrade = new DialogUpgrade ();
            App.Dialog_Upgrade.Activate ();

            App.Dialog_Upgrade.OnUpgradeRoute += (s, ea) => decision = ea.Route;
            App.Dialog_Upgrade.ShowDialog ();
            App.Dialog_Upgrade.OnUpgradeRoute -= (s, ea) => decision = ea.Route;

            switch (decision) {
                default:
                case Bootstrap.UpgradeRoute.NULL:
                case Bootstrap.UpgradeRoute.DELAY:
                    return;

                case Bootstrap.UpgradeRoute.MUTE:
                    Properties.Settings.Default.MuteUpgrade = true;
                    Properties.Settings.Default.MuteUpgradeVersion = App.Server.UpgradeVersion;
                    Properties.Settings.Default.Save ();
                    return;

                case Bootstrap.UpgradeRoute.WEBSITE:
                    if (!String.IsNullOrEmpty (App.Server.UpgradeWebpage))
                        System.Diagnostics.Process.Start (App.Server.UpgradeWebpage);
                    return;

                case Bootstrap.UpgradeRoute.INSTALL:
                    if (!String.IsNullOrEmpty (App.Server.BootstrapExeUri) && !String.IsNullOrEmpty (App.Server.BootstrapHashMd5)) {
                        _ = Task.Run (() => System.Windows.MessageBox.Show (
                            App.Language.Localize ("UPGRADE:Downloading"),
                            "", MessageBoxButton.OK, MessageBoxImage.Information));

                        await Bootstrap.BootstrapInstall_Windows (App.Server);
                        this.Close ();
                    }
                    return;
            }
        }

        private void SetParameterStatus (bool autoApplyChanges) {
            ParameterStatus = autoApplyChanges
               ? ParameterStatuses.AutoApply
               : ParameterStatuses.ChangesApplied;

            UpdateParameterIndicators ();
        }

        private void AdvanceParameterStatus (ParameterStatuses status) {
            /* Toggles between pending changes or changes applied; bypasses if auto-applying or null */
            if (status == ParameterStatuses.ChangesApplied && ParameterStatus == ParameterStatuses.ChangesPending)
                ParameterStatus = ParameterStatuses.ChangesApplied;
            else if (status == ParameterStatuses.ChangesPending && ParameterStatus == ParameterStatuses.ChangesApplied)
                ParameterStatus = ParameterStatuses.ChangesPending;

            UpdateParameterIndicators ();
        }

        private void UpdateParameterIndicators () {
            switch (ParameterStatus) {
                default:
                case ParameterStatuses.Loading:
                    brdPendingChangesIndicator.BorderBrush = Brushes.Transparent;
                    break;

                case ParameterStatuses.ChangesPending:
                    brdPendingChangesIndicator.BorderBrush = Brushes.Red;
                    lblStatusText.Content = App.Language.Localize ("PE:StatusPatientChangesPending");
                    break;

                case ParameterStatuses.ChangesApplied:
                    brdPendingChangesIndicator.BorderBrush = Brushes.Green;

                    if (App.Mirror.Status == Mirror.Statuses.INACTIVE)
                        lblStatusText.Content = App.Language.Localize ("PE:StatusPatientUpdated");
                    else if (App.Mirror.Status == Mirror.Statuses.HOST)
                        lblStatusText.Content = App.Language.Localize ("PE:StatusMirroredPatientUpdated");
                    break;

                case ParameterStatuses.AutoApply:
                    brdPendingChangesIndicator.BorderBrush = Brushes.Orange;
                    break;
            }
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

        private void LoadOpen (string fileName) {
            if (System.IO.File.Exists (fileName)) {
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
            /* Savefile type 1: validated and encrypted
                * Line 1 is metadata (.ii:t1)
                * Line 2 is hash for validation (hash taken of raw string data, unobfuscated)
                * Line 3 is savefile data encrypted by AES encoding
                */

            string hash = sr.ReadLine ().Trim ();
            string file = Encryption.DecryptAES (sr.ReadToEnd ().Trim ());
            sr.Close ();

            if (hash == Encryption.HashSHA256 (file))
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
                    if (line == "> Begin: Patient") {           // Load files saved by Infirmary Integrated (base)
                        pbuffer = new StringBuilder ();
                        while ((pline = sRead.ReadLine ()) != null && pline != "> End: Patient")
                            pbuffer.AppendLine (pline);

                        RefreshScenario (true);
                        App.Patient.Load_Process (pbuffer.ToString ());
                    } else if (line == "> Begin: Scenario") {   // Load files saved by Infirmary Integrated Scenario Editor
                        pbuffer = new StringBuilder ();
                        while ((pline = sRead.ReadLine ()) != null && pline != "> End: Scenario")
                            pbuffer.AppendLine (pline);

                        RefreshScenario (false);
                        App.Scenario.Load_Process (pbuffer.ToString ());
                        InitPatient ();     // Needs to be called manually since InitScenario(false) doesn't init a Patient
                    } else if (line == "> Begin: Editor") {
                        pbuffer = new StringBuilder ();
                        while ((pline = sRead.ReadLine ()) != null && pline != "> End: Editor")
                            pbuffer.AppendLine (pline);

                        this.LoadOptions (pbuffer.ToString ());
                    } else if (line == "> Begin: Cardiac Monitor") {
                        pbuffer = new StringBuilder ();
                        while ((pline = sRead.ReadLine ()) != null && pline != "> End: Cardiac Monitor")
                            pbuffer.AppendLine (pline);

                        App.Device_Monitor = new DeviceMonitor ();
                        InitDeviceMonitor ();
                        App.Device_Monitor.Load_Process (pbuffer.ToString ());
                    } else if (line == "> Begin: 12 Lead ECG") {
                        pbuffer = new StringBuilder ();
                        while ((pline = sRead.ReadLine ()) != null && pline != "> End: 12 Lead ECG")
                            pbuffer.AppendLine (pline);

                        App.Device_ECG = new DeviceECG ();
                        InitDeviceECG ();
                        App.Device_ECG.Load_Process (pbuffer.ToString ());
                    } else if (line == "> Begin: Defibrillator") {
                        pbuffer = new StringBuilder ();
                        while ((pline = sRead.ReadLine ()) != null && pline != "> End: Defibrillator")
                            pbuffer.AppendLine (pline);

                        App.Device_Defib = new DeviceDefib ();
                        InitDeviceDefib ();
                        App.Device_Defib.Load_Process (pbuffer.ToString ());
                    } else if (line == "> Begin: Intra-aortic Balloon Pump") {
                        pbuffer = new StringBuilder ();
                        while ((pline = sRead.ReadLine ()) != null && pline != "> End: Intra-aortic Balloon Pump")
                            pbuffer.AppendLine (pline);

                        App.Device_IABP = new DeviceIABP ();
                        InitDeviceIABP ();
                        App.Device_IABP.Load_Process (pbuffer.ToString ());
                    }
                }
            } catch {
                LoadFail ();
            } finally {
                sRead.Close ();
            }

            // On loading a file, ensure Mirroring is not in Client mode! Will conflict...
            if (App.Mirror.Status == Mirror.Statuses.CLIENT) {
                App.Mirror.Status = Mirror.Statuses.INACTIVE;
                App.Mirror.CancelOperation ();      // Attempt to cancel any possible Mirror downloads
                lblStatusText.Content = App.Language.Localize ("PE:StatusMirroringDisabled");
            }

            // Initialize the first step of the scenario
            if (App.Scenario.IsScenario)
                InitStep ();

            // Set Expanders IsExpanded and IsEnabled on whether is a Scenario
            bool isScene = App.Scenario.IsScenario;
            expScenarioPlayer.IsEnabled = isScene;
            expScenarioPlayer.IsExpanded = isScene;
            expVitalSigns.IsExpanded = !isScene;
            expHemodynamics.IsExpanded = !isScene;
            expRespiratoryProfile.IsExpanded = !isScene;
            expCardiacProfile.IsExpanded = !isScene;
            expObstetricProfile.IsExpanded = !isScene;
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

        private void LoadFail () {
            System.Windows.MessageBox.Show (
                    App.Language.Localize ("PE:LoadFailMessage"),
                    App.Language.Localize ("PE:LoadFailTitle"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void SaveFile () {

            // Only save single Patient files in base Infirmary Integrated!
            // Scenario files should be created/edited/saved via II Scenario Editor!
            if (App.Scenario.IsScenario) {
                System.Windows.MessageBox.Show (
                    App.Language.Localize ("PE:SaveFailScenarioMessage"),
                    App.Language.Localize ("PE:SaveFailScenarioTitle"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

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

        private void SaveT1 (Stream s) {

            // Ensure only saving Patient file, not Scenario file; is screened in SaveFile()
            if (App.Scenario.IsScenario) {
                s.Close ();
                return;
            }

            StringBuilder sb = new StringBuilder ();

            sb.AppendLine ("> Begin: Patient");
            sb.Append (App.Patient.Save ());
            sb.AppendLine ("> End: Patient");

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
            sw.WriteLine (".ii:t1");                                           // Metadata (type 1 savefile)
            sw.WriteLine (Encryption.HashSHA256 (sb.ToString ().Trim ()));     // Hash for validation
            sw.Write (Encryption.EncryptAES (sb.ToString ().Trim ()));         // Savefile data encrypted with AES
            sw.Close ();
            s.Close ();
        }

        private string SaveOptions () {
            StringBuilder sWrite = new StringBuilder ();

            sWrite.AppendLine (String.Format ("{0}:{1}", "checkDefaultVitals", checkDefaultVitals.IsChecked));

            return sWrite.ToString ();
        }

        public bool Exit () {
            Application.Current.Shutdown ();
            return true;
        }

        private void OnMirrorTick (object sender, EventArgs e)
            => App.Mirror.TimerTick (App.Patient, App.Server);

        private void OnStepChangeRequest (object sender, EventArgs e)
            => UnloadPatientEvents ();

        private void OnStepChanged (object sender, EventArgs e) {
            App.Patient = App.Scenario.Patient;

            InitPatientEvents ();
            InitStep ();
        }

        private void InitStep () {
            Scenario.Step s = App.Scenario.Current;

            // Set Previous, Next, Pause, and Play buttons .IsEnabled based on Step properties
            btnPreviousStep.IsEnabled = (s.ProgressFrom >= 0);
            btnNextStep.IsEnabled = (s.ProgressTo >= 0 || s.Progressions.Count > 0);
            btnPauseStep.IsEnabled = (s.ProgressTimer > 0);
            btnPlayStep.IsEnabled = false;

            // Display Scenario's Step count
            lblScenarioStep.Content = String.Format ("{0} {1} / {2}",
                App.Language.Localize ("PE:ProgressionStep"),
                App.Scenario.CurrentIndex,          // Retaining zero-based index to confuse end-user
                App.Scenario.Steps.Count - 1);      // But also for consistency with Scenario Editor and program development

            // Display Progress Timer if applicable, otherwise instruct that the Step requires manual progression
            if (s.ProgressTimer == -1)
                lblTimerStep.Content = App.Language.Localize ("PE:ProgressionManual");
            else
                lblTimerStep.Content = String.Format ("{0} {1} {2}",
                    App.Language.Localize ("PE:ProgressionAutomatic"),
                    s.ProgressTimer,
                    App.Language.Localize ("PE:ProgressionSeconds"));

            // Re-populate a StackPanel with RadioButtons for Progression options, including "Default Option"
            stackProgressions.Children.Clear ();

            stackProgressions.Children.Add (new RadioButton () {
                IsChecked = true,
                Name = "radioProgression_Default",
                Content = App.Language.Localize ("PE:ProgressionDefault"),
                GroupName = "ProgressionOptions"
            });

            for (int i = 0; i < s.Progressions.Count; i++) {
                Scenario.Step.Progression p = s.Progressions [i];

                stackProgressions.Children.Add (new RadioButton () {
                    IsChecked = false,
                    Content = p.Description,
                    Name = String.Format ("radioProgression_{0}", i),
                    GroupName = "ProgressionOptions"
                });
            }
        }

        private void NextStep () {
            if (App.Scenario.Current.Progressions.Count == 0)
                App.Scenario.NextStep ();
            else {
                foreach (RadioButton rb in stackProgressions.Children)
                    if (rb.IsChecked ?? false && rb.Name.Contains ("_")) {
                        string prog = rb.Name.Substring (rb.Name.IndexOf ("_") + 1);
                        int optProg = -1;
                        App.Scenario.NextStep (
                            prog == "Default" ? -1
                                : (int.TryParse (prog, out optProg) ? optProg : -1));
                        break;
                    }
            }
        }

        private void PreviousStep () {
            App.Scenario.LastStep ();
        }

        private void PauseStep () {
            btnPauseStep.IsEnabled = false;
            btnPlayStep.IsEnabled = true;

            App.Scenario.PauseStep ();

            lblTimerStep.Content = App.Language.Localize ("PE:ProgressionPaused");
        }

        private void PlayStep () {
            btnPauseStep.IsEnabled = true;
            btnPlayStep.IsEnabled = false;

            App.Scenario.PlayStep ();

            if (App.Scenario.Current.ProgressTimer == -1)
                lblTimerStep.Content = App.Language.Localize ("PE:ProgressionManual");
            else
                lblTimerStep.Content = String.Format ("{0} {1} {2}",
                    App.Language.Localize ("PE:ProgressionAutomatic"),
                    App.Scenario.Current.ProgressTimer - (App.Scenario.ProgressTimer.Elapsed / 1000),
                    App.Language.Localize ("PE:ProgressionSeconds"));
        }

        private void ApplyMirroring () {
            App.Mirror.PatientUpdated = new DateTime ();
            App.Mirror.ServerQueried = new DateTime ();

            if (radioInactive.IsChecked ?? true) {
                App.Mirror.Status = Mirror.Statuses.INACTIVE;
                lblStatusText.Content = App.Language.Localize ("PE:StatusMirroringDisabled");
            } else if (radioClient.IsChecked ?? true) {
                /* Set client mirroring status */
                App.Mirror.Status = Mirror.Statuses.CLIENT;
                App.Mirror.Accession = txtAccessionKey.Text;
                App.Mirror.PasswordAccess = txtAccessPassword.Password;
                lblStatusText.Content = App.Language.Localize ("PE:StatusMirroringActivated");

                /* When mirroring another patient, disable scenario player and Scenario timer */
                expScenarioPlayer.IsExpanded = false;   // Can be re-enabled by loading a scenario
                expScenarioPlayer.IsEnabled = false;
                App.Scenario.StopTimer ();
            } else if (radioServer.IsChecked ?? true) {
                if (txtAccessionKey.Text == "")
                    txtAccessionKey.Text = Utility.RandomString (8);

                App.Mirror.Status = Mirror.Statuses.HOST;
                App.Mirror.Accession = txtAccessionKey.Text;
                App.Mirror.PasswordAccess = txtAccessPassword.Password;
                App.Mirror.PasswordEdit = txtAdminPassword.Password;
                lblStatusText.Content = App.Language.Localize ("PE:StatusMirroringActivated");
            }
        }

        private void ResetPatientParameters () {
            RefreshPatient ();
            lblStatusText.Content = App.Language.Localize ("PE:StatusPatientReset");
        }

        private void ApplyPatientParameters () {
            ApplyMirroring ();

            List<FetalHeartDecelerations.Values> FHRRhythms = new List<FetalHeartDecelerations.Values> ();
            foreach (object o in listFHRRhythms.SelectedItems)
                FHRRhythms.Add ((FetalHeartDecelerations.Values)Enum.GetValues (typeof (FetalHeartDecelerations.Values)).GetValue (listFHRRhythms.Items.IndexOf (o)));

            App.Patient.UpdateParameters (

                // Basic vital signs
                (int)(numHR?.Value ?? 0),

                (int)(numNSBP?.Value ?? 0),
                (int)(numNDBP?.Value ?? 0),
                Patient.CalculateMAP ((int)(numNSBP?.Value ?? 0), (int)(numNDBP.Value ?? 0)),

                (int)(numRR?.Value ?? 0),
                (int)(numSPO2?.Value ?? 0),
                (double)(numT?.Value ?? 0),

                (Cardiac_Rhythms.Values)Enum.GetValues (typeof (Cardiac_Rhythms.Values)).GetValue (
                    comboCardiacRhythm.SelectedIndex < 0 ? 0 : comboCardiacRhythm.SelectedIndex),
                (Respiratory_Rhythms.Values)Enum.GetValues (typeof (Respiratory_Rhythms.Values)).GetValue (
                    comboRespiratoryRhythm.SelectedIndex < 0 ? 0 : comboRespiratoryRhythm.SelectedIndex),

                // Advanced hemodynamics
                (int)(numETCO2?.Value ?? 0),
                (int)(numCVP?.Value ?? 0),

                (int)(numASBP?.Value ?? 0),
                (int)(numADBP?.Value ?? 0),
                Patient.CalculateMAP ((int)(numASBP?.Value ?? 0), (int)(numADBP.Value ?? 0)),

                (PulmonaryArtery_Rhythms.Values)Enum.GetValues (typeof (PulmonaryArtery_Rhythms.Values)).GetValue (
                    comboPACatheterPlacement.SelectedIndex < 0 ? 0 : comboPACatheterPlacement.SelectedIndex),

                (int)(numPSP?.Value ?? 0),
                (int)(numPDP?.Value ?? 0),
                Patient.CalculateMAP ((int)(numPSP?.Value ?? 0), (int)(numPDP.Value ?? 0)),

                (int)(numICP?.Value ?? 0),
                (int)(numIAP?.Value ?? 0),

                // Respiratory profile
                chkMechanicallyVentilated.IsChecked ?? false,

                (float)(numInspiratoryRatio?.Value ?? 0),
                (float)(numExpiratoryRatio?.Value ?? 0),

                // Cardiac Profile
                (int)(numPacemakerCaptureThreshold?.Value ?? 0),
                chkPulsusParadoxus.IsChecked ?? false,
                chkPulsusAlternans.IsChecked ?? false,

                (Cardiac_Axes.Values)Enum.GetValues (typeof (Cardiac_Axes.Values)).GetValue (
                    comboCardiacAxis.SelectedIndex < 0 ? 0 : comboCardiacAxis.SelectedIndex),

                new float [] {
                (float)(numSTE_I?.Value ?? 0), (float)(numSTE_II?.Value ?? 0), (float)(numSTE_III?.Value ?? 0),
                (float)(numSTE_aVR?.Value ?? 0), (float)(numSTE_aVL?.Value ?? 0), (float)(numSTE_aVF?.Value ?? 0),
                (float)(numSTE_V1?.Value ?? 0), (float)(numSTE_V2?.Value ?? 0), (float)(numSTE_V3?.Value ?? 0),
                (float)(numSTE_V4?.Value ?? 0), (float)(numSTE_V5?.Value ?? 0), (float)(numSTE_V6.Value ?? 0)
                },
                new float [] {
                (float)(numTWE_I?.Value ?? 0), (float)(numTWE_II?.Value ?? 0), (float)(numTWE_III?.Value ?? 0),
                (float)(numTWE_aVR?.Value ?? 0), (float)(numTWE_aVL?.Value ?? 0), (float)(numTWE_aVF?.Value ?? 0),
                (float)(numTWE_V1?.Value ?? 0), (float)(numTWE_V2?.Value ?? 0), (float)(numTWE_V3?.Value ?? 0),
                (float)(numTWE_V4?.Value ?? 0), (float)(numTWE_V5?.Value ?? 0), (float)(numTWE_V6.Value ?? 0)
                },

                // Obstetric profile
                (int)(numFHR?.Value ?? 0),
                (Scales.Intensity.Values)Enum.GetValues (typeof (Scales.Intensity.Values)).GetValue (
                    comboFHRVariability.SelectedIndex < 0 ? 0 : comboFHRVariability.SelectedIndex),
                FHRRhythms,
                (int)(numUCFrequency?.Value ?? 0),
                (int)(numUCDuration?.Value ?? 0),
                (Scales.Intensity.Values)Enum.GetValues (typeof (Scales.Intensity.Values)).GetValue (
                    comboUCIntensity.SelectedIndex < 0 ? 0 : comboUCIntensity.SelectedIndex)
            );

            App.Mirror.PostPatient (App.Patient, App.Server);
            txtAccessionKey.Text = App.Mirror.Accession;

            AdvanceParameterStatus (ParameterStatuses.ChangesApplied);
        }

        private void MenuNewSimulation_Click (object s, RoutedEventArgs e) => RefreshScenario (true);

        private void MenuLoadFile_Click (object s, RoutedEventArgs e) => LoadFile ();

        private void MenuSaveFile_Click (object s, RoutedEventArgs e) => SaveFile ();

        private void MenuExit_Click (object s, RoutedEventArgs e) => Exit ();

        private void MenuSetLanguage_Click (object s, RoutedEventArgs e) => DialogInitial (true);

        private void MenuAbout_Click (object s, RoutedEventArgs e) => DialogAbout ();

        private void ButtonDeviceMonitor_Click (object s, RoutedEventArgs e) => InitDeviceMonitor ();

        private void ButtonDeviceECG_Click (object s, RoutedEventArgs e) => InitDeviceECG ();

        private void ButtonDeviceIABP_Click (object s, RoutedEventArgs e) => InitDeviceIABP ();

        private void ButtonDeviceDefib_Click (object s, RoutedEventArgs e) => InitDeviceDefib ();

        private void ButtonGenerateAccessionKey_Click (object sender, RoutedEventArgs e)
            => txtAccessionKey.Text = Utility.RandomString (8);

        private void ButtonApplyMirroring_Click (object s, RoutedEventArgs e)
            => ApplyMirroring ();

        private void ButtonPreviousStep_Click (object s, RoutedEventArgs e) => PreviousStep ();

        private void ButtonNextStep_Click (object s, RoutedEventArgs e) => NextStep ();

        private void ButtonPauseStep_Click (object s, RoutedEventArgs e) => PauseStep ();

        private void ButtonPlayStep_Click (object s, RoutedEventArgs e) => PlayStep ();

        private void ButtonResetParameters_Click (object s, RoutedEventArgs e)
            => ResetPatientParameters ();

        private void ButtonApplyParameters_Click (object sender, RoutedEventArgs e)
            => ApplyPatientParameters ();

        private void Window_Loaded (object sender, RoutedEventArgs e) {
            Application.Current.Dispatcher.BeginInvoke (DispatcherPriority.Background,
                    new Action (delegate () {
                        /* Set the window state, width, and height to saved settings */
                        this.WindowState = (System.Windows.WindowState)(Properties.Settings.Default.WindowState);

                        this.Width = Properties.Settings.Default.WindowSize.X;
                        this.Height = Properties.Settings.Default.WindowSize.Y;

                        /* Get the current screen; check that the desired window position is visible on the screen! */
                        System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromPoint
                            (new System.Drawing.Point ((int)this.Left, (int)this.Top));

                        if (Properties.Settings.Default.WindowPosition.X < screen.WorkingArea.Width
                               && Properties.Settings.Default.WindowPosition.Y < screen.WorkingArea.Height) {
                            this.Left = Properties.Settings.Default.WindowPosition.X;
                            this.Top = Properties.Settings.Default.WindowPosition.Y;
                        }

                        this.uiLoadCompleted = true;
                    }));
        }

        private void Window_SizeChanged (object sender, SizeChangedEventArgs e) {
            if (!uiLoadCompleted)
                return;

            Properties.Settings.Default.WindowSize = new System.Drawing.Point (
                (int)(sender as Window).ActualWidth,
                (int)(sender as Window).ActualHeight);
            Properties.Settings.Default.Save ();
        }

        private void Window_LocationChanged (object sender, EventArgs e) {
            if (!uiLoadCompleted)
                return;

            Properties.Settings.Default.WindowPosition = new System.Drawing.Point (
                (int)(sender as Window).Left,
                (int)(sender as Window).Top);
            Properties.Settings.Default.Save ();
        }

        private void Window_StateChanged (object sender, EventArgs e) {
            if (!uiLoadCompleted)
                return;

            Properties.Settings.Default.WindowState = (int)(sender as Window).WindowState;
            Properties.Settings.Default.Save ();
        }

        private void Window_Closed (object sender, EventArgs e)
            => Exit ();

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

        private void OnAutoApplyChanges_Changed (object sender, RoutedEventArgs e) {
            if (ParameterStatus == ParameterStatuses.Loading)
                return;

            Properties.Settings.Default.AutoApplyChanges = chkAutoApplyChanges.IsChecked ?? true;
            Properties.Settings.Default.Save ();

            SetParameterStatus (Properties.Settings.Default.AutoApplyChanges);
        }

        private void OnUIPatientParameter_KeyDown (object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter)
                OnUIPatientParameter_ProcessChanged (sender, null);
        }

        private void OnUIPatientParameter_GotFocus (object sender, RoutedEventArgs e) {
            if (sender is IntegerUpDown)
                uiBufferValue = (sender as IntegerUpDown).Value;
            else if (sender is DecimalUpDown)
                uiBufferValue = (sender as DecimalUpDown).Value;
            else if (sender is CheckBox)
                uiBufferValue = (sender as CheckBox).IsChecked;
            else if (sender is ComboBox)
                uiBufferValue = (sender as ComboBox).SelectedIndex;
        }

        private void OnUIPatientParameter_LostFocus (object sender, RoutedEventArgs e)
            => OnUIPatientParameter_ProcessChanged (sender, e);

        private void OnUIPatientParameter_ProcessChanged (object sender, RoutedEventArgs e) {
            if (sender is IntegerUpDown && (sender as IntegerUpDown).Value != (int)uiBufferValue)
                OnUIPatientParameter_Changed (sender, e);
            else if (sender is DecimalUpDown && (sender as DecimalUpDown).Value != (decimal)uiBufferValue)
                OnUIPatientParameter_Changed (sender, e);
            else if (sender is CheckBox && (sender as CheckBox).IsChecked != (bool)uiBufferValue)
                OnUIPatientParameter_Changed (sender, e);
            else if (sender is ComboBox && (sender as ComboBox).SelectedIndex != (int)uiBufferValue)
                OnUIPatientParameter_Changed (sender, e);
        }

        private void OnUIPatientParameter_Changed (object sender, RoutedEventArgs e) {
            switch (ParameterStatus) {
                default:
                case ParameterStatuses.Loading:            // For loading state
                    break;

                case ParameterStatuses.ChangesApplied:
                case ParameterStatuses.ChangesPending:
                    AdvanceParameterStatus (ParameterStatuses.ChangesPending);
                    break;

                case ParameterStatuses.AutoApply:
                    ApplyPatientParameters ();
                    UpdateParameterIndicators ();
                    break;
            }
        }

        private void OnCardiacRhythm_Selected (object sender, SelectionChangedEventArgs e) {
            if (!(bool)checkDefaultVitals.IsChecked || App.Patient == null)
                return;

            int si = comboCardiacRhythm.SelectedIndex;
            Array ev = Enum.GetValues (typeof (Cardiac_Rhythms.Values));
            if (si < 0 || si > ev.Length - 1)
                return;

            Cardiac_Rhythms.Default_Vitals v = Cardiac_Rhythms.DefaultVitals (
                (Cardiac_Rhythms.Values)ev.GetValue (si));

            numHR.Value = (int)II.Math.Clamp ((double)(numHR.Value ?? 0), v.HRMin, v.HRMax);
            numNSBP.Value = (int)II.Math.Clamp ((double)(numNSBP.Value ?? 0), v.SBPMin, v.SBPMax);
            numNDBP.Value = (int)II.Math.Clamp ((double)(numNDBP.Value ?? 0), v.DBPMin, v.DBPMax);
            numRR.Value = (int)II.Math.Clamp ((double)(numRR.Value ?? 0), v.RRMin, v.RRMax);
            numSPO2.Value = (int)II.Math.Clamp ((double)(numSPO2.Value ?? 0), v.SPO2Min, v.SPO2Max);
            numETCO2.Value = (int)II.Math.Clamp ((double)(numETCO2.Value ?? 0), v.ETCO2Min, v.ETCO2Max);
            numASBP.Value = (int)II.Math.Clamp ((double)(numASBP.Value ?? 0), v.SBPMin, v.SBPMax);
            numADBP.Value = (int)II.Math.Clamp ((double)(numADBP.Value ?? 0), v.DBPMin, v.DBPMax);
            numPSP.Value = (int)II.Math.Clamp ((double)(numPSP.Value ?? 0), v.PSPMin, v.PSPMax);
            numPDP.Value = (int)II.Math.Clamp ((double)(numPDP.Value ?? 0), v.PDPMin, v.PDPMax);

            OnUIPatientParameter_Changed (sender, e);
        }

        private void OnRespiratoryRhythm_Selected (object sender, SelectionChangedEventArgs e) {
            if (!(bool)checkDefaultVitals.IsChecked || App.Patient == null)
                return;

            int si = comboRespiratoryRhythm.SelectedIndex;
            Array ev = Enum.GetValues (typeof (Respiratory_Rhythms.Values));
            if (si < 0 || si > ev.Length - 1)
                return;

            Respiratory_Rhythms.Default_Vitals v = Respiratory_Rhythms.DefaultVitals (
                (Respiratory_Rhythms.Values)ev.GetValue (si));

            numRR.Value = (int)II.Math.Clamp ((double)(numRR.Value ?? 0), v.RRMin, v.RRMax);
            numInspiratoryRatio.Value = (int)II.Math.Clamp ((double)(numInspiratoryRatio.Value ?? 0), v.RR_IE_I_Min, v.RR_IE_I_Max);
            numExpiratoryRatio.Value = (int)II.Math.Clamp ((double)(numExpiratoryRatio.Value ?? 0), v.RR_IE_E_Min, v.RR_IE_E_Max);

            OnUIPatientParameter_Changed (sender, e);
        }

        private void OnPulmonaryArteryRhythm_Selected (object sender, SelectionChangedEventArgs e) {
            if (App.Patient == null)
                return;

            int si = comboPACatheterPlacement.SelectedIndex;
            Array ev = Enum.GetValues (typeof (PulmonaryArtery_Rhythms.Values));
            if (si < 0 || si > ev.Length - 1)
                return;

            PulmonaryArtery_Rhythms.Default_Vitals v = PulmonaryArtery_Rhythms.DefaultVitals (
                (PulmonaryArtery_Rhythms.Values)ev.GetValue (si));

            numPSP.Value = (int)II.Math.Clamp ((double)(numPSP.Value ?? 0), v.PSPMin, v.PSPMax);
            numPDP.Value = (int)II.Math.Clamp ((double)(numPDP.Value ?? 0), v.PDPMin, v.PDPMax);

            OnUIPatientParameter_Changed (sender, e);
        }

        private void FormUpdateFields (object sender, Patient.PatientEventArgs e) {
            if (e.EventType == Patient.PatientEventTypes.Vitals_Change) {

                // Basic vital signs
                numHR.Value = e.Patient.VS_Settings.HR;
                numNSBP.Value = e.Patient.VS_Settings.NSBP;
                numNDBP.Value = e.Patient.VS_Settings.NDBP;
                numRR.Value = e.Patient.VS_Settings.RR;
                numSPO2.Value = e.Patient.VS_Settings.SPO2;
                numT.Value = (decimal)e.Patient.VS_Settings.T;
                comboCardiacRhythm.SelectedIndex = (int)e.Patient.Cardiac_Rhythm.Value;
                comboRespiratoryRhythm.SelectedIndex = (int)e.Patient.Respiratory_Rhythm.Value;

                // Advanced hemodynamics
                numETCO2.Value = e.Patient.VS_Settings.ETCO2;
                numCVP.Value = e.Patient.VS_Settings.CVP;
                numASBP.Value = e.Patient.VS_Settings.ASBP;
                numADBP.Value = e.Patient.VS_Settings.ADBP;
                comboPACatheterPlacement.SelectedIndex = (int)e.Patient.PulmonaryArtery_Placement.Value;
                numPSP.Value = e.Patient.VS_Settings.PSP;
                numPDP.Value = e.Patient.VS_Settings.PDP;
                numICP.Value = e.Patient.VS_Settings.ICP;
                numIAP.Value = e.Patient.VS_Settings.IAP;

                // Respiratory profile
                chkMechanicallyVentilated.IsChecked = e.Patient.Mechanically_Ventilated;
                numInspiratoryRatio.Value = (decimal)e.Patient.VS_Settings.RR_IE_I;
                numExpiratoryRatio.Value = (decimal)e.Patient.VS_Settings.RR_IE_E;

                // Cardiac profile
                numPacemakerCaptureThreshold.Value = e.Patient.Pacemaker_Threshold;
                chkPulsusParadoxus.IsChecked = e.Patient.Pulsus_Paradoxus;
                chkPulsusAlternans.IsChecked = e.Patient.Pulsus_Alternans;
                comboCardiacAxis.SelectedIndex = (int)e.Patient.Cardiac_Axis.Value;

                numSTE_I.Value = (decimal)e.Patient.ST_Elevation [(int)Lead.Values.ECG_I];
                numSTE_II.Value = (decimal)e.Patient.ST_Elevation [(int)Lead.Values.ECG_II];
                numSTE_III.Value = (decimal)e.Patient.ST_Elevation [(int)Lead.Values.ECG_III];
                numSTE_aVR.Value = (decimal)e.Patient.ST_Elevation [(int)Lead.Values.ECG_AVR];
                numSTE_aVL.Value = (decimal)e.Patient.ST_Elevation [(int)Lead.Values.ECG_AVL];
                numSTE_aVF.Value = (decimal)e.Patient.ST_Elevation [(int)Lead.Values.ECG_AVF];
                numSTE_V1.Value = (decimal)e.Patient.ST_Elevation [(int)Lead.Values.ECG_V1];
                numSTE_V2.Value = (decimal)e.Patient.ST_Elevation [(int)Lead.Values.ECG_V2];
                numSTE_V3.Value = (decimal)e.Patient.ST_Elevation [(int)Lead.Values.ECG_V3];
                numSTE_V4.Value = (decimal)e.Patient.ST_Elevation [(int)Lead.Values.ECG_V4];
                numSTE_V5.Value = (decimal)e.Patient.ST_Elevation [(int)Lead.Values.ECG_V5];
                numSTE_V6.Value = (decimal)e.Patient.ST_Elevation [(int)Lead.Values.ECG_V6];

                numTWE_I.Value = (decimal)e.Patient.T_Elevation [(int)Lead.Values.ECG_I];
                numTWE_II.Value = (decimal)e.Patient.T_Elevation [(int)Lead.Values.ECG_II];
                numTWE_III.Value = (decimal)e.Patient.T_Elevation [(int)Lead.Values.ECG_III];
                numTWE_aVR.Value = (decimal)e.Patient.T_Elevation [(int)Lead.Values.ECG_AVR];
                numTWE_aVL.Value = (decimal)e.Patient.T_Elevation [(int)Lead.Values.ECG_AVL];
                numTWE_aVF.Value = (decimal)e.Patient.T_Elevation [(int)Lead.Values.ECG_AVF];
                numTWE_V1.Value = (decimal)e.Patient.T_Elevation [(int)Lead.Values.ECG_V1];
                numTWE_V2.Value = (decimal)e.Patient.T_Elevation [(int)Lead.Values.ECG_V2];
                numTWE_V3.Value = (decimal)e.Patient.T_Elevation [(int)Lead.Values.ECG_V3];
                numTWE_V4.Value = (decimal)e.Patient.T_Elevation [(int)Lead.Values.ECG_V4];
                numTWE_V5.Value = (decimal)e.Patient.T_Elevation [(int)Lead.Values.ECG_V5];
                numTWE_V6.Value = (decimal)e.Patient.T_Elevation [(int)Lead.Values.ECG_V6];

                // Obstetric profile
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
    }
}