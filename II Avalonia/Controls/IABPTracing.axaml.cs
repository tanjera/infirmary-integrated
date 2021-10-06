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
using II.Drawing;
using II.Localization;
using II.Rhythm;

namespace II_Avalonia.Controls {

    public partial class IABPTracing : UserControl {
        public Strip Strip;
        public Lead Lead { get { return Strip.Lead; } }
        public RenderTargetBitmap Tracing;

        /* Drawing variables, offsets and multipliers */
        private Pen tracingPen = new Pen ();
        private IBrush tracingBrush = Brushes.Black;
        private IBrush referenceBrush = Brushes.DarkGray;

        private PointD drawOffset;
        private PointD drawMultiplier;

        public IABPTracing () {
            InitializeComponent ();
        }

        public IABPTracing (Strip strip) {
            InitializeComponent ();
            DataContext = this;

            Strip = strip;

            UpdateInterface (null, null);
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private void UpdateInterface (object? sender, EventArgs e) {
            switch (Lead.Value) {
                default:
                    tracingBrush = Brushes.Green;
                    break;

                case Lead.Values.ABP:
                    tracingBrush = Brushes.Red;
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

        public async Task DrawTracing ()
            => Draw (Strip, tracingBrush, 1);

        public async Task Draw (Strip _Strip, IBrush _Brush, double _Thickness) {
            Image imgTracing = this.FindControl<Image> ("imgTracing");

            PixelSize size = new PixelSize (    // Must use a size > 0
                imgTracing.Bounds.Width > 0 ? (int)imgTracing.Bounds.Width : 100,
                imgTracing.Bounds.Height > 0 ? (int)imgTracing.Bounds.Height : 100);

            Tracing = new RenderTargetBitmap (size);

            tracingPen.Brush = _Brush;
            tracingPen.Thickness = _Thickness;

            Trace.DrawPath (_Strip, Tracing, tracingPen, drawOffset, drawMultiplier);

            imgTracing.Source = Tracing;
        }
    }
}