using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

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

            List<ComboBoxItem> cardiacRhythms = new List<ComboBoxItem> (),
                respiratoryRhythms = new List<ComboBoxItem> (),
                pulmonaryArteryRhythms = new List<ComboBoxItem> (),
                cardiacAxes = new List<ComboBoxItem> (),
                intensityScale = new List<ComboBoxItem> ();
            List<ListBoxItem> fetalHeartRhythms = new List<ListBoxItem> ();

            foreach (Cardiac_Rhythms.Values v in Enum.GetValues (typeof (Cardiac_Rhythms.Values)))
                cardiacRhythms.Add (new ComboBoxItem () {
                    Content = App.Language.Localize (Cardiac_Rhythms.LookupString (v))
                });
            this.FindControl<ComboBox> ("comboCardiacRhythm").Items = cardiacRhythms;

            foreach (Respiratory_Rhythms.Values v in Enum.GetValues (typeof (Respiratory_Rhythms.Values)))
                respiratoryRhythms.Add (new ComboBoxItem () {
                    Tag = v.ToString (),
                    Content = App.Language.Localize (Respiratory_Rhythms.LookupString (v))
                }); ;
            this.FindControl<ComboBox> ("comboRespiratoryRhythm").Items = respiratoryRhythms;

            foreach (PulmonaryArtery_Rhythms.Values v in Enum.GetValues (typeof (PulmonaryArtery_Rhythms.Values)))
                pulmonaryArteryRhythms.Add (new ComboBoxItem () {
                    Tag = v.ToString (),
                    Content = App.Language.Localize (PulmonaryArtery_Rhythms.LookupString (v))
                });
            this.FindControl<ComboBox> ("comboPACatheterPlacement").Items = pulmonaryArteryRhythms;

            foreach (Cardiac_Axes.Values v in Enum.GetValues (typeof (Cardiac_Axes.Values)))
                cardiacAxes.Add (new ComboBoxItem () {
                    Tag = v.ToString (),
                    Content = App.Language.Localize (Cardiac_Axes.LookupString (v))
                });
            this.FindControl<ComboBox> ("comboCardiacAxis").Items = cardiacAxes;

            foreach (Scales.Intensity.Values v in Enum.GetValues (typeof (Scales.Intensity.Values)))
                intensityScale.Add (new ComboBoxItem () {
                    Tag = v.ToString (),
                    Content = App.Language.Localize (Scales.Intensity.LookupString (v))
                });
            this.FindControl<ComboBox> ("comboFHRVariability").Items = intensityScale;
            this.FindControl<ComboBox> ("comboUCIntensity").Items = intensityScale;

            foreach (FHRAccelDecels.Values v in Enum.GetValues (typeof (FHRAccelDecels.Values)))
                fetalHeartRhythms.Add (new ListBoxItem () {
                    Tag = v.ToString (),
                    Content = App.Language.Localize (FHRAccelDecels.LookupString (v))
                });
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
            App.Patient.PatientEvent += FormUpdateFields;
            FormUpdateFields (this, new Patient.PatientEventArgs (App.Patient, Patient.PatientEventTypes.Vitals_Change));

            if (App.Device_Monitor != null && App.Device_Monitor.IsActive)
                App.Patient.PatientEvent += App.Device_Monitor.OnPatientEvent;
            if (App.Device_ECG != null && App.Device_ECG.IsActive)
                App.Patient.PatientEvent += App.Device_ECG.OnPatientEvent;
            if (App.Device_Defib != null && App.Device_Defib.IsActive)
                App.Patient.PatientEvent += App.Device_Defib.OnPatientEvent;
            if (App.Device_IABP != null && App.Device_IABP.IsActive)
                App.Patient.PatientEvent += App.Device_IABP.OnPatientEvent;
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
            Border brdPendingChangesIndicator = this.FindControl<Border> ("brdPendingChangesIndicator");

            switch (ParameterStatus) {
                default:
                case ParameterStatuses.Loading:
                    brdPendingChangesIndicator.BorderBrush = Brushes.Transparent;
                    break;

                case ParameterStatuses.ChangesPending:
                    brdPendingChangesIndicator.BorderBrush = Brushes.Red;
                    //lblStatusText.Content = App.Language.Localize ("PE:StatusPatientChangesPending");
                    break;

                case ParameterStatuses.ChangesApplied:
                    brdPendingChangesIndicator.BorderBrush = Brushes.Green;

                    /*
                    if (App.Mirror.Status == Mirror.Statuses.INACTIVE)
                        lblStatusText.Content = App.Language.Localize ("PE:StatusPatientUpdated");
                    else if (App.Mirror.Status == Mirror.Statuses.HOST)
                        lblStatusText.Content = App.Language.Localize ("PE:StatusMirroredPatientUpdated");
                    */
                    break;

                case ParameterStatuses.AutoApply:
                    brdPendingChangesIndicator.BorderBrush = Brushes.Orange;
                    break;
            }
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

            FormUpdateFields (this, new Patient.PatientEventArgs (App.Patient, Patient.PatientEventTypes.Vitals_Change));
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
                //TODO lblStatusText.Content = App.Language.Localize ("PE:StatusMirroringDisabled");
            }

            // Initialize the first step of the scenario
            if (App.Scenario.IsScenario)
                InitStep ();

            // Set Expanders IsExpanded and IsEnabled on whether is a Scenario
            bool isScene = App.Scenario.IsScenario;

            this.FindControl<Expander> ("expScenarioPlayer").IsEnabled = isScene;
            this.FindControl<Expander> ("expScenarioPlayer").IsExpanded = isScene;
            this.FindControl<Expander> ("expVitalSigns").IsExpanded = !isScene;
            this.FindControl<Expander> ("expHemodynamics").IsExpanded = !isScene;
            this.FindControl<Expander> ("expRespiratoryProfile").IsExpanded = !isScene;
            this.FindControl<Expander> ("expCardiacProfile").IsExpanded = !isScene;
            this.FindControl<Expander> ("expObstetricProfile").IsExpanded = !isScene;
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
                            case "checkDefaultVitals": this.FindControl<CheckBox> ("checkDefaultVitals").IsChecked = bool.Parse (pValue); break;
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
            var assets = AvaloniaLocator.Current.GetService<Avalonia.Platform.IAssetLoader> ();
            var icon = new Bitmap (assets.Open (new Uri ("avares://Infirmary Integrated/Third_Party/Icon_DeviceMonitor_48.png")));

            var msBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
            .GetMessageBoxCustomWindow (new MessageBox.Avalonia.DTO.MessageBoxCustomParamsWithImage {
                ButtonDefinitions = new [] {
                    new MessageBox.Avalonia.Models.ButtonDefinition {
                        Name = "OK",
                        Type = MessageBox.Avalonia.Enums.ButtonType.Default,
                        IsCancel=true}
                },
                ContentTitle = App.Language.Localize ("PE:LoadFailTitle"),
                ContentMessage = App.Language.Localize ("PE:LoadFailMessage"),
                Icon = icon,
                Style = MessageBox.Avalonia.Enums.Style.None,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowIcon = this.Icon,
                Topmost = true,
                CanResize = false,
            });
            msBoxStandardWindow.Show ();
        }

        private async void SaveFile () {
            // Only save single Patient files in base Infirmary Integrated!
            // Scenario files should be created/edited/saved via II Scenario Editor!

            if (App.Scenario.IsScenario) {
                var assets = AvaloniaLocator.Current.GetService<Avalonia.Platform.IAssetLoader> ();
                var icon = new Bitmap (assets.Open (new Uri ("avares://Infirmary Integrated/Third_Party/Icon_DeviceMonitor_48.png")));

                var msBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxCustomWindow (new MessageBox.Avalonia.DTO.MessageBoxCustomParamsWithImage {
                    ButtonDefinitions = new [] {
                    new MessageBox.Avalonia.Models.ButtonDefinition {
                        Name = "OK",
                        Type = MessageBox.Avalonia.Enums.ButtonType.Default,
                        IsCancel=true}
                    },
                    ContentTitle = ("PE:SaveFailScenarioTitle"),
                    ContentMessage = App.Language.Localize ("PE:SaveFailScenarioMessage"),
                    Icon = icon,
                    Style = MessageBox.Avalonia.Enums.Style.None,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    WindowIcon = this.Icon,
                    Topmost = true,
                    CanResize = false,
                });
                msBoxStandardWindow.Show ();

                return;
            }

            SaveFileDialog dlgSave = new SaveFileDialog ();

            dlgSave.DefaultExtension = "ii";
            dlgSave.Filters.Add (new FileDialogFilter () { Name = "Infirmary Integrated Simulations", Extensions = { "ii" } });
            dlgSave.Filters.Add (new FileDialogFilter () { Name = "All files", Extensions = { "*" } });

            string file = await dlgSave.ShowAsync (this);

            if (!String.IsNullOrEmpty (file)) {
                SaveT1 (file);
            }
        }

        private void SaveT1 (string filename) {
            if (System.IO.File.Exists (filename))
                System.IO.File.Delete (filename);

            FileStream s = new FileStream (filename, FileMode.OpenOrCreate, FileAccess.Write);

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
                sb.Append (App.Device_Monitor.Save ());
                sb.AppendLine ("> End: Cardiac Monitor");
            }
            if (App.Device_ECG != null && App.Device_ECG.IsInitialized) {
                sb.AppendLine ("> Begin: 12 Lead ECG");
                sb.Append (App.Device_ECG.Save ());
                sb.AppendLine ("> End: 12 Lead ECG");
            }
            if (App.Device_Defib != null && App.Device_Defib.IsInitialized) {
                sb.AppendLine ("> Begin: Defibrillator");
                sb.Append (App.Device_Defib.Save ());
                sb.AppendLine ("> End: Defibrillator");
            }
            if (App.Device_IABP != null && App.Device_IABP.IsInitialized) {
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

            sWrite.AppendLine (String.Format ("{0}:{1}", "checkDefaultVitals", this.FindControl<CheckBox> ("checkDefaultVitals").IsChecked));

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
            App.Patient = App.Scenario?.Patient;

            InitPatientEvents ();
            InitStep ();
        }

        private void InitStep () {
            Scenario.Step s = App.Scenario.Current;

            Label lblScenarioStep = this.FindControl<Label> ("lblScenarioStep");
            Label lblTimerStep = this.FindControl<Label> ("lblTimerStep");
            StackPanel stackProgressions = this.FindControl<StackPanel> ("stackProgressions");

            // Set Previous, Next, Pause, and Play buttons .IsEnabled based on Step properties
            this.FindControl<Button> ("btnPreviousStep").IsEnabled = (s.ProgressFrom >= 0);
            this.FindControl<Button> ("btnNextStep").IsEnabled = (s.ProgressTo >= 0 || s.Progressions.Count > 0);
            this.FindControl<Button> ("btnPauseStep").IsEnabled = (s.ProgressTimer > 0);
            this.FindControl<Button> ("btnPlayStep").IsEnabled = false;

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
            StackPanel stackProgressions = this.FindControl<StackPanel> ("stackProgressions");

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
            App.Scenario?.LastStep ();
        }

        private void PauseStep () {
            this.FindControl<Button> ("btnPauseStep").IsEnabled = false;
            this.FindControl<Button> ("btnPlayStep").IsEnabled = true;

            App.Scenario.PauseStep ();

            this.FindControl<Label> ("lblTimerStep").Content = App.Language.Localize ("PE:ProgressionPaused");
        }

        private void PlayStep () {
            this.FindControl<Button> ("btnPauseStep").IsEnabled = true;
            this.FindControl<Button> ("btnPlayStep").IsEnabled = false;

            App.Scenario.PlayStep ();

            Label lblTimerStep = this.FindControl<Label> ("lblTimerStep");

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

            TextBox txtAccessionKey = this.FindControl<TextBox> ("txtAccessionKey");
            TextBox txtAccessPassword = this.FindControl<TextBox> ("txtAccessPassword");
            TextBox txtAdminPassword = this.FindControl<TextBox> ("txtAdminPassword");
            Expander expScenarioPlayer = this.FindControl<Expander> ("expScenarioPlayer");

            if (this.FindControl<RadioButton> ("radioInactive").IsChecked ?? true) {
                App.Mirror.Status = Mirror.Statuses.INACTIVE;
                //this.FindControl<Label> ("lblStatusText").Content = App.Language.Localize ("PE:StatusMirroringDisabled");
            } else if (this.FindControl<RadioButton> ("radioClient").IsChecked ?? true) {
                /* Set client mirroring status */
                App.Mirror.Status = Mirror.Statuses.CLIENT;
                App.Mirror.Accession = txtAccessionKey.Text;
                App.Mirror.PasswordAccess = txtAccessPassword.Text;
                //this.FindControl<Label> ("lblStatusText").Content = App.Language.Localize ("PE:StatusMirroringActivated");

                /* When mirroring another patient, disable scenario player and Scenario timer */
                expScenarioPlayer.IsExpanded = false;   // Can be re-enabled by loading a scenario
                expScenarioPlayer.IsEnabled = false;
                App.Scenario.StopTimer ();
            } else if (this.FindControl<RadioButton> ("radioServer").IsChecked ?? true) {
                if (txtAccessionKey.Text == "")
                    txtAccessionKey.Text = Utility.RandomString (8);

                App.Mirror.Status = Mirror.Statuses.HOST;
                App.Mirror.Accession = txtAccessionKey.Text;
                App.Mirror.PasswordAccess = txtAccessPassword.Text;
                App.Mirror.PasswordEdit = txtAdminPassword.Text;
                //this.FindControl<Label> ("lblStatusText").Content = App.Language.Localize ("PE:StatusMirroringActivated");
            }
        }

        private void ResetPatientParameters () {
            RefreshPatient ();
            //this.FindControl<Label> ("lblStatusText").Content = App.Language.Localize ("PE:StatusPatientReset");
        }

        private void ApplyPatientParameters () {
            ApplyMirroring ();

            ComboBox comboCardiacRhythm = this.FindControl<ComboBox> ("comboCardiacRhythm");
            ComboBox comboRespiratoryRhythm = this.FindControl<ComboBox> ("comboRespiratoryRhythm");
            ComboBox comboPACatheterPlacement = this.FindControl<ComboBox> ("comboPACatheterPlacement");
            ComboBox comboCardiacAxis = this.FindControl<ComboBox> ("comboCardiacAxis");
            ComboBox comboFHRVariability = this.FindControl<ComboBox> ("comboFHRVariability");
            ComboBox comboUCIntensity = this.FindControl<ComboBox> ("comboUCIntensity");
            ListBox listFHRRhythms = this.FindControl<ListBox> ("listFHRRhythms");

            List<FHRAccelDecels.Values> FHRRhythms = new List<FHRAccelDecels.Values> ();

            foreach (ListBoxItem lbi in listFHRRhythms.SelectedItems) {
                FHRRhythms.Add ((FHRAccelDecels.Values)Enum.Parse (typeof (FHRAccelDecels.Values), (string)lbi.Tag));
            }

            App.Patient.UpdateParameters (

                // Basic vital signs
                (int)(this.FindControl<NumericUpDown> ("numHR")?.Value ?? 0),

                (int)(this.FindControl<NumericUpDown> ("numNSBP")?.Value ?? 0),
                (int)(this.FindControl<NumericUpDown> ("numNDBP")?.Value ?? 0),
                Patient.CalculateMAP ((int)(this.FindControl<NumericUpDown> ("numNSBP")?.Value ?? 0), (int)(this.FindControl<NumericUpDown> ("numNDBP")?.Value ?? 0)),

                (int)(this.FindControl<NumericUpDown> ("numRR")?.Value ?? 0),
                (int)(this.FindControl<NumericUpDown> ("numSPO2")?.Value ?? 0),
                (double)(this.FindControl<NumericUpDown> ("numT")?.Value ?? 0),

                (Cardiac_Rhythms.Values)Enum.GetValues (typeof (Cardiac_Rhythms.Values)).GetValue (
                    comboCardiacRhythm.SelectedIndex < 0 ? 0 : comboCardiacRhythm.SelectedIndex),
                (Respiratory_Rhythms.Values)Enum.GetValues (typeof (Respiratory_Rhythms.Values)).GetValue (
                    comboRespiratoryRhythm.SelectedIndex < 0 ? 0 : comboRespiratoryRhythm.SelectedIndex),

                // Advanced hemodynamics
                (int)(this.FindControl<NumericUpDown> ("numETCO2")?.Value ?? 0),
                (int)(this.FindControl<NumericUpDown> ("numCVP")?.Value ?? 0),

                (int)(this.FindControl<NumericUpDown> ("numASBP")?.Value ?? 0),
                (int)(this.FindControl<NumericUpDown> ("numADBP")?.Value ?? 0),
                Patient.CalculateMAP ((int)(this.FindControl<NumericUpDown> ("numASBP")?.Value ?? 0), (int)(this.FindControl<NumericUpDown> ("numADBP")?.Value ?? 0)),

                (float)(this.FindControl<NumericUpDown> ("numCO")?.Value ?? 0),

                (PulmonaryArtery_Rhythms.Values)Enum.GetValues (typeof (PulmonaryArtery_Rhythms.Values)).GetValue (
                    comboPACatheterPlacement.SelectedIndex < 0 ? 0 : comboPACatheterPlacement.SelectedIndex),

                (int)(this.FindControl<NumericUpDown> ("numPSP")?.Value ?? 0),
                (int)(this.FindControl<NumericUpDown> ("numPDP")?.Value ?? 0),
                Patient.CalculateMAP ((int)(this.FindControl<NumericUpDown> ("numPSP")?.Value ?? 0), (int)(this.FindControl<NumericUpDown> ("numPDP")?.Value ?? 0)),

                (int)(this.FindControl<NumericUpDown> ("numICP")?.Value ?? 0),
                (int)(this.FindControl<NumericUpDown> ("numIAP")?.Value ?? 0),

                // Respiratory profile
                this.FindControl<CheckBox> ("chkMechanicallyVentilated").IsChecked ?? false,

                (float)(this.FindControl<NumericUpDown> ("numInspiratoryRatio")?.Value ?? 0),
                (float)(this.FindControl<NumericUpDown> ("numExpiratoryRatio")?.Value ?? 0),

                // Cardiac Profile
                (int)(this.FindControl<NumericUpDown> ("numPacemakerCaptureThreshold")?.Value ?? 0),
                this.FindControl<CheckBox> ("chkPulsusParadoxus").IsChecked ?? false,
                this.FindControl<CheckBox> ("chkPulsusAlternans").IsChecked ?? false,

                (Cardiac_Axes.Values)Enum.GetValues (typeof (Cardiac_Axes.Values)).GetValue (
                    comboCardiacAxis.SelectedIndex < 0 ? 0 : comboCardiacAxis.SelectedIndex),

                new double [] {
                    (double)(this.FindControl<NumericUpDown>("numSTE_I")?.Value ?? 0),
                    (double)(this.FindControl<NumericUpDown>("numSTE_II")?.Value ?? 0),
                    (double)(this.FindControl<NumericUpDown>("numSTE_III")?.Value ?? 0),
                    (double)(this.FindControl<NumericUpDown>("numSTE_aVR")?.Value ?? 0),
                    (double)(this.FindControl<NumericUpDown>("numSTE_aVL")?.Value ?? 0),
                    (double)(this.FindControl<NumericUpDown>("numSTE_aVF")?.Value ?? 0),
                    (double)(this.FindControl<NumericUpDown>("numSTE_V1")?.Value ?? 0),
                    (double)(this.FindControl<NumericUpDown>("numSTE_V2")?.Value ?? 0),
                    (double)(this.FindControl<NumericUpDown>("numSTE_V3")?.Value ?? 0),
                    (double)(this.FindControl<NumericUpDown>("numSTE_V4")?.Value ?? 0),
                    (double)(this.FindControl<NumericUpDown>("numSTE_V5")?.Value ?? 0),
                    (double)(this.FindControl<NumericUpDown>("numSTE_V6")?.Value?? 0)
                },
                new double [] {
                    (double)(this.FindControl<NumericUpDown>("numTWE_I")?.Value ?? 0),
                    (double)(this.FindControl<NumericUpDown>("numTWE_II")?.Value ?? 0),
                    (double)(this.FindControl<NumericUpDown>("numTWE_III")?.Value ?? 0),
                    (double)(this.FindControl<NumericUpDown>("numTWE_aVR")?.Value ?? 0),
                    (double)(this.FindControl<NumericUpDown>("numTWE_aVL")?.Value ?? 0),
                    (double)(this.FindControl<NumericUpDown>("numTWE_aVF")?.Value ?? 0),
                    (double)(this.FindControl<NumericUpDown>("numTWE_V1")?.Value ?? 0),
                    (double)(this.FindControl<NumericUpDown>("numTWE_V2")?.Value ?? 0),
                    (double)(this.FindControl<NumericUpDown>("numTWE_V3")?.Value ?? 0),
                    (double)(this.FindControl<NumericUpDown>("numTWE_V4")?.Value ?? 0),
                    (double)(this.FindControl<NumericUpDown>("numTWE_V5")?.Value ?? 0),
                    (double)(this.FindControl<NumericUpDown>("numTWE_V6")?.Value ?? 0)
                },

                // Obstetric profile
                (int)(this.FindControl<NumericUpDown> ("numFHR")?.Value ?? 0),
                (Scales.Intensity.Values)Enum.GetValues (typeof (Scales.Intensity.Values)).GetValue (
                    comboFHRVariability.SelectedIndex < 0 ? 0 : comboFHRVariability.SelectedIndex),
                FHRRhythms,
                (float)(this.FindControl<NumericUpDown> ("numUCFrequency")?.Value ?? 0),
                (int)(this.FindControl<NumericUpDown> ("numUCDuration")?.Value ?? 0),
                (Scales.Intensity.Values)Enum.GetValues (typeof (Scales.Intensity.Values)).GetValue (
                    comboUCIntensity.SelectedIndex < 0 ? 0 : comboUCIntensity.SelectedIndex)
            );

            App.Mirror.PostPatient (App.Patient, App.Server);
            this.FindControl<TextBox> ("txtAccessionKey").Text = App.Mirror.Accession;

            AdvanceParameterStatus (ParameterStatuses.ChangesApplied);
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

        private void TextBoxAccessionKey_PreviewTextInput (object sender, TextInputEventArgs e) {
            Regex regex = new Regex ("^[a-zA-Z0-9]*$");
            e.Handled = !regex.IsMatch (e.Text);
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
            if (ParameterStatus == ParameterStatuses.Loading)
                return;

            App.Settings.AutoApplyChanges = this.FindControl<CheckBox> ("chkAutoApplyChanges").IsChecked ?? true;
            App.Settings.Save ();

            SetParameterStatus (App.Settings.AutoApplyChanges);
        }

        private void OnUIPatientParameter_KeyDown (object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter)
                OnUIPatientParameter_Process (sender, e);
        }

        private void OnUIPatientParameter_Changed (object sender, NumericUpDownValueChangedEventArgs e)
            => OnUIPatientParameter_Process (sender, e);

        private void OnUIPatientParameter_Changed (object sender, SelectionChangedEventArgs e)
            => OnUIPatientParameter_Process (sender, e);

        private void OnUIPatientParameter_Changed (object sender, RoutedEventArgs e)
            => OnUIPatientParameter_Process (sender, e);

        private void OnUIPatientParameter_LostFocus (object sender, RoutedEventArgs e)
            => OnUIPatientParameter_Process (sender, e);

        private void OnUIPatientParameter_Process (object sender, RoutedEventArgs e) {
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
            if (!this.FindControl<CheckBox> ("checkDefaultVitals").IsChecked ?? false || App.Patient == null)
                return;

            int si = this.FindControl<ComboBox> ("comboCardiacRhythm").SelectedIndex;
            Array ev = Enum.GetValues (typeof (Cardiac_Rhythms.Values));
            if (si < 0 || si > ev.Length - 1)
                return;

            Cardiac_Rhythms.Default_Vitals v = Cardiac_Rhythms.DefaultVitals (
                (Cardiac_Rhythms.Values)ev.GetValue (si));

            this.FindControl<NumericUpDown> ("numHR").Value = (int)II.Math.Clamp ((double)(this.FindControl<NumericUpDown> ("numHR")?.Value ?? 0), v.HRMin, v.HRMax);
            this.FindControl<NumericUpDown> ("numNSBP").Value = (int)II.Math.Clamp ((double)(this.FindControl<NumericUpDown> ("numNSBP")?.Value ?? 0), v.SBPMin, v.SBPMax);
            this.FindControl<NumericUpDown> ("numNDBP").Value = (int)II.Math.Clamp ((double)(this.FindControl<NumericUpDown> ("numNDBP")?.Value ?? 0), v.DBPMin, v.DBPMax);
            this.FindControl<NumericUpDown> ("numRR").Value = (int)II.Math.Clamp ((double)(this.FindControl<NumericUpDown> ("numRR")?.Value ?? 0), v.RRMin, v.RRMax);
            this.FindControl<NumericUpDown> ("numSPO2").Value = (int)II.Math.Clamp ((double)(this.FindControl<NumericUpDown> ("numSPO2")?.Value ?? 0), v.SPO2Min, v.SPO2Max);
            this.FindControl<NumericUpDown> ("numETCO2").Value = (int)II.Math.Clamp ((double)(this.FindControl<NumericUpDown> ("numETCO2")?.Value ?? 0), v.ETCO2Min, v.ETCO2Max);
            this.FindControl<NumericUpDown> ("numASBP").Value = (int)II.Math.Clamp ((double)(this.FindControl<NumericUpDown> ("numASBP")?.Value ?? 0), v.SBPMin, v.SBPMax);
            this.FindControl<NumericUpDown> ("numADBP").Value = (int)II.Math.Clamp ((double)(this.FindControl<NumericUpDown> ("numADBP")?.Value ?? 0), v.DBPMin, v.DBPMax);
            this.FindControl<NumericUpDown> ("numPSP").Value = (int)II.Math.Clamp ((double)(this.FindControl<NumericUpDown> ("numPSP")?.Value ?? 0), v.PSPMin, v.PSPMax);
            this.FindControl<NumericUpDown> ("numPDP").Value = (int)II.Math.Clamp ((double)(this.FindControl<NumericUpDown> ("numPDP")?.Value ?? 0), v.PDPMin, v.PDPMax);

            OnUIPatientParameter_Process (sender, e);
        }

        private void OnRespiratoryRhythm_Selected (object sender, SelectionChangedEventArgs e) {
            if (!this.FindControl<CheckBox> ("checkDefaultVitals")?.IsChecked ?? false || App.Patient == null)
                return;

            int si = this.FindControl<ComboBox> ("comboRespiratoryRhythm").SelectedIndex;
            Array ev = Enum.GetValues (typeof (Respiratory_Rhythms.Values));
            if (si < 0 || si > ev.Length - 1)
                return;

            Respiratory_Rhythms.Default_Vitals v = Respiratory_Rhythms.DefaultVitals (
                (Respiratory_Rhythms.Values)ev.GetValue (si));

            NumericUpDown numRR = this.FindControl<NumericUpDown> ("numRR");
            NumericUpDown numInspiratoryRatio = this.FindControl<NumericUpDown> ("numInspiratoryRatio");
            NumericUpDown numExpiratoryRatio = this.FindControl<NumericUpDown> ("numExpiratoryRatio");

            numRR.Value = (int)II.Math.Clamp ((double)(numRR?.Value ?? 0), v.RRMin, v.RRMax);
            numInspiratoryRatio.Value = (int)II.Math.Clamp ((double)(numInspiratoryRatio?.Value ?? 0), v.RR_IE_I_Min, v.RR_IE_I_Max);
            numExpiratoryRatio.Value = (int)II.Math.Clamp ((double)(numExpiratoryRatio?.Value ?? 0), v.RR_IE_E_Min, v.RR_IE_E_Max);

            OnUIPatientParameter_Process (sender, e);
        }

        private void OnPulmonaryArteryRhythm_Selected (object sender, SelectionChangedEventArgs e) {
            if (App.Patient == null)
                return;

            int si = this.FindControl<ComboBox> ("comboPACatheterPlacement").SelectedIndex;
            Array ev = Enum.GetValues (typeof (PulmonaryArtery_Rhythms.Values));
            if (si < 0 || si > ev.Length - 1)
                return;

            PulmonaryArtery_Rhythms.Default_Vitals v = PulmonaryArtery_Rhythms.DefaultVitals (
                (PulmonaryArtery_Rhythms.Values)ev.GetValue (si));

            NumericUpDown numPSP = this.FindControl<NumericUpDown> ("numPSP");
            NumericUpDown numPDP = this.FindControl<NumericUpDown> ("numPDP");

            numPSP.Value = (int)II.Math.Clamp ((double)(numPSP?.Value ?? 0), v.PSPMin, v.PSPMax);
            numPDP.Value = (int)II.Math.Clamp ((double)(numPDP?.Value ?? 0), v.PDPMin, v.PDPMax);

            OnUIPatientParameter_Process (sender, e);
        }

        private void FormUpdateFields (object sender, Patient.PatientEventArgs e) {
            if (e.EventType == Patient.PatientEventTypes.Vitals_Change) {
                // Basic vital signs
                this.FindControl<NumericUpDown> ("numHR").Value = e.Patient.VS_Settings.HR;
                this.FindControl<NumericUpDown> ("numNSBP").Value = e.Patient.VS_Settings.NSBP;
                this.FindControl<NumericUpDown> ("numNDBP").Value = e.Patient.VS_Settings.NDBP;
                this.FindControl<NumericUpDown> ("numRR").Value = e.Patient.VS_Settings.RR;
                this.FindControl<NumericUpDown> ("numSPO2").Value = e.Patient.VS_Settings.SPO2;
                this.FindControl<NumericUpDown> ("numT").Value = (double)e.Patient.VS_Settings.T;
                this.FindControl<ComboBox> ("comboCardiacRhythm").SelectedIndex = (int)e.Patient.Cardiac_Rhythm.Value;
                this.FindControl<ComboBox> ("comboRespiratoryRhythm").SelectedIndex = (int)e.Patient.Respiratory_Rhythm.Value;

                // Advanced hemodynamics
                this.FindControl<NumericUpDown> ("numETCO2").Value = e.Patient.VS_Settings.ETCO2;
                this.FindControl<NumericUpDown> ("numCVP").Value = e.Patient.VS_Settings.CVP;
                this.FindControl<NumericUpDown> ("numASBP").Value = e.Patient.VS_Settings.ASBP;
                this.FindControl<NumericUpDown> ("numADBP").Value = e.Patient.VS_Settings.ADBP;
                this.FindControl<NumericUpDown> ("numCO").Value = (double)e.Patient.VS_Settings.CO;
                this.FindControl<ComboBox> ("comboPACatheterPlacement").SelectedIndex = (int)e.Patient.PulmonaryArtery_Placement.Value;
                this.FindControl<NumericUpDown> ("numPSP").Value = e.Patient.VS_Settings.PSP;
                this.FindControl<NumericUpDown> ("numPDP").Value = e.Patient.VS_Settings.PDP;
                this.FindControl<NumericUpDown> ("numICP").Value = e.Patient.VS_Settings.ICP;
                this.FindControl<NumericUpDown> ("numIAP").Value = e.Patient.VS_Settings.IAP;

                // Respiratory profile
                this.FindControl<CheckBox> ("chkMechanicallyVentilated").IsChecked = e.Patient.Mechanically_Ventilated;
                this.FindControl<NumericUpDown> ("numInspiratoryRatio").Value = (double)e.Patient.VS_Settings.RR_IE_I;
                this.FindControl<NumericUpDown> ("numExpiratoryRatio").Value = (double)e.Patient.VS_Settings.RR_IE_E;

                // Cardiac profile
                this.FindControl<NumericUpDown> ("numPacemakerCaptureThreshold").Value = e.Patient.Pacemaker_Threshold;
                this.FindControl<CheckBox> ("chkPulsusParadoxus").IsChecked = e.Patient.Pulsus_Paradoxus;
                this.FindControl<CheckBox> ("chkPulsusAlternans").IsChecked = e.Patient.Pulsus_Alternans;
                this.FindControl<ComboBox> ("comboCardiacAxis").SelectedIndex = (int)e.Patient.Cardiac_Axis.Value;

                this.FindControl<NumericUpDown> ("numSTE_I").Value = (double)e.Patient.ST_Elevation [(int)Lead.Values.ECG_I];
                this.FindControl<NumericUpDown> ("numSTE_II").Value = (double)e.Patient.ST_Elevation [(int)Lead.Values.ECG_II];
                this.FindControl<NumericUpDown> ("numSTE_III").Value = (double)e.Patient.ST_Elevation [(int)Lead.Values.ECG_III];
                this.FindControl<NumericUpDown> ("numSTE_aVR").Value = (double)e.Patient.ST_Elevation [(int)Lead.Values.ECG_AVR];
                this.FindControl<NumericUpDown> ("numSTE_aVL").Value = (double)e.Patient.ST_Elevation [(int)Lead.Values.ECG_AVL];
                this.FindControl<NumericUpDown> ("numSTE_aVF").Value = (double)e.Patient.ST_Elevation [(int)Lead.Values.ECG_AVF];
                this.FindControl<NumericUpDown> ("numSTE_V1").Value = (double)e.Patient.ST_Elevation [(int)Lead.Values.ECG_V1];
                this.FindControl<NumericUpDown> ("numSTE_V2").Value = (double)e.Patient.ST_Elevation [(int)Lead.Values.ECG_V2];
                this.FindControl<NumericUpDown> ("numSTE_V3").Value = (double)e.Patient.ST_Elevation [(int)Lead.Values.ECG_V3];
                this.FindControl<NumericUpDown> ("numSTE_V4").Value = (double)e.Patient.ST_Elevation [(int)Lead.Values.ECG_V4];
                this.FindControl<NumericUpDown> ("numSTE_V5").Value = (double)e.Patient.ST_Elevation [(int)Lead.Values.ECG_V5];
                this.FindControl<NumericUpDown> ("numSTE_V6").Value = (double)e.Patient.ST_Elevation [(int)Lead.Values.ECG_V6];

                this.FindControl<NumericUpDown> ("numTWE_I").Value = (double)e.Patient.T_Elevation [(int)Lead.Values.ECG_I];
                this.FindControl<NumericUpDown> ("numTWE_II").Value = (double)e.Patient.T_Elevation [(int)Lead.Values.ECG_II];
                this.FindControl<NumericUpDown> ("numTWE_III").Value = (double)e.Patient.T_Elevation [(int)Lead.Values.ECG_III];
                this.FindControl<NumericUpDown> ("numTWE_aVR").Value = (double)e.Patient.T_Elevation [(int)Lead.Values.ECG_AVR];
                this.FindControl<NumericUpDown> ("numTWE_aVL").Value = (double)e.Patient.T_Elevation [(int)Lead.Values.ECG_AVL];
                this.FindControl<NumericUpDown> ("numTWE_aVF").Value = (double)e.Patient.T_Elevation [(int)Lead.Values.ECG_AVF];
                this.FindControl<NumericUpDown> ("numTWE_V1").Value = (double)e.Patient.T_Elevation [(int)Lead.Values.ECG_V1];
                this.FindControl<NumericUpDown> ("numTWE_V2").Value = (double)e.Patient.T_Elevation [(int)Lead.Values.ECG_V2];
                this.FindControl<NumericUpDown> ("numTWE_V3").Value = (double)e.Patient.T_Elevation [(int)Lead.Values.ECG_V3];
                this.FindControl<NumericUpDown> ("numTWE_V4").Value = (double)e.Patient.T_Elevation [(int)Lead.Values.ECG_V4];
                this.FindControl<NumericUpDown> ("numTWE_V5").Value = (double)e.Patient.T_Elevation [(int)Lead.Values.ECG_V5];
                this.FindControl<NumericUpDown> ("numTWE_V6").Value = (double)e.Patient.T_Elevation [(int)Lead.Values.ECG_V6];

                // Obstetric profile
                this.FindControl<NumericUpDown> ("numFHR").Value = e.Patient.FHR;
                this.FindControl<NumericUpDown> ("numUCFrequency").Value = (double)e.Patient.Contraction_Frequency;
                this.FindControl<NumericUpDown> ("numUCDuration").Value = e.Patient.Contraction_Duration;
                this.FindControl<ComboBox> ("comboFHRVariability").SelectedIndex = (int)e.Patient.FHR_Variability.Value;
                this.FindControl<ComboBox> ("comboUCIntensity").SelectedIndex = (int)e.Patient.Contraction_Intensity.Value;

                ListBox listFHRRhythms = this.FindControl<ListBox> ("listFHRRhythms");
                listFHRRhythms.SelectedItems.Clear ();
                foreach (FHRAccelDecels.Values fhr_rhythm in e.Patient.FHR_AccelDecels.ValueList) {
                    foreach (ListBoxItem lbi in listFHRRhythms.Items) {
                        if ((string)lbi.Tag == fhr_rhythm.ToString ())
                            listFHRRhythms.SelectedItems.Add (lbi);
                    }
                }
            }
        }
    }
}