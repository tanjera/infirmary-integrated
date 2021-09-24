using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

using II;
using II.Localization;
using II.Rhythm;

namespace II_Avalonia.Controls {

    public partial class MonitorTracing : UserControl {
        /* Properties for applying DPI scaling options */
        public double UIScale { get { return App.Settings.UIScale; } }
        public int FontScale { get { return (int)(14 * App.Settings.UIScale); } }

        public Strip Strip;
        public Lead Lead { get { return Strip.Lead; } }
        public RenderTargetBitmap Tracing;

        /* Drawing variables, offsets and multipliers */
        private Pen tracingPen = new Pen ();
        private IBrush tracingBrush = Brushes.Black;
        private IBrush referenceBrush = Brushes.DarkGray;

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
            this.FindControl<Image> ("imgTracing").ContextMenu = menuContext;
            this.FindControl<Label> ("lblLead").ContextMenu = menuContext;

            menuZeroTransducer = new MenuItem ();
            menuZeroTransducer.Header = App.Language.Localize ("MENU:MenuZeroTransducer");
            menuZeroTransducer.Classes.Add ("item");
            menuZeroTransducer.Click += MenuZeroTransducer_Click;
            menuitemsContext.Add (menuZeroTransducer);

            menuitemsContext.Add (new Separator ());

            MenuItem menuAddTracing = new MenuItem ();
            menuAddTracing.Header = App.Language.Localize ("MENU:MenuAddTracing");
            menuAddTracing.Classes.Add ("item");
            menuAddTracing.Click += MenuAddTracing_Click;
            menuitemsContext.Add (menuAddTracing);

            MenuItem menuRemoveTracing = new MenuItem ();
            menuRemoveTracing.Header = App.Language.Localize ("MENU:MenuRemoveTracing");
            menuRemoveTracing.Classes.Add ("item");
            menuRemoveTracing.Click += MenuRemoveTracing_Click;
            menuitemsContext.Add (menuRemoveTracing);

            menuitemsContext.Add (new Separator ());

            MenuItem menuIncreaseAmplitude = new MenuItem ();
            menuIncreaseAmplitude.Header = App.Language.Localize ("MENU:IncreaseAmplitude");
            menuIncreaseAmplitude.Classes.Add ("item");
            menuIncreaseAmplitude.Click += MenuIncreaseAmplitude_Click;
            menuitemsContext.Add (menuIncreaseAmplitude);

            MenuItem menuDecreaseAmplitude = new MenuItem ();
            menuDecreaseAmplitude.Header = App.Language.Localize ("MENU:DecreaseAmplitude");
            menuDecreaseAmplitude.Classes.Add ("item");
            menuDecreaseAmplitude.Click += MenuDecreaseAmplitude_Click;
            menuitemsContext.Add (menuDecreaseAmplitude);

            menuitemsContext.Add (new Separator ());

            menuToggleAutoScale = new MenuItem ();
            menuToggleAutoScale.Header = App.Language.Localize ("MENU:ToggleAutoScaling");
            menuToggleAutoScale.Classes.Add ("item");
            menuToggleAutoScale.Click += MenuToggleAutoScale_Click;
            menuitemsContext.Add (menuToggleAutoScale);

            menuitemsContext.Add (new Separator ());

            MenuItem menuSelectInput = new MenuItem (),
                     menuECGLeads = new MenuItem ();
            List<object> menuitemsSelectInput = new List<object> (),
                menuitemsECGLeads = new List<object> ();

            menuSelectInput.Header = App.Language.Localize ("MENU:MenuSelectInputSource");
            menuSelectInput.Classes.Add ("item");

            menuECGLeads.Header = App.Language.Localize ("TRACING:ECG");
            menuECGLeads.Classes.Add ("item");

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
                mi.Classes.Add ("item");
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
                    tracingBrush = Brushes.Green;
                    break;

                case Lead.Values.SPO2:
                    tracingBrush = Brushes.Orange;
                    break;

                case Lead.Values.RR:
                    tracingBrush = Brushes.Salmon;
                    break;

                case Lead.Values.ETCO2:
                    tracingBrush = Brushes.Aqua;
                    break;

                case Lead.Values.ABP:
                    tracingBrush = Brushes.Red;
                    break;

                case Lead.Values.CVP:
                    tracingBrush = Brushes.Blue;
                    break;

                case Lead.Values.PA:
                    tracingBrush = Brushes.Yellow;
                    break;

                case Lead.Values.ICP:
                    tracingBrush = Brushes.Khaki;
                    break;

                case Lead.Values.IAP:
                    tracingBrush = Brushes.Aquamarine;
                    break;

                case Lead.Values.IABP:
                    tracingBrush = Brushes.SkyBlue;
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
            Image imgTracing = this.FindControl<Image> ("imgTracing");

            II.Rhythm.Tracing.CalculateOffsets (Strip,
               imgTracing.Bounds.Width, imgTracing.Bounds.Height,
               ref drawOffset, ref drawMultiplier);
        }

        public async void DrawTracing ()
            => Draw (Strip.Points, tracingBrush, 1);

        public void DrawReference ()
            => Draw (Strip.Reference, referenceBrush, 1);

        public async void Draw (List<System.Drawing.PointF> _Points, IBrush _Brush, double _Thickness) {
            Image imgTracing = this.FindControl<Image> ("imgTracing");

            PixelSize size = new PixelSize (    // Must use a size > 0
                imgTracing.Bounds.Width > 0 ? (int)imgTracing.Bounds.Width : 100,
                imgTracing.Bounds.Height > 0 ? (int)imgTracing.Bounds.Height : 100);

            Tracing = new RenderTargetBitmap (size);

            tracingPen.Brush = _Brush;
            tracingPen.Thickness = _Thickness;

            await Trace.DrawPath (_Points, Tracing, tracingPen, drawOffset, drawMultiplier);

            imgTracing.Source = Tracing;
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
    }
}