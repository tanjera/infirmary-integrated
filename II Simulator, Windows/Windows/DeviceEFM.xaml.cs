using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
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
using II.Settings;
using II.Waveform;

using IISIM.Classes;

namespace IISIM.Windows {

    /// <summary>
    /// Interaction logic for DeviceEFM.xaml
    /// </summary>
    public partial class DeviceEFM : Window {
        public App? Instance { get; set; }

        public States State;

        private Color.Schemes colorScheme = Color.Schemes.Light;

        private List<Controls.EFMTracing> listTracings = new ();

        private ImageBrush? gridFHR, gridToco;

        public II.Timer TimerTracing = new ();

        public enum States {
            Running,
            Paused,
            Closed
        }

        public DeviceEFM () {
            InitializeComponent ();
        }

        public DeviceEFM (App? app) {
            InitializeComponent ();

            Instance = app;

            Closed += this.OnClosed;
            Closing += this.OnClosing;

            InitTimers ();

            State = States.Running;

            InitInterface ();

#if DEBUG
            SetStripSpeed (25);                         // Debug default Strip speed
#else
            SetStripSpeed (10);                         // Set default Strip speed
#endif
        }

        ~DeviceEFM () {
            Dispose ();
        }

        public virtual void Dispose () {
            /* Clean subscriptions from the Main Timer */
            if (Instance is not null) {
                Instance.Timer_Main.Elapsed -= TimerTracing.Process;
            }

            /* Dispose of local Timers */
            TimerTracing.Dispose ();

            /* Unsubscribe from the main Patient event listing */
            if (Instance?.Physiology != null)
                Instance.Physiology.PhysiologyEvent -= OnPhysiologyEvent;
        }

        public virtual void InitTimers () {
            if (Instance is null)
                return;

            /* EFM only uses TimerTracing... no Cardiac, Respiratory, of Alarms to process */

            Instance.Timer_Main.Elapsed += TimerTracing.Process;
            TimerTracing.Tick += OnTick_Tracing;
            TimerTracing.Set (Draw.RefreshTime);
            TimerTracing.Start ();
        }

        private void InitInterface () {
            if (Instance?.Language is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitInterface)}");
                return;
            }

            /* Populate UI strings per language selection */

            wdwDeviceEFM.Title = Instance.Language.Localize ("EFM:WindowTitle");
            menuDevice.Header = Instance.Language.Localize ("MENU:MenuDeviceOptions");
            menuPauseDevice.Header = Instance.Language.Localize ("MENU:MenuPauseDevice");
            menuToggleFullscreen.Header = Instance.Language.Localize ("MENU:MenuToggleFullscreen");
            menuCloseDevice.Header = Instance.Language.Localize ("MENU:MenuCloseDevice");

            menuStripSpeed.Header = Instance.Language.Localize ("MENU:StripSpeed");
            menuStripSpeedx1.Header = Instance.Language.Localize ("MENU:StripSpeedx1");
            menuStripSpeedx10.Header = Instance.Language.Localize ("MENU:StripSpeedx10");
            menuStripSpeedx25.Header = Instance.Language.Localize ("MENU:StripSpeedx25");

            /* Init Tracing layout */

            // Instantiate and load backgroung images

            gridFHR = new ImageBrush (new BitmapImage (new Uri ("pack://application:,,,/Resources/FHR_Grid.png")));
            gridFHR.Stretch = Stretch.Fill;

            gridToco = new ImageBrush (new BitmapImage (new Uri ("pack://application:,,,/Resources/Toco_Grid.png")));
            gridToco.Stretch = Stretch.Fill;

            // Instantiate and add Tracings to UI
            Controls.EFMTracing fhrTracing = new (Instance, new Strip (Lead.Values.FHR, 600d), colorScheme);
            fhrTracing.SetValue (Grid.RowProperty, 0);
            fhrTracing.SetValue (Grid.ColumnProperty, 0);
            fhrTracing.Background = gridFHR;
            listTracings.Add (fhrTracing);
            displayGrid.Children.Add (fhrTracing);

            Controls.EFMTracing tocoTracing = new (Instance, new Strip (Lead.Values.TOCO, 600d), colorScheme);
            tocoTracing.SetValue (Grid.RowProperty, 2);
            tocoTracing.SetValue (Grid.ColumnProperty, 0);
            tocoTracing.Background = gridToco;
            listTracings.Add (tocoTracing);
            displayGrid.Children.Add (tocoTracing);
            /* Init Hotkeys (Commands & InputBinding) */

            RoutedCommand
                cmdMenuTogglePause_Click = new (),
                cmdMenuToggleFullscreen_Click = new (),
                cmdMenuStripSpeed_x1 = new (),
                cmdMenuStripSpeed_x10 = new (),
                cmdMenuStripSpeed_x25 = new ();

            cmdMenuTogglePause_Click.InputGestures.Add (new KeyGesture (Key.Pause));
            CommandBindings.Add (new CommandBinding (cmdMenuTogglePause_Click, MenuTogglePause_Click));

            cmdMenuToggleFullscreen_Click.InputGestures.Add (new KeyGesture (Key.Enter, ModifierKeys.Alt));
            CommandBindings.Add (new CommandBinding (cmdMenuToggleFullscreen_Click, MenuToggleFullscreen_Click));

            cmdMenuStripSpeed_x1.InputGestures.Add (new KeyGesture (Key.D1, ModifierKeys.Control));
            CommandBindings.Add (new CommandBinding (cmdMenuStripSpeed_x1, MenuStripSpeed_x1));

            cmdMenuStripSpeed_x10.InputGestures.Add (new KeyGesture (Key.D2, ModifierKeys.Control));
            CommandBindings.Add (new CommandBinding (cmdMenuStripSpeed_x10, MenuStripSpeed_x10));

            cmdMenuStripSpeed_x25.InputGestures.Add (new KeyGesture (Key.D3, ModifierKeys.Control));
            CommandBindings.Add (new CommandBinding (cmdMenuStripSpeed_x25, MenuStripSpeed_x25));
        }

        public async Task Load (string inc) {
            using StringReader sRead = new (inc);
            List<string> numericTypes = new (),
                         tracingTypes = new ();

            try {
                string? line;
                while ((line = await sRead.ReadLineAsync ()) != null) {
                    if (line.Contains (':')) {
                        string pName = line.Substring (0, line.IndexOf (':')),
                                pValue = line.Substring (line.IndexOf (':') + 1);
                        switch (pName) {
                            default: break;
                        }
                    }
                }
            } catch {
            } finally {
                sRead.Close ();
            }
        }

        public string Save () {
            StringBuilder sWrite = new ();

            return sWrite.ToString ();
        }

        public void TogglePause () {
            if (State == States.Running)
                State = States.Paused;
            else if (State == States.Paused)
                State = States.Running;
        }

        public void OnClosed (object? sender, EventArgs e) {
            State = States.Closed;

            Dispose ();
        }

        public void OnClosing (object? sender, CancelEventArgs e) {
            if (Instance?.Physiology is not null)
                Instance.Physiology.PhysiologyEvent -= OnPhysiologyEvent;
        }

        private void SetStripSpeed (int multiplier) {
            _ = Instance?.Physiology?.SetTimerMultiplier_Obstetric (multiplier);

            menuStripSpeedx1.IsChecked = multiplier == 1;
            menuStripSpeedx10.IsChecked = multiplier == 10;
            menuStripSpeedx25.IsChecked = multiplier == 25;
        }

        public void ToggleFullscreen () {
            if (wdwDeviceEFM.WindowState == System.Windows.WindowState.Maximized)
                wdwDeviceEFM.WindowState = System.Windows.WindowState.Normal;
            else
                wdwDeviceEFM.WindowState = System.Windows.WindowState.Maximized;
        }

        private void MenuToggleFullscreen_Click (object s, RoutedEventArgs e)
            => ToggleFullscreen ();

        private void MenuClose_Click (object s, RoutedEventArgs e) {
            wdwDeviceEFM.Close ();
        }

        private void MenuTogglePause_Click (object s, RoutedEventArgs e)
            => TogglePause ();

        private void MenuStripSpeed_x1 (object sender, RoutedEventArgs e)
            => SetStripSpeed (1);

        private void MenuStripSpeed_x10 (object sender, RoutedEventArgs e)
            => SetStripSpeed (10);

        private void MenuStripSpeed_x25 (object sender, RoutedEventArgs e)
            => SetStripSpeed (25);

        public void OnTick_Tracing (object? sender, EventArgs e) {
            for (int i = 0; i < listTracings.Count; i++) {
                listTracings [i].Strip?.Scroll (Instance?.Physiology?.TimerObstetric_Multiplier);

                if (State == States.Running) {  // Only pauses advancement of tracing; simulation still active!
                    App.Current.Dispatcher.InvokeAsync (listTracings [i].DrawTracing);
                }
            }
        }

        public void OnPhysiologyEvent (object? sender, Physiology.PhysiologyEventArgs e) {
            switch (e.EventType) {
                default: break;

                case Physiology.PhysiologyEventTypes.Obstetric_Baseline:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Obstetric_Baseline (Instance?.Physiology));
                    break;

                case Physiology.PhysiologyEventTypes.Obstetric_Fetal_Baseline:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Obstetric_Fetal_Baseline (Instance?.Physiology));
                    break;

                case Physiology.PhysiologyEventTypes.Obstetric_Contraction_Start:
                    listTracings.ForEach (c => c.Strip?.ClearFuture (Instance?.Physiology));
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Obstetric_Contraction_Start (Instance?.Physiology));
                    break;

                case Physiology.PhysiologyEventTypes.Obstetric_Contraction_End:
                    listTracings.ForEach (c => c.Strip?.ClearFuture (Instance?.Physiology));
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Obstetric_Baseline (Instance?.Physiology));
                    break;
            }
        }
    }
}