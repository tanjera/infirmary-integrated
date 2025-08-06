using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using II;
using II.Rhythm;
using II.Waveform;

namespace IISIM.Windows {

    /// <summary>
    /// Interaction logic for DeviceMonitor.xaml
    /// </summary>
    public partial class DeviceECG : Window {
        public App? Instance { get; set; }

        public States State;

        private Color.Schemes colorScheme = Color.Schemes.Dark;
        private ImageBrush? gridBackground;

        private List<Controls.ECGTracing> listTracings = new ();

        public II.Timer
            TimerAlarm = new (),
            TimerTracing = new (),
            TimerNumerics_Cardiac = new (),
            TimerNumerics_Respiratory = new (),
            TimerAncillary_Delay = new ();

        /* Variables controlling for audio alarms */
        public SoundPlayer? AudioPlayer;

        public enum States {
            Running,
            Paused,
            Closed
        }

        public DeviceECG () {
            InitializeComponent ();
        }

        public DeviceECG (App? app) {
            InitializeComponent ();

            DataContext = this;

            Instance = app;

            Closed += this.OnClosed;
            Closing += this.OnClosing;

            InitTimers ();

            State = States.Running;

            InitInterface ();
            SetColorScheme (colorScheme);
        }

        ~DeviceECG () {
            Dispose ();
        }

        public virtual void Dispose () {
            /* Clean subscriptions from the Main Timer */
            if (Instance is not null) {
                Instance.Timer_Main.Elapsed -= TimerTracing.Process;
                Instance.Timer_Main.Elapsed -= TimerNumerics_Cardiac.Process;
                Instance.Timer_Main.Elapsed -= TimerNumerics_Respiratory.Process;
                Instance.Timer_Main.Elapsed -= TimerAncillary_Delay.Process;
            }

            /* Dispose of local Timers */
            TimerTracing.Dispose ();
            TimerNumerics_Cardiac.Dispose ();
            TimerNumerics_Respiratory.Dispose ();
            TimerAncillary_Delay.Dispose ();

            /* Unsubscribe from the main Patient event listing */
            if (Instance?.Physiology != null)
                Instance.Physiology.PhysiologyEvent -= OnPhysiologyEvent;
        }

        public virtual void DisposeAudio () {
            AudioPlayer?.Dispose ();
        }

        public virtual void InitTimers () {
            if (Instance is null)
                return;

            /* TimerAncillary_Delay is attached/detached to events in the Devices for their
             * specific uses (e.g. IABP priming, Defib charging, etc.) ... only want to link it
             * to Timer_Main, otherwise do not set, start, or link to any events here!
             */
            Instance.Timer_Main.Elapsed += TimerAncillary_Delay.Process;

            Instance.Timer_Main.Elapsed += TimerAlarm.Process;
            Instance.Timer_Main.Elapsed += TimerTracing.Process;
            Instance.Timer_Main.Elapsed += TimerNumerics_Cardiac.Process;
            Instance.Timer_Main.Elapsed += TimerNumerics_Respiratory.Process;

            TimerAlarm.Tick += OnTick_Alarm;
            TimerTracing.Tick += OnTick_Tracing;
            TimerNumerics_Cardiac.Tick += OnTick_Vitals_Cardiac;
            TimerNumerics_Respiratory.Tick += OnTick_Vitals_Respiratory;

            TimerAlarm.Set (2500);
            TimerTracing.Set (Draw.RefreshTime);
            TimerNumerics_Cardiac.Set (3000);
            TimerNumerics_Respiratory.Set (5000);

            TimerAlarm.Start ();
            TimerTracing.Start ();
            TimerNumerics_Cardiac.Start ();
            TimerNumerics_Respiratory.Start ();
        }

        private void InitInterface () {
            if (Instance?.Language is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitInterface)}");
                return;
            }

            /* Populate UI strings per language selection */

            wdwDeviceECG.Title = Instance.Language.Localize ("ECG:WindowTitle");
            menuDevice.Header = Instance.Language.Localize ("MENU:MenuDeviceOptions");
            menuPauseDevice.Header = Instance.Language.Localize ("MENU:MenuPauseDevice");
            menuToggleFullscreen.Header = Instance.Language.Localize ("MENU:MenuToggleFullscreen");
            menuCloseDevice.Header = Instance.Language.Localize ("MENU:MenuCloseDevice");
            menuColor.Header = Instance.Language.Localize ("MENU:MenuColorScheme");
            menuColorGrid.Header = Instance.Language.Localize ("MENU:MenuColorSchemeGrid");
            menuColorLight.Header = Instance.Language.Localize ("MENU:MenuColorSchemeLight");
            menuColorDark.Header = Instance.Language.Localize ("MENU:MenuColorSchemeDark");

            /* Set background image for grid lines */

            gridBackground = new ImageBrush (new BitmapImage (new Uri ("pack://application:,,,/Resources/12L_ECG_Grid.png")));
            gridBackground.Stretch = Stretch.Fill;

            /* 12 Lead ECG Interface layout */
            List<Lead.Values> listLeads = new ();
            foreach (Lead.Values v in Enum.GetValues (typeof (Lead.Values))) {
                if (v.ToString ().StartsWith ("ECG"))
                    listLeads.Add (v);
            }

            int amtRows = 3,
                amtColumns = 4,
                indexLeads = 0;

            // Set grid's row and column definitions
            for (int i = 0; i < amtRows; i++)
                layoutGrid.RowDefinitions.Add (new RowDefinition ());
            for (int i = 0; i < amtColumns; i++)
                layoutGrid.ColumnDefinitions.Add (new ColumnDefinition ());

            // Populate the grid with tracings for each lead
            for (int iColumns = 0; iColumns < amtColumns; iColumns++) {
                for (int iRows = 0; iRows < amtRows && indexLeads < listLeads.Count; iRows++) {
                    listTracings.Add (new Controls.ECGTracing (Instance, new Strip (listLeads [indexLeads], (4 - iColumns) * 2.5f, 2.5f), colorScheme));
                    listTracings [indexLeads].SetValue (Grid.ColumnProperty, iColumns);
                    listTracings [indexLeads].SetValue (Grid.RowProperty, iRows);
                    layoutGrid.Children.Add (listTracings [indexLeads]);
                    indexLeads++;
                }
            }

            // Add Lead II running along bottom spanning all columns
            Controls.ECGTracing leadII = new (Instance, new Strip (Lead.Values.ECG_II, 10f), colorScheme);
            leadII.SetValue (Grid.ColumnProperty, 0);
            leadII.SetValue (Grid.RowProperty, 4);
            leadII.SetValue (Grid.ColumnSpanProperty, 4);
            listTracings.Add (leadII);
            layoutGrid.RowDefinitions.Add (new RowDefinition ());
            layoutGrid.Children.Add (listTracings [indexLeads]);


            /* Init Hotkeys (Commands & InputBinding) */

            RoutedCommand
                cmdMenuTogglePause_Click = new (),
                cmdMenuToggleFullscreen_Click = new (),
                cmdMenuColorGrid_Click = new (),
                cmdMenuColorScheme_Light = new (),
                cmdMenuColorScheme_Dark = new ();

            cmdMenuTogglePause_Click.InputGestures.Add (new KeyGesture (Key.Pause));
            CommandBindings.Add (new CommandBinding (cmdMenuTogglePause_Click, MenuTogglePause_Click));

            cmdMenuToggleFullscreen_Click.InputGestures.Add (new KeyGesture (Key.Enter, ModifierKeys.Alt));
            CommandBindings.Add (new CommandBinding (cmdMenuToggleFullscreen_Click, MenuToggleFullscreen_Click));

            cmdMenuColorGrid_Click.InputGestures.Add (new KeyGesture (Key.F1));
            CommandBindings.Add (new CommandBinding (cmdMenuColorGrid_Click, MenuColorGrid_Click));

            cmdMenuColorScheme_Light.InputGestures.Add (new KeyGesture (Key.F2));
            CommandBindings.Add (new CommandBinding (cmdMenuColorScheme_Light, MenuColorScheme_Light));

            cmdMenuColorScheme_Dark.InputGestures.Add (new KeyGesture (Key.F3));
            CommandBindings.Add (new CommandBinding (cmdMenuColorScheme_Dark, MenuColorScheme_Dark));
        }

        private void UpdateInterface () {
            // TODO: Implement
        }

        public Task Load (string inc) {
            // TODO: Implement
            return Task.CompletedTask;
        }

        public string Save () {
            // TODO: Implement
            return "";
        }

        public void SetColorScheme_Grid () => SetColorScheme (Color.Schemes.Grid);

        public void SetColorScheme_Light () => SetColorScheme (Color.Schemes.Light);

        public void SetColorScheme_Dark () => SetColorScheme (Color.Schemes.Dark);

        public void SetColorScheme (Color.Schemes scheme) {
            colorScheme = scheme;
            UpdateInterface ();
        }

        public virtual void TogglePause () {
            if (State == States.Running)
                State = States.Paused;
            else if (State == States.Paused)
                State = States.Running;
        }

        public virtual void OnClosed (object? sender, EventArgs e) {
            State = States.Closed;

            Dispose ();
        }

        public void ToggleFullscreen () {
        }

        private void MenuToggleFullscreen_Click (object s, RoutedEventArgs e)
            => ToggleFullscreen ();

        private void MenuClose_Click (object s, RoutedEventArgs e) {
            // TODO: Implement
        }

        private void MenuTogglePause_Click (object s, RoutedEventArgs e)
            => TogglePause ();

        private void MenuColorGrid_Click (object sender, RoutedEventArgs e) {
            // TODO: Implement
        }

        private void MenuColorScheme_Light (object sender, RoutedEventArgs e) {
            // TODO: Implement
        }

        private void MenuColorScheme_Dark (object sender, RoutedEventArgs e) {
            // TODO: Implement
        }

        public virtual void OnClosing (object? sender, CancelEventArgs e) {
            TimerAlarm?.Dispose ();
            DisposeAudio ();

            // TODO: Implement
        }

        public virtual void OnTick_Alarm (object? sender, EventArgs e) {
        }

        public void OnTick_Tracing (object? sender, EventArgs e) {
            // TODO: Implement
        }

        public virtual void OnTick_Vitals_Cardiac (object? sender, EventArgs e) {
        }

        public virtual void OnTick_Vitals_Respiratory (object? sender, EventArgs e) {
        }

        public void OnPhysiologyEvent (object? sender, Physiology.PhysiologyEventArgs e) {
            // TODO: Implement
        }
    }
}