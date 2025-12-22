#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Text;
using Cairo;
using Gdk;
using GLib;
using Gtk;
using Pango;

using II;
using II.Localization;
using II.Server;
using Application = Gtk.Application;
using DateTime = System.DateTime;
using File = II.File;
using Key = Gdk.Key;
using Menu = Gtk.Menu;
using MenuItem = Gtk.MenuItem;
using Task = System.Threading.Tasks.Task;
using Window = Gtk.Window;

namespace IISIM
{
    class Control : Window
    {
        private App? Instance;
        
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

        /* GTK GUI Objects: Items that need class-wide scope */
        
        private MenuItem miSettingsAudio;
        private MenuItem miMirrorStatus;
        private Box bxDevicesCompact;
        private Box bxDevicesExpanded;
        
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
            this.KeyPressEvent += OnKeyPressEvent;
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
                if (Instance?.StartArgs?.Length > 0) {
                    string loadfile = Instance.StartArgs [0].Trim (' ', '\n', '\r');
                    if (!String.IsNullOrEmpty (loadfile)) {
                        LoadOpen (loadfile);
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
            if (!(Instance?.Settings.AcceptedEULA ?? false)) {
                DialogEULA ();
            }   
        }

        private void InitInterface () {
            int sp = 2;                 // Spacing: General (int)
            uint usp = 3;               // Spacing: General (uint)
            uint upd = 5;               // Padding: General (uint)
            uint tlepd = 2;             // Padding: Top-level elements
            
            /* GUI: Top-Level Elements; 1 [ 2 ] */
            
            Box vbMain1 = new Box (Orientation.Vertical, sp) {
                MarginBottom = 5
            };
            
            Box hbMain2 = new Box (Orientation.Horizontal, sp) {
                BorderWidth = usp
            };
            
            /* GUI: Menu Bar Item Instantiation*/

            MenuBar mbMain = new MenuBar () {
                Margin = 4,
                PackDirection = PackDirection.Ltr
            };
            
            Menu muFile = new Menu ();
            Menu muOptions = new Menu ();
            Menu muMirror = new Menu ();
            Menu muSettings = new Menu ();
            Menu muHelp = new Menu ();
            MenuItem miFile = new MenuItem () {
                Label = Instance?.Language.Localize ("PE:MenuFile").Replace ("_", "")
            };
            MenuItem miFileNew = new MenuItem () {
                Label = Instance?.Language.Localize ("PE:MenuNewFile").Replace ("_", "")
            };
            MenuItem miFileLoad = new MenuItem () {
                Label = Instance?.Language.Localize ("PE:MenuLoadSimulation").Replace ("_", "")
            };
            MenuItem miFileSave = new MenuItem () {
                Label = Instance?.Language.Localize ("PE:MenuSaveSimulation").Replace ("_", "")
            };
            MenuItem miFileFullScreen = new MenuItem () {
                Label = Instance?.Language.Localize ("MENU:MenuToggleFullscreen").Replace ("_", "")
            };
            MenuItem miFileExit = new MenuItem () {
                Label = Instance?.Language.Localize ("PE:MenuExitProgram").Replace ("_", "")
            };
            MenuItem miOptions = new MenuItem () {
                Label = Instance?.Language.Localize ("PE:MenuOptions").Replace ("_", "")
            };
            MenuItem miOptionsPause = new MenuItem () {
                Label = Instance?.Language.Localize ("PE:MenuPause").Replace ("_", "")
            };
            MenuItem miMirror = new MenuItem () {
                Label = Instance?.Language.Localize ("PE:MenuMirror").Replace ("_", "")
            };
            MenuItem miMirrorStatus = new MenuItem () {
                Label = (Instance?.Mirror.Status) switch {
                    Mirror.Statuses.HOST =>
                        $"{Instance?.Language.Localize ("MIRROR:Status")}: {Instance?.Language.Localize ("MIRROR:Server")}",
                    Mirror.Statuses.CLIENT =>
                        $"{Instance?.Language.Localize ("MIRROR:Status")}: {Instance?.Language.Localize ("MIRROR:Client")}",
                    _ =>
                        $"{Instance?.Language.Localize ("MIRROR:Status")}: {Instance?.Language.Localize ("MIRROR:Inactive")}"
                }
            };
            MenuItem miMirrorDeactivate = new MenuItem () {
                Label = Instance?.Language.Localize ("PE:MenuMirrorDeactivate").Replace ("_", "")
            };
            MenuItem miMirrorReceive = new MenuItem () {
                Label = Instance?.Language.Localize ("PE:MenuMirrorReceive").Replace ("_", "")
            };
            MenuItem miMirrorBroadcast = new MenuItem () {
                Label = Instance?.Language.Localize ("PE:MenuMirrorBroadcast").Replace ("_", "")
            };
            MenuItem miSettings = new MenuItem () {
                Label = Instance?.Language.Localize ("PE:MenuSettings").Replace ("_", "")
            };
            MenuItem miSettingsAudio = new MenuItem () {
                Label = String.Format ("{0}: {1}",
                    Instance?.Language.Localize ("PE:MenuToggleAudio").Replace ("_", ""),
                    Instance?.Settings.AudioEnabled ?? false ? Instance?.Language.Localize ("BOOLEAN:On") : Instance?.Language.Localize ("BOOLEAN:Off"))
            };
            MenuItem miSettingsLanguage = new MenuItem () {
                Label = Instance?.Language.Localize ("PE:MenuSetLanguage").Replace ("_", "")
            };
            MenuItem miHelp = new MenuItem () {
                Label = Instance?.Language.Localize ("PE:MenuHelp").Replace ("_", "")
            };
            MenuItem miHelpUpdate = new MenuItem () {
                Label = Instance?.Language.Localize ("PE:MenuCheckUpdates").Replace ("_", "")
            };
            MenuItem miHelpAbout = new MenuItem () {
                Label = Instance?.Language.Localize ("PE:MenuAboutProgram").Replace ("_", "")
            };
            
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
            muFile.Append (new SeparatorMenuItem ());
            muFile.Append (miFileLoad);
            muFile.Append (miFileSave);
            muFile.Append (new SeparatorMenuItem ());
            muFile.Append (miFileFullScreen);
            muFile.Append (new SeparatorMenuItem ());
            muFile.Append (miFileExit);
            mbMain.Append(miFile);
            
            miOptions.Submenu = muOptions;
            muOptions.Append (miOptionsPause);
            mbMain.Append(miOptions);
            
            miMirror.Submenu = muMirror;
            muMirror.Append (miMirrorStatus);
            muMirror.Append (new SeparatorMenuItem ());
            muMirror.Append (miMirrorDeactivate);
            muMirror.Append (new SeparatorMenuItem ());
            muMirror.Append (miMirrorReceive);
            muMirror.Append (miMirrorBroadcast);
            mbMain.Append(miMirror);
            
            miSettings.Submenu = muSettings;
            muSettings.Append (miSettingsAudio);
            muSettings.Append (new SeparatorMenuItem ());
            muSettings.Append (miSettingsLanguage);
            mbMain.Append(miSettings);
            
            miHelp.Submenu = muHelp;
            muHelp.Append (miHelpUpdate);
            muHelp.Append (new SeparatorMenuItem ());
            muHelp.Append (miHelpAbout);
            mbMain.Append(miHelp);
            
            /* GUI: Devices Section; Expanded View */
            
            bxDevicesExpanded = new Box (Orientation.Vertical, sp) {
                MarginBottom = sp
            };
            
            Label lblDevices = new Label () {
                Halign = Align.Start,
                UseMarkup = true,
                Markup = $"<span size='12pt' weight='bold'>{Instance?.Language.Localize ("PE:Devices")}</span>"
            };
            
            bxDevicesExpanded.PackStart (lblDevices, false, false, upd);
            
            Box hbDeviceCardiacMonitor_exp = new Box (Orientation.Horizontal, sp);
            hbDeviceCardiacMonitor_exp.PackStart (
                new Image(new Gdk.Pixbuf("Third_Party/Icon_DeviceMonitor_128.png", 40, 40, true))
                , false, false, upd);
            hbDeviceCardiacMonitor_exp.PackStart(new Label(Instance?.Language.Localize ("PE:CardiacMonitor")), false, false, 6);

            Button btnDeviceCardiacMonitor_exp = new Button () {
                Child = hbDeviceCardiacMonitor_exp,
                Relief = ReliefStyle.None
            };
            
            Box hbDeviceDefibrillator_exp = new Box (Orientation.Horizontal, sp);
            hbDeviceDefibrillator_exp.PackStart (
                new Image(new Gdk.Pixbuf("Third_Party/Icon_DeviceDefibrillator_128.png", 40, 40, true))
                , false, false, upd);
            hbDeviceDefibrillator_exp.PackStart(new Label(Instance?.Language.Localize ("PE:Defibrillator")), false, false, 6);
            
            Button btnDeviceDefibrillator_exp = new Button () {
                Child = hbDeviceDefibrillator_exp,
                Relief = ReliefStyle.None
            };
            
            Box hbDevice12LeadECG_exp = new Box (Orientation.Horizontal, sp);
            hbDevice12LeadECG_exp.PackStart (
                new Image(new Gdk.Pixbuf("Third_Party/Icon_Device12LeadECG_128.png", 40, 40, true))
                , false, false, upd);
            hbDevice12LeadECG_exp.PackStart(new Label(Instance?.Language.Localize ("PE:12LeadECG")), false, false, 6);
            
            Button btnDevice12LeadECG_exp = new Button () {
                Child = hbDevice12LeadECG_exp,
                Relief = ReliefStyle.None
            };
            
            bxDevicesExpanded.PackStart (btnDeviceCardiacMonitor_exp, false, false, 0);
            bxDevicesExpanded.PackStart (btnDeviceDefibrillator_exp, false, false, 0);
            bxDevicesExpanded.PackStart (btnDevice12LeadECG_exp, false, false, 0);

            bxDevicesExpanded.PackStart(new Separator(Orientation.Horizontal), false, false, 0);
            
            Label lblHideDevices = new Label () {
                Halign = Align.Start,
                UseMarkup = true,
                Markup = $"<span size='12pt' weight='bold'>{Instance?.Language.Localize ("PE:Options")}</span>",
                MarginTop = 10
            };
            
            bxDevicesExpanded.PackStart (lblHideDevices, false, false, upd);
            
            Box hbHideDevices = new Box (Orientation.Horizontal, sp);
            hbHideDevices.PackStart (
                new Image(new Gdk.Pixbuf("Third_Party/Icon_Clipboard_128.png", 40, 40, true))
                , false, false, upd);
            hbHideDevices.PackStart(new Label(Instance?.Language.Localize ("PE:HideDevices")), false, false, 6);
            
            Button btnHideDevices = new Button () {
                Child = hbHideDevices,
                Relief = ReliefStyle.None
            };

            btnHideDevices.Pressed += (sender, args) => {
                ButtonOptionsHide_Click(this, EventArgs.Empty);
            };
            
            bxDevicesExpanded.PackStart (btnHideDevices, false, false, 0);

            /* GUI: Devices Section; Hidden/Compact View */
            
            bxDevicesCompact = new Box (Orientation.Vertical, sp){
                MarginBottom = sp
            };
            
            Box hbDeviceCardiacMonitor_cmp = new Box (Orientation.Horizontal, sp);
            hbDeviceCardiacMonitor_cmp.PackStart (
                new Image(new Gdk.Pixbuf("Third_Party/Icon_DeviceMonitor_128.png", 40, 40, true))
                , false, false, upd);

            Button btnDeviceCardiacMonitor_cmp = new Button () {
                Child = hbDeviceCardiacMonitor_cmp,
                Relief = ReliefStyle.None
            };
            
            Box hbDeviceDefibrillator_cmp = new Box (Orientation.Horizontal, sp);
            hbDeviceDefibrillator_cmp.PackStart (
                new Image(new Gdk.Pixbuf("Third_Party/Icon_DeviceDefibrillator_128.png", 40, 40, true))
                , false, false, upd);
            
            Button btnDeviceDefibrillator_cmp = new Button () {
                Child = hbDeviceDefibrillator_cmp,
                Relief = ReliefStyle.None
            };
            
            Box hbDevice12LeadECG_cmp = new Box (Orientation.Horizontal, sp);
            hbDevice12LeadECG_cmp.PackStart (
                new Image(new Gdk.Pixbuf("Third_Party/Icon_Device12LeadECG_128.png", 40, 40, true))
                , false, false, upd);
            
            Button btnDevice12LeadECG_cmp = new Button () {
                Child = hbDevice12LeadECG_cmp,
                Relief = ReliefStyle.None
            };
            
            bxDevicesCompact.PackStart (btnDeviceCardiacMonitor_cmp, false, false, 0);
            bxDevicesCompact.PackStart (btnDeviceDefibrillator_cmp, false, false, 0);
            bxDevicesCompact.PackStart (btnDevice12LeadECG_cmp, false, false, 0);
            
            bxDevicesCompact.PackStart(new Separator(Orientation.Horizontal), false, false, 0);
            
            Box hbShowDevices = new Box (Orientation.Horizontal, sp);
            hbShowDevices.PackStart (
                new Image(new Gdk.Pixbuf("Third_Party/Icon_Clipboard_128.png", 40, 40, true))
                , false, false, upd);
            
            Button btnShowDevices = new Button () {
                Child = hbShowDevices,
                Relief = ReliefStyle.None
            };

            btnShowDevices.Pressed += (sender, args) => {
                ButtonOptionsHide_Click(this, EventArgs.Empty);
            };
            
            bxDevicesCompact.PackStart (btnShowDevices, false, false, 0);
            
            /* GUI: Parameters */
            
            Grid gridParameters = new Grid () {
                RowSpacing = usp,
                ColumnSpacing = usp
            };
            
            /* GUI: Parameters: Vital Signs */
     
            Label lblHR = new Label ($"{Instance?.Language.Localize ("PE:HeartRate")}:") {
                Halign = Align.Start
            };
            gridParameters.Attach (lblHR, 0, 0, 1, 1);
            gridParameters.Attach (numHR, 1, 0, 3, 1);
            
            Label lblNIBP = new Label ($"{Instance?.Language.Localize ("PE:BloodPressure")}:") {
                Halign = Align.Start
            };
            
            gridParameters.Attach (lblNIBP, 0, 1, 1, 1);
            gridParameters.Attach (numNSBP, 1, 1, 1, 1);
            gridParameters.Attach (new Label("/"), 2, 1, 1, 1);
            gridParameters.Attach (numNDBP, 3, 1, 1, 1);
            
            Label lblRR = new Label ($"{Instance?.Language.Localize ("PE:RespiratoryRate")}:") {
                Halign = Align.Start
            };
            gridParameters.Attach (lblRR, 0, 2, 1, 1);
            gridParameters.Attach (numRR, 1, 2, 3, 1);

            Label lblSPO2 = new Label ($"{Instance?.Language.Localize ("PE:PulseOximetry")}:") {
                Halign = Align.Start
            };
            gridParameters.Attach (lblSPO2, 0, 3, 1, 1);
            gridParameters.Attach (numSPO2, 1, 3, 3, 1);
            
            Label lblT = new Label ($"{Instance?.Language.Localize ("PE:Temperature")}:") {
                Halign = Align.Start
            };
            gridParameters.Attach (lblT, 0, 4, 1, 1);
            gridParameters.Attach (numT, 1, 4, 3, 1);

            Label lblCardiacRhythm = new Label ($"{Instance?.Language.Localize ("PE:CardiacRhythm")}:") {
                Halign = Align.Start
            };
            gridParameters.Attach (lblCardiacRhythm, 0, 5, 1, 1);

            List<string> listCardiacRhythm = new List<string> ();
            foreach (Cardiac_Rhythms.Values v in Enum.GetValues (typeof(Cardiac_Rhythms.Values)))
                listCardiacRhythm.Add (Instance?.Language.Localize (Cardiac_Rhythms.LookupString (v)));
            cmbCardiacRhythm = new ComboBox(listCardiacRhythm.ToArray ());
            gridParameters.Attach (cmbCardiacRhythm, 1, 5, 3, 1);
            
            chkDefaultVitals = new CheckButton (Instance?.Language.Localize ("PE:UseDefaultVitalSignRanges"));
            gridParameters.Attach (chkDefaultVitals, 0, 6, 4, 1);
            
            
            hbMain2.PackStart (bxDevicesExpanded, false, false, 10);
            hbMain2.PackStart (bxDevicesCompact, false, false, 10);
            hbMain2.PackStart (new Separator(Orientation.Vertical), false, false, 0);

            Label lblVitalSigns = new Label () {
                UseMarkup = true,
                Markup = $"<span size='12pt' weight='bold'>  {Instance?.Language.Localize ("PE:VitalSigns")}</span>",
                HeightRequest = 34
            };
            
            Expander expVitalSigns = new Expander ("Test") {
                LabelWidget = lblVitalSigns,
                Expanded = true
            };
            expVitalSigns.Add(gridParameters);
            
            hbMain2.PackStart (expVitalSigns, false, false, upd);
            
            /* GUI: Assemble... */
            
            vbMain1.PackStart (mbMain, false, true, tlepd);
            vbMain1.PackStart (hbMain2, false, false, tlepd);
            Add (vbMain1);
            
            HeaderBar hb = new HeaderBar() {
                ShowCloseButton = true,
                Title = Instance?.Language.Localize ("PE:WindowTitle")
            };
            this.Titlebar = hb;
            
            ShowAll ();
            
            bxDevicesCompact.Hide();
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
                
                // For center-screen positioning for pop-up and child windows: X11 Center; Wayland CenterOnParent
                dlgEULA.SetPosition(
                    Instance.DisplayServer == App.Compositors.Wayland
                        ? WindowPosition.CenterOnParent
                        : WindowPosition.Center);
                dlgEULA.KeepAbove = true;
                dlgEULA.Show();
            });
        }

        private async Task DialogLanguage (bool reloadUI = false) {
            Application.Invoke((sender, args) => {
                var dlgLanguage = new DialogLanguage(Instance);
                dlgLanguage.TransientFor = this;
                Application.AddWindow(dlgLanguage);
                
                // For center-screen positioning for pop-up and child windows: X11 Center; Wayland CenterOnParent
                dlgLanguage.SetPosition(
                    Instance.DisplayServer == App.Compositors.Wayland
                        ? WindowPosition.CenterOnParent
                        : WindowPosition.Center);
                dlgLanguage.KeepAbove = true;
                dlgLanguage.Show();
            });
        }

        private async Task DialogMirrorBroadcast () {
            Application.Invoke((sender, args) => {
                DialogMirrorBroadcast dlgMirrorBroadcast = new DialogMirrorBroadcast(Instance);
                dlgMirrorBroadcast.TransientFor = this;
                Application.AddWindow(dlgMirrorBroadcast);
                
                // For center-screen positioning for pop-up and child windows: X11 Center; Wayland CenterOnParent
                dlgMirrorBroadcast.SetPosition(
                    Instance.DisplayServer == App.Compositors.Wayland
                        ? WindowPosition.CenterOnParent
                        : WindowPosition.Center);
                dlgMirrorBroadcast.KeepAbove = true;
                dlgMirrorBroadcast.Show();
                
                // If dlgMirrorBroadcast did not successfully set Instance.Mirror.Status as a HOST, then
                // the following lines will abort anyway... no need to check the dialog response since
                // mirror broadcasting already operates on a state machine in Instance
                Task.Run (() => {
                    Instance?.Mirror.PostStep (
                        new Scenario.Step () {
                            Physiology = Instance.Physiology ?? new Physiology (),
                        },
                        Instance.Server);
                });
            });
        }

        private async Task DialogMirrorReceive () {
            Application.Invoke ((sender, args) => {
                DialogMirrorReceive dlgMirrorReceive = new DialogMirrorReceive (Instance);
                dlgMirrorReceive.TransientFor = this;
                Application.AddWindow (dlgMirrorReceive);

                // For center-screen positioning for pop-up and child windows: X11 Center; Wayland CenterOnParent
                dlgMirrorReceive.SetPosition(
                    Instance.DisplayServer == App.Compositors.Wayland
                        ? WindowPosition.CenterOnParent
                        : WindowPosition.Center);
                dlgMirrorReceive.KeepAbove = true;
                dlgMirrorReceive.Show ();
            });
        }

        public async Task DialogAbout () {
            Application.Invoke ((sender, args) => {
                DialogAbout dlgAbout = new DialogAbout (Instance);
                dlgAbout.TransientFor = this;
                Application.AddWindow (dlgAbout);
                
                // For center-screen positioning for pop-up and child windows: X11 Center; Wayland CenterOnParent
                dlgAbout.SetPosition(
                    Instance.DisplayServer == App.Compositors.Wayland
                        ? WindowPosition.CenterOnParent
                        : WindowPosition.Center);
                dlgAbout.KeepAbove = true;
                dlgAbout.Show ();
            });
        }

        private async Task DialogUpgrade () {
            Application.Invoke ((sender, args) => {
                DialogUpgrade dlgUpgrade = new DialogUpgrade (Instance);
                dlgUpgrade.TransientFor = this;
                Application.AddWindow (dlgUpgrade);
                
                // For center-screen positioning for pop-up and child windows: X11 Center; Wayland CenterOnParent
                dlgUpgrade.SetPosition(
                    Instance.DisplayServer == App.Compositors.Wayland
                        ? WindowPosition.CenterOnParent
                        : WindowPosition.Center);
                dlgUpgrade.KeepAbove = true;

                dlgUpgrade.OnUpgradeRoute += (s, e) => {
                    switch (e.Route) {
                        default:
                        case IISIM.DialogUpgrade.UpgradeOptions.None:
                        case IISIM.DialogUpgrade.UpgradeOptions.Delay:
                            return;

                        case IISIM.DialogUpgrade.UpgradeOptions.Mute:
                            if (Instance is not null) {
                                Instance.Settings.MuteUpgrade = true;
                                Instance.Settings.MuteUpgradeDate = DateTime.Now;
                                Instance.Settings.Save ();
                            }
                            return;

                        case IISIM.DialogUpgrade.UpgradeOptions.Website:
                            string url = String.IsNullOrEmpty (Instance?.Server.UpgradeWebpage)
                                ? "https://github.com/tanjera/infirmary-integrated/releases".Replace ("&", "^&")
                                : Instance.Server.UpgradeWebpage;
                            System.Diagnostics.Process.Start (new ProcessStartInfo ("xdg-open", url) { CreateNoWindow = true });
                            return;
                    }
                };
                
                dlgUpgrade.Show ();
            });
        }

        public async Task ToggleAudio () {
            await SetAudio (!Instance?.Settings.AudioEnabled);
        }

        public void SetAudio_On () => _ = SetAudio (true);

        public void SetAudio_Off () => _ = SetAudio (false);

        public Task SetAudio (bool? toSet) {
            if (Instance is not null && toSet is not null) {
                Instance.Settings.AudioEnabled = (bool)toSet;
                Instance.Settings.Save ();

                Application.Invoke((sender, args) => {
                    miSettingsAudio.Label = String.Format ("{0}: {1}",
                        Instance.Language.Localize ("PE:MenuToggleAudio"),
                        Instance.Settings.AudioEnabled
                            ? Instance.Language.Localize ("BOOLEAN:On")
                            : Instance.Language.Localize ("BOOLEAN:Off"));
                });
            }

            return Task.CompletedTask;
        }

        private Task ToggleHideDevices () {
            HideDeviceLabels = !HideDeviceLabels;
            
            Application.Invoke((sender, args) => {
                if (HideDeviceLabels) {
                    bxDevicesExpanded.Hide ();
                    bxDevicesCompact.Show ();
                } else {
                    bxDevicesExpanded.Show ();
                    bxDevicesCompact.Hide ();
                }
            });

            return Task.CompletedTask;
        }

        private async Task CheckUpgrade () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (CheckUpgrade)}");
                return;
            }
            
            // Check with server for updated version of Infirmary Integrated- notify user either way
            await Instance.Server.Get_LatestVersion ();

            string version = Assembly.GetExecutingAssembly ()?.GetName ()?.Version?.ToString (3) ?? "0.0.0";
            if (Utility.IsNewerVersion (version, Instance.Server.UpgradeVersion)) {
                await DialogUpgrade ();
            } else {
                Application.Invoke ((sender, args) => {
                    MessageDialog dlgNoUpgrade = new MessageDialog (this, 0, MessageType.Info,
                        ButtonsType.Ok, false, Instance?.Language.Localize ("UPGRADE:NoUpdateAvailable"));
                    dlgNoUpgrade.Response += (o, args) => { dlgNoUpgrade.Destroy (); };
                    dlgNoUpgrade.Show();
                });
            }
        }

        private Task OpenUpgrade () {
            string url = String.IsNullOrEmpty (Instance?.Server.UpgradeWebpage)
                ? "https://github.com/tanjera/infirmary-integrated/releases".Replace ("&", "^&")
                : Instance.Server.UpgradeWebpage;
            System.Diagnostics.Process.Start (new ProcessStartInfo ("xdg-open", url) { CreateNoWindow = true });
            
            return Task.CompletedTask;
        }

        private async Task MirrorDeactivate () {
            if (Instance is not null)
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
            Application.Invoke((sender, args) => {
                miMirrorStatus.Label = (Instance?.Mirror.Status) switch {
                    Mirror.Statuses.HOST => $"{Instance?.Language.Localize ("MIRROR:Status")}: {Instance?.Language.Localize ("MIRROR:Server")}",
                    Mirror.Statuses.CLIENT => $"{Instance?.Language.Localize ("MIRROR:Status")}: {Instance?.Language.Localize ("MIRROR:Client")}",
                    _ => $"{Instance?.Language.Localize ("MIRROR:Status")}: {Instance?.Language.Localize ("MIRROR:Inactive")}",
                };
            });

            return Task.CompletedTask;
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
            /* Toggles between pending changes or changes applied; bypasses if auto-applying or null */

            if (status == ParameterStatuses.ChangesApplied && ParameterStatus == ParameterStatuses.ChangesPending)
                ParameterStatus = ParameterStatuses.ChangesApplied;
            else if (status == ParameterStatuses.ChangesPending && ParameterStatus == ParameterStatuses.ChangesApplied)
                ParameterStatus = ParameterStatuses.ChangesPending;

            await UpdateParameterIndicators ();
        }

        private async Task UpdateParameterIndicators () {
            // TODO: IMPLEMENT!!
        }

        private async Task LoadFile () {
            FileChooserDialog dlgLoad = new FileChooserDialog (
                Instance?.Language.Localize ("PE:MenuLoadSimulation").Replace ("_", ""),
                this, FileChooserAction.Open, 
                Instance?.Language.Localize ("BUTTON:Cancel"), ResponseType.Cancel, 
                Instance?.Language.Localize ("BUTTON:Continue"), ResponseType.Accept);
            dlgLoad.SelectMultiple = false;
            dlgLoad.SetCurrentFolder (Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            
            FileFilter ff = new  FileFilter ();
            ff.AddPattern ("*.ii");
            ff.Name = "Infirmary Integrated simulation files (*.ii)";
            dlgLoad.Filter = ff;
            
            
            dlgLoad.Response += async (o, args) => {
                if (args.ResponseId == ResponseType.Accept) {
                    FileStream f = new FileStream(dlgLoad.Filename, FileMode.Open, FileAccess.Read);
                    
                    dlgLoad.Destroy ();
                    await LoadInit (f);
                } else {
                    dlgLoad.Destroy ();
                }
            };
            
            dlgLoad.Show();
        }

        private void LoadOpen (string fileName) {
            if (System.IO.File.Exists (fileName)) {
                Task.Run(() => LoadInit (fileName));
            } else {
                Task.Run(LoadFail);
            }

            OnPhysiologyEvent (this, new Physiology.PhysiologyEventArgs (Instance?.Physiology, Physiology.PhysiologyEventTypes.Vitals_Change));
        }

        private async Task LoadInit (Stream incFile) {
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
            using StringReader sRead = new (incFile);
            string? line, pline;
            StringBuilder pbuffer;

            try {
                while ((line = (await sRead.ReadLineAsync ())?.Trim ()) != null) {
                    if (Instance is null)
                        continue;

                    if (line == "> Begin: Physiology") {           // Load files saved by Infirmary Integrated (base)
                        if (Instance.Scenario?.Physiology is null)
                            Instance.Scenario = new (true);

                        pbuffer = new StringBuilder ();
                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                && pline != "> End: Physiology")
                            pbuffer.AppendLine (pline);

                        await RefreshScenario (true);
                        await (Instance?.Physiology?.Load (pbuffer.ToString ()) ?? Task.CompletedTask);
                    } else if (line == "> Begin: Scenario") {   // Load files saved by Infirmary Integrated Scenario Editor
                        Instance.Scenario ??= new (false);

                        pbuffer = new StringBuilder ();
                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                && pline != "> End: Scenario")
                            pbuffer.AppendLine (pline);

                        await RefreshScenario (false);
                        await Instance.Scenario.Load (pbuffer.ToString ());
                        InitScenarioStep ();     // Needs to be called manually since InitScenario(false) doesn't init a Patient
                    } else if (line == "> Begin: Editor") {
                        pbuffer = new StringBuilder ();
                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                && pline != "> End: Editor")
                            pbuffer.AppendLine (pline);

                        await this.LoadOptions (pbuffer.ToString ());
                    } 
                    
                    // TODO: Implement
                    /*
                    else if (line == "> Begin: Cardiac Monitor") {
                        pbuffer = new StringBuilder ();
                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                && pline != "> End: Cardiac Monitor")
                            pbuffer.AppendLine (pline);

                        Instance.Device_Monitor = new DeviceMonitor (Instance);
                        await InitDeviceMonitor ();
                        await Instance.Device_Monitor.Load (pbuffer.ToString ());
                    } else if (line == "> Begin: 12 Lead ECG") {
                        pbuffer = new StringBuilder ();
                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                && pline != "> End: 12 Lead ECG")
                            pbuffer.AppendLine (pline);

                        Instance.Device_ECG = new DeviceECG (Instance);
                        await InitDeviceECG ();
                        await Instance.Device_ECG.Load (pbuffer.ToString ());
                    } else if (line == "> Begin: Defibrillator") {
                        pbuffer = new StringBuilder ();
                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                && pline != "> End: Defibrillator")
                            pbuffer.AppendLine (pline);

                        Instance.Device_Defib = new DeviceDefib (Instance);
                        await InitDeviceDefib ();
                        await Instance.Device_Defib.Load (pbuffer.ToString ());
                    } else if (line == "> Begin: Intra-aortic Balloon Pump") {
                        pbuffer = new StringBuilder ();
                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                && pline != "> End: Intra-aortic Balloon Pump")
                            pbuffer.AppendLine (pline);

                        Instance.Device_IABP = new DeviceIABP (Instance);
                        await InitDeviceIABP ();
                        await Instance.Device_IABP.Load (pbuffer.ToString ());
                    }
                    */
                }
            } catch {
                await LoadFail ();
            } finally {
                sRead.Close ();
            }

            // On loading a file, ensure Mirroring is not in Client mode! Will conflict...
            if (Instance is not null && Instance.Mirror.Status == Mirror.Statuses.CLIENT) {
                Instance.Mirror.Status = Mirror.Statuses.INACTIVE;
                Instance.Mirror.CancelOperation ();      // Attempt to cancel any possible Mirror downloads
            }

            // Initialize the first step of the scenario
            if (Instance?.Scenario?.IsLoaded ?? false) {
                InitStep ();

                if (Instance.Scenario.DeviceMonitor.IsEnabled)
                    await InitDeviceMonitor ();
                if (Instance.Scenario.DeviceDefib.IsEnabled)
                    await InitDeviceDefib ();
                if (Instance.Scenario.DeviceECG.IsEnabled)
                    await InitDeviceECG ();
                if (Instance.Scenario.DeviceIABP.IsEnabled)
                    await InitDeviceIABP ();
            }

            // Set UI Expanders IsExpanded and IsEnabled on whether is a Scenario
            await UpdateExpanders ();

            /* Load completed but possibly in any order (e.g. physiology before devices)
             * Fire events to begin synchronizing devices with physiology
             */
            Instance?.Physiology?.OnPhysiologyEvent (Physiology.PhysiologyEventTypes.Vitals_Change);
        }

        private async Task LoadOptions (string inc) {
            using StringReader sRead = new (inc);

            try {
                string? line;
                while ((line = await sRead.ReadLineAsync ()) != null) {
                    if (line.Contains (':')) {
                        string pName = line.Substring (0, line.IndexOf (':')),
                            pValue = line.Substring (line.IndexOf (':') + 1);
                        switch (pName) {
                            default: break;
                            case "checkDefaultVitals": chkDefaultVitals.Active = bool.Parse (pValue); break;
                        }
                    }
                }
            } catch {
                sRead.Close ();
                return;
            }

            sRead.Close ();
        }

        private Task LoadFail () {
            MessageDialog dlgLoadFail = new MessageDialog (this, 0, MessageType.Error,
                ButtonsType.Ok, false, Instance?.Language.Localize ("PE:LoadFailMessage"));
            dlgLoadFail.Response += (o, args) => { dlgLoadFail.Destroy (); };
            dlgLoadFail.Show();
            
            return Task.CompletedTask;
        }

        private async Task SaveFile () {
            // Only save single Patient files in base Infirmary Integrated!
            // Scenario files should be created/edited/saved via II Scenario Editor!

            if (Instance?.Scenario?.IsLoaded ?? false) {
                MessageDialog dlgSaveFail = new MessageDialog (this, 0, MessageType.Error,
                    ButtonsType.Ok, false, Instance?.Language.Localize ("PE:SaveFailScenarioMessage"));
                dlgSaveFail.Response += (o, args) => { dlgSaveFail.Destroy (); };
                dlgSaveFail.Show();
                
                return;
            }
            
            FileChooserDialog dlgSave = new FileChooserDialog (
                Instance?.Language.Localize ("PE:MenuSaveSimulation").Replace ("_", ""),
                this, FileChooserAction.Save,
                Instance?.Language.Localize ("BUTTON:Cancel"), ResponseType.Cancel, 
                Instance?.Language.Localize ("BUTTON:Continue"), ResponseType.Accept);
            dlgSave.SelectMultiple = false;
            dlgSave.DoOverwriteConfirmation = true;
            dlgSave.CurrentName = ".ii";
            dlgSave.SetCurrentFolder (Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            
            
            FileFilter ff = new  FileFilter ();
            ff.AddPattern ("*.ii");
            ff.Name = "Infirmary Integrated simulation files (*.ii)";
            dlgSave.Filter = ff;
            
            dlgSave.Response += async (o, args) => {
                if (args.ResponseId == ResponseType.Accept) {
                    string filename = dlgSave.Filename.EndsWith(".ii") ? dlgSave.Filename : dlgSave.Filename + ".ii";
                    FileStream f = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
                    
                    dlgSave.Destroy ();
                    await SaveT1 (f);
                } else {
                    dlgSave.Destroy ();
                }
            };
            
            dlgSave.Show();
        }

        private async Task SaveT1 (string filename) {
            if (System.IO.File.Exists (filename))
                System.IO.File.Delete (filename);

            using FileStream s = new (filename, FileMode.OpenOrCreate, FileAccess.Write);
            await SaveT1 (s);
        }

        private async Task SaveT1 (Stream stream) {
             // Ensure only saving Patient file, not Scenario file; is screened in SaveFile()
            if (Instance?.Scenario != null && Instance.Scenario.IsLoaded) {
                stream.Close ();
                return;
            }

            StringBuilder sb = new ();

            sb.AppendLine ("> Begin: Physiology");
            sb.Append (Instance?.Physiology?.Save ());
            sb.AppendLine ("> End: Physiology");

            sb.AppendLine ("> Begin: Editor");
            sb.Append (this.SaveOptions ());
            sb.AppendLine ("> End: Editor");

            // TODO: Implement
            /*
            if (Instance?.Device_Monitor is not null) {
                sb.AppendLine ("> Begin: Cardiac Monitor");
                sb.Append (Instance.Device_Monitor.Save ());
                sb.AppendLine ("> End: Cardiac Monitor");
            }
            if (Instance?.Device_ECG is not null) {
                sb.AppendLine ("> Begin: 12 Lead ECG");
                sb.Append (Instance.Device_ECG.Save ());
                sb.AppendLine ("> End: 12 Lead ECG");
            }
            if (Instance?.Device_Defib is not null) {
                sb.AppendLine ("> Begin: Defibrillator");
                sb.Append (Instance.Device_Defib.Save ());
                sb.AppendLine ("> End: Defibrillator");
            }
            if (Instance?.Device_IABP is not null) {
                sb.AppendLine ("> Begin: Intra-aortic Balloon Pump");
                sb.Append (Instance.Device_IABP.Save ());
                sb.AppendLine ("> End: Intra-aortic Balloon Pump");
            }
            */

            using StreamWriter sw = new (stream);
            await sw.WriteLineAsync (".ii:t1");                                           // Metadata (type 1 savefile)
            await sw.WriteLineAsync (Encryption.HashSHA256 (sb.ToString ().Trim ()));     // Hash for validation
            await sw.WriteAsync (Encryption.EncryptAES (sb.ToString ().Trim ()));         // Savefile data encrypted with AES
            await sw.FlushAsync ();

            sw.Close ();
            stream.Close ();
        }

        private string SaveOptions () {
            StringBuilder sWrite = new ();

            sWrite.AppendLine (String.Format ("{0}:{1}", "checkDefaultVitals", chkDefaultVitals.Active));

            return sWrite.ToString ();
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

        private void MenuNewSimulation_Click (object? sender, EventArgs e)
            => _ = RefreshScenario (true);

        private void MenuLoadFile_Click (object? sender, EventArgs e)
            => _ = LoadFile ();

        private void MenuSaveFile_Click (object? sender, EventArgs e)
            => _ = SaveFile ();

        private void MenuPauseSimulation_Click (object? sender, EventArgs e)
            => PauseSimulation ();

        private void MenuToggleFullscreen_Click (object? sender, EventArgs e)
            => ToggleFullscreen ();

        private void MenuExit_Click (object? sender, EventArgs e)
            => _ = Exit ();

        private void MenuToggleAudio_Click (object? sender, EventArgs e)
            => _ = ToggleAudio ();

        private void MenuSetLanguage_Click (object? sender, EventArgs e)
            => _ = DialogLanguage (true);

        private void MenuMirrorDeactivate_Click (object? sender, EventArgs e)
            => _ = MirrorDeactivate ();

        private void MenuMirrorBroadcast_Click (object? sender, EventArgs e)
            => _ = MirrorBroadcast ();

        private void MenuMirrorReceive_Click (object? sender, EventArgs e)
            => _ = MirrorReceive ();

        private void MenuAbout_Click (object? sender, EventArgs e)
            => _ = DialogAbout ();

        private void MenuCheckUpdate_Click (object? sender, EventArgs e)
            => _ = CheckUpgrade ();

        private void ButtonDeviceMonitor_Click (object? sender, EventArgs e)
            => _ = InitDeviceMonitor ();

        private void ButtonDeviceDefib_Click (object? sender, EventArgs e)
            => _ = InitDeviceDefib ();

        private void ButtonDeviceECG_Click (object? sender, EventArgs e)
            => _ = InitDeviceECG ();

        private void ButtonDeviceIABP_Click (object? sender, EventArgs e)
            => _ = InitDeviceIABP ();

        private void ButtonDeviceEFM_Click (object? sender, EventArgs e)
            => _ = InitDeviceEFM ();

        private void ButtonOptionsHide_Click (object? sender, EventArgs e)
            => _ = ToggleHideDevices ();

        private void ButtonPreviousStep_Click (object? sender, EventArgs e)
            => _ = PreviousStep ();

        private void ButtonNextStep_Click (object? sender, EventArgs e)
            => _ = NextStep ();

        private void ButtonPauseStep_Click (object? sender, EventArgs e)
            => _ = PauseStep ();

        private void ButtonPlayStep_Click (object? sender, EventArgs e)
            => _ = PlayStep ();

        private void ButtonResetParameters_Click (object? sender, EventArgs e)
            => _ = ResetPhysiologyParameters ();

        private void ButtonApplyParameters_Click (object? sender, EventArgs e)
            => _ = ApplyPhysiologyParameters ();

        private void OnKeyPressEvent (object? sender, KeyPressEventArgs args) {
            
            /* GUI Keyboard Shortcuts */

            // Event.State is a bitmask of all modifier buttons; check using & AND
            
            // Control
            if ((args.Event.State.GetHashCode() & ModifierType.ControlMask.GetHashCode()) != 0) {
                switch (args.Event.Key) {
                    case Key.N:
                    case Key.n:
                        MenuNewSimulation_Click (this, EventArgs.Empty);
                        break;
                    
                    case Key.O:
                    case Key.o:
                        MenuLoadFile_Click (this, EventArgs.Empty);
                        break;
                    
                    case Key.S:
                    case Key.s:
                        MenuSaveFile_Click(this, EventArgs.Empty);
                        break;
                    
                    case Key.A:
                    case Key.a:
                        MenuToggleAudio_Click(this, EventArgs.Empty);
                        break;
                }
            }
            
            // Alt
            if ((args.Event.State.GetHashCode () & ModifierType.Mod1Mask.GetHashCode ()) != 0) {
                switch (args.Event.Key) {
                    case Key.Return:
                        MenuToggleFullscreen_Click(this, EventArgs.Empty);
                        break;
                }
            }
            
            // No Modifiers
            switch (args.Event.Key) {
                case Key.Pause:
                    MenuPauseSimulation_Click (this, EventArgs.Empty);
                    break;
            }

            //Console.WriteLine($"{args.Event.State} {args.Event.Key}");
        }
        
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