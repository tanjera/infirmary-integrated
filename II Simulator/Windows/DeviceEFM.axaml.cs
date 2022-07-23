using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
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
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private void InitInterface () {
            /* Populate UI strings per language selection */
            if (Instance is not null) {
                this.FindControl<Window> ("wdwDeviceEFM").Title = Instance.Language.Localize ("EFM:WindowTitle");
                this.FindControl<MenuItem> ("menuDevice").Header = Instance.Language.Localize ("MENU:MenuDeviceOptions");
                this.FindControl<MenuItem> ("menuPauseDevice").Header = Instance.Language.Localize ("MENU:MenuPauseDevice");
                this.FindControl<MenuItem> ("menuCloseDevice").Header = Instance.Language.Localize ("MENU:MenuCloseDevice");
                this.FindControl<MenuItem> ("menuStripSpeed").Header = Instance.Language.Localize ("MENU:StripSpeed");
                this.FindControl<MenuItem> ("menuStripSpeedx1").Header = Instance.Language.Localize ("MENU:StripSpeedx1");
                this.FindControl<MenuItem> ("menuStripxSpeedx10").Header = Instance.Language.Localize ("MENU:StripSpeedx10");
                this.FindControl<MenuItem> ("menuStripxSpeedx25").Header = Instance.Language.Localize ("MENU:StripSpeedx25");
                this.FindControl<MenuItem> ("menuColor").Header = Instance.Language.Localize ("MENU:MenuColorScheme");
                this.FindControl<MenuItem> ("menuColorLight").Header = Instance.Language.Localize ("MENU:MenuColorSchemeLight");
                this.FindControl<MenuItem> ("menuColorDark").Header = Instance.Language.Localize ("MENU:MenuColorSchemeDark");
            }

            Grid displayGrid = this.FindControl<Grid> ("displayGrid");

            // Instantiate and load backgroung images
            var assets = AvaloniaLocator.Current.GetService<Avalonia.Platform.IAssetLoader> ();
            gridFHR = new ImageBrush (new Bitmap (assets.Open (new Uri ("avares://Infirmary Integrated/Resources/FHR_Grid.png"))));
            gridFHR.Stretch = Stretch.Fill;

            gridToco = new ImageBrush (new Bitmap (assets.Open (new Uri ("avares://Infirmary Integrated/Resources/Toco_Grid.png"))));
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

                Window window = this.FindControl<Window> ("wdwDeviceEFM");
                window.Background = Color.GetBackground (Color.Devices.DeviceEFM, colorScheme);
            });
        }

        public void Load_Process (string inc) {
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

        public void SetColorScheme (Color.Schemes scheme) {
            colorScheme = scheme;
            UpdateInterface ();
        }

        private void SetStripSpeed (int multiplier) {
            _ = Instance?.Patient?.SetTimerMultiplier_Obstetric (multiplier);
        }

        public override void TogglePause () {
            base.TogglePause ();

            if (State == States.Running)
                listTracings.ForEach (c => c.Strip?.Unpause ());
        }

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

        public override void OnTick_Tracing (object? sender, EventArgs e) {
            if (State != States.Running)
                return;

            for (int i = 0; i < listTracings.Count; i++) {
                listTracings [i].Strip?.Scroll (Instance?.Patient?.TimerObstetric_Multiplier);
                Dispatcher.UIThread.InvokeAsync (listTracings [i].DrawTracing);
            }
        }

        public override void OnPatientEvent (object? sender, Patient.PatientEventArgs e) {
            switch (e.EventType) {
                default: break;
                case Patient.PatientEventTypes.Obstetric_Baseline:
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Obstetric_Baseline (Instance?.Patient));
                    break;

                case Patient.PatientEventTypes.Obstetric_Contraction_Start:
                    listTracings.ForEach (c => c.Strip?.ClearFuture (Instance?.Patient));
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Obstetric_Contraction_Start (Instance?.Patient));
                    break;

                case Patient.PatientEventTypes.Obstetric_Contraction_End:
                    listTracings.ForEach (c => c.Strip?.ClearFuture (Instance?.Patient));
                    listTracings.ForEach (c => c.Strip?.Add_Beat__Obstetric_Baseline (Instance?.Patient));
                    break;
            }
        }
    }
}