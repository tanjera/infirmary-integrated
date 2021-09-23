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

    public partial class ECGTracing : UserControl {
        /* Properties for applying DPI scaling options */
        public double UIScale { get { return App.Settings.UIScale; } }
        public int FontScale { get { return (int)(14 * App.Settings.UIScale); } }

        public Strip Strip;
        public Lead Lead { get { return Strip.Lead; } }
        public RenderTargetBitmap Tracing;

        /* Drawing variables, offsets and multipliers */
        private DeviceECG.ColorSchemes colorScheme;

        private Pen tracingPen = new Pen ();
        private IBrush tracingBrush = Brushes.Green;
        private IBrush referenceBrush = Brushes.DarkGray;

        private System.Drawing.Point drawOffset;
        private System.Drawing.PointF drawMultiplier;

        private MenuItem menuZeroTransducer;
        private MenuItem menuToggleAutoScale;

        public ECGTracing () {
            InitializeComponent ();
        }

        public ECGTracing (Strip strip) {
            InitializeComponent ();
            DataContext = this;

            Strip = strip;

            UpdateInterface (null, null);
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private void UpdateInterface (object? sender, EventArgs e) {
            Label lblLead = this.FindControl<Label> ("lblLead");

            lblLead.Foreground = tracingBrush;
            lblLead.Content = App.Language.Localize (Lead.LookupString (Lead.Value, true));

            //TODO Set background color of Grid according to backgroundBrush

            CalculateOffsets ();
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

        public void SetColors (DeviceECG.ColorSchemes scheme) {
            colorScheme = scheme;

            switch (scheme) {
                default:

                case DeviceECG.ColorSchemes.Grid:
                case DeviceECG.ColorSchemes.Light:
                    tracingBrush = Brushes.Black;
                    referenceBrush = Brushes.DarkGray;
                    break;

                case DeviceECG.ColorSchemes.Dark:
                    tracingBrush = Brushes.Green;
                    referenceBrush = Brushes.DarkGray;
                    break;
            }

            UpdateInterface (null, null);
        }

        private void cnvTracing_SizeChanged (object? sender, EventArgs e) {
            CalculateOffsets ();

            //DrawReference ();
        }
    }
}