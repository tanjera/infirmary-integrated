using System.Collections.Generic;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

using II;
using II.Localization;
using II.Rhythm;

namespace II_Windows.Controls {

    /// <summary>
    /// Interaction logic for Tracing.xaml
    /// </summary>
    public partial class DefibTracing : UserControl {
        public Strip Strip;
        public Lead Lead { get { return Strip.Lead; } }

        /* Drawing variables, offsets and multipliers */
        private System.Drawing.Brush tracingPen = System.Drawing.Brushes.Black;
        private System.Windows.Media.Brush tracingBrush = System.Windows.Media.Brushes.Black;
        private System.Drawing.Brush referencePen = System.Drawing.Brushes.DarkGray;

        private System.Drawing.Point drawOffset;
        private System.Drawing.PointF drawMultiplier;

        private MenuItem menuZeroTransducer;
        private MenuItem menuToggleAutoScale;

        public DefibTracing (Strip strip) {
            InitializeComponent ();
            DataContext = this;

            Strip = strip;

            InitInterface ();
            UpdateInterface (null, null);
        }

        private void InitInterface () {

            // Populate UI strings per language selection
            Language.Values l = App.Language.Value;

            // Context Menu (right-click menu!)
            ContextMenu contextMenu = new ContextMenu ();
            imgTracing.ContextMenu = contextMenu;
            lblLead.ContextMenu = contextMenu;

            menuZeroTransducer = new MenuItem ();
            menuZeroTransducer.Header = App.Language.Localize ("MENU:MenuZeroTransducer");
            menuZeroTransducer.Click += MenuZeroTransducer_Click;
            contextMenu.Items.Add (menuZeroTransducer);

            contextMenu.Items.Add (new Separator ());

            MenuItem menuAddTracing = new MenuItem ();
            menuAddTracing.Header = App.Language.Localize ("MENU:MenuAddTracing");
            menuAddTracing.Click += MenuAddTracing_Click;
            contextMenu.Items.Add (menuAddTracing);

            MenuItem menuRemoveTracing = new MenuItem ();
            menuRemoveTracing.Header = App.Language.Localize ("MENU:MenuRemoveTracing");
            menuRemoveTracing.Click += MenuRemoveTracing_Click;
            contextMenu.Items.Add (menuRemoveTracing);

            contextMenu.Items.Add (new Separator ());

            MenuItem menuIncreaseAmplitude = new MenuItem ();
            menuIncreaseAmplitude.Header = App.Language.Localize ("MENU:IncreaseAmplitude");
            menuIncreaseAmplitude.Click += MenuIncreaseAmplitude_Click;
            contextMenu.Items.Add (menuIncreaseAmplitude);

            MenuItem menuDecreaseAmplitude = new MenuItem ();
            menuDecreaseAmplitude.Header = App.Language.Localize ("MENU:DecreaseAmplitude");
            menuDecreaseAmplitude.Click += MenuDecreaseAmplitude_Click;
            contextMenu.Items.Add (menuDecreaseAmplitude);

            contextMenu.Items.Add (new Separator ());

            menuToggleAutoScale = new MenuItem ();
            menuToggleAutoScale.Header = App.Language.Localize ("MENU:ToggleAutoScaling");
            menuToggleAutoScale.Click += MenuToggleAutoScale_Click;
            contextMenu.Items.Add (menuToggleAutoScale);

            contextMenu.Items.Add (new Separator ());
            MenuItem menuSelectInput = new MenuItem (),
                     menuECGLeads = new MenuItem ();
            menuSelectInput.Header = App.Language.Localize ("MENU:MenuSelectInputSource");
            menuECGLeads.Header = App.Language.Localize ("TRACING:ECG");
            menuSelectInput.Items.Add (menuECGLeads);

            foreach (Lead.Values v in Enum.GetValues (typeof (Lead.Values))) {

                // Only include certain leads- e.g. bedside monitors don't interface with IABP or EFM
                string el = v.ToString ();
                if (!el.StartsWith ("ECG") && el != "SPO2" && el != "CVP" && el != "ABP"
                    && el != "PA" && el != "RR" && el != "ETCO2")
                    continue;

                MenuItem mi = new MenuItem ();
                mi.Header = App.Language.Localize (Lead.LookupString (v));
                mi.Name = v.ToString ();
                mi.Click += MenuSelectInputSource;
                if (mi.Name.StartsWith ("ECG"))
                    menuECGLeads.Items.Add (mi);
                else
                    menuSelectInput.Items.Add (mi);
            }

            contextMenu.Items.Add (menuSelectInput);
        }

        private void UpdateInterface (object sender, SizeChangedEventArgs e) {
            switch (Lead.Value) {
                default:
                    tracingPen = System.Drawing.Brushes.Green;
                    tracingBrush = System.Windows.Media.Brushes.Green;
                    break;

                case Lead.Values.ABP:
                    tracingPen = System.Drawing.Brushes.Red;
                    tracingBrush = System.Windows.Media.Brushes.Red;
                    break;

                case Lead.Values.CVP:
                    tracingPen = System.Drawing.Brushes.Blue;
                    tracingBrush = System.Windows.Media.Brushes.Blue;
                    break;

                case Lead.Values.PA:
                    tracingPen = System.Drawing.Brushes.Yellow;
                    tracingBrush = System.Windows.Media.Brushes.Yellow;
                    break;

                case Lead.Values.IABP:
                    tracingPen = System.Drawing.Brushes.SkyBlue;
                    tracingBrush = System.Windows.Media.Brushes.SkyBlue;
                    break;

                case Lead.Values.RR:
                    tracingPen = System.Drawing.Brushes.Salmon;
                    tracingBrush = System.Windows.Media.Brushes.Salmon;
                    break;

                case Lead.Values.ETCO2:
                    tracingPen = System.Drawing.Brushes.Aqua;
                    tracingBrush = System.Windows.Media.Brushes.Aqua;
                    break;

                case Lead.Values.SPO2:
                    tracingPen = System.Drawing.Brushes.Orange;
                    tracingBrush = System.Windows.Media.Brushes.Orange;
                    break;
            }

            borderTracing.BorderBrush = tracingBrush;

            lblLead.Foreground = tracingBrush;
            lblLead.Content = App.Language.Localize (Lead.LookupString (Lead.Value));

            menuZeroTransducer.IsEnabled = Strip.Lead.IsTransduced ();
            menuToggleAutoScale.IsEnabled = Strip.CanScale;

            if (Strip.CanScale) {
                lblScaleAuto.Foreground = tracingBrush;
                lblScaleMin.Foreground = tracingBrush;
                lblScaleMax.Foreground = tracingBrush;

                lblScaleAuto.Content = Strip.ScaleAuto
                    ? App.Language.Localize ("TRACING:Auto")
                    : App.Language.Localize ("TRACING:Fixed");
                lblScaleMin.Content = Strip.ScaleMin.ToString ();
                lblScaleMax.Content = Strip.ScaleMax.ToString ();
            }

            CalculateOffsets ();
        }

        public void UpdateScale () {
            if (Strip.CanScale) {
                lblScaleMin.Foreground = tracingBrush;
                lblScaleMax.Foreground = tracingBrush;

                lblScaleMin.Content = Strip.ScaleMin.ToString ();
                lblScaleMax.Content = Strip.ScaleMax.ToString ();
            }
        }

        public void CalculateOffsets ()
            => Tracing.CalculateOffsets (Strip,
                cnvTracing.ActualWidth, cnvTracing.ActualHeight,
                ref drawOffset, ref drawMultiplier);

        public void DrawTracing ()
            => DrawPath (Strip.Points, tracingPen, 1);

        public void DrawReference ()
            => DrawPath (Strip.Reference, referencePen, 1);

        public void DrawPath (List<PointF> _Points, System.Drawing.Brush _Brush, float _Thickness) {
            Tracing.Init (ref Strip.Tracing, (int)cnvTracing.ActualWidth, (int)cnvTracing.ActualHeight);

            Tracing.DrawPath (_Points, Strip.Tracing, new System.Drawing.Pen (_Brush, _Thickness),
                System.Drawing.Color.Black, drawOffset, drawMultiplier);

            imgTracing.Source = Trace.BitmapToImageSource (Strip.Tracing);
        }

        private void MenuZeroTransducer_Click (object sender, RoutedEventArgs e) {
            switch (Lead.Value) {
                case Lead.Values.ABP: App.Patient.TransducerZeroed_ABP = true; return;
                case Lead.Values.CVP: App.Patient.TransducerZeroed_CVP = true; return;
                case Lead.Values.PA: App.Patient.TransducerZeroed_PA = true; return;
            }
        }

        private void MenuAddTracing_Click (object sender, RoutedEventArgs e)
            => App.Device_Defib.AddTracing ();

        private void MenuRemoveTracing_Click (object sender, RoutedEventArgs e)
            => App.Device_Defib.RemoveTracing (this);

        private void MenuIncreaseAmplitude_Click (object sender, RoutedEventArgs e) {
            Strip.IncreaseAmplitude ();
            CalculateOffsets ();
        }

        private void MenuDecreaseAmplitude_Click (object sender, RoutedEventArgs e) {
            Strip.DecreaseAmplitude ();
            CalculateOffsets ();
        }

        private void MenuToggleAutoScale_Click (object sender, RoutedEventArgs e) {
            Strip.ScaleAuto = !Strip.ScaleAuto;
            UpdateInterface (null, null);
        }

        private void MenuSelectInputSource (object sender, RoutedEventArgs e) {
            Lead.Values selectedValue;
            if (!Enum.TryParse<Lead.Values> (((MenuItem)sender).Name, out selectedValue))
                return;

            Strip.SetLead (selectedValue);
            Strip.Reset ();
            Strip.Add_Beat__Cardiac_Baseline (App.Patient);
            Strip.Add_Breath__Respiratory_Baseline (App.Patient);

            CalculateOffsets ();
            UpdateInterface (null, null);
        }

        private void cnvTracing_SizeChanged (object sender, SizeChangedEventArgs e) {
            CalculateOffsets ();

            //DrawReference ();
        }
    }
}