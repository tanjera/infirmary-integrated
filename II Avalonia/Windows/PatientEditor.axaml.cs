using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using II;
using II.Localization;
using II.Server;

namespace II_Avalonia {

    public partial class PatientEditor : Window {
        /* Properties for applying DPI scaling options */
        public double UIScale { get { return App.Settings.UIScale; } }
        public int FontScale { get { return (int)(14 * App.Settings.UIScale); } }

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
#if DEBUG
            this.AttachDevTools ();
#endif
            Init ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private void Init () {
            DataContext = this;

            this.Width *= UIScale;
            this.Height *= UIScale;

            App.Patient_Editor = this;

            InitInitialRun ();
#if !DEBUG
            InitUsageStatistics ();
#endif
            InitInterface ();
            InitUpgrade ();
            InitMirroring ();
            InitScenario (true);

            if (App.Start_Args?.Length > 0)
                LoadOpen (App.Start_Args [0]);

            SetParameterStatus (App.Settings.AutoApplyChanges);

            /* Debugging and testing code below */
        }

        private void InitInitialRun () {
            string setLang = App.Settings.Language;
            if (setLang == null || setLang == ""
                || !Enum.TryParse<Language.Values> (setLang, out App.Language.Value)) {
                App.Language = new Language ();
                DialogInitial ();
            }
        }

        private void InitUsageStatistics () {
            /* Send usage statistics to server in background */
            _ = Task.Run (() => App.Server.Run_UsageStats (new Server.UsageStat (App.Language)));
        }

        private void InitInterface () {
            /* Populate UI strings per language selection */
            this.FindControl<Window> ("wdwPatientEditor").Title = App.Language.Localize ("PE:WindowTitle");
            this.FindControl<MenuItem> ("menuNew").Header = App.Language.Localize ("PE:MenuNewFile");
            this.FindControl<MenuItem> ("menuFile").Header = App.Language.Localize ("PE:MenuFile");
            this.FindControl<MenuItem> ("menuLoad").Header = App.Language.Localize ("PE:MenuLoadSimulation");
            this.FindControl<MenuItem> ("menuSave").Header = App.Language.Localize ("PE:MenuSaveSimulation");
            this.FindControl<MenuItem> ("menuExit").Header = App.Language.Localize ("PE:MenuExitProgram");

            this.FindControl<MenuItem> ("menuSettings").Header = App.Language.Localize ("PE:MenuSettings");
            this.FindControl<MenuItem> ("menuSetLanguage").Header = App.Language.Localize ("PE:MenuSetLanguage");

            this.FindControl<MenuItem> ("menuHelp").Header = App.Language.Localize ("PE:MenuHelp");
            this.FindControl<MenuItem> ("menuAbout").Header = App.Language.Localize ("PE:MenuAboutProgram");

            this.FindControl<HeaderedContentControl> ("lblGroupDevices").Header = App.Language.Localize ("PE:Devices");
            this.FindControl<Label> ("lblDeviceMonitor").Content = App.Language.Localize ("PE:CardiacMonitor");
            this.FindControl<Label> ("lblDevice12LeadECG").Content = App.Language.Localize ("PE:12LeadECG");
            this.FindControl<Label> ("lblDeviceDefibrillator").Content = App.Language.Localize ("PE:Defibrillator");
            this.FindControl<Label> ("lblDeviceIABP").Content = App.Language.Localize ("PE:IABP");
            this.FindControl<Label> ("lblDeviceEFM").Content = App.Language.Localize ("PE:EFM");

            //lblDeviceVentilator.Content = App.Language.Dictionary["PE:Ventilator"];
            //lblDeviceIVPump.Content = App.Language.Dictionary["PE:IVPump"];
            //lblDeviceLabResults.Content = App.Language.Dictionary["PE:LabResults"];

            this.FindControl<Label> ("lblGroupMirroring").Content = App.Language.Localize ("PE:MirrorPatientData");
            this.FindControl<HeaderedContentControl> ("lblMirrorStatus").Header = App.Language.Localize ("PE:Status");
            this.FindControl<RadioButton> ("radioInactive").Content = App.Language.Localize ("PE:Inactive");
            this.FindControl<RadioButton> ("radioServer").Content = App.Language.Localize ("PE:Server");
            this.FindControl<RadioButton> ("radioClient").Content = App.Language.Localize ("PE:Client");

            this.FindControl<Label> ("lblAccessionKey").Content = App.Language.Localize ("PE:AccessionKey");
            this.FindControl<Label> ("lblAccessPassword").Content = App.Language.Localize ("PE:AccessPassword");
            this.FindControl<Label> ("lblAdminPassword").Content = App.Language.Localize ("PE:AdminPassword");
            this.FindControl<Button> ("btnApplyMirroring").Content = App.Language.Localize ("BUTTON:ApplyChanges");

            this.FindControl<Label> ("lblGroupScenarioPlayer").Content = App.Language.Localize ("PE:ScenarioPlayer");
            this.FindControl<HeaderedContentControl> ("lblProgressionOptions").Header = App.Language.Localize ("PE:ProgressionOptions");

            this.FindControl<Label> ("lblGroupVitalSigns").Content = App.Language.Localize ("PE:VitalSigns");
            this.FindControl<Label> ("lblHR").Content = $"{App.Language.Localize ("PE:HeartRate")}:";
            this.FindControl<Label> ("lblNIBP").Content = $"{App.Language.Localize ("PE:BloodPressure")}:";
            this.FindControl<Label> ("lblRR").Content = $"{App.Language.Localize ("PE:RespiratoryRate")}:";
            this.FindControl<Label> ("lblSPO2").Content = $"{App.Language.Localize ("PE:PulseOximetry")}:";
            this.FindControl<Label> ("lblT").Content = $"{App.Language.Localize ("PE:Temperature")}:";
            this.FindControl<Label> ("lblCardiacRhythm").Content = $"{App.Language.Localize ("PE:CardiacRhythm")}:";
            this.FindControl<CheckBox> ("checkDefaultVitals").Content = App.Language.Localize ("PE:UseDefaultVitalSignRanges");

            this.FindControl<Label> ("lblGroupHemodynamics").Content = App.Language.Localize ("PE:AdvancedHemodynamics");
            this.FindControl<Label> ("lblETCO2").Content = $"{ App.Language.Localize ("PE:EndTidalCO2")}:";
            this.FindControl<Label> ("lblCVP").Content = $"{ App.Language.Localize ("PE:CentralVenousPressure")}:";
            this.FindControl<Label> ("lblASBP").Content = $"{ App.Language.Localize ("PE:ArterialBloodPressure")}:";
            this.FindControl<Label> ("lblPACatheterPlacement").Content = $"{ App.Language.Localize ("PE:PulmonaryArteryCatheterPlacement")}:";
            this.FindControl<Label> ("lblCO").Content = $"{ App.Language.Localize ("PE:CardiacOutput")}:";
            this.FindControl<Label> ("lblPSP").Content = $"{ App.Language.Localize ("PE:PulmonaryArteryPressure")}:";
            this.FindControl<Label> ("lblICP").Content = $"{ App.Language.Localize ("PE:IntracranialPressure")}:";
            this.FindControl<Label> ("lblIAP").Content = $"{ App.Language.Localize ("PE:IntraabdominalPressure")}:";

            this.FindControl<Label> ("lblGroupRespiratoryProfile").Content = App.Language.Localize ("PE:RespiratoryProfile");
            this.FindControl<Label> ("lblRespiratoryRhythm").Content = $"{App.Language.Localize ("PE:RespiratoryRhythm")}:";
            this.FindControl<Label> ("lblMechanicallyVentilated").Content = $"{App.Language.Localize ("PE:MechanicallyVentilated")}:";
            this.FindControl<Label> ("lblInspiratoryRatio").Content = $"{App.Language.Localize ("PE:InspiratoryExpiratoryRatio")}:";

            this.FindControl<Label> ("lblGroupCardiacProfile").Content = App.Language.Localize ("PE:CardiacProfile");
            this.FindControl<Label> ("lblPacemakerCaptureThreshold").Content = $"{ App.Language.Localize ("PE:PacemakerCaptureThreshold")}:";
            this.FindControl<Label> ("lblPulsusParadoxus").Content = $"{ App.Language.Localize ("PE:PulsusParadoxus")}:";
            this.FindControl<Label> ("lblPulsusAlternans").Content = $"{ App.Language.Localize ("PE:PulsusAlternans")}:";
            this.FindControl<Label> ("lblCardiacAxis").Content = $"{App.Language.Localize ("PE:CardiacAxis")}:";
            this.FindControl<HeaderedContentControl> ("grpSTSegmentElevation").Header = App.Language.Localize ("PE:STSegmentElevation");
            this.FindControl<HeaderedContentControl> ("grpTWaveElevation").Header = App.Language.Localize ("PE:TWaveElevation");

            this.FindControl<Label> ("lblGroupObstetricProfile").Content = App.Language.Localize ("PE:ObstetricProfile");
            this.FindControl<Label> ("lblFHR").Content = $"{ App.Language.Localize ("PE:FetalHeartRate")}:";
            this.FindControl<Label> ("lblFHRRhythms").Content = $"{ App.Language.Localize ("PE:FetalHeartRhythms")}:";
            this.FindControl<Label> ("lblFHRVariability").Content = $"{ App.Language.Localize ("PE:FetalHeartVariability")}:";
            this.FindControl<Label> ("lblUCFrequency").Content = $"{ App.Language.Localize ("PE:UterineContractionFrequency")}:";
            this.FindControl<Label> ("lblUCDuration").Content = $"{ App.Language.Localize ("PE:UterineContractionDuration")}:";
            this.FindControl<Label> ("lblUCIntensity").Content = $"{ App.Language.Localize ("PE:UterineContractionIntensity")}:";

            this.FindControl<CheckBox> ("chkAutoApplyChanges").Content = App.Language.Localize ("BUTTON:AutoApplyChanges");
            this.FindControl<Label> ("lblParametersApply").Content = App.Language.Localize ("BUTTON:ApplyChanges");
            this.FindControl<Label> ("lblParametersReset").Content = App.Language.Localize ("BUTTON:ResetParameters");

            this.FindControl<CheckBox> ("chkAutoApplyChanges").IsChecked = App.Settings.AutoApplyChanges;

            List<string> cardiacRhythms = new List<string> (),
                respiratoryRhythms = new List<string> (),
                pulmonaryArteryRhythms = new List<string> (),
                cardiacAxes = new List<string> (),
                intensityScale = new List<string> (),
                fetalHeartRhythms = new List<string> ();

            foreach (Cardiac_Rhythms.Values v in Enum.GetValues (typeof (Cardiac_Rhythms.Values)))
                cardiacRhythms.Add (App.Language.Localize (Cardiac_Rhythms.LookupString (v)));
            this.FindControl<ComboBox> ("comboCardiacRhythm").Items = cardiacRhythms;

            foreach (Respiratory_Rhythms.Values v in Enum.GetValues (typeof (Respiratory_Rhythms.Values)))
                respiratoryRhythms.Add (App.Language.Localize (Respiratory_Rhythms.LookupString (v)));
            this.FindControl<ComboBox> ("comboRespiratoryRhythm").Items = respiratoryRhythms;

            foreach (PulmonaryArtery_Rhythms.Values v in Enum.GetValues (typeof (PulmonaryArtery_Rhythms.Values)))
                pulmonaryArteryRhythms.Add (App.Language.Localize (PulmonaryArtery_Rhythms.LookupString (v)));
            this.FindControl<ComboBox> ("comboPACatheterPlacement").Items = pulmonaryArteryRhythms;

            foreach (Cardiac_Axes.Values v in Enum.GetValues (typeof (Cardiac_Axes.Values)))
                cardiacAxes.Add (App.Language.Localize (Cardiac_Axes.LookupString (v)));
            this.FindControl<ComboBox> ("comboCardiacAxis").Items = cardiacAxes;

            foreach (Scales.Intensity.Values v in Enum.GetValues (typeof (Scales.Intensity.Values)))
                intensityScale.Add (App.Language.Localize (Scales.Intensity.LookupString (v)));
            this.FindControl<ComboBox> ("comboFHRVariability").Items = intensityScale;
            this.FindControl<ComboBox> ("comboUCIntensity").Items = intensityScale;

            foreach (FHRAccelDecels.Values v in Enum.GetValues (typeof (FHRAccelDecels.Values)))
                fetalHeartRhythms.Add (App.Language.Localize (FHRAccelDecels.LookupString (v)));
            this.FindControl<ListBox> ("listFHRRhythms").Items = fetalHeartRhythms;
        }

        private async void InitUpgrade () {
            /* Newer version available? Check Server, populate status bar, prompt user for upgrade */

            await Task.Run (() => App.Server.Get_LatestVersion_Windows ());

            if (Utility.IsNewerVersion (Utility.Version, App.Server.UpgradeVersion)) {
                //TODO txtUpdateAvailable.Text = String.Format (App.Language.Localize ("STATUS:UpdateAvailable"), App.Server.UpgradeVersion).Trim ();
            } else {            // If no update available, no status update
                //TODO statusUpdateAvailable.Visibility = Visibility.Collapsed;
                return;
            }

            if (App.Settings.MuteUpgrade) {
                if (Utility.IsNewerVersion (App.Settings.MuteUpgradeVersion, App.Server.UpgradeVersion)) {
                    App.Settings.MuteUpgrade = false;
                    App.Settings.Save ();
                } else {        // Mutes update popup notification
                    return;
                }
            }

            DialogUpgrade ();
        }

        private void InitMirroring () {
            App.Timer_Main.Elapsed += App.Mirror.ProcessTimer;
            App.Mirror.timerUpdate.Tick += OnMirrorTick;
            App.Mirror.timerUpdate.ResetAuto (5000);
        }

        private void InitScenario (bool toInit) {
            App.Scenario = new Scenario (toInit);
            App.Scenario.StepChangeRequest += OnStepChangeRequest;    // Allows unlinking of Timers immediately prior to Step change
            App.Scenario.StepChanged += OnStepChanged;                  // Updates App.Patient, allows PatientEditor UI to update
            App.Timer_Main.Elapsed += App.Scenario.ProcessTimer;

            if (toInit)         // If toInit is false, Patient is null- InitPatient() will need to be called manually
                InitPatient ();
        }

        private void UnloadScenario () {
            if (App.Scenario != null) {
                App.Timer_Main.Elapsed -= App.Scenario.ProcessTimer;   // Unlink Scenario from App/Main Timer
                App.Scenario.Dispose ();        // Disposes Scenario's events and timer, and all Patients' events and timers
            }
        }

        private void RefreshScenario (bool toInit) {
            UnloadScenario ();
            InitScenario (toInit);
        }

        private void InitPatient () {
            if (App.Scenario != null)
                App.Patient = App.Scenario.Patient;

            InitPatientEvents ();
            InitStep ();
        }

        private void RefreshPatient () {
            UnloadPatientEvents ();

            if (App.Patient != null)
                App.Patient.Dispose ();
            App.Patient = new Patient ();

            InitPatient ();
        }

        private void InitPatientEvents () {
            /* Tie the Patient's Timer to the Main Timer */
            App.Timer_Main.Elapsed += App.Patient.ProcessTimers;

            /* Tie PatientEvents to the PatientEditor UI! And trigger. */
            //TODO App.Patient.PatientEvent += FormUpdateFields;
            //TODO FormUpdateFields (this, new Patient.PatientEventArgs (App.Patient, Patient.PatientEventTypes.Vitals_Change));

            /* Tie PatientEvents to Devices! */
            /* TODO
            if (App.Device_Monitor != null && App.Device_Monitor.IsLoaded)
                App.Patient.PatientEvent += App.Device_Monitor.OnPatientEvent;
            if (App.Device_ECG != null && App.Device_ECG.IsLoaded)
                App.Patient.PatientEvent += App.Device_ECG.OnPatientEvent;
            if (App.Device_Defib != null && App.Device_Defib.IsLoaded)
                App.Patient.PatientEvent += App.Device_Defib.OnPatientEvent;
            if (App.Device_IABP != null && App.Device_IABP.IsLoaded)
                App.Patient.PatientEvent += App.Device_IABP.OnPatientEvent;
            */
        }

        private void UnloadPatientEvents () {
            /* Unloading the Patient from the Main Timer also stops all the Patient's Timers
            /* and results in that Patient not triggering PatientEvent's */
            App.Timer_Main.Elapsed -= App.Patient.ProcessTimers;

            /* But it's still important to clear PatientEvent subscriptions so they're not adding
            /* as duplicates when InitPatientEvents() is called!! */
            App.Patient.UnsubscribePatientEvent ();
        }

        private void InitDeviceMonitor () {
            if (App.Device_Monitor == null || !App.Device_Monitor.IsActive)
                App.Device_Monitor = new DeviceMonitor ();

            App.Device_Monitor.Activate ();
            App.Device_Monitor.Show ();

            if (App.Patient != null)
                App.Patient.PatientEvent += App.Device_Monitor.OnPatientEvent;
        }

        private void InitDeviceECG () {
            if (App.Device_ECG == null || !App.Device_ECG.IsActive)
                App.Device_ECG = new DeviceECG ();

            App.Device_ECG.Activate ();
            App.Device_ECG.Show ();

            if (App.Patient != null)
                App.Patient.PatientEvent += App.Device_ECG.OnPatientEvent;
        }

        private void InitDeviceDefib () {
            if (App.Device_Defib == null || !App.Device_Defib.IsActive)
                App.Device_Defib = new DeviceDefib ();

            App.Device_Defib.Activate ();
            App.Device_Defib.Show ();

            if (App.Patient != null)
                App.Patient.PatientEvent += App.Device_Defib.OnPatientEvent;
        }

        private void InitDeviceIABP () {
            if (App.Device_IABP == null || !App.Device_IABP.IsActive)
                App.Device_IABP = new DeviceIABP ();

            App.Device_IABP.Activate ();
            App.Device_IABP.Show ();

            if (App.Patient != null)
                App.Patient.PatientEvent += App.Device_IABP.OnPatientEvent;
        }

        private void InitDeviceEFM () {
            if (App.Device_EFM == null || !App.Device_EFM.IsActive)
                App.Device_EFM = new DeviceEFM ();

            App.Device_EFM.Activate ();
            App.Device_EFM.Show ();

            if (App.Patient != null)
                App.Patient.PatientEvent += App.Device_EFM.OnPatientEvent;
        }

        private void DialogInitial (bool reloadUI = false) {
            App.Dialog_Language = new DialogInitial ();
            App.Dialog_Language.Activate ();
            App.Dialog_Language.ShowDialog (this);

            if (reloadUI)
                InitInterface ();
        }

        private void DialogAbout () {
            App.Dialog_About = new DialogAbout ();
            App.Dialog_About.Activate ();
            App.Dialog_About.ShowDialog (this);
        }

        private async void DialogUpgrade () {
            Bootstrap.UpgradeRoute decision = Bootstrap.UpgradeRoute.NULL;

            App.Dialog_Upgrade = new DialogUpgrade ();
            App.Dialog_Upgrade.Activate ();

            App.Dialog_Upgrade.OnUpgradeRoute += (s, ea) => decision = ea.Route;
            await App.Dialog_Upgrade.ShowDialog (this);
            App.Dialog_Upgrade.OnUpgradeRoute -= (s, ea) => decision = ea.Route;

            switch (decision) {
                default:
                case Bootstrap.UpgradeRoute.NULL:
                case Bootstrap.UpgradeRoute.DELAY:
                    return;

                case Bootstrap.UpgradeRoute.MUTE:
                    App.Settings.MuteUpgrade = true;
                    App.Settings.MuteUpgradeVersion = App.Server.UpgradeVersion;
                    App.Settings.Save ();
                    return;

                case Bootstrap.UpgradeRoute.WEBSITE:
                case Bootstrap.UpgradeRoute.INSTALL:
                    if (!String.IsNullOrEmpty (App.Server.UpgradeWebpage))
                        InterOp.OpenBrowser (App.Server.UpgradeWebpage);
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
            /* TODO
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
            */
        }

        private async void LoadFile () {
            OpenFileDialog dlgLoad = new OpenFileDialog ();

            dlgLoad.Filters.Add (new FileDialogFilter () { Name = "Infirmary Integrated Simulations", Extensions = { "ii" } });
            dlgLoad.Filters.Add (new FileDialogFilter () { Name = "All files", Extensions = { "*" } });
            dlgLoad.AllowMultiple = false;

            string [] loadFile = await dlgLoad.ShowAsync (this);
            if (loadFile.Length > 0) {
                LoadInit (loadFile [0]);
            }
        }

        private void LoadOpen (string fileName) {
            if (System.IO.File.Exists (fileName)) {
                LoadInit (fileName);
            } else {
                LoadFail ();
            }

            //TODO FormUpdateFields (this, new Patient.PatientEventArgs (App.Patient, Patient.PatientEventTypes.Vitals_Change));
        }

        private void LoadInit (string incFile) {
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

            // Original save files used MD5, later changed to SHA256
            if (hash == Encryption.HashSHA256 (file) || hash == Encryption.HashMD5 (file))
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
                        //TODO App.Device_Monitor.Load_Process (pbuffer.ToString ());
                    } else if (line == "> Begin: 12 Lead ECG") {
                        pbuffer = new StringBuilder ();
                        while ((pline = sRead.ReadLine ()) != null && pline != "> End: 12 Lead ECG")
                            pbuffer.AppendLine (pline);

                        App.Device_ECG = new DeviceECG ();
                        InitDeviceECG ();
                        //TODO App.Device_ECG.Load_Process (pbuffer.ToString ());
                    } else if (line == "> Begin: Defibrillator") {
                        pbuffer = new StringBuilder ();
                        while ((pline = sRead.ReadLine ()) != null && pline != "> End: Defibrillator")
                            pbuffer.AppendLine (pline);

                        App.Device_Defib = new DeviceDefib ();
                        InitDeviceDefib ();
                        //TODO App.Device_Defib.Load_Process (pbuffer.ToString ());
                    } else if (line == "> Begin: Intra-aortic Balloon Pump") {
                        pbuffer = new StringBuilder ();
                        while ((pline = sRead.ReadLine ()) != null && pline != "> End: Intra-aortic Balloon Pump")
                            pbuffer.AppendLine (pline);

                        App.Device_IABP = new DeviceIABP ();
                        InitDeviceIABP ();
                        //TODO App.Device_IABP.Load_Process (pbuffer.ToString ());
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
                //TODO lblStatusText.Content = App.Language.Localize ("PE:StatusMirroringDisabled");
            }

            // Initialize the first step of the scenario
            if (App.Scenario.IsScenario)
                InitStep ();

            // Set Expanders IsExpanded and IsEnabled on whether is a Scenario
            bool isScene = App.Scenario.IsScenario;

            this.FindControl<Expander> ("expScenarioPlayer").IsEnabled = isScene;
            this.FindControl<Expander> ("expScenarioPlayer").IsExpanded = isScene;

            /* TODO
            expVitalSigns.IsExpanded = !isScene;
            expHemodynamics.IsExpanded = !isScene;
            expRespiratoryProfile.IsExpanded = !isScene;
            expCardiacProfile.IsExpanded = !isScene;
            expObstetricProfile.IsExpanded = !isScene;
            */
        }

        private void LoadOptions (string inc) {
            //TODO Repopulate Code
        }

        private void LoadFail () {
            //TODO Repopulate Code
        }

        private void SaveFile () {
            //TODO Repopulate Code
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

            if (App.Device_Monitor != null && App.Device_Monitor.IsInitialized) {
                sb.AppendLine ("> Begin: Cardiac Monitor");
                //TODO sb.Append (App.Device_Monitor.Save ());
                sb.AppendLine ("> End: Cardiac Monitor");
            }
            if (App.Device_ECG != null && App.Device_ECG.IsInitialized) {
                sb.AppendLine ("> Begin: 12 Lead ECG");
                //TODO sb.Append (App.Device_ECG.Save ());
                sb.AppendLine ("> End: 12 Lead ECG");
            }
            if (App.Device_Defib != null && App.Device_Defib.IsInitialized) {
                sb.AppendLine ("> Begin: Defibrillator");
                //TODO sb.Append (App.Device_Defib.Save ());
                sb.AppendLine ("> End: Defibrillator");
            }
            if (App.Device_IABP != null && App.Device_IABP.IsInitialized) {
                sb.AppendLine ("> Begin: Intra-aortic Balloon Pump");
                //TODO sb.Append (App.Device_IABP.Save ());
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

            //TODO sWrite.AppendLine (String.Format ("{0}:{1}", "checkDefaultVitals", checkDefaultVitals.IsChecked));

            return sWrite.ToString ();
        }

        public bool Exit () {
            App.Settings.Save ();

            this.Close ();
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
            //TODO Repopulate Code
        }

        private void NextStep () {
            //TODO Repopulate Code
        }

        private void PreviousStep () {
            App.Scenario.LastStep ();
        }

        private void PauseStep () {
            //TODO Repopulate Code
        }

        private void PlayStep () {
            //TODO Repopulate Code
        }

        private void ApplyMirroring () {
            //TODO Repopulate Code
        }

        private void ResetPatientParameters () {
            //TODO Repopulate Code
        }

        private void ApplyPatientParameters () {
            //TODO Repopulate Code
        }

        private void MenuNewSimulation_Click (object sender, RoutedEventArgs e) => RefreshScenario (true);

        private void MenuLoadFile_Click (object s, RoutedEventArgs e) => LoadFile ();

        private void MenuSaveFile_Click (object s, RoutedEventArgs e) => SaveFile ();

        private void MenuExit_Click (object s, RoutedEventArgs e) => Exit ();

        private void MenuSetLanguage_Click (object s, RoutedEventArgs e) => DialogInitial (true);

        private void MenuAbout_Click (object s, RoutedEventArgs e) => DialogAbout ();

        private void ButtonDeviceMonitor_Click (object s, RoutedEventArgs e) => InitDeviceMonitor ();

        private void ButtonDeviceDefib_Click (object s, RoutedEventArgs e) => InitDeviceDefib ();

        private void ButtonDeviceECG_Click (object s, RoutedEventArgs e) => InitDeviceECG ();

        private void ButtonDeviceIABP_Click (object s, RoutedEventArgs e) => InitDeviceIABP ();

        private void ButtonDeviceEFM_Click (object s, RoutedEventArgs e) => InitDeviceEFM ();

        private void ButtonGenerateAccessionKey_Click (object sender, RoutedEventArgs e)
            => this.FindControl<TextBox> ("txtAccessionKey").Text = Utility.RandomString (8);

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

        private void Window_Activated (object sender, EventArgs e) {
            if (!uiLoadCompleted) {
                this.Position = new PixelPoint (App.Settings.WindowPosition.X, App.Settings.WindowPosition.Y);
                this.uiLoadCompleted = true;
            }
        }

        private void Window_Closed (object sender, EventArgs e)
            => Exit ();

        private void Window_LayoutUpdated (object sender, EventArgs e) {
            if (!uiLoadCompleted) {
                this.Width = App.Settings.WindowSize.X;
                this.Height = App.Settings.WindowSize.Y;
            } else {
                App.Settings.WindowSize.X = (int)this.Width;
                App.Settings.WindowSize.Y = (int)this.Height;
            }
        }

        private void Window_PositionChanged (object sender, PixelPointEventArgs e) {
            if (uiLoadCompleted)
                App.Settings.WindowPosition = new System.Drawing.Point (e.Point.X, e.Point.Y);
        }

        private void TextBoxAccessionKey_PreviewTextInput (object sender, TextInputEventArgs e) {
            //TODO Repopulate Code
        }

        private void RadioMirrorSelected_Click (object sender, RoutedEventArgs e) {
            if (!this.IsInitialized || sender == null
                || this.FindControl<TextBox> ("txtAccessionKey") == null
                || this.FindControl<TextBox> ("txtAccessPassword") == null
                || this.FindControl<TextBox> ("txtAdminPassword") == null)
                return;

            if (((RadioButton)sender).Name == "radioInactive") {
                this.FindControl<TextBox> ("txtAccessionKey").IsEnabled = false;
                this.FindControl<Button> ("btnGenerateAccessionKey").IsEnabled = false;
                this.FindControl<TextBox> ("txtAccessPassword").IsEnabled = false;
                this.FindControl<TextBox> ("txtAdminPassword").IsEnabled = false;
            } else if (((RadioButton)sender).Name == "radioClient") {
                this.FindControl<TextBox> ("txtAccessionKey").IsEnabled = true;
                this.FindControl<Button> ("btnGenerateAccessionKey").IsEnabled = false;
                this.FindControl<TextBox> ("txtAccessPassword").IsEnabled = true;
                this.FindControl<TextBox> ("txtAdminPassword").IsEnabled = false;
            } else if (((RadioButton)sender).Name == "radioServer") {
                this.FindControl<TextBox> ("txtAccessionKey").IsEnabled = true;
                this.FindControl<Button> ("btnGenerateAccessionKey").IsEnabled = true;
                this.FindControl<TextBox> ("txtAccessPassword").IsEnabled = true;
                this.FindControl<TextBox> ("txtAdminPassword").IsEnabled = true;
            }
        }

        private void OnAutoApplyChanges_Changed (object sender, RoutedEventArgs e) {
            //TODO Repopulate Code
        }

        private void OnUIPatientParameter_KeyDown (object sender, KeyEventArgs e) {
            //TODO Repopulate Code
        }

        private void OnUIPatientParameter_GotFocus (object sender, GotFocusEventArgs e) {
            /* TODO
            if (sender is IntegerUpDown)
                uiBufferValue = (sender as IntegerUpDown).Value ?? 0;
            else if (sender is DecimalUpDown)
                uiBufferValue = (sender as DecimalUpDown).Value ?? 0;
            else if (sender is CheckBox)
                uiBufferValue = (sender as CheckBox).IsChecked ?? false;
            else if (sender is ComboBox)
                uiBufferValue = (sender as ComboBox).SelectedIndex;
            */
        }

        private void OnUIPatientParameter_LostFocus (object sender, RoutedEventArgs e)
            => OnUIPatientParameter_ProcessChanged (sender, e);

        private void OnUIPatientParameter_ProcessChanged (object sender, RoutedEventArgs e) {
            /* TODO
            if (sender is IntegerUpDown && (sender as IntegerUpDown).Value != (int)uiBufferValue)
                OnUIPatientParameter_Changed (sender, e);
            else if (sender is DecimalUpDown && (sender as DecimalUpDown).Value != (decimal)uiBufferValue)
                OnUIPatientParameter_Changed (sender, e);
            else if (sender is CheckBox && (sender as CheckBox).IsChecked != (bool)uiBufferValue)
                OnUIPatientParameter_Changed (sender, e);
            else if (sender is ComboBox && (sender as ComboBox).SelectedIndex != (int)uiBufferValue)
                OnUIPatientParameter_Changed (sender, e);
            */
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
            //TODO Repopulate Code
        }

        private void OnRespiratoryRhythm_Selected (object sender, SelectionChangedEventArgs e) {
            //TODO Repopulate Code
        }

        private void OnPulmonaryArteryRhythm_Selected (object sender, SelectionChangedEventArgs e) {
            //TODO Repopulate Code
        }

        private void FormUpdateFields (object sender, Patient.PatientEventArgs e) {
            //TODO Repopulate Code
        }
    }
}