using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
using Avalonia.Threading;

using II;
using II.Localization;
using II.Server;

namespace IISIM {

    public partial class WindowMain : Window {
        public App? Instance;

        /* Variables for UI loading */
        private bool IsUILoadCompleted = false;

        /* Variables for Auto-Apply functionality */
        private ParameterStatuses ParameterStatus = ParameterStatuses.Loading;

        private enum ParameterStatuses {
            Loading,
            AutoApply,
            ChangesPending,
            ChangesApplied
        }

        public WindowMain (App? app) {
            Instance = app;

            InitializeComponent ();

            _ = Init ();
        }

        public WindowMain () {
            InitializeComponent ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private async Task Init () {
            if (Instance is null)
                return;

            DataContext = this;

            Instance.Window_Main = this;

            await InitInitialRun ();

#if !DEBUG
            await InitUsageStatistics ();
#endif

            InitInterface ();
            await InitUpgrade ();
            await InitMirroring ();
            await InitScenario (true);

            if (Instance.Start_Args?.Length > 0)
                await LoadOpen (Instance.Start_Args [0]);

            await SetParameterStatus (Instance.Settings.AutoApplyChanges);
        }

        private async Task InitInitialRun () {
            if (!global::II.Settings.Simulator.Exists ()) {
                await DialogEULA ();
            }
        }

        private async Task InitUsageStatistics () {
            Server.UsageStat stat = new ();
            await stat.Init (Instance?.Language ?? new Language ());

            /* Send usage statistics to server in background */
            _ = Instance?.Server.Run_UsageStats (stat);
        }

        private void InitInterface () {
            /* Populate UI strings per language selection */
            if (Instance is not null) {
                this.FindControl<Window> ("wdwMain").Title = Instance.Language.Localize ("PE:WindowTitle");
                this.FindControl<MenuItem> ("menuNew").Header = Instance.Language.Localize ("PE:MenuNewFile");
                this.FindControl<MenuItem> ("menuFile").Header = Instance.Language.Localize ("PE:MenuFile");
                this.FindControl<MenuItem> ("menuLoad").Header = Instance.Language.Localize ("PE:MenuLoadSimulation");
                this.FindControl<MenuItem> ("menuSave").Header = Instance.Language.Localize ("PE:MenuSaveSimulation");
                this.FindControl<MenuItem> ("menuExit").Header = Instance.Language.Localize ("PE:MenuExitProgram");

                this.FindControl<MenuItem> ("menuMirror").Header = Instance.Language.Localize ("PE:MenuMirror");
                this.FindControl<MenuItem> ("menuMirrorDeactivate").Header = Instance.Language.Localize ("PE:MenuMirrorDeactivate");
                this.FindControl<MenuItem> ("menuMirrorReceive").Header = Instance.Language.Localize ("PE:MenuMirrorReceive");
                this.FindControl<MenuItem> ("menuMirrorBroadcast").Header = Instance.Language.Localize ("PE:MenuMirrorBroadcast");

                this.FindControl<MenuItem> ("menuSettings").Header = Instance.Language.Localize ("PE:MenuSettings");
                this.FindControl<MenuItem> ("menuToggleAudio").Header = String.Format ("{0}: {1}",
                    Instance.Language.Localize ("PE:MenuToggleAudio"),
                    Instance.Settings.AudioEnabled ? Instance.Language.Localize ("BOOLEAN:On") : Instance.Language.Localize ("BOOLEAN:Off"));
                this.FindControl<MenuItem> ("menuSetLanguage").Header = Instance.Language.Localize ("PE:MenuSetLanguage");

                this.FindControl<MenuItem> ("menuHelp").Header = Instance.Language.Localize ("PE:MenuHelp");
                this.FindControl<MenuItem> ("menuCheckUpdate").Header = Instance.Language.Localize ("PE:MenuCheckUpdates");
                this.FindControl<MenuItem> ("menuAbout").Header = Instance.Language.Localize ("PE:MenuAboutProgram");

                this.FindControl<HeaderedContentControl> ("lblGroupDevices").Header = Instance.Language.Localize ("PE:Devices");
                this.FindControl<Label> ("lblDeviceMonitor").Content = Instance.Language.Localize ("PE:CardiacMonitor");
                this.FindControl<Label> ("lblDevice12LeadECG").Content = Instance.Language.Localize ("PE:12LeadECG");
                this.FindControl<Label> ("lblDeviceDefibrillator").Content = Instance.Language.Localize ("PE:Defibrillator");
                this.FindControl<Label> ("lblDeviceIABP").Content = Instance.Language.Localize ("PE:IABP");
                this.FindControl<Label> ("lblDeviceEFM").Content = Instance.Language.Localize ("PE:EFM");

                this.FindControl<Label> ("lblGroupScenarioPlayer").Content = Instance.Language.Localize ("PE:ScenarioPlayer");
                this.FindControl<HeaderedContentControl> ("lblProgressionOptions").Header = Instance.Language.Localize ("PE:ProgressionOptions");

                this.FindControl<Label> ("lblGroupVitalSigns").Content = Instance.Language.Localize ("PE:VitalSigns");
                this.FindControl<Label> ("lblHR").Content = $"{Instance.Language.Localize ("PE:HeartRate")}:";
                this.FindControl<Label> ("lblNIBP").Content = $"{Instance.Language.Localize ("PE:BloodPressure")}:";
                this.FindControl<Label> ("lblRR").Content = $"{Instance.Language.Localize ("PE:RespiratoryRate")}:";
                this.FindControl<Label> ("lblSPO2").Content = $"{Instance.Language.Localize ("PE:PulseOximetry")}:";
                this.FindControl<Label> ("lblT").Content = $"{Instance.Language.Localize ("PE:Temperature")}:";
                this.FindControl<Label> ("lblCardiacRhythm").Content = $"{Instance.Language.Localize ("PE:CardiacRhythm")}:";
                this.FindControl<CheckBox> ("checkDefaultVitals").Content = Instance.Language.Localize ("PE:UseDefaultVitalSignRanges");

                this.FindControl<Label> ("lblGroupHemodynamics").Content = Instance.Language.Localize ("PE:AdvancedHemodynamics");
                this.FindControl<Label> ("lblETCO2").Content = $"{Instance.Language.Localize ("PE:EndTidalCO2")}:";
                this.FindControl<Label> ("lblCVP").Content = $"{Instance.Language.Localize ("PE:CentralVenousPressure")}:";
                this.FindControl<Label> ("lblASBP").Content = $"{Instance.Language.Localize ("PE:ArterialBloodPressure")}:";
                this.FindControl<Label> ("lblPACatheterPlacement").Content = $"{Instance.Language.Localize ("PE:PulmonaryArteryCatheterPlacement")}:";
                this.FindControl<Label> ("lblCO").Content = $"{Instance.Language.Localize ("PE:CardiacOutput")}:";
                this.FindControl<Label> ("lblPSP").Content = $"{Instance.Language.Localize ("PE:PulmonaryArteryPressure")}:";
                this.FindControl<Label> ("lblICP").Content = $"{Instance.Language.Localize ("PE:IntracranialPressure")}:";
                this.FindControl<Label> ("lblIAP").Content = $"{Instance.Language.Localize ("PE:IntraabdominalPressure")}:";

                this.FindControl<Label> ("lblGroupRespiratoryProfile").Content = Instance.Language.Localize ("PE:RespiratoryProfile");
                this.FindControl<Label> ("lblRespiratoryRhythm").Content = $"{Instance.Language.Localize ("PE:RespiratoryRhythm")}:";
                this.FindControl<Label> ("lblMechanicallyVentilated").Content = $"{Instance.Language.Localize ("PE:MechanicallyVentilated")}:";
                this.FindControl<Label> ("lblInspiratoryRatio").Content = $"{Instance.Language.Localize ("PE:InspiratoryExpiratoryRatio")}:";

                this.FindControl<Label> ("lblGroupCardiacProfile").Content = Instance.Language.Localize ("PE:CardiacProfile");
                this.FindControl<Label> ("lblPacemakerCaptureThreshold").Content = $"{Instance.Language.Localize ("PE:PacemakerCaptureThreshold")}:";
                this.FindControl<Label> ("lblPulsusParadoxus").Content = $"{Instance.Language.Localize ("PE:PulsusParadoxus")}:";
                this.FindControl<Label> ("lblPulsusAlternans").Content = $"{Instance.Language.Localize ("PE:PulsusAlternans")}:";
                this.FindControl<Label> ("lblCardiacAxis").Content = $"{Instance.Language.Localize ("PE:CardiacAxis")}:";
                this.FindControl<HeaderedContentControl> ("grpSTSegmentElevation").Header = Instance.Language.Localize ("PE:STSegmentElevation");
                this.FindControl<HeaderedContentControl> ("grpTWaveElevation").Header = Instance.Language.Localize ("PE:TWaveElevation");

                this.FindControl<Label> ("lblGroupObstetricProfile").Content = Instance.Language.Localize ("PE:ObstetricProfile");
                this.FindControl<Label> ("lblFHR").Content = $"{Instance.Language.Localize ("PE:FetalHeartRate")}:";
                this.FindControl<Label> ("lblFHRRhythms").Content = $"{Instance.Language.Localize ("PE:FetalHeartRhythms")}:";
                this.FindControl<Label> ("lblFHRVariability").Content = $"{Instance.Language.Localize ("PE:FetalHeartVariability")}:";
                this.FindControl<Label> ("lblUCFrequency").Content = $"{Instance.Language.Localize ("PE:UterineContractionFrequency")}:";
                this.FindControl<Label> ("lblUCDuration").Content = $"{Instance.Language.Localize ("PE:UterineContractionDuration")}:";
                this.FindControl<Label> ("lblUCIntensity").Content = $"{Instance.Language.Localize ("PE:UterineContractionIntensity")}:";

                this.FindControl<CheckBox> ("chkAutoApplyChanges").Content = Instance.Language.Localize ("BUTTON:AutoApplyChanges");
                this.FindControl<Label> ("lblParametersApply").Content = Instance.Language.Localize ("BUTTON:ApplyChanges");
                this.FindControl<Label> ("lblParametersReset").Content = Instance.Language.Localize ("BUTTON:ResetParameters");

                this.FindControl<CheckBox> ("chkAutoApplyChanges").IsChecked = Instance.Settings.AutoApplyChanges;

                List<ComboBoxItem> cardiacRhythms = new (),
                    respiratoryRhythms = new (),
                    pulmonaryArteryRhythms = new (),
                    cardiacAxes = new (),
                    intensityScale = new ();
                List<ListBoxItem> fetalHeartRhythms = new ();

                foreach (Cardiac_Rhythms.Values v in Enum.GetValues (typeof (Cardiac_Rhythms.Values)))
                    cardiacRhythms.Add (new ComboBoxItem () {
                        Content = Instance.Language.Localize (Cardiac_Rhythms.LookupString (v))
                    });
                this.FindControl<ComboBox> ("comboCardiacRhythm").Items = cardiacRhythms;

                foreach (Respiratory_Rhythms.Values v in Enum.GetValues (typeof (Respiratory_Rhythms.Values)))
                    respiratoryRhythms.Add (new ComboBoxItem () {
                        Tag = v.ToString (),
                        Content = Instance.Language.Localize (Respiratory_Rhythms.LookupString (v))
                    }); ;
                this.FindControl<ComboBox> ("comboRespiratoryRhythm").Items = respiratoryRhythms;

                foreach (PulmonaryArtery_Rhythms.Values v in Enum.GetValues (typeof (PulmonaryArtery_Rhythms.Values)))
                    pulmonaryArteryRhythms.Add (new ComboBoxItem () {
                        Tag = v.ToString (),
                        Content = Instance.Language.Localize (PulmonaryArtery_Rhythms.LookupString (v))
                    });
                this.FindControl<ComboBox> ("comboPACatheterPlacement").Items = pulmonaryArteryRhythms;

                foreach (Cardiac_Axes.Values v in Enum.GetValues (typeof (Cardiac_Axes.Values)))
                    cardiacAxes.Add (new ComboBoxItem () {
                        Tag = v.ToString (),
                        Content = Instance.Language.Localize (Cardiac_Axes.LookupString (v))
                    });
                this.FindControl<ComboBox> ("comboCardiacAxis").Items = cardiacAxes;

                foreach (Scales.Intensity.Values v in Enum.GetValues (typeof (Scales.Intensity.Values)))
                    intensityScale.Add (new ComboBoxItem () {
                        Tag = v.ToString (),
                        Content = Instance.Language.Localize (Scales.Intensity.LookupString (v))
                    });
                this.FindControl<ComboBox> ("comboFHRVariability").Items = intensityScale;
                this.FindControl<ComboBox> ("comboUCIntensity").Items = intensityScale;

                foreach (FHRAccelDecels.Values v in Enum.GetValues (typeof (FHRAccelDecels.Values)))
                    fetalHeartRhythms.Add (new ListBoxItem () {
                        Tag = v.ToString (),
                        Content = Instance.Language.Localize (FHRAccelDecels.LookupString (v))
                    });
                this.FindControl<ListBox> ("listFHRRhythms").Items = fetalHeartRhythms;
            }
        }

        private async Task InitUpgrade () {
            // Newer version available? Check Server, populate status bar, prompt user for upgrade
            await Instance.Server.Get_LatestVersion ();

            string version = Assembly.GetExecutingAssembly ()?.GetName ()?.Version?.ToString (3) ?? "0.0.0";
            if (Utility.IsNewerVersion (version, Instance.Server.UpgradeVersion)) {
                MenuItem miUpdate = this.FindControl<MenuItem> ("menuUpdate");
                miUpdate.Header = Instance.Language.Localize ("STATUS:UpdateAvailable");
                miUpdate.IsVisible = true;
            } else {            // If no update available, no status update; remove any notification muting
                Instance.Settings.MuteUpgrade = false;
                Instance.Settings.Save ();
                return;
            }

            if (Instance.Settings.MuteUpgrade) {
                if (DateTime.Compare (Instance.Settings.MuteUpgradeDate, DateTime.Now - new TimeSpan (30, 0, 0, 0)) < 0) {
                    Instance.Settings.MuteUpgrade = false;                       // Reset the notification mute every 30 days
                    Instance.Settings.Save ();
                } else {        // Mutes update popup notification
                    return;
                }
            }

            await DialogUpgrade ();
        }

        private async Task InitMirroring () {
            if (Instance is null)
                return;

            Instance.Timer_Main.Elapsed += Instance.Mirror.ProcessTimer;
            Instance.Mirror.timerUpdate.Tick += OnMirrorTick;
            await Instance.Mirror.timerUpdate.ResetAuto (5000);

            await UpdateMirrorStatus ();
        }

        private async Task InitScenario (bool toInit) {
            if (Instance is null)
                return;

            Instance.Scenario = new Scenario (toInit);
            Instance.Scenario.StepChangeRequest += OnStepChangeRequest;    // Allows unlinking of Timers immediately prior to Step change
            Instance.Scenario.StepChanged += OnStepChanged;                  // Updates IIApp.Patient, allows PatientEditor UI to update
            Instance.Timer_Main.Elapsed += Instance.Scenario.ProcessTimer;

            if (toInit)         // If toInit is false, Patient is null- InitPatient() will need to be called manually
                await InitPatient ();
        }

        private async Task UnloadScenario () {
            if (Instance is null)
                return;

            if (Instance.Scenario != null) {
                Instance.Timer_Main.Elapsed -= Instance.Scenario.ProcessTimer;   // Unlink Scenario from App/Main Timer
                await Instance.Scenario.Dispose ();        // Disposes Scenario's events and timer, and all Patients' events and timers
            }
        }

        private async Task RefreshScenario (bool toInit) {
            await UnloadScenario ();
            await InitScenario (toInit);

            await UpdateExpanders ();
        }

        private async Task InitPatient () {
            if (Instance?.Scenario != null)
                Instance.Patient = Instance.Scenario.Patient;

            await InitPatientEvents ();
            await InitStep ();
        }

        private async Task RefreshPatient () {
            if (Instance is null)
                return;

            await UnloadPatientEvents ();

            if (Instance?.Patient != null)
                await Instance.Patient.Dispose ();
            Instance.Patient = new Patient ();

            await InitPatient ();
        }

        private Task InitPatientEvents () {
            if (Instance is null)
                return Task.CompletedTask;

            /* Tie the Patient's Timer to the Main Timer */
            Instance.Timer_Main.Elapsed += Instance.Patient.ProcessTimers;

            /* Tie PatientEvents to the PatientEditor UI! And trigger. */
            Instance.Patient.PatientEvent += FormUpdateFields;
            FormUpdateFields (this, new Patient.PatientEventArgs (Instance.Patient, Patient.PatientEventTypes.Vitals_Change));

            if (Instance.Device_Monitor is not null)
                Instance.Patient.PatientEvent += Instance.Device_Monitor.OnPatientEvent;
            if (Instance.Device_ECG is not null)
                Instance.Patient.PatientEvent += Instance.Device_ECG.OnPatientEvent;
            if (Instance.Device_Defib is not null)
                Instance.Patient.PatientEvent += Instance.Device_Defib.OnPatientEvent;
            if (Instance.Device_IABP is not null)
                Instance.Patient.PatientEvent += Instance.Device_IABP.OnPatientEvent;

            return Task.CompletedTask;
        }

        private async Task UnloadPatientEvents () {
            if (Instance is null)
                return;

            /* Unloading the Patient from the Main Timer also stops all the Patient's Timers
            /* and results in that Patient not triggering PatientEvent's */
            Instance.Timer_Main.Elapsed -= Instance.Patient.ProcessTimers;

            /* But it's still important to clear PatientEvent subscriptions so they're not adding
            /* as duplicates when InitPatientEvents() is called!! */
            await Instance.Patient.UnsubscribePatientEvent ();
        }

        private Task InitDeviceMonitor () {
            if (Instance is null)
                return Task.CompletedTask;

            if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                this.Show ();

            if (Instance.Device_Monitor is null || Instance.Device_Monitor.State == DeviceWindow.States.Closed)
                Instance.Device_Monitor = new DeviceMonitor (Instance);

            Instance.Device_Monitor.Activate ();
            Instance.Device_Monitor.Show ();

            if (Instance.Patient is not null)
                Instance.Patient.PatientEvent += Instance.Device_Monitor.OnPatientEvent;

            return Task.CompletedTask;
        }

        private Task InitDeviceECG () {
            if (Instance is null)
                return Task.CompletedTask;

            if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                this.Show ();

            if (Instance.Device_ECG is null || Instance.Device_ECG.State == DeviceWindow.States.Closed)
                Instance.Device_ECG = new DeviceECG (Instance);

            Instance.Device_ECG.Activate ();
            Instance.Device_ECG.Show ();

            if (Instance.Patient is not null)
                Instance.Patient.PatientEvent += Instance.Device_ECG.OnPatientEvent;

            return Task.CompletedTask;
        }

        private Task InitDeviceDefib () {
            if (Instance is null)
                return Task.CompletedTask;

            if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                this.Show ();

            if (Instance.Device_Defib is null || Instance.Device_Defib.State == DeviceWindow.States.Closed)
                Instance.Device_Defib = new DeviceDefib (Instance);

            Instance.Device_Defib.Activate ();
            Instance.Device_Defib.Show ();

            if (Instance.Patient is not null)
                Instance.Patient.PatientEvent += Instance.Device_Defib.OnPatientEvent;

            return Task.CompletedTask;
        }

        private Task InitDeviceIABP () {
            if (Instance is null)
                return Task.CompletedTask;

            if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                this.Show ();

            if (Instance.Device_IABP is null || Instance.Device_IABP.State == DeviceWindow.States.Closed)
                Instance.Device_IABP = new DeviceIABP (Instance);

            Instance.Device_IABP.Activate ();
            Instance.Device_IABP.Show ();

            if (Instance.Patient is not null)
                Instance.Patient.PatientEvent += Instance.Device_IABP.OnPatientEvent;

            return Task.CompletedTask;
        }

        private Task InitDeviceEFM () {
            if (Instance is null)
                return Task.CompletedTask;

            if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                this.Show ();

            if (Instance.Device_EFM is null || Instance.Device_EFM.State == DeviceWindow.States.Closed)
                Instance.Device_EFM = new DeviceEFM (Instance);

            Instance.Device_EFM.Activate ();
            Instance.Device_EFM.Show ();

            if (Instance.Patient is not null)
                Instance.Patient.PatientEvent += Instance.Device_EFM.OnPatientEvent;

            return Task.CompletedTask;
        }

        private async Task DialogEULA () {
            if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                this.Show ();

            DialogEULA dlg = new (Instance);
            dlg.Activate ();
            await dlg.ShowDialog (this);
        }

        private async Task DialogLanguage (bool reloadUI = false) {
            if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                this.Show ();

            var oldLang = Instance?.Language.Value;
            DialogLanguage dlg = new (Instance);
            dlg.Activate ();
            await dlg.ShowDialog (this);

            reloadUI = oldLang != Instance?.Language.Value;

            if (reloadUI)
                InitInterface ();
        }

        private async Task DialogMirrorBroadcast () {
            if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                this.Show ();

            DialogMirrorBroadcast dlg = new (Instance);
            dlg.Activate ();
            await dlg.ShowDialog (this);

            if (Instance is not null)
                await Instance.Mirror.PostPatient (Instance.Patient, Instance.Server);
        }

        private async Task DialogMirrorReceive () {
            if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                this.Show ();

            DialogMirrorReceive dlg = new (Instance);
            dlg.Activate ();
            await dlg.ShowDialog (this);
        }

        private async Task DialogAbout () {
            if (!this.IsVisible)                    // Avalonia's parent must be visible to attach a window
                this.Show ();

            DialogAbout dlg = new (Instance);
            dlg.Activate ();
            await dlg.ShowDialog (this);
        }

        private async Task DialogUpgrade () {
            DialogUpgrade.UpgradeOptions decision = IISIM.DialogUpgrade.UpgradeOptions.None;

            DialogUpgrade dlg = new (Instance);
            dlg.Activate ();

            dlg.OnUpgradeRoute += (s, ea) => decision = ea.Route;
            await dlg.ShowDialog (this);
            dlg.OnUpgradeRoute -= (s, ea) => decision = ea.Route;

            switch (decision) {
                default:
                case IISIM.DialogUpgrade.UpgradeOptions.None:
                case IISIM.DialogUpgrade.UpgradeOptions.Delay:
                    return;

                case IISIM.DialogUpgrade.UpgradeOptions.Mute:
                    Instance.Settings.MuteUpgrade = true;
                    Instance.Settings.MuteUpgradeDate = DateTime.Now;
                    Instance.Settings.Save ();
                    return;

                case IISIM.DialogUpgrade.UpgradeOptions.Website:
                    if (!String.IsNullOrEmpty (Instance?.Server.UpgradeWebpage))
                        InterOp.OpenBrowser (Instance.Server.UpgradeWebpage);
                    return;
            }
        }

        private Task ToggleAudio () {
            if (Instance is null)
                return Task.CompletedTask;

            Instance.Settings.AudioEnabled = !Instance.Settings.AudioEnabled;

            Dispatcher.UIThread.InvokeAsync (() => {
                this.FindControl<MenuItem> ("menuToggleAudio").Header = String.Format ("{0}: {1}",
                    Instance.Language.Localize ("PE:MenuToggleAudio"),
                    Instance.Settings.AudioEnabled ? Instance.Language.Localize ("BOOLEAN:On") : Instance.Language.Localize ("BOOLEAN:Off"));
            });

            return Task.CompletedTask;
        }

        private async Task CheckUpgrade () {
            if (Instance is null)
                return;

            // Check with server for updated version of Infirmary Integrated- notify user either way
            await Instance.Server.Get_LatestVersion ();

            string version = Assembly.GetExecutingAssembly ()?.GetName ()?.Version?.ToString (3) ?? "0.0.0";
            if (Utility.IsNewerVersion (version, Instance.Server.UpgradeVersion)) {
                await DialogUpgrade ();
            } else {
                DialogUpgradeCurrent dlg = new (Instance);
                dlg.Activate ();
                await dlg.ShowDialog (this);
            }
        }

        private Task OpenUpgrade () {
            if (!String.IsNullOrEmpty (Instance?.Server.UpgradeWebpage))
                InterOp.OpenBrowser (Instance.Server.UpgradeWebpage);

            return Task.CompletedTask;
        }

        private async Task MirrorDeactivate () {
            if (Instance is null)
                return;

            Instance.Mirror.Status = Mirror.Statuses.INACTIVE;

            await UpdateMirrorStatus ();
            await UpdateExpanders (false);
        }

        private async Task MirrorBroadcast () {
            await DialogMirrorBroadcast ();

            await UpdateMirrorStatus ();
            await UpdateExpanders (false);
        }

        private async Task MirrorReceive () {
            await DialogMirrorReceive ();

            await UpdateMirrorStatus ();
            await UpdateExpanders (false);
        }

        private Task UpdateMirrorStatus () {
            Dispatcher.UIThread.InvokeAsync (() => {
                MenuItem miStatus = this.FindControl<MenuItem> ("menuMirrorStatus");

                switch (Instance?.Mirror.Status) {
                    default:
                    case Mirror.Statuses.INACTIVE:
                        miStatus.Header = $"{Instance?.Language.Localize ("MIRROR:Status")}: {Instance?.Language.Localize ("MIRROR:Inactive")}";
                        break;

                    case Mirror.Statuses.HOST:
                        miStatus.Header = $"{Instance?.Language.Localize ("MIRROR:Status")}: {Instance?.Language.Localize ("MIRROR:Server")}";
                        break;

                    case Mirror.Statuses.CLIENT:
                        miStatus.Header = $"{Instance?.Language.Localize ("MIRROR:Status")}: {Instance?.Language.Localize ("MIRROR:Client")}";
                        break;
                }
            });

            return Task.CompletedTask;
        }

        private async Task UpdateExpanders ()
            => await UpdateExpanders (Instance?.Scenario?.IsLoaded ?? false);

        private Task UpdateExpanders (bool isScene) {
            Dispatcher.UIThread.InvokeAsync (() => {
                this.FindControl<Border> ("brdScenarioPlayer").IsVisible = isScene;
                this.FindControl<Expander> ("expScenarioPlayer").IsEnabled = isScene;
                this.FindControl<Expander> ("expScenarioPlayer").IsExpanded = isScene;
                this.FindControl<Expander> ("expVitalSigns").IsExpanded = !isScene;
                this.FindControl<Expander> ("expHemodynamics").IsExpanded = !isScene;
                this.FindControl<Expander> ("expRespiratoryProfile").IsExpanded = !isScene;
                this.FindControl<Expander> ("expCardiacProfile").IsExpanded = !isScene;
                this.FindControl<Expander> ("expObstetricProfile").IsExpanded = !isScene;
            });

            return Task.CompletedTask;
        }

        private async Task SetParameterStatus (bool autoApplyChanges) {
            ParameterStatus = autoApplyChanges
               ? ParameterStatuses.AutoApply
               : ParameterStatuses.ChangesApplied;

            await UpdateParameterIndicators ();
        }

        private async Task AdvanceParameterStatus (ParameterStatuses status) {
            /* Toggles between pending changes or changes applied; bypasses if auto-applying or null */

            if (status == ParameterStatuses.ChangesApplied && ParameterStatus == ParameterStatuses.ChangesPending)
                ParameterStatus = ParameterStatuses.ChangesApplied;
            else if (status == ParameterStatuses.ChangesPending && ParameterStatus == ParameterStatuses.ChangesApplied)
                ParameterStatus = ParameterStatuses.ChangesPending;

            await UpdateParameterIndicators ();
        }

        private Task UpdateParameterIndicators () {
            Dispatcher.UIThread.InvokeAsync (() => {
                Border brdPendingChangesIndicator = this.FindControl<Border> ("brdPendingChangesIndicator");

                brdPendingChangesIndicator.BorderBrush = ParameterStatus switch {
                    ParameterStatuses.ChangesPending => Brushes.Red,
                    ParameterStatuses.ChangesApplied => Brushes.Green,
                    ParameterStatuses.AutoApply => Brushes.Orange,
                    _ => Brushes.Transparent,
                };
            });
            return Task.CompletedTask;
        }

        private async Task LoadFile () {
            OpenFileDialog dlgLoad = new ();

            dlgLoad.Filters.Add (new FileDialogFilter () { Name = "Infirmary Integrated Simulations", Extensions = { "ii" } });
            dlgLoad.Filters.Add (new FileDialogFilter () { Name = "All files", Extensions = { "*" } });
            dlgLoad.AllowMultiple = false;

            string [] loadFile = await dlgLoad.ShowAsync (this);
            if (loadFile.Length > 0) {
                await LoadInit (loadFile [0]);
            }
        }

        private async Task LoadOpen (string fileName) {
            if (System.IO.File.Exists (fileName)) {
                await LoadInit (fileName);
            } else {
                await LoadFail ();
            }

            FormUpdateFields (this, new Patient.PatientEventArgs (Instance?.Patient, Patient.PatientEventTypes.Vitals_Change));
        }

        private async Task LoadInit (string incFile) {
            using StreamReader sr = new (incFile);
            string? metadata = await sr.ReadLineAsync ();
            string? data = await sr.ReadToEndAsync ();
            sr.Close ();

            /* Read savefile metadata indicating data formatting
                * Multiple data formats for forward compatibility
                */

            if (!String.IsNullOrEmpty (metadata) && metadata.StartsWith (".ii:t1"))
                await LoadValidateT1 (data);
            else
                await LoadFail ();
        }

        private async Task LoadValidateT1 (string data) {
            using StringReader sr = new (data);

            try {
                /* Savefile type 1: validated and encrypted
                    * Line 1 is metadata (.ii:t1)
                    * Line 2 is hash for validation (hash taken of raw string data, unobfuscated)
                    * Line 3 is savefile data encrypted by AES encoding
                    */

                string? hash = (await sr.ReadLineAsync ())?.Trim ();
                string? file = Encryption.DecryptAES ((await sr.ReadToEndAsync ())?.Trim ());
                sr.Close ();

                // Original save files used MD5, later changed to SHA256
                if (hash == Encryption.HashSHA256 (file) || hash == Encryption.HashMD5 (file))
                    await LoadProcess (file);
                else
                    await LoadFail ();
            } catch {
                await LoadFail ();
            } finally {
                sr.Close ();
            }
        }

        private async Task LoadProcess (string incFile) {
            if (Instance.Patient is null)
                Instance.Patient = new ();
            if (Instance.Scenario is null)
                Instance.Scenario = new ();

            using StringReader sRead = new (incFile);
            string? line, pline;
            StringBuilder pbuffer;

            try {
                while ((line = (await sRead.ReadLineAsync ())?.Trim ()) != null) {
                    if (line == "> Begin: Patient") {           // Load files saved by Infirmary Integrated (base)
                        pbuffer = new StringBuilder ();
                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null && pline != "> End: Patient")
                            pbuffer.AppendLine (pline);

                        await RefreshScenario (true);
                        await Instance.Patient.Load_Process (pbuffer.ToString ());
                    } else if (line == "> Begin: Scenario") {   // Load files saved by Infirmary Integrated Scenario Editor
                        pbuffer = new StringBuilder ();
                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null && pline != "> End: Scenario")
                            pbuffer.AppendLine (pline);

                        await RefreshScenario (false);
                        await Instance.Scenario.Load (pbuffer.ToString ());
                        await InitPatient ();     // Needs to be called manually since InitScenario(false) doesn't init a Patient
                    } else if (line == "> Begin: Editor") {
                        pbuffer = new StringBuilder ();
                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null && pline != "> End: Editor")
                            pbuffer.AppendLine (pline);

                        await this.LoadOptions (pbuffer.ToString ());
                    } else if (line == "> Begin: Cardiac Monitor") {
                        pbuffer = new StringBuilder ();
                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null && pline != "> End: Cardiac Monitor")
                            pbuffer.AppendLine (pline);

                        Instance.Device_Monitor = new DeviceMonitor (Instance) { };
                        await InitDeviceMonitor ();
                        await Instance.Device_Monitor.Load_Process (pbuffer.ToString ());
                    } else if (line == "> Begin: 12 Lead ECG") {
                        pbuffer = new StringBuilder ();
                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null && pline != "> End: 12 Lead ECG")
                            pbuffer.AppendLine (pline);

                        Instance.Device_ECG = new DeviceECG (Instance);
                        await InitDeviceECG ();
                        await Instance.Device_ECG.Load_Process (pbuffer.ToString ());
                    } else if (line == "> Begin: Defibrillator") {
                        pbuffer = new StringBuilder ();
                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null && pline != "> End: Defibrillator")
                            pbuffer.AppendLine (pline);

                        Instance.Device_Defib = new DeviceDefib (Instance);
                        await InitDeviceDefib ();
                        await Instance.Device_Defib.Load_Process (pbuffer.ToString ());
                    } else if (line == "> Begin: Intra-aortic Balloon Pump") {
                        pbuffer = new StringBuilder ();
                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null && pline != "> End: Intra-aortic Balloon Pump")
                            pbuffer.AppendLine (pline);

                        Instance.Device_IABP = new DeviceIABP (Instance);
                        await InitDeviceIABP ();
                        await Instance.Device_IABP.Load_Process (pbuffer.ToString ());
                    }
                }
            } catch {
                await LoadFail ();
            } finally {
                sRead.Close ();
            }

            // On loading a file, ensure Mirroring is not in Client mode! Will conflict...
            if (Instance.Mirror.Status == Mirror.Statuses.CLIENT) {
                Instance.Mirror.Status = Mirror.Statuses.INACTIVE;
                Instance.Mirror.CancelOperation ();      // Attempt to cancel any possible Mirror downloads
            }

            // Initialize the first step of the scenario
            if (Instance.Scenario.IsLoaded) {
                await InitStep ();

                if (Instance.Scenario.DeviceMonitor.IsEnabled)
                    await InitDeviceMonitor ();
                if (Instance.Scenario.DeviceDefib.IsEnabled)
                    await InitDeviceDefib ();
                if (Instance.Scenario.DeviceECG.IsEnabled)
                    await InitDeviceECG ();
                if (Instance.Scenario.DeviceIABP.IsEnabled)
                    await InitDeviceIABP ();
            }

            // Set Expanders IsExpanded and IsEnabled on whether is a Scenario
            await UpdateExpanders ();
        }

        private async Task LoadOptions (string inc) {
            using StringReader sRead = new (inc);

            try {
                string? line;
                while ((line = await sRead.ReadLineAsync ()) != null) {
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

        private async Task LoadFail () {
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
                ContentTitle = Instance.Language.Localize ("PE:LoadFailTitle"),
                ContentMessage = Instance.Language.Localize ("PE:LoadFailMessage"),
                Icon = icon,
                Style = MessageBox.Avalonia.Enums.Style.None,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowIcon = this.Icon,
                Topmost = true,
                CanResize = false,
            });

            await msBoxStandardWindow.Show ();
        }

        private async Task SaveFile () {
            // Only save single Patient files in base Infirmary Integrated!
            // Scenario files should be created/edited/saved via II Scenario Editor!

            if (Instance.Scenario?.IsLoaded ?? false) {
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
                    ContentMessage = Instance.Language.Localize ("PE:SaveFailScenarioMessage"),
                    Icon = icon,
                    Style = MessageBox.Avalonia.Enums.Style.None,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    WindowIcon = this.Icon,
                    Topmost = true,
                    CanResize = false,
                });

                await msBoxStandardWindow.Show ();

                return;
            }

            SaveFileDialog dlgSave = new ();

            dlgSave.DefaultExtension = "ii";
            dlgSave.Filters.Add (new FileDialogFilter () { Name = "Infirmary Integrated Simulations", Extensions = { "ii" } });
            dlgSave.Filters.Add (new FileDialogFilter () { Name = "All files", Extensions = { "*" } });

            string file = await dlgSave.ShowAsync (this);

            if (!String.IsNullOrEmpty (file)) {
                await SaveT1 (file);
            }
        }

        private async Task SaveT1 (string filename) {
            if (System.IO.File.Exists (filename))
                System.IO.File.Delete (filename);

            using FileStream s = new (filename, FileMode.OpenOrCreate, FileAccess.Write);

            // Ensure only saving Patient file, not Scenario file; is screened in SaveFile()
            if (Instance?.Scenario != null && Instance.Scenario.IsLoaded) {
                s.Close ();
                return;
            }

            StringBuilder sb = new ();

            sb.AppendLine ("> Begin: Patient");
            sb.Append (Instance.Patient?.Save ());
            sb.AppendLine ("> End: Patient");

            sb.AppendLine ("> Begin: Editor");
            sb.Append (this.SaveOptions ());
            sb.AppendLine ("> End: Editor");

            if (Instance.Device_Monitor is not null) {
                sb.AppendLine ("> Begin: Cardiac Monitor");
                sb.Append (Instance.Device_Monitor.Save ());
                sb.AppendLine ("> End: Cardiac Monitor");
            }
            if (Instance.Device_ECG is not null) {
                sb.AppendLine ("> Begin: 12 Lead ECG");
                sb.Append (Instance.Device_ECG.Save ());
                sb.AppendLine ("> End: 12 Lead ECG");
            }
            if (Instance.Device_Defib is not null) {
                sb.AppendLine ("> Begin: Defibrillator");
                sb.Append (Instance.Device_Defib.Save ());
                sb.AppendLine ("> End: Defibrillator");
            }
            if (Instance.Device_IABP is not null) {
                sb.AppendLine ("> Begin: Intra-aortic Balloon Pump");
                sb.Append (Instance.Device_IABP.Save ());
                sb.AppendLine ("> End: Intra-aortic Balloon Pump");
            }

            using StreamWriter sw = new (s);
            await sw.WriteLineAsync (".ii:t1");                                           // Metadata (type 1 savefile)
            await sw.WriteLineAsync (Encryption.HashSHA256 (sb.ToString ().Trim ()));     // Hash for validation
            await sw.WriteAsync (Encryption.EncryptAES (sb.ToString ().Trim ()));         // Savefile data encrypted with AES
            await sw.FlushAsync ();

            sw.Close ();
            s.Close ();
        }

        private string SaveOptions () {
            StringBuilder sWrite = new ();

            sWrite.AppendLine (String.Format ("{0}:{1}", "checkDefaultVitals", this.FindControl<CheckBox> ("checkDefaultVitals").IsChecked));

            return sWrite.ToString ();
        }

        public Task Exit () {
            Instance?.Settings.Save ();
            Instance?.Exit ();

            return Task.CompletedTask;
        }

        private void OnMirrorTick (object? sender, EventArgs e) {
            Instance?.Mirror.TimerTick (Instance.Patient, Instance.Server);

            if (Instance?.Mirror.Status == Mirror.Statuses.CLIENT) {
                ForceUpdateFields (Instance.Patient);
            }
        }

        private void OnStepChangeRequest (object? sender, EventArgs e)
            => _ = UnloadPatientEvents ();

        private void OnStepChanged (object? sender, EventArgs e) {
            Instance.Patient = Instance.Scenario?.Patient;

            _ = InitPatientEvents ();
            _ = InitStep ();

            ForceUpdateFields (Instance.Patient);
        }

        private Task InitStep () {
            Scenario.Step s = Instance?.Scenario?.Current ?? new Scenario.Step ();

            Label lblScenarioStep = this.FindControl<Label> ("lblScenarioStep");
            Label lblTimerStep = this.FindControl<Label> ("lblTimerStep");
            StackPanel stackProgressions = this.FindControl<StackPanel> ("stackProgressions");

            // Set Previous, Next, Pause, and Play buttons .IsEnabled based on Step properties
            this.FindControl<Button> ("btnPreviousStep").IsEnabled = (!String.IsNullOrEmpty (s.DefaultSource));
            this.FindControl<Button> ("btnNextStep").IsEnabled = (!String.IsNullOrEmpty (s.DefaultProgression?.UUID) || s.Progressions.Count > 0);
            this.FindControl<Button> ("btnPauseStep").IsEnabled = (s.ProgressTimer > 0);
            this.FindControl<Button> ("btnPlayStep").IsEnabled = false;

            // Display Scenario's Step count
            lblScenarioStep.Content = $"{Instance?.Language.Localize ("PE:ProgressionStep")}: {s.Name}";

            // Display Progress Timer if applicable, otherwise instruct that the Step requires manual progression
            if (s.ProgressTimer == -1)
                lblTimerStep.Content = Instance?.Language.Localize ("PE:ProgressionManual");
            else
                lblTimerStep.Content = String.Format ("{0} {1} {2}",
                    Instance?.Language.Localize ("PE:ProgressionAutomatic"),
                    s.ProgressTimer,
                    Instance?.Language.Localize ("PE:ProgressionSeconds"));

            // Re-populate a StackPanel with RadioButtons for Progression options, including "Default Option"
            stackProgressions.Children.Clear ();

            stackProgressions.Children.Add (new RadioButton () {
                IsChecked = true,
                Name = "radioProgression_Default",
                Content = Instance?.Language.Localize ("PE:ProgressionDefault"),
                GroupName = "ProgressionOptions",
                Margin = new Thickness (10, 10, 10, 5)
            });

            for (int i = 0; i < s.Progressions.Count; i++) {
                Scenario.Step.Progression p = s.Progressions [i];

                stackProgressions.Children.Add (new RadioButton () {
                    IsChecked = false,
                    Content = p.Description,
                    Name = String.Format ("radioProgression_{0}", p.DestinationUUID),
                    GroupName = "ProgressionOptions",
                    Margin = (i == s.Progressions.Count - 1 ? new Thickness (10, 5, 10, 10) : new Thickness (10, 5))
                });
            }

            return Task.CompletedTask;
        }

        private async Task NextStep () {
            StackPanel stackProgressions = this.FindControl<StackPanel> ("stackProgressions");

            if (Instance?.Scenario?.Current?.Progressions.Count == 0)
                await Instance.Scenario.NextStep ();
            else {
                foreach (RadioButton rb in stackProgressions.Children)
                    if (rb.IsChecked ?? false && rb.Name.Contains ("_")) {
                        string? prog = rb.Name?.Substring (rb.Name.IndexOf ("_") + 1);
                        await Instance.Scenario.NextStep (prog == "Default" ? null : prog);
                        break;
                    }
            }
        }

        private async Task PreviousStep () {
            await Instance.Scenario?.LastStep ();
        }

        private async Task PauseStep () {
            this.FindControl<Button> ("btnPauseStep").IsEnabled = false;
            this.FindControl<Button> ("btnPlayStep").IsEnabled = true;

            await Instance.Scenario?.PauseStep ();

            this.FindControl<Label> ("lblTimerStep").Content = Instance.Language.Localize ("PE:ProgressionPaused");
        }

        private async Task PlayStep () {
            this.FindControl<Button> ("btnPauseStep").IsEnabled = true;
            this.FindControl<Button> ("btnPlayStep").IsEnabled = false;

            await Instance.Scenario?.PlayStep ();

            Label lblTimerStep = this.FindControl<Label> ("lblTimerStep");

            if (Instance.Scenario?.Current?.ProgressTimer == -1)
                lblTimerStep.Content = Instance.Language.Localize ("PE:ProgressionManual");
            else
                lblTimerStep.Content = String.Format ("{0} {1} {2}",
                    Instance.Language.Localize ("PE:ProgressionAutomatic"),
                    Instance.Scenario?.Current?.ProgressTimer - (Instance.Scenario?.ProgressTimer.Elapsed / 1000),
                    Instance.Language.Localize ("PE:ProgressionSeconds"));
        }

        private async Task ResetPatientParameters () {
            await RefreshPatient ();
        }

        private async Task ApplyPatientParameters () {
            ComboBox comboCardiacRhythm = this.FindControl<ComboBox> ("comboCardiacRhythm");
            ComboBox comboRespiratoryRhythm = this.FindControl<ComboBox> ("comboRespiratoryRhythm");
            ComboBox comboPACatheterPlacement = this.FindControl<ComboBox> ("comboPACatheterPlacement");
            ComboBox comboCardiacAxis = this.FindControl<ComboBox> ("comboCardiacAxis");
            ComboBox comboFHRVariability = this.FindControl<ComboBox> ("comboFHRVariability");
            ComboBox comboUCIntensity = this.FindControl<ComboBox> ("comboUCIntensity");
            ListBox listFHRRhythms = this.FindControl<ListBox> ("listFHRRhythms");

            List<FHRAccelDecels.Values> FHRRhythms = new ();

            foreach (ListBoxItem lbi in listFHRRhythms.SelectedItems) {
                if (lbi.Tag != null)
                    FHRRhythms.Add ((FHRAccelDecels.Values)Enum.Parse (typeof (FHRAccelDecels.Values), (string)lbi.Tag));
            }

            await Instance.Patient.UpdateParameters (

                // Basic vital signs
                (int)(this.FindControl<NumericUpDown> ("numHR")?.Value ?? 0),

                (int)(this.FindControl<NumericUpDown> ("numNSBP")?.Value ?? 0),
                (int)(this.FindControl<NumericUpDown> ("numNDBP")?.Value ?? 0),
                Patient.CalculateMAP ((int)(this.FindControl<NumericUpDown> ("numNSBP")?.Value ?? 0), (int)(this.FindControl<NumericUpDown> ("numNDBP")?.Value ?? 0)),

                (int)(this.FindControl<NumericUpDown> ("numRR")?.Value ?? 0),
                (int)(this.FindControl<NumericUpDown> ("numSPO2")?.Value ?? 0),
                (double)(this.FindControl<NumericUpDown> ("numT")?.Value ?? 0),

                (Cardiac_Rhythms.Values)(Enum.GetValues (typeof (Cardiac_Rhythms.Values)).GetValue (
                    comboCardiacRhythm.SelectedIndex < 0 ? 0 : comboCardiacRhythm.SelectedIndex)
                    ?? Cardiac_Rhythms.Values.Sinus_Rhythm),
                (Respiratory_Rhythms.Values)(Enum.GetValues (typeof (Respiratory_Rhythms.Values)).GetValue (
                    comboRespiratoryRhythm.SelectedIndex < 0 ? 0 : comboRespiratoryRhythm.SelectedIndex)
                    ?? Respiratory_Rhythms.Values.Regular),

                // Advanced hemodynamics
                (int)(this.FindControl<NumericUpDown> ("numETCO2")?.Value ?? 0),
                (int)(this.FindControl<NumericUpDown> ("numCVP")?.Value ?? 0),

                (int)(this.FindControl<NumericUpDown> ("numASBP")?.Value ?? 0),
                (int)(this.FindControl<NumericUpDown> ("numADBP")?.Value ?? 0),
                Patient.CalculateMAP ((int)(this.FindControl<NumericUpDown> ("numASBP")?.Value ?? 0), (int)(this.FindControl<NumericUpDown> ("numADBP")?.Value ?? 0)),

                (float)(this.FindControl<NumericUpDown> ("numCO")?.Value ?? 0),

                (PulmonaryArtery_Rhythms.Values)(Enum.GetValues (typeof (PulmonaryArtery_Rhythms.Values)).GetValue (
                    comboPACatheterPlacement.SelectedIndex < 0 ? 0 : comboPACatheterPlacement.SelectedIndex)
                    ?? PulmonaryArtery_Rhythms.Values.Right_Atrium),

                (int)(this.FindControl<NumericUpDown> ("numPSP")?.Value ?? 0),
                (int)(this.FindControl<NumericUpDown> ("numPDP")?.Value ?? 0),
                Patient.CalculateMAP ((int)(this.FindControl<NumericUpDown> ("numPSP")?.Value ?? 0),
                (int)(this.FindControl<NumericUpDown> ("numPDP")?.Value ?? 0)),

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

                (Cardiac_Axes.Values)(Enum.GetValues (typeof (Cardiac_Axes.Values)).GetValue (
                    comboCardiacAxis.SelectedIndex < 0 ? 0 : comboCardiacAxis.SelectedIndex)
                    ?? Cardiac_Axes.Values.Normal),

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
                (Scales.Intensity.Values)(Enum.GetValues (typeof (Scales.Intensity.Values)).GetValue (
                    comboFHRVariability.SelectedIndex < 0 ? 0 : comboFHRVariability.SelectedIndex)
                    ?? Scales.Intensity.Values.Absent),
                FHRRhythms,
                (float)(this.FindControl<NumericUpDown> ("numUCFrequency")?.Value ?? 0),
                (int)(this.FindControl<NumericUpDown> ("numUCDuration")?.Value ?? 0),
                (Scales.Intensity.Values)(Enum.GetValues (typeof (Scales.Intensity.Values)).GetValue (
                    comboUCIntensity.SelectedIndex < 0 ? 0 : comboUCIntensity.SelectedIndex)
                    ?? Scales.Intensity.Values.Absent)
            );

            await Instance.Mirror.PostPatient (Instance.Patient, Instance.Server);

            await AdvanceParameterStatus (ParameterStatuses.ChangesApplied);
        }

        private void MenuNewSimulation_Click (object sender, RoutedEventArgs e)
            => _ = RefreshScenario (true);

        private void MenuLoadFile_Click (object s, RoutedEventArgs e)
            => _ = LoadFile ();

        private void MenuSaveFile_Click (object s, RoutedEventArgs e)
            => _ = SaveFile ();

        private void MenuExit_Click (object s, RoutedEventArgs e)
            => _ = Exit ();

        private void MenuToggleAudio_Click (object s, RoutedEventArgs e)
            => _ = ToggleAudio ();

        private void MenuSetLanguage_Click (object s, RoutedEventArgs e)
            => _ = DialogLanguage (true);

        private void MenuMirrorDeactivate_Click (object s, RoutedEventArgs e)
            => _ = MirrorDeactivate ();

        private void MenuMirrorBroadcast_Click (object s, RoutedEventArgs e)
            => _ = MirrorBroadcast ();

        private void MenuMirrorReceive_Click (object s, RoutedEventArgs e)
            => _ = MirrorReceive ();

        private void MenuAbout_Click (object s, RoutedEventArgs e)
            => _ = DialogAbout ();

        private void MenuCheckUpdate_Click (object s, RoutedEventArgs e)
            => _ = CheckUpgrade ();

        private void MenuUpdate_Click (object s, RoutedEventArgs e)
            => _ = OpenUpgrade ();

        private void ButtonDeviceMonitor_Click (object s, RoutedEventArgs e)
            => _ = InitDeviceMonitor ();

        private void ButtonDeviceDefib_Click (object s, RoutedEventArgs e)
            => _ = InitDeviceDefib ();

        private void ButtonDeviceECG_Click (object s, RoutedEventArgs e)
            => _ = InitDeviceECG ();

        private void ButtonDeviceIABP_Click (object s, RoutedEventArgs e)
            => _ = InitDeviceIABP ();

        private void ButtonDeviceEFM_Click (object s, RoutedEventArgs e)
            => _ = InitDeviceEFM ();

        private void ButtonPreviousStep_Click (object s, RoutedEventArgs e)
            => _ = PreviousStep ();

        private void ButtonNextStep_Click (object s, RoutedEventArgs e)
            => _ = NextStep ();

        private void ButtonPauseStep_Click (object s, RoutedEventArgs e)
            => _ = PauseStep ();

        private void ButtonPlayStep_Click (object s, RoutedEventArgs e)
            => _ = PlayStep ();

        private void ButtonResetParameters_Click (object s, RoutedEventArgs e)
            => _ = ResetPatientParameters ();

        private void ButtonApplyParameters_Click (object sender, RoutedEventArgs e)
            => _ = ApplyPatientParameters ();

        private void OnActivated (object sender, EventArgs e) {
            if (!IsUILoadCompleted) {
                this.Position = new PixelPoint (Instance?.Settings.WindowPosition.X ?? 0, Instance?.Settings.WindowPosition.Y ?? 0);
                this.IsUILoadCompleted = true;
            }
        }

        private void OnClosed (object sender, EventArgs e)
            => _ = Exit ();

        private void OnLayoutUpdated (object sender, EventArgs e) {
            if (!IsUILoadCompleted) {
                this.Width = Instance?.Settings.WindowSize.X ?? 0;
                this.Height = Instance?.Settings.WindowSize.Y ?? 0;
            } else if (Instance is not null) {
                Instance.Settings.WindowSize.X = (int)this.Width;
                Instance.Settings.WindowSize.Y = (int)this.Height;
            }
        }

        private void OnAutoApplyChanges_Changed (object sender, RoutedEventArgs e) {
            if (ParameterStatus == ParameterStatuses.Loading || Instance is null)
                return;

            Instance.Settings.AutoApplyChanges = this.FindControl<CheckBox> ("chkAutoApplyChanges").IsChecked ?? true;
            Instance.Settings.Save ();

            _ = SetParameterStatus (Instance.Settings.AutoApplyChanges);
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
                    _ = AdvanceParameterStatus (ParameterStatuses.ChangesPending);
                    break;

                case ParameterStatuses.AutoApply:
                    _ = ApplyPatientParameters ();
                    _ = UpdateParameterIndicators ();
                    break;
            }
        }

        private void OnCardiacRhythm_Selected (object sender, SelectionChangedEventArgs e) {
            if (!this.FindControl<CheckBox> ("checkDefaultVitals").IsChecked ?? false || Instance.Patient == null)
                return;

            int si = this.FindControl<ComboBox> ("comboCardiacRhythm").SelectedIndex;
            Array ev = Enum.GetValues (typeof (Cardiac_Rhythms.Values));
            if (si < 0 || si > ev.Length - 1)
                return;

            Cardiac_Rhythms.Default_Vitals v = Cardiac_Rhythms.DefaultVitals (
                (Cardiac_Rhythms.Values)(ev.GetValue (si) ?? Cardiac_Rhythms.Values.Sinus_Rhythm));

            this.FindControl<NumericUpDown> ("numHR").Value = (int)global::II.Math.Clamp ((double)(this.FindControl<NumericUpDown> ("numHR")?.Value ?? 0), v.HRMin, v.HRMax);
            this.FindControl<NumericUpDown> ("numNSBP").Value = (int)global::II.Math.Clamp ((double)(this.FindControl<NumericUpDown> ("numNSBP")?.Value ?? 0), v.SBPMin, v.SBPMax);
            this.FindControl<NumericUpDown> ("numNDBP").Value = (int)global::II.Math.Clamp ((double)(this.FindControl<NumericUpDown> ("numNDBP")?.Value ?? 0), v.DBPMin, v.DBPMax);
            this.FindControl<NumericUpDown> ("numRR").Value = (int)global::II.Math.Clamp ((double)(this.FindControl<NumericUpDown> ("numRR")?.Value ?? 0), v.RRMin, v.RRMax);
            this.FindControl<NumericUpDown> ("numSPO2").Value = (int)global::II.Math.Clamp ((double)(this.FindControl<NumericUpDown> ("numSPO2")?.Value ?? 0), v.SPO2Min, v.SPO2Max);
            this.FindControl<NumericUpDown> ("numETCO2").Value = (int)global::II.Math.Clamp ((double)(this.FindControl<NumericUpDown> ("numETCO2")?.Value ?? 0), v.ETCO2Min, v.ETCO2Max);
            this.FindControl<NumericUpDown> ("numASBP").Value = (int)global::II.Math.Clamp ((double)(this.FindControl<NumericUpDown> ("numASBP")?.Value ?? 0), v.SBPMin, v.SBPMax);
            this.FindControl<NumericUpDown> ("numADBP").Value = (int)global::II.Math.Clamp ((double)(this.FindControl<NumericUpDown> ("numADBP")?.Value ?? 0), v.DBPMin, v.DBPMax);
            this.FindControl<NumericUpDown> ("numPSP").Value = (int)global::II.Math.Clamp ((double)(this.FindControl<NumericUpDown> ("numPSP")?.Value ?? 0), v.PSPMin, v.PSPMax);
            this.FindControl<NumericUpDown> ("numPDP").Value = (int)global::II.Math.Clamp ((double)(this.FindControl<NumericUpDown> ("numPDP")?.Value ?? 0), v.PDPMin, v.PDPMax);

            OnUIPatientParameter_Process (sender, e);
        }

        private void OnRespiratoryRhythm_Selected (object sender, SelectionChangedEventArgs e) {
            if (!this.FindControl<CheckBox> ("checkDefaultVitals")?.IsChecked ?? false || Instance?.Patient == null)
                return;

            int si = this.FindControl<ComboBox> ("comboRespiratoryRhythm").SelectedIndex;
            Array ev = Enum.GetValues (typeof (Respiratory_Rhythms.Values));
            if (si < 0 || si > ev.Length - 1)
                return;

            Respiratory_Rhythms.Default_Vitals v = Respiratory_Rhythms.DefaultVitals (
                (Respiratory_Rhythms.Values)(ev.GetValue (si) ?? Respiratory_Rhythms.Values.Regular));

            NumericUpDown numRR = this.FindControl<NumericUpDown> ("numRR");
            NumericUpDown numInspiratoryRatio = this.FindControl<NumericUpDown> ("numInspiratoryRatio");
            NumericUpDown numExpiratoryRatio = this.FindControl<NumericUpDown> ("numExpiratoryRatio");

            numRR.Value = (int)global::II.Math.Clamp ((double)(numRR?.Value ?? 0), v.RRMin, v.RRMax);
            numInspiratoryRatio.Value = (int)global::II.Math.Clamp ((double)(numInspiratoryRatio?.Value ?? 0), v.RR_IE_I_Min, v.RR_IE_I_Max);
            numExpiratoryRatio.Value = (int)global::II.Math.Clamp ((double)(numExpiratoryRatio?.Value ?? 0), v.RR_IE_E_Min, v.RR_IE_E_Max);

            OnUIPatientParameter_Process (sender, e);
        }

        private void OnPulmonaryArteryRhythm_Selected (object sender, SelectionChangedEventArgs e) {
            if (Instance?.Patient is null)
                return;

            int si = this.FindControl<ComboBox> ("comboPACatheterPlacement").SelectedIndex;
            Array ev = Enum.GetValues (typeof (PulmonaryArtery_Rhythms.Values));
            if (si < 0 || si > ev.Length - 1)
                return;

            PulmonaryArtery_Rhythms.Default_Vitals v = PulmonaryArtery_Rhythms.DefaultVitals (
                (PulmonaryArtery_Rhythms.Values)(ev.GetValue (si) ?? PulmonaryArtery_Rhythms.Values.Right_Atrium));

            NumericUpDown numPSP = this.FindControl<NumericUpDown> ("numPSP");
            NumericUpDown numPDP = this.FindControl<NumericUpDown> ("numPDP");

            numPSP.Value = (int)global::II.Math.Clamp ((double)(numPSP?.Value ?? 0), v.PSPMin, v.PSPMax);
            numPDP.Value = (int)global::II.Math.Clamp ((double)(numPDP?.Value ?? 0), v.PDPMin, v.PDPMax);

            OnUIPatientParameter_Process (sender, e);
        }

        private void ForceUpdateFields (Patient? p) {
            Dispatcher.UIThread.InvokeAsync (() => {                        // Updating the UI requires being on the proper thread
                ParameterStatus = ParameterStatuses.Loading;                // To prevent each form update from auto-applying back to Patient
                FormUpdateFields (this, new Patient.PatientEventArgs (Instance?.Patient, Patient.PatientEventTypes.Vitals_Change));
                _ = SetParameterStatus (Instance?.Settings.AutoApplyChanges ?? false);     // Re-establish parameter status
            });
        }

        private void FormUpdateFields (object? sender, Patient.PatientEventArgs e) {
            if (e.Patient is null)
                return;

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

                if (e.Patient.ST_Elevation is not null) {
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
                }

                if (e.Patient.T_Elevation is not null) {
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
                }

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
                        if (lbi.Tag != null && (string)lbi.Tag == fhr_rhythm.ToString ())
                            listFHRRhythms.SelectedItems.Add (lbi);
                    }
                }
            }
        }
    }
}