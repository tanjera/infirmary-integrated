/* Infirmary Integrated Simulator
 * By Ibi Keller (Tanjera), (c) 2023
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;

using II;
using II.Rhythm;
using II.Waveform;

namespace IISIM {

    public partial class DeviceEFM : DeviceWindow {
        private Color.Schemes colorScheme = Color.Schemes.Light;

        private List<Controls.EFMTracing> listTracings = new ();

        private ImageBrush? gridFHR, gridToco;

        public DeviceEFM () {
            InitializeComponent ();
        }

        public DeviceEFM (App? app) : base (app) {
            InitializeComponent ();
#if DEBUG
            this.AttachDevTools ();
#endif

            DataContext = this;

            InitInterface ();
#if DEBUG
            SetStripSpeed (25);                         // Debug default Strip speed
#else
            SetStripSpeed (10);                         // Set default Strip speed
#endif
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private void InitInterface () {
            if (Instance is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitInterface)}");
                return;
            }

            /* Populate UI strings per language selection */
            this.GetControl<Window> ("wdwDeviceEFM").Title = Instance.Language.Localize ("EFM:WindowTitle");
            this.GetControl<MenuItem> ("menuDevice").Header = Instance.Language.Localize ("MENU:MenuDeviceOptions");
            this.GetControl<MenuItem> ("menuPauseDevice").Header = Instance.Language.Localize ("MENU:MenuPauseDevice");
            this.GetControl<MenuItem> ("menuToggleFullscreen").Header = Instance.Language.Localize ("MENU:MenuToggleFullscreen");
            this.GetControl<MenuItem> ("menuCloseDevice").Header = Instance.Language.Localize ("MENU:MenuCloseDevice");
            this.GetControl<MenuItem> ("menuStripSpeed").Header = Instance.Language.Localize ("MENU:StripSpeed");
            this.GetControl<MenuItem> ("menuStripSpeedx1").Header = Instance.Language.Localize ("MENU:StripSpeedx1");
            this.GetControl<MenuItem> ("menuStripxSpeedx10").Header = Instance.Language.Localize ("MENU:StripSpeedx10");
            this.GetControl<MenuItem> ("menuStripxSpeedx25").Header = Instance.Language.Localize ("MENU:StripSpeedx25");
            //this.GetControl<MenuItem> ("menuColor").Header = Instance.Language.Localize ("MENU:MenuColorScheme");
            //this.GetControl<MenuItem> ("menuColorLight").Header = Instance.Language.Localize ("MENU:MenuColorSchemeLight");
            //this.GetControl<MenuItem> ("menuColorDark").Header = Instance.Language.Localize ("MENU:MenuColorSchemeDark");

            Grid displayGrid = this.GetControl<Grid> ("displayGrid");

            // Instantiate and load backgroung images
            gridFHR = new ImageBrush (new Bitmap (AssetLoader.Open (new Uri ("avares://Infirmary Integrated/Resources/FHR_Grid.png"))));
            gridFHR.Stretch = Stretch.Fill;

            gridToco = new ImageBrush (new Bitmap (AssetLoader.Open (new Uri ("avares://Infirmary Integrated/Resources/Toco_Grid.png"))));
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
        }

        private void UpdateInterface () {
            Dispatcher.UIThread.InvokeAsync (() => {
                for (int i = 0; i < listTracings.Count; i++)
                    listTracings [i].SetColorScheme (colorScheme);

                Window window = this.GetControl<Window> ("wdwDeviceEFM");
                window.Background = Color.GetBackground (Color.Devices.DeviceEFM, colorScheme);
            });
        }

        public void Load (string inc) {
            using StringReader sRead = new (inc);

            try {
                string? line;
                while ((line = sRead.ReadLine ()) != null) {
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

        public void SetColorScheme_Light () => SetColorScheme (Color.Schemes.Light);

        public void SetColorScheme_Dark () => SetColorScheme (Color.Schemes.Dark);

        public void SetColorScheme (Color.Schemes scheme) {
            colorScheme = scheme;
            UpdateInterface ();
        }

        private void SetStripSpeed_x1 () => SetStripSpeed (1);

        private void SetStripSpeed_x10 () => SetStripSpeed (10);

        private void SetStripSpeed_x25 () => SetStripSpeed (25);

        private void SetStripSpeed (int multiplier) {
            _ = Instance?.Physiology?.SetTimerMultiplier_Obstetric (multiplier);
        }

        public void ToggleFullscreen () {
            if (WindowState == WindowState.FullScreen)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.FullScreen;
        }

        public override void TogglePause () {
            base.TogglePause ();

            if (State == States.Running)
                listTracings.ForEach (c => c.Strip?.Unpause ());
        }

        private void MenuToggleFullscreen_Click (object s, RoutedEventArgs e)
            => ToggleFullscreen ();

        private void MenuClose_Click (object s, RoutedEventArgs e)
            => this.Close ();

        private void MenuTogglePause_Click (object s, RoutedEventArgs e)
            => TogglePause ();

        private void MenuStripSpeed_x1 (object sender, RoutedEventArgs e)
            => SetStripSpeed (1);

        private void MenuStripSpeed_x10 (object sender, RoutedEventArgs e)
            => SetStripSpeed (10);

        private void MenuStripSpeed_x25 (object sender, RoutedEventArgs e)
            => SetStripSpeed (25);

        private void MenuColorScheme_Light (object sender, RoutedEventArgs e)
            => SetColorScheme (Color.Schemes.Light);

        private void MenuColorScheme_Dark (object sender, RoutedEventArgs e)
            => SetColorScheme (Color.Schemes.Dark);

        public override void OnClosing (object? sender, CancelEventArgs e) {
            base.OnClosing (sender, e);

            if (Instance?.Physiology is not null)
                Instance.Physiology.PhysiologyEvent -= OnPhysiologyEvent;
        }

        public override void OnTick_Tracing (object? sender, EventArgs e) {
            if (State != States.Running)
                return;

            for (int i = 0; i < listTracings.Count; i++) {
                listTracings [i].Strip?.Scroll (Instance?.Physiology?.TimerObstetric_Multiplier);
                Dispatcher.UIThread.InvokeAsync (listTracings [i].DrawTracing);
            }
        }

        public override void OnPhysiologyEvent (object? sender, Physiology.PhysiologyEventArgs e) {
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