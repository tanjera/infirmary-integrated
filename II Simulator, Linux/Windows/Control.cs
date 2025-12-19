using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Cairo;
using GLib;
using Gtk;
using Pango;

using II;
using II.Localization;
using II.Server;
using Application = Gtk.Application;
using DateTime = System.DateTime;
using Menu = Gtk.Menu;
using MenuItem = Gtk.MenuItem;
using Task = System.Threading.Tasks.Task;

namespace IISIM
{
    class Control : Window
    {
        private App Instance;
        
        private bool HideDeviceLabels = false;
        private bool IsUILoadCompleted = false;

        /* Buffers for ViewModel handling and temporal smoothing of upstream Model data changes */
        private Physiology? ApplyBuffer;

        private bool ApplyPending_Cardiac = false,
            ApplyPending_Respiratory = false,
            ApplyPending_Obstetric = false;

        private II.Timer ApplyTimer_Cardiac = new (),
            ApplyTimer_Respiratory = new (),
            ApplyTimer_Obstetric = new ();

        /* Variables for Auto-Apply functionality */
        private ParameterStatuses ParameterStatus = ParameterStatuses.Loading;

        private enum ParameterStatuses {
            Loading,
            AutoApply,
            ChangesPending,
            ChangesApplied
        }

        /* GTK GUI Objects */
        
        MenuBar mbMain = new MenuBar ();
        Menu muFile = new Menu ();
        Menu muOptions = new Menu ();
        Menu muMirror = new Menu ();
        Menu muSettings = new Menu ();
        Menu muHelp = new Menu ();
        MenuItem miFile = new MenuItem ();
        MenuItem miFileNew = new MenuItem ();
        MenuItem miFileLoad = new MenuItem ();
        MenuItem miFileSave = new MenuItem ();
        MenuItem miFileFullScreen = new MenuItem ();
        MenuItem miFileExit = new MenuItem ();
        MenuItem miOptions = new MenuItem ();
        MenuItem miOptionsPause = new MenuItem ();
        MenuItem miMirror = new MenuItem ();
        MenuItem miMirrorStatus = new MenuItem ();
        MenuItem miMirrorDeactivate = new MenuItem ();
        MenuItem miMirrorReceive = new MenuItem ();
        MenuItem miMirrorBroadcast = new MenuItem ();
        MenuItem miSettings = new MenuItem ();
        MenuItem miSettingsAudio = new MenuItem ();
        MenuItem miSettingsLanguage = new MenuItem ();
        MenuItem miHelp = new MenuItem ();
        MenuItem miHelpUpdate = new MenuItem ();
        MenuItem miHelpAbout = new MenuItem ();
        
        private SpinButton numHR = new SpinButton (0, 300, 5);
        private SpinButton numNSBP = new SpinButton (0, 300, 5);
        private SpinButton numNDBP = new SpinButton (0, 300, 5);
        private SpinButton numRR = new SpinButton (0, 100, 2);
        private SpinButton numSPO2 = new SpinButton (0, 100, 2);
        private SpinButton numT = new SpinButton (0, 100, 0.2);
        private ComboBox cmbCardiacRhythm = new ComboBox ();
        private CheckButton chkDefaultVitals = new CheckButton ();

        private SpinButton numCVP = new SpinButton (-20, 100, 1);
        private SpinButton numASBP = new SpinButton (0, 300, 5);
        private SpinButton numADBP = new SpinButton (0, 300, 5);
        private SpinButton numCO = new SpinButton (0, 20, 0.1);
        private ComboBox comboPACatheterPlacement = new ComboBox ();
        private SpinButton numPSP = new SpinButton (-20, 200, 2);
        private SpinButton numPDP = new SpinButton (-20, 200, 2);
        private SpinButton numICP = new SpinButton (-50, 50, 1);
        private SpinButton numIAP = new SpinButton (-50, 50, 1);
        
        private SpinButton numPacemakerCaptureThreshold = new SpinButton (0, 200, 5);
        private CheckButton chkPulsusParadoxus = new CheckButton ();
        private CheckButton chkPulsusAlternans = new CheckButton ();
        private CheckButton chkElectricalAlternans = new CheckButton ();
        private ComboBox comboCardiacAxis = new ComboBox ();

        private SpinButton numQRSInterval = new SpinButton (0.04, 0.4, 0.02);
        private SpinButton numQTcInterval = new SpinButton (0.2, 0.8, 0.02);
        
        private SpinButton numSTE_I = new SpinButton (-1.0, 1.0, 0.1);
        private SpinButton numSTE_II = new SpinButton (-1.0, 1.0, 0.1);
        private SpinButton numSTE_III = new SpinButton (-1.0, 1.0, 0.1);
        private SpinButton numSTE_aVR = new SpinButton (-1.0, 1.0, 0.1);
        private SpinButton numSTE_aVL = new SpinButton (-1.0, 1.0, 0.1);
        private SpinButton numSTE_aVF = new SpinButton (-1.0, 1.0, 0.1);
        private SpinButton numSTE_V1 = new SpinButton (-1.0, 1.0, 0.1);
        private SpinButton numSTE_V2 = new SpinButton (-1.0, 1.0, 0.1);
        private SpinButton numSTE_V3 = new SpinButton (-1.0, 1.0, 0.1);
        private SpinButton numSTE_V4 = new SpinButton (-1.0, 1.0, 0.1);
        private SpinButton numSTE_V5 = new SpinButton (-1.0, 1.0, 0.1);
        private SpinButton numSTE_V6 = new SpinButton (-1.0, 1.0, 0.1);
        
        private SpinButton numTWE_I = new SpinButton (-1.0, 1.0, 0.1);
        private SpinButton numTWE_II = new SpinButton (-1.0, 1.0, 0.1);
        private SpinButton numTWE_III = new SpinButton (-1.0, 1.0, 0.1);
        private SpinButton numTWE_aVR = new SpinButton (-1.0, 1.0, 0.1);
        private SpinButton numTWE_aVL = new SpinButton (-1.0, 1.0, 0.1);
        private SpinButton numTWE_aVF = new SpinButton (-1.0, 1.0, 0.1);
        private SpinButton numTWE_V1 = new SpinButton (-1.0, 1.0, 0.1);
        private SpinButton numTWE_V2 = new SpinButton (-1.0, 1.0, 0.1);
        private SpinButton numTWE_V3 = new SpinButton (-1.0, 1.0, 0.1);
        private SpinButton numTWE_V4 = new SpinButton (-1.0, 1.0, 0.1);
        private SpinButton numTWE_V5 = new SpinButton (-1.0, 1.0, 0.1);
        private SpinButton numTWE_V6 = new SpinButton (-1.0, 1.0, 0.1);

        private ComboBox comboRespiratoryRhythm = new ComboBox ();
        private SpinButton numETCO2 = new SpinButton (0, 100, 2);
        private CheckButton chkMechanicallyVentilated = new CheckButton ();
        private SpinButton numInspiratoryRatio = new SpinButton (0.1, 10, 0.1);
        private SpinButton numExpiratoryRatio = new SpinButton (0.1, 10, 0.1);
        
        private SpinButton numFHR = new SpinButton (0, 500, 5);
        private SpinButton numFHRVariability = new SpinButton (0, 80, 5);
        private SpinButton numUCFrequency = new SpinButton (0, 600, 30);
        private SpinButton numUCDuration = new SpinButton (30, 600, 10);
        private SpinButton numUCIntensity = new SpinButton (0, 10, 5);
        private SpinButton numUCResting = new SpinButton (0, 100, 5);
        private ComboBox comboFHRRhythm = new ComboBox ();

        
        public Control (App inst, Window splash) : base (Gtk.WindowType.Toplevel) {
            
            Instance = inst;

            DeleteEvent += OnClose;
            this.Shown += OnShown;
            splash.Hidden += OnSplashed;
        }

        private void OnShown (object sender, EventArgs args) {
            /* Init essential functions first */
            InitInterface ();
            InitMirroring ();
            InitScenario (true);
            InitTimers ();

            Application.Invoke ((sender, args) => {
                /* Init important but non-essential functions */
                if (Instance.StartArgs?.Length > 0) {
                    string loadfile = Instance.StartArgs [0].Trim (' ', '\n', '\r');
                    if (!String.IsNullOrEmpty (loadfile)) {
                        // TODO: Implement: LoadOpen (loadfile);
                    }
                }

                /* Update UI from loading functionality */
                Task.WaitAll(SetParameterStatus (Instance?.Settings.AutoApplyChanges ?? false));
            });
        }
        
        private void OnSplashed (object o, EventArgs args) {
            InitInitialRun ();

            /* Run useful but otherwise vanity functions last */
            Task.Run (InitUpgrade);
        }
        
        private void OnClose(object sender, DeleteEventArgs a) {
            Application.Quit();
        }
        
        private void InitInitialRun () {
            if (!II.Settings.Simulator.Exists () || !(Instance?.Settings.AcceptedEULA ?? false)) {
                DialogEULA ();
            }   
        }

        private void InitInterface () {
            int sp = 2;                 // Spacing: General (int)
            uint usp = 3;               // Spacing: General (uint)
            uint upd = 5;               // Padding: General (uint)
            uint tlepd = 2;             // Padding: Top-level elements
            
            /* GUI: Top-Level Elements; 1 [ 2 ] */
            
            Title = Instance.Language.Localize ("PE:WindowTitle");
            VBox vbMain1 = new VBox (false, sp);
            vbMain1.MarginBottom = 5;
            
            HBox hbMain2 = new HBox (false, sp);
            hbMain2.BorderWidth = usp;
            
            /* GUI: Menu Bar */
            
            mbMain.MarginBottom = 2;
            mbMain.PackDirection = PackDirection.Ltr;

            /* GUI: Menu Bar: Labels */
            
            miFile.Label = Instance.Language.Localize ("PE:MenuFile").Replace ("_", "");
            miFileNew.Label = Instance.Language.Localize ("PE:MenuNewFile").Replace ("_", "");
            miFileLoad.Label = Instance.Language.Localize ("PE:MenuLoadSimulation").Replace ("_", "");
            miFileSave.Label = Instance.Language.Localize ("PE:MenuSaveSimulation").Replace ("_", "");
            miFileFullScreen.Label = Instance.Language.Localize ("MENU:MenuToggleFullscreen").Replace ("_", "");
            miFileExit.Label = Instance.Language.Localize ("PE:MenuExitProgram").Replace ("_", "");
             
            miOptions.Label = Instance.Language.Localize ("PE:MenuOptions").Replace ("_", "");
            miOptionsPause.Label = Instance.Language.Localize ("PE:MenuPause").Replace ("_", "");
             
            miMirror.Label = Instance.Language.Localize ("PE:MenuMirror").Replace ("_", "");
            miMirrorStatus.Label = (Instance?.Mirror.Status) switch {
                Mirror.Statuses.HOST =>
                    $"{Instance?.Language.Localize ("MIRROR:Status")}: {Instance?.Language.Localize ("MIRROR:Server")}",
                Mirror.Statuses.CLIENT =>
                    $"{Instance?.Language.Localize ("MIRROR:Status")}: {Instance?.Language.Localize ("MIRROR:Client")}",
                _ =>
                    $"{Instance?.Language.Localize ("MIRROR:Status")}: {Instance?.Language.Localize ("MIRROR:Inactive")}"
            };
            
            miMirrorDeactivate.Label = Instance.Language.Localize ("PE:MenuMirrorDeactivate").Replace ("_", "");
            miMirrorReceive.Label = Instance.Language.Localize ("PE:MenuMirrorReceive").Replace ("_", "");
            miMirrorBroadcast.Label = Instance.Language.Localize ("PE:MenuMirrorBroadcast").Replace ("_", "");
             
            miSettings.Label = Instance.Language.Localize ("PE:MenuOptions").Replace ("_", "");
            miSettingsAudio.Label = String.Format ("{0}: {1}",
                Instance.Language.Localize ("PE:MenuToggleAudio").Replace ("_", ""),
                Instance.Settings.AudioEnabled ? Instance.Language.Localize ("BOOLEAN:On") : Instance.Language.Localize ("BOOLEAN:Off"));
            miSettingsLanguage.Label = Instance.Language.Localize ("PE:MenuSetLanguage").Replace ("_", "");
             
            miHelp.Label = Instance.Language.Localize ("PE:MenuHelp").Replace ("_", "");
            miHelpUpdate.Label = Instance.Language.Localize ("PE:MenuCheckUpdates").Replace ("_", "");
            miHelpAbout.Label = Instance.Language.Localize ("PE:MenuAboutProgram").Replace ("_", "");
            
            /* GUI: Event Linking */

            miFileNew.Activated += MenuNewSimulation_Click;
            miFileLoad.Activated += MenuLoadFile_Click;
            miFileSave.Activated += MenuSaveFile_Click;
            miFileFullScreen.Activated += MenuToggleFullscreen_Click;
            miFileExit.Activated += MenuExit_Click;

            miOptionsPause.Activated += MenuPauseSimulation_Click;

            miMirrorDeactivate.Activated += MenuMirrorDeactivate_Click;
            miMirrorReceive.Activated += MenuMirrorReceive_Click;
            miMirrorBroadcast.Activated += MenuMirrorBroadcast_Click;

            miSettingsAudio.Activated += MenuToggleAudio_Click;
            miSettingsLanguage.Activated += MenuSetLanguage_Click;

            miHelpUpdate.Activated += MenuCheckUpdate_Click;
            miHelpAbout.Activated += MenuAbout_Click;
            
            /* GUI: Menu Bar: Building */
            
            miFile.Submenu = muFile;
            muFile.Append (miFileNew);
            muFile.Append (miFileLoad);
            muFile.Append (miFileSave);
            muFile.Append (miFileFullScreen);
            muFile.Append (miFileExit);
            mbMain.Append(miFile);
            
            miOptions.Submenu = muOptions;
            muOptions.Append (miOptionsPause);
            mbMain.Append(miOptions);
            
            miMirror.Submenu = muMirror;
            muMirror.Append (miMirrorStatus);
            muMirror.Append (miMirrorDeactivate);
            muMirror.Append (miMirrorReceive);
            muMirror.Append (miMirrorBroadcast);
            mbMain.Append(miMirror);
            
            miSettings.Submenu = muSettings;
            muSettings.Append (miSettingsAudio);
            muSettings.Append (miSettingsLanguage);
            mbMain.Append(miSettings);
            
            miHelp.Submenu = muHelp;
            muHelp.Append (miHelpUpdate);
            muHelp.Append (miHelpAbout);
            mbMain.Append(miHelp);
            
            /* GUI: Devices Section */
            
            VBox vboxDevices = new VBox (false, sp);
            
            Label lblDevices = new Label ();
            lblDevices.Halign = Align.Start;
            lblDevices.UseMarkup = true;
            lblDevices.Markup = $"<span size='12pt' weight='bold'>{Instance.Language.Localize ("PE:Devices")}</span>";
            vboxDevices.PackStart (lblDevices, false, false, upd);
            
            HBox hboxDeviceCardiacMonitor = new HBox (false, sp);
            hboxDeviceCardiacMonitor.PackStart (
                new Image(new Gdk.Pixbuf("Third_Party/Icon_DeviceMonitor_128.png", 40, 40, true))
                , false, false, upd);
            hboxDeviceCardiacMonitor.PackStart(new Label(Instance.Language.Localize ("PE:CardiacMonitor")), false, false, 6);
            
            HBox hboxDeviceDefibrillator = new HBox (false, sp);
            hboxDeviceDefibrillator.PackStart (
                new Image(new Gdk.Pixbuf("Third_Party/Icon_DeviceDefibrillator_128.png", 40, 40, true))
                , false, false, upd);
            hboxDeviceDefibrillator.PackStart(new Label(Instance.Language.Localize ("PE:Defibrillator")), false, false, 6);
            
            HBox hbox12LeadECG = new HBox (false, sp);
            hbox12LeadECG.PackStart (
                new Image(new Gdk.Pixbuf("Third_Party/Icon_Device12LeadECG_128.png", 40, 40, true))
                , false, false, upd);
            hbox12LeadECG.PackStart(new Label(Instance.Language.Localize ("PE:12LeadECG")), false, false, 6);
            
            vboxDevices.PackStart (hboxDeviceCardiacMonitor, false, false, upd);
            vboxDevices.PackStart (hboxDeviceDefibrillator, false, false, upd);
            vboxDevices.PackStart (hbox12LeadECG, false, false, upd);
            
            /* GUI: Parameters */
            
            Grid gridParameters = new Grid ();
            
            gridParameters.RowSpacing = usp;
            gridParameters.ColumnSpacing = usp;
            
            /* GUI: Parameters: Vital Signs */
     
            Label lblHR = new Label ($"{Instance.Language.Localize ("PE:HeartRate")}:");
            lblHR.Halign = Align.Start;
            gridParameters.Attach (lblHR, 0, 0, 1, 1);
            gridParameters.Attach (numHR, 1, 0, 3, 1);
            
            Label lblNIBP = new Label ($"{Instance.Language.Localize ("PE:BloodPressure")}:");
            lblNIBP.Halign = Align.Start;
            
            gridParameters.Attach (lblNIBP, 0, 1, 1, 1);
            gridParameters.Attach (numNSBP, 1, 1, 1, 1);
            gridParameters.Attach (new Label("/"), 2, 1, 1, 1);
            gridParameters.Attach (numNDBP, 3, 1, 1, 1);
            
            Label lblRR = new Label ($"{Instance.Language.Localize ("PE:RespiratoryRate")}:");
            lblRR.Halign = Align.Start;
            gridParameters.Attach (lblRR, 0, 2, 1, 1);
            gridParameters.Attach (numRR, 1, 2, 3, 1);

            Label lblSPO2 = new Label ($"{Instance.Language.Localize ("PE:PulseOximetry")}:");
            lblSPO2.Halign = Align.Start;
            gridParameters.Attach (lblSPO2, 0, 3, 1, 1);
            gridParameters.Attach (numSPO2, 1, 3, 3, 1);
            
            Label lblT = new Label ($"{Instance.Language.Localize ("PE:Temperature")}:");
            lblT.Halign = Align.Start;
            gridParameters.Attach (lblT, 0, 4, 1, 1);
            gridParameters.Attach (numT, 1, 4, 3, 1);

            Label lblCardiacRhythm = new Label ($"{Instance.Language.Localize ("PE:CardiacRhythm")}:");
            lblCardiacRhythm.Halign = Align.Start;
            gridParameters.Attach (lblCardiacRhythm, 0, 5, 1, 1);

            List<string> listCardiacRhythm = new List<string> ();
            foreach (Cardiac_Rhythms.Values v in Enum.GetValues (typeof(Cardiac_Rhythms.Values)))
                listCardiacRhythm.Add (Instance.Language.Localize (Cardiac_Rhythms.LookupString (v)));
            cmbCardiacRhythm = new ComboBox(listCardiacRhythm.ToArray ());
            gridParameters.Attach (cmbCardiacRhythm, 1, 5, 3, 1);
            
            chkDefaultVitals = new CheckButton (Instance.Language.Localize ("PE:UseDefaultVitalSignRanges"));
            gridParameters.Attach (chkDefaultVitals, 0, 6, 4, 1);
            
            
            hbMain2.PackStart (vboxDevices, false, false, 10);
            hbMain2.PackStart (new Separator(Orientation.Vertical), false, false, 0);

            Label lblVitalSigns = new Label ();
            lblVitalSigns.UseMarkup = true;
            lblVitalSigns.Markup = $"<span size='12pt' weight='bold'>  {Instance.Language.Localize ("PE:VitalSigns")}</span>";
            lblVitalSigns.HeightRequest = 34;
            
            Expander expVitalSigns = new Expander ("Test");
            expVitalSigns.LabelWidget = lblVitalSigns;
            expVitalSigns.Add(gridParameters);
            expVitalSigns.Expanded = true;
            
            hbMain2.PackStart (expVitalSigns, false, false, upd);
            
            /* GUI: Assemble... */
            
            vbMain1.PackStart (mbMain, false, true, tlepd);
            vbMain1.PackStart (hbMain2, false, false, tlepd);
            Add (vbMain1);
            ShowAll ();
            
            // TODO: Implement: Hotkeys!!
        }

        
        private async Task InitUpgrade () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitUpgrade)}");
                return;
            }

            // Newer version available? Check Server, populate status bar, prompt user for upgrade
            await Instance.Server.Get_LatestVersion ();

            string version = Assembly.GetExecutingAssembly ()?.GetName ()?.Version?.ToString (3) ?? "0.0.0";
            bool upgradeAvailable = Utility.IsNewerVersion (version, Instance.Server.UpgradeVersion);

            if (!upgradeAvailable) {    // If no update available, no status update; remove any notification muting
                Instance.Settings.MuteUpgrade = false;
                Instance.Settings.Save ();
                return;
            }

            if (Instance.Settings.MuteUpgrade) {
                if (DateTime.Compare (Instance.Settings.MuteUpgradeDate, DateTime.Now - new TimeSpan (30, 0, 0, 0)) < 0) {
                    Instance.Settings.MuteUpgrade = false;              // Reset the notification mute every 30 days
                    Instance.Settings.Save ();
                } else {        // Mutes update popup notification
                    return;
                }
            }

            // Show the upgrade dialog to the user
            await DialogUpgrade ();
        }

        private void InitMirroring () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitMirroring)}");
                return;
            }

            Instance.Timer_Main.Elapsed += Instance.Mirror.ProcessTimer;
            Instance.Mirror.timerUpdate.Tick += OnMirrorTick;

            Task.WhenAll (
                Instance.Mirror.timerUpdate.ResetStart (5000),
                UpdateMirrorStatus ()
            );
        }

        private void InitScenario (bool toInit) {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitScenario)}");
                return;
            }

            Instance.Scenario = new Scenario (toInit);
            Instance.Scenario.StepChangeRequest += OnStepChangeRequest;     // Allows unlinking of Timers immediately prior to Step change
            Instance.Scenario.StepChanged += OnStepChanged;                 // Updates IIApp.Patient, allows PatientEditor UI to update
            Instance.Timer_Main.Elapsed += Instance.Scenario.ProcessTimer;

            if (toInit)         // If toInit is false, Patient is null- InitPatient() will need to be called manually
                InitScenarioStep ();
        }

        private async Task UnloadScenario () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (UnloadScenario)}");
                return;
            }

            if (Instance.Scenario != null) {
                Instance.Timer_Main.Elapsed -= Instance.Scenario.ProcessTimer;   // Unlink Scenario from App/Main Timer
                Instance.Scenario.Dispose ();        // Disposes Scenario's events and timer, and all Patients' events and timers
            }
        }

        private void NewScenario () => _ = RefreshScenario (true);

        private async Task RefreshScenario (bool toInit) {
            await UnloadScenario ();
            InitScenario (toInit);

            await UpdateExpanders ();
        }

        private void InitTimers () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitTimers)}");
                return;
            }

            Instance.Timer_Main.Elapsed += ApplyTimer_Cardiac.Process;
            Instance.Timer_Main.Elapsed += ApplyTimer_Respiratory.Process;
            Instance.Timer_Main.Elapsed += ApplyTimer_Obstetric.Process;

            ApplyTimer_Cardiac.Tick += ApplyPhysiologyParameters_Cardiac;
            ApplyTimer_Respiratory.Tick += ApplyPhysiologyParameters_Respiratory;
            ApplyTimer_Obstetric.Tick += ApplyPhysiologyParameters_Obstetric;

            Task.WhenAll (
                ApplyTimer_Cardiac.Set (5000),
                ApplyTimer_Respiratory.Set (5000),
                ApplyTimer_Obstetric.Set (30000)
            );
        }

        private void InitScenarioStep () {
            InitPhysiologyEvents ();
            InitStep ();
        }

        private void InitPhysiologyEvents () {
            if (Instance?.Physiology is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitPhysiologyEvents)}");
                return;
            }

            /* Tie the Patient's Timer to the Main Timer */
            Instance.Physiology.State = Physiology.States.Running;
            Instance.Timer_Main.Elapsed += Instance.Physiology.ProcessTimers;

            /* Tie PatientEvents to the PatientEditor UI! And trigger. */
            Instance.Physiology.PhysiologyEvent += OnPhysiologyEvent;

            /* Tie PatientEvents to each device! So devices change and trace according to the patient! */
            
            /* TODO: Implement!
            if (Instance.Physiology is not null) {
                if (Instance.Device_Monitor is not null)
                    Instance.Physiology.PhysiologyEvent += Instance.Device_Monitor.OnPhysiologyEvent;
                if (Instance.Device_Defib is not null)
                    Instance.Physiology.PhysiologyEvent += Instance.Device_Defib.OnPhysiologyEvent;
                if (Instance.Device_ECG is not null)
                    Instance.Physiology.PhysiologyEvent += Instance.Device_ECG.OnPhysiologyEvent;
                if (Instance.Device_EFM is not null)
                    Instance.Physiology.PhysiologyEvent += Instance.Device_EFM.OnPhysiologyEvent;
                if (Instance.Device_IABP is not null)
                    Instance.Physiology.PhysiologyEvent += Instance.Device_IABP.OnPhysiologyEvent;
            } */

            OnPhysiologyEvent (this, new Physiology.PhysiologyEventArgs (Instance.Physiology, Physiology.PhysiologyEventTypes.Vitals_Change));
        }

        private async Task UnloadPatientEvents () {
            if (Instance?.Physiology is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (UnloadPatientEvents)}");
                return;
            }

            /* Unloading the Patient from the Main Timer also stops all the Patient's Timers
            /* and results in that Patient not triggering PatientEvent's */
            Instance.Timer_Main.Elapsed -= Instance.Physiology.ProcessTimers;

            /* But it's still important to clear PatientEvent subscriptions so they're not adding
            /* as duplicates when InitPatientEvents() is called!! */
            await Instance.Physiology.UnsubscribePhysiologyEvent ();
        }

        private async Task InitDeviceMonitor () {
            // TODO: IMPLEMENT!!
        }

        private async Task InitDeviceECG () {
            // TODO: IMPLEMENT!!
        }

        private async Task InitDeviceDefib () {
            // TODO: IMPLEMENT!!
        }

        private async Task InitDeviceIABP () {
            // TODO: IMPLEMENT!!
        }

        private async Task InitDeviceEFM () {
            // TODO: IMPLEMENT!!
        }

        private void DialogEULA ()
        {
            Application.Invoke((sender, args) =>
            {
                var dlgEULA = new DialogEULA(Instance);
                dlgEULA.TransientFor = this;
                
                Application.AddWindow(dlgEULA);
                
                dlgEULA.SetPosition(WindowPosition.CenterOnParent);
                dlgEULA.KeepAbove = true;
                dlgEULA.Show();
            });
        }

        private async Task DialogLanguage (bool reloadUI = false) {
            Application.Invoke((sender, args) =>
            {
                var dlgLanguage = new DialogLanguage(Instance);
                dlgLanguage.TransientFor = this;
                
                Application.AddWindow(dlgLanguage);
                
                dlgLanguage.SetPosition(WindowPosition.CenterOnParent);
                dlgLanguage.KeepAbove = true;
                dlgLanguage.Show();
            });
        }

        private async Task DialogMirrorBroadcast () {
            // TODO: IMPLEMENT!!
        }

        private async Task DialogMirrorReceive () {
            // TODO: IMPLEMENT!!
        }

        public async Task DialogAbout () {
            // TODO: IMPLEMENT!!
        }

        private async Task DialogUpgrade () {
            // TODO: IMPLEMENT!!
        }

        public async Task ToggleAudio () {
            // TODO: IMPLEMENT!!
        }

        public void SetAudio_On () => _ = SetAudio (true);

        public void SetAudio_Off () => _ = SetAudio (false);

        public async Task SetAudio (bool toSet) {
            // TODO: IMPLEMENT!!
        }

        private async Task ToggleHideDevices () {
            // TODO: IMPLEMENT!!
        }

        private async Task CheckUpgrade () {
            // TODO: IMPLEMENT!!
        }

        private async Task OpenUpgrade () {
            // TODO: IMPLEMENT!!
        }

        private async Task MirrorDeactivate () {
            // TODO: IMPLEMENT!!
        }

        private async Task MirrorBroadcast () {
            // TODO: IMPLEMENT!!
        }

        private async Task MirrorReceive () {
            // TODO: IMPLEMENT!!
        }

        private async Task UpdateMirrorStatus () {
            // TODO: IMPLEMENT!!
        }

        private async Task UpdateExpanders ()
            => await UpdateExpanders (Instance?.Scenario?.IsLoaded ?? false);

        private async Task UpdateExpanders (bool isScene) {
            // TODO: IMPLEMENT!!
        }

        private async Task SetParameterStatus (bool autoApplyChanges) {
            ParameterStatus = autoApplyChanges
                ? ParameterStatuses.AutoApply
                : ParameterStatuses.ChangesApplied;

            await UpdateParameterIndicators ();
        }

        private async Task AdvanceParameterStatus (ParameterStatuses status) {
            // TODO: IMPLEMENT!!
        }

        private async Task UpdateParameterIndicators () {
            // TODO: IMPLEMENT!!
        }

        private async Task LoadFile () {
            // TODO: IMPLEMENT!!
        }

        private async Task LoadOpen (string fileName) {
            // TODO: IMPLEMENT!!
        }

        private async Task LoadInit (Stream incFile) {
            // TODO: IMPLEMENT!!
        }

        private async Task LoadInit (string incFile) {
            // TODO: IMPLEMENT!!
        }

        private async Task LoadValidateT1 (string data) {
            // TODO: IMPLEMENT!!
        }

        private async Task LoadProcess (string incFile) {
            // TODO: IMPLEMENT!!
        }

        private async Task LoadOptions (string inc) {
            // TODO: IMPLEMENT!!
        }

        private async Task LoadFail () {
            // TODO: IMPLEMENT!!
        }

        private async Task SaveFile () {
            // TODO: IMPLEMENT!!
        }

        private async Task SaveT1 (string filename) {
            // TODO: IMPLEMENT!!
        }

        private async Task SaveT1 (Stream stream) {
            // TODO: IMPLEMENT!!
        }

        private string SaveOptions () {
            // TODO: IMPLEMENT!!
            return "";
        }

        public Task Exit () {
            Instance?.Settings.Save ();
            this.Close();
            
            return Task.CompletedTask;
        }

        private void OnMirrorTick (object? sender, EventArgs e) {
            // TODO: IMPLEMENT!!
        }

        private void OnStepChangeRequest (object? sender, EventArgs e)
            => _ = UnloadPatientEvents ();

        private void OnStepChanged (object? sender, EventArgs e) {
            // TODO: IMPLEMENT!!
        }

        private void InitStep () {
            // TODO: IMPLEMENT!!
        }

        private async Task NextStep () {
            // TODO: IMPLEMENT!!
        }

        private async Task PreviousStep () {
            if (Instance?.Scenario is not null)
                await Instance.Scenario.LastStep ();
        }

        private async Task PauseStep () {
            // TODO: IMPLEMENT!!
        }

        private async Task PlayStep () {
            // TODO: IMPLEMENT!!
        }

        private Task ResetPhysiologyParameters () {
            UpdateView (Instance?.Physiology);
            return Task.CompletedTask;
        }

        private async Task ApplyPhysiologyParameters () {
            await AdvanceParameterStatus (ParameterStatuses.ChangesApplied);

            ApplyBuffer ??= new ();

            await ApplyPhysiologyParameters_Buffer (ApplyBuffer);
            ApplyPending_Cardiac = true;
            ApplyPending_Respiratory = true;
            ApplyPending_Obstetric = true;

            await ApplyTimer_Cardiac.ResetStart ();
            await ApplyTimer_Respiratory.ResetStart ();
            await ApplyTimer_Obstetric.ResetStart ();
        }

        private async Task ApplyPhysiologyParameters_Buffer (Physiology? p) {
            if (p is null)
                return;

            /* TODO: Implement!
            
            await p.UpdateParameters_Cardiac (
                // Basic vital signs
                (int)(numHR?.Value ?? 0),
                (int)(numNSBP?.Value ?? 0),
                (int)(numNDBP?.Value ?? 0),
                Physiology.CalculateMAP ((int)(numNSBP?.Value ?? 0), (int)(numNDBP?.Value ?? 0)),
                (int)(numSPO2?.Value ?? 0),
                (double)(numT?.Value ?? 0),

                (Cardiac_Rhythms.Values)(Enum.GetValues (typeof (Cardiac_Rhythms.Values)).GetValue (
                    comboCardiacRhythm.SelectedIndex < 0 ? 0 : comboCardiacRhythm.SelectedIndex)
                    ?? Cardiac_Rhythms.Values.Sinus_Rhythm),

                
                // Advanced hemodynamics
                (int)(numCVP?.Value ?? 0),
                (int)(numASBP?.Value ?? 0),
                (int)(numADBP?.Value ?? 0),
                Physiology.CalculateMAP ((int)(numASBP?.Value ?? 0), (int)(numADBP?.Value ?? 0)),

                (float)(numCO?.Value ?? 0),

                (PulmonaryArtery_Rhythms.Values)(Enum.GetValues (typeof (PulmonaryArtery_Rhythms.Values)).GetValue (
                    comboPACatheterPlacement.SelectedIndex < 0 ? 0 : comboPACatheterPlacement.SelectedIndex)
                    ?? PulmonaryArtery_Rhythms.Values.Right_Atrium),

                (int)(numPSP?.Value ?? 0),
                (int)(numPDP?.Value ?? 0),
                Physiology.CalculateMAP ((int)(numPSP?.Value ?? 0),
                (int)(numPDP?.Value ?? 0)),

                (int)(numICP?.Value ?? 0),
                (int)(numIAP?.Value ?? 0),

                // Cardiac Profile
                (int)(numPacemakerCaptureThreshold?.Value ?? 0),
                chkPulsusParadoxus.IsChecked ?? false,
                chkPulsusAlternans.IsChecked ?? false,
                chkElectricalAlternans.IsChecked ?? false,

                (Cardiac_Axes.Values)(Enum.GetValues (typeof (Cardiac_Axes.Values)).GetValue (
                    comboCardiacAxis.SelectedIndex < 0 ? 0 : comboCardiacAxis.SelectedIndex)
                    ?? Cardiac_Axes.Values.Normal),

                (double)(numQRSInterval?.Value ?? 0),
                (double)(numQTcInterval?.Value ?? 0),

                new double [] {
                    (double)(numSTE_I?.Value ?? 0),
                    (double)(numSTE_II?.Value ?? 0),
                    (double)(numSTE_III?.Value ?? 0),
                    (double)(numSTE_aVR?.Value ?? 0),
                    (double)(numSTE_aVL?.Value ?? 0),
                    (double)(numSTE_aVF?.Value ?? 0),
                    (double)(numSTE_V1?.Value ?? 0),
                    (double)(numSTE_V2?.Value ?? 0),
                    (double)(numSTE_V3?.Value ?? 0),
                    (double)(numSTE_V4?.Value ?? 0),
                    (double)(numSTE_V5?.Value ?? 0),
                    (double)(numSTE_V6?.Value?? 0)
                },
                new double [] {
                    (double)(numTWE_I?.Value ?? 0),
                    (double)(numTWE_II?.Value ?? 0),
                    (double)(numTWE_III?.Value ?? 0),
                    (double)(numTWE_aVR?.Value ?? 0),
                    (double)(numTWE_aVL?.Value ?? 0),
                    (double)(numTWE_aVF?.Value ?? 0),
                    (double)(numTWE_V1?.Value ?? 0),
                    (double)(numTWE_V2?.Value ?? 0),
                    (double)(numTWE_V3?.Value ?? 0),
                    (double)(numTWE_V4?.Value ?? 0),
                    (double)(numTWE_V5?.Value ?? 0),
                    (double)(numTWE_V6?.Value ?? 0)
                }
                );

            await p.UpdateParameters_Respiratory (

                (int)(numRR?.Value ?? 0),
                (Respiratory_Rhythms.Values)(Enum.GetValues (typeof (Respiratory_Rhythms.Values)).GetValue (
                    comboRespiratoryRhythm.SelectedIndex < 0 ? 0 : comboRespiratoryRhythm.SelectedIndex)
                    ?? Respiratory_Rhythms.Values.Regular),
                (int)(numETCO2?.Value ?? 0),

                chkMechanicallyVentilated.IsChecked ?? false,
                (float)(numInspiratoryRatio?.Value ?? 0),
                (float)(numExpiratoryRatio?.Value ?? 0));

            await p.UpdateParameters_Obstetric (
                (int)(numFHR?.Value ?? 0),
                (int)(numFHRVariability?.Value ?? 0),

                (FetalHeart_Rhythms.Values)(Enum.GetValues (typeof (FetalHeart_Rhythms.Values)).GetValue (
                    comboFHRRhythm.SelectedIndex < 0 ? 0 : comboFHRRhythm.SelectedIndex)
                    ?? FetalHeart_Rhythms.Values.Baseline),

                (int)(numUCFrequency?.Value ?? 0),
                (int)(numUCDuration?.Value ?? 0),
                (int)(numUCIntensity?.Value ?? 0),
                (int)(numUCResting?.Value ?? 0));
                */
        }

        private void ApplyPhysiologyParameters_Cardiac (object? sender, EventArgs e) {
            if (ApplyPending_Cardiac != true || ApplyBuffer is null)
                return;

            ApplyPending_Cardiac = false;
            _ = ApplyTimer_Cardiac.ResetStop ();

            if (Instance?.Physiology is not null) {
                _ = Instance.Physiology.UpdateParameters_Cardiac (
                    ApplyBuffer.HR,
                    ApplyBuffer.NSBP, ApplyBuffer.NDBP, ApplyBuffer.NMAP,
                    ApplyBuffer.SPO2,
                    ApplyBuffer.T,
                    ApplyBuffer.Cardiac_Rhythm.Value,

                    ApplyBuffer.CVP,
                    ApplyBuffer.ASBP, ApplyBuffer.ADBP, ApplyBuffer.AMAP,

                    ApplyBuffer.CO,
                    ApplyBuffer.PulmonaryArtery_Placement.Value,
                    ApplyBuffer.PSP, ApplyBuffer.PDP, ApplyBuffer.PMP,

                    ApplyBuffer.ICP,
                    ApplyBuffer.IAP,

                    ApplyBuffer.Pacemaker_Threshold,
                    ApplyBuffer.Pulsus_Paradoxus,
                    ApplyBuffer.Pulsus_Alternans,
                    ApplyBuffer.Electrical_Alternans,

                    ApplyBuffer.Cardiac_Axis.Value,
                    ApplyBuffer.QRS_Interval, ApplyBuffer.QTc_Interval,
                    ApplyBuffer.ST_Elevation, ApplyBuffer.T_Elevation);
            }

            if (Instance?.Mirror is not null && Instance?.Server is not null)
                _ = Instance.Mirror.PostStep (
                    new Scenario.Step () {
                        Physiology = Instance.Physiology,
                    },
                    Instance.Server);
        }

        private void ApplyPhysiologyParameters_Respiratory (object? sender, EventArgs e) {
            if (ApplyPending_Respiratory != true || ApplyBuffer is null)
                return;

            ApplyPending_Respiratory = false;
            _ = ApplyTimer_Respiratory.ResetStop ();

            if (Instance?.Physiology is not null) {
                _ = Instance.Physiology.UpdateParameters_Respiratory (
                    ApplyBuffer.RR,
                    ApplyBuffer.Respiratory_Rhythm.Value,
                    ApplyBuffer.ETCO2,
                    ApplyBuffer.Mechanically_Ventilated,
                    ApplyBuffer.RR_IE_I, ApplyBuffer.RR_IE_E);
            }

            if (Instance?.Mirror is not null && Instance?.Server is not null)
                _ = Instance.Mirror.PostStep (
                    new Scenario.Step () {
                        Physiology = Instance.Physiology,
                    },
                    Instance.Server);
        }

        private void ApplyPhysiologyParameters_Obstetric (object? sender, EventArgs e) {
            if (ApplyPending_Obstetric != true || ApplyBuffer is null)
                return;

            ApplyPending_Obstetric = false;
            _ = ApplyTimer_Obstetric.ResetStop ();

            if (Instance?.Physiology is not null) {
                _ = Instance.Physiology.UpdateParameters_Obstetric (
                    ApplyBuffer.Fetal_HR,
                    ApplyBuffer.ObstetricFetalRateVariability,
                    ApplyBuffer.ObstetricFetalHeartRhythm.Value,
                    ApplyBuffer.ObstetricContractionFrequency,
                    ApplyBuffer.ObstetricContractionDuration,
                    ApplyBuffer.ObstetricContractionIntensity,
                    ApplyBuffer.ObstetricUterineRestingTone);
            }

            if (Instance?.Mirror is not null && Instance?.Server is not null)
                _ = Instance.Mirror.PostStep (
                    new Scenario.Step () {
                        Physiology = Instance.Physiology,
                    },
                    Instance.Server);
        }

        private void UpdateView (Physiology? p) {
            Application.Invoke((sender, args) => {                     // Updating the UI requires being on the proper thread
                ParameterStatus = ParameterStatuses.Loading;                // To prevent each form update from auto-applying back to Patient

                if (p is null) {
                    Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitDeviceMonitor)}");
                    return;
                }

                /* TODO: Implement!
                if (!ApplyPending_Cardiac) {
                    // Basic vital signs
                    numHR.Value = p.VS_Settings.HR;
                    numNSBP.Value = p.VS_Settings.NSBP;
                    numNDBP.Value = p.VS_Settings.NDBP;
                    numSPO2.Value = p.VS_Settings.SPO2;
                    numT.Value = (decimal)p.VS_Settings.T;
                    comboCardiacRhythm.SelectedIndex = (int)p.Cardiac_Rhythm.Value;

                    // Advanced hemodynamics
                    numCVP.Value = p.VS_Settings.CVP;
                    numASBP.Value = p.VS_Settings.ASBP;
                    numADBP.Value = p.VS_Settings.ADBP;
                    numCO.Value = (decimal)p.VS_Settings.CO;
                    comboPACatheterPlacement.SelectedIndex = (int)p.PulmonaryArtery_Placement.Value;
                    numPSP.Value = p.VS_Settings.PSP;
                    numPDP.Value = p.VS_Settings.PDP;
                    numICP.Value = p.VS_Settings.ICP;
                    numIAP.Value = p.VS_Settings.IAP;

                    // Cardiac profile
                    numPacemakerCaptureThreshold.Value = p.Pacemaker_Threshold;
                    chkPulsusParadoxus.IsChecked = p.Pulsus_Paradoxus;
                    chkPulsusAlternans.IsChecked = p.Pulsus_Alternans;
                    chkElectricalAlternans.IsChecked = p.Electrical_Alternans;
                    comboCardiacAxis.SelectedIndex = (int)p.Cardiac_Axis.Value;

                    numQRSInterval.Value = (decimal)p.QRS_Interval;
                    numQTcInterval.Value = (decimal)p.QTc_Interval;

                    if (p.ST_Elevation is not null) {
                        numSTE_I.Value = (decimal)p.ST_Elevation [(int)Lead.Values.ECG_I];
                        numSTE_II.Value = (decimal)p.ST_Elevation [(int)Lead.Values.ECG_II];
                        numSTE_III.Value = (decimal)p.ST_Elevation [(int)Lead.Values.ECG_III];
                        numSTE_aVR.Value = (decimal)p.ST_Elevation [(int)Lead.Values.ECG_AVR];
                        numSTE_aVL.Value = (decimal)p.ST_Elevation [(int)Lead.Values.ECG_AVL];
                        numSTE_aVF.Value = (decimal)p.ST_Elevation [(int)Lead.Values.ECG_AVF];
                        numSTE_V1.Value = (decimal)p.ST_Elevation [(int)Lead.Values.ECG_V1];
                        numSTE_V2.Value = (decimal)p.ST_Elevation [(int)Lead.Values.ECG_V2];
                        numSTE_V3.Value = (decimal)p.ST_Elevation [(int)Lead.Values.ECG_V3];
                        numSTE_V4.Value = (decimal)p.ST_Elevation [(int)Lead.Values.ECG_V4];
                        numSTE_V5.Value = (decimal)p.ST_Elevation [(int)Lead.Values.ECG_V5];
                        numSTE_V6.Value = (decimal)p.ST_Elevation [(int)Lead.Values.ECG_V6];
                    }

                    if (p.T_Elevation is not null) {
                        numTWE_I.Value = (decimal)p.T_Elevation [(int)Lead.Values.ECG_I];
                        numTWE_II.Value = (decimal)p.T_Elevation [(int)Lead.Values.ECG_II];
                        numTWE_III.Value = (decimal)p.T_Elevation [(int)Lead.Values.ECG_III];
                        numTWE_aVR.Value = (decimal)p.T_Elevation [(int)Lead.Values.ECG_AVR];
                        numTWE_aVL.Value = (decimal)p.T_Elevation [(int)Lead.Values.ECG_AVL];
                        numTWE_aVF.Value = (decimal)p.T_Elevation [(int)Lead.Values.ECG_AVF];
                        numTWE_V1.Value = (decimal)p.T_Elevation [(int)Lead.Values.ECG_V1];
                        numTWE_V2.Value = (decimal)p.T_Elevation [(int)Lead.Values.ECG_V2];
                        numTWE_V3.Value = (decimal)p.T_Elevation [(int)Lead.Values.ECG_V3];
                        numTWE_V4.Value = (decimal)p.T_Elevation [(int)Lead.Values.ECG_V4];
                        numTWE_V5.Value = (decimal)p.T_Elevation [(int)Lead.Values.ECG_V5];
                        numTWE_V6.Value = (decimal)p.T_Elevation [(int)Lead.Values.ECG_V6];
                    }
                }

                if (!ApplyPending_Respiratory) {
                    // Respiratory profile
                    numRR.Value = p.VS_Settings.RR;
                    comboRespiratoryRhythm.SelectedIndex = (int)p.Respiratory_Rhythm.Value;
                    numETCO2.Value = p.VS_Settings.ETCO2;
                    chkMechanicallyVentilated.IsChecked = p.Mechanically_Ventilated;
                    numInspiratoryRatio.Value = (decimal)p.VS_Settings.RR_IE_I;
                    numExpiratoryRatio.Value = (decimal)p.VS_Settings.RR_IE_E;
                }

                if (!ApplyPending_Obstetric) {
                    // Obstetric profile
                    numFHR.Value = p.VS_Settings.FetalHR;
                    numFHRVariability.Value = p.ObstetricFetalRateVariability;
                    numUCFrequency.Value = (decimal)p.ObstetricContractionFrequency;
                    numUCDuration.Value = p.ObstetricContractionDuration;
                    numUCIntensity.Value = p.ObstetricContractionIntensity;
                    numUCResting.Value = p.ObstetricUterineRestingTone;
                    comboFHRRhythm.SelectedIndex = (int)p.ObstetricFetalHeartRhythm.Value;
                }
                */

                _ = SetParameterStatus (Instance?.Settings.AutoApplyChanges ?? false);     // Re-establish parameter status
            });
        }

        public void PauseSimulation () {
            // TODO: IMPLEMENT!!
        }

        public void ToggleFullscreen () {
            // TODO: IMPLEMENT!!
        }

        private void MenuNewSimulation_Click (object sender, EventArgs e)
            => _ = RefreshScenario (true);

        private void MenuLoadFile_Click (object s, EventArgs e)
            => _ = LoadFile ();

        private void MenuSaveFile_Click (object s, EventArgs e)
            => _ = SaveFile ();

        private void MenuPauseSimulation_Click (object s, EventArgs e)
            => PauseSimulation ();

        private void MenuToggleFullscreen_Click (object s, EventArgs e)
            => ToggleFullscreen ();

        private void MenuExit_Click (object s, EventArgs e)
            => _ = Exit ();

        private void MenuToggleAudio_Click (object s, EventArgs e)
            => _ = ToggleAudio ();

        private void MenuSetLanguage_Click (object s, EventArgs e)
            => _ = DialogLanguage (true);

        private void MenuMirrorDeactivate_Click (object s, EventArgs e)
            => _ = MirrorDeactivate ();

        private void MenuMirrorBroadcast_Click (object s, EventArgs e)
            => _ = MirrorBroadcast ();

        private void MenuMirrorReceive_Click (object s, EventArgs e)
            => _ = MirrorReceive ();

        private void MenuAbout_Click (object s, EventArgs e)
            => _ = DialogAbout ();

        private void MenuCheckUpdate_Click (object s, EventArgs e)
            => _ = CheckUpgrade ();

        private void ButtonDeviceMonitor_Click (object s, EventArgs e)
            => _ = InitDeviceMonitor ();

        private void ButtonDeviceDefib_Click (object s, EventArgs e)
            => _ = InitDeviceDefib ();

        private void ButtonDeviceECG_Click (object s, EventArgs e)
            => _ = InitDeviceECG ();

        private void ButtonDeviceIABP_Click (object s, EventArgs e)
            => _ = InitDeviceIABP ();

        private void ButtonDeviceEFM_Click (object s, EventArgs e)
            => _ = InitDeviceEFM ();

        private void ButtonOptionsHide_Click (object s, EventArgs e)
            => _ = ToggleHideDevices ();

        private void ButtonPreviousStep_Click (object s, EventArgs e)
            => _ = PreviousStep ();

        private void ButtonNextStep_Click (object s, EventArgs e)
            => _ = NextStep ();

        private void ButtonPauseStep_Click (object s, EventArgs e)
            => _ = PauseStep ();

        private void ButtonPlayStep_Click (object s, EventArgs e)
            => _ = PlayStep ();

        private void ButtonResetParameters_Click (object s, EventArgs e)
            => _ = ResetPhysiologyParameters ();

        private void ButtonApplyParameters_Click (object sender, EventArgs e)
            => _ = ApplyPhysiologyParameters ();

        private void OnClosed (object? sender, EventArgs e)
            => _ = Exit ();

        private void OnActivated (object? sender, EventArgs e) {
            // TODO: IMPLEMENT!!
        }

        private void OnLayoutUpdated (object sender, EventArgs e) {
            // TODO: IMPLEMENT!!
        }

        private void OnAutoApplyChanges_Changed (object sender, EventArgs e) {
            // TODO: IMPLEMENT!!
        }

        private void OnUIPhysiologyParameter_KeyDown (object sender, EventArgs e) {
            // TODO: IMPLEMENT!!
        }

        private void OnUIPhysiologyParameter_Changed (object sender, EventArgs e)
            => OnUIPhysiologyParameter_Process (sender, e);

        private void OnUIPhysiologyParameter_LostFocus (object sender, EventArgs e)
            => OnUIPhysiologyParameter_Process (sender, e);

        private void OnUIPhysiologyParameter_Process (object sender, EventArgs e) {
            // TODO: IMPLEMENT!!
        }

        private void OnCardiacRhythm_Selected (object sender, EventArgs e) {
            // TODO: IMPLEMENT!!
        }

        private void OnRespiratoryRhythm_Selected (object sender, EventArgs e) {
            OnUIPhysiologyParameter_Process (sender, e);
        }

        private void OnCentralVenousPressure_Changed (object sender, EventArgs e) {
            // TODO: IMPLEMENT!!
        }

        private void OnPulmonaryArteryRhythm_Selected (object sender, EventArgs e) {
            // TODO: IMPLEMENT!!
        }

        private void OnPhysiologyEvent (object? sender, Physiology.PhysiologyEventArgs e) {
            if (e.Physiology is null)
                return;

            switch (e.EventType) {
                default:
                    break;

                case Physiology.PhysiologyEventTypes.Cardiac_Baseline:
                    ApplyPhysiologyParameters_Cardiac (sender, new EventArgs ());
                    break;

                case Physiology.PhysiologyEventTypes.Respiratory_Baseline:
                    ApplyPhysiologyParameters_Respiratory (sender, new EventArgs ());
                    break;

                case Physiology.PhysiologyEventTypes.Obstetric_Baseline:
                    ApplyPhysiologyParameters_Obstetric (sender, new EventArgs ());
                    break;

                case Physiology.PhysiologyEventTypes.Vitals_Change:
                    UpdateView (e.Physiology);
                    break;
            }
        }
    }
}