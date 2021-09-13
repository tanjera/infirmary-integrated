using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

using II;
using II.Localization;
using II.Rhythm;

namespace II_Avalonia.Controls {

    public partial class MonitorTracing : UserControl {
        public Strip Strip;
        public Lead Lead { get { return Strip.Lead; } }

        /* Drawing variables, offsets and multipliers */
        private System.Drawing.Brush tracingPen = System.Drawing.Brushes.Black;
        private Avalonia.Media.Brush tracingBrush = (Avalonia.Media.Brush)Avalonia.Media.Brushes.Black;
        private System.Drawing.Brush referencePen = System.Drawing.Brushes.DarkGray;

        private System.Drawing.Point drawOffset;
        private System.Drawing.PointF drawMultiplier;

        private MenuItem menuZeroTransducer;
        private MenuItem menuToggleAutoScale;

        public MonitorTracing () {
            InitializeComponent ();
        }

        public MonitorTracing (Strip strip) {
            InitializeComponent ();
            DataContext = this;

            Strip = strip;

            InitInterface ();
            UpdateInterface (null, null);
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private void InitInterface () {
            // Populate UI strings per language selection
            Language.Values l = App.Language.Value;

            // Context Menu (right-click menu!)
            ContextMenu menuContext = new ContextMenu ();
            List<object> menuitemsContext = new List<object> ();
            this.FindControl<Avalonia.Controls.Image> ("imgTracing").ContextMenu = menuContext;
            this.FindControl<Label> ("lblLead").ContextMenu = menuContext;

            menuZeroTransducer = new MenuItem ();
            menuZeroTransducer.Header = App.Language.Localize ("MENU:MenuZeroTransducer");
            menuZeroTransducer.Click += MenuZeroTransducer_Click;
            menuitemsContext.Add (menuZeroTransducer);

            menuitemsContext.Add (new Separator ());

            MenuItem menuAddTracing = new MenuItem ();
            menuAddTracing.Header = App.Language.Localize ("MENU:MenuAddTracing");
            menuAddTracing.Click += MenuAddTracing_Click;
            menuitemsContext.Add (menuAddTracing);

            MenuItem menuRemoveTracing = new MenuItem ();
            menuRemoveTracing.Header = App.Language.Localize ("MENU:MenuRemoveTracing");
            menuRemoveTracing.Click += MenuRemoveTracing_Click;
            menuitemsContext.Add (menuRemoveTracing);

            menuitemsContext.Add (new Separator ());

            MenuItem menuIncreaseAmplitude = new MenuItem ();
            menuIncreaseAmplitude.Header = App.Language.Localize ("MENU:IncreaseAmplitude");
            menuIncreaseAmplitude.Click += MenuIncreaseAmplitude_Click;
            menuitemsContext.Add (menuIncreaseAmplitude);

            MenuItem menuDecreaseAmplitude = new MenuItem ();
            menuDecreaseAmplitude.Header = App.Language.Localize ("MENU:DecreaseAmplitude");
            menuDecreaseAmplitude.Click += MenuDecreaseAmplitude_Click;
            menuitemsContext.Add (menuDecreaseAmplitude);

            menuitemsContext.Add (new Separator ());

            menuToggleAutoScale = new MenuItem ();
            menuToggleAutoScale.Header = App.Language.Localize ("MENU:ToggleAutoScaling");
            menuToggleAutoScale.Click += MenuToggleAutoScale_Click;
            menuitemsContext.Add (menuToggleAutoScale);

            menuitemsContext.Add (new Separator ());

            MenuItem menuSelectInput = new MenuItem (),
                     menuECGLeads = new MenuItem ();
            List<object> menuitemsSelectInput = new List<object> (),
                menuitemsECGLeads = new List<object> ();
            menuSelectInput.Header = App.Language.Localize ("MENU:MenuSelectInputSource");
            menuECGLeads.Header = App.Language.Localize ("TRACING:ECG");
            menuitemsSelectInput.Add (menuECGLeads);

            foreach (Lead.Values v in Enum.GetValues (typeof (Lead.Values))) {
                // Only include certain leads- e.g. bedside monitors don't interface with IABP or EFM
                string el = v.ToString ();
                if (!el.StartsWith ("ECG") && el != "SPO2" && el != "RR" && el != "ETCO2"
                    && el != "CVP" && el != "ABP" && el != "PA"
                    && el != "ICP" && el != "IAP")
                    continue;

                MenuItem mi = new MenuItem ();
                mi.Header = App.Language.Localize (Lead.LookupString (v));
                mi.Name = v.ToString ();
                mi.Click += MenuSelectInputSource;
                if (mi.Name.StartsWith ("ECG"))
                    menuitemsECGLeads.Add (mi);
                else
                    menuitemsSelectInput.Add (mi);
            }

            menuitemsContext.Add (menuSelectInput);

            menuSelectInput.Items = menuitemsSelectInput;
            menuECGLeads.Items = menuitemsECGLeads;
            menuContext.Items = menuitemsContext;
        }

        private void UpdateInterface (object? sender, EventArgs e) {
            switch (Lead.Value) {
                default:
                    tracingPen = System.Drawing.Brushes.Green;
                    tracingBrush = (Avalonia.Media.Brush)Avalonia.Media.Brushes.Green;
                    break;

                case Lead.Values.SPO2:
                    tracingPen = System.Drawing.Brushes.Orange;
                    tracingBrush = (Avalonia.Media.Brush)Avalonia.Media.Brushes.Orange;
                    break;

                case Lead.Values.RR:
                    tracingPen = System.Drawing.Brushes.Salmon;
                    tracingBrush = (Avalonia.Media.Brush)Avalonia.Media.Brushes.Salmon;
                    break;

                case Lead.Values.ETCO2:
                    tracingPen = System.Drawing.Brushes.Aqua;
                    tracingBrush = (Avalonia.Media.Brush)Avalonia.Media.Brushes.Aqua;
                    break;

                case Lead.Values.ABP:
                    tracingPen = System.Drawing.Brushes.Red;
                    tracingBrush = (Avalonia.Media.Brush)Avalonia.Media.Brushes.Red;
                    break;

                case Lead.Values.CVP:
                    tracingPen = System.Drawing.Brushes.Blue;
                    tracingBrush = (Avalonia.Media.Brush)Avalonia.Media.Brushes.Blue;
                    break;

                case Lead.Values.PA:
                    tracingPen = System.Drawing.Brushes.Yellow;
                    tracingBrush = (Avalonia.Media.Brush)Avalonia.Media.Brushes.Yellow;
                    break;

                case Lead.Values.ICP:
                    tracingPen = System.Drawing.Brushes.Khaki;
                    tracingBrush = (Avalonia.Media.Brush)Avalonia.Media.Brushes.Khaki;
                    break;

                case Lead.Values.IAP:
                    tracingPen = System.Drawing.Brushes.Aquamarine;
                    tracingBrush = (Avalonia.Media.Brush)Avalonia.Media.Brushes.Aquamarine;
                    break;

                case Lead.Values.IABP:
                    tracingPen = System.Drawing.Brushes.SkyBlue;
                    tracingBrush = (Avalonia.Media.Brush)Avalonia.Media.Brushes.SkyBlue;
                    break;
            }

            Border borderTracing = this.FindControl<Border> ("borderTracing");
            Label lblLead = this.FindControl<Label> ("lblLead");
            Label lblScaleAuto = this.FindControl<Label> ("lblScaleAuto");
            Label lblScaleMin = this.FindControl<Label> ("lblScaleMin");
            Label lblScaleMax = this.FindControl<Label> ("lblScaleMax");

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
                Label lblScaleMin = this.FindControl<Label> ("lblScaleMin");
                Label lblScaleMax = this.FindControl<Label> ("lblScaleMax");

                lblScaleMin.Foreground = tracingBrush;
                lblScaleMax.Foreground = tracingBrush;

                lblScaleMin.Content = Strip.ScaleMin.ToString ();
                lblScaleMax.Content = Strip.ScaleMax.ToString ();
            }
        }

        public void CalculateOffsets () {
            Canvas cnvTracing = this.FindControl<Canvas> ("cnvTracing");

            Tracing.CalculateOffsets (Strip,
               cnvTracing.Width, cnvTracing.Height,
               ref drawOffset, ref drawMultiplier);
        }

        public void DrawTracing ()
            => DrawPath (Strip.Points, tracingPen, 1);

        public void DrawReference ()
            => DrawPath (Strip.Reference, referencePen, 1);

        public void DrawPath (List<PointF> _Points, System.Drawing.Brush _Brush, float _Thickness) {
            Canvas cnvTracing = this.FindControl<Canvas> ("cnvTracing");
            Avalonia.Controls.Image imgTracing = this.FindControl<Avalonia.Controls.Image> ("imgTracing");

            Tracing.Init (ref Strip.Tracing, (int)cnvTracing.Width, (int)cnvTracing.Height);

            Tracing.DrawPath (_Points, Strip.Tracing, new System.Drawing.Pen (_Brush, _Thickness),
                System.Drawing.Color.Black, drawOffset, drawMultiplier);

            imgTracing.Source = Trace.BitmapToImageSource (Strip.Tracing);
        }

        private void MenuZeroTransducer_Click (object? sender, RoutedEventArgs e) {
            switch (Lead.Value) {
                case Lead.Values.ABP: App.Patient.TransducerZeroed_ABP = true; return;
                case Lead.Values.CVP: App.Patient.TransducerZeroed_CVP = true; return;
                case Lead.Values.PA: App.Patient.TransducerZeroed_PA = true; return;
                case Lead.Values.ICP: App.Patient.TransducerZeroed_ICP = true; return;
                case Lead.Values.IAP: App.Patient.TransducerZeroed_IAP = true; return;
            }
        }

        private void MenuAddTracing_Click (object? sender, RoutedEventArgs e)
            => App.Device_Monitor.AddTracing ();

        private void MenuRemoveTracing_Click (object? sender, RoutedEventArgs e)
            => App.Device_Monitor.RemoveTracing (this);

        private void MenuIncreaseAmplitude_Click (object? sender, RoutedEventArgs e) {
            Strip.IncreaseAmplitude ();
            CalculateOffsets ();
        }

        private void MenuDecreaseAmplitude_Click (object? sender, RoutedEventArgs e) {
            Strip.DecreaseAmplitude ();
            CalculateOffsets ();
        }

        private void MenuToggleAutoScale_Click (object? sender, RoutedEventArgs e) {
            Strip.ScaleAuto = !Strip.ScaleAuto;
            UpdateInterface (null, null);
        }

        private void MenuSelectInputSource (object? sender, RoutedEventArgs e) {
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

        private void cnvTracing_SizeChanged (object? sender, EventArgs e) {
            CalculateOffsets ();

            //DrawReference ();
        }
    }
}