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
using Avalonia.Threading;

using II;
using II.Drawing;
using II.Localization;
using II.Rhythm;

namespace IISIM {

    public class DeviceTracing : UserControl {
        public App? Instance { get; set; }

        public Strip? Strip;
        public Lead? Lead { get { return Strip?.Lead; } }
        public RenderTargetBitmap? Tracing;

        /* Drawing variables, offsets and multipliers */
        public Color.Schemes? ColorScheme;
        public Pen? TracingPen = new ();
        public IBrush? TracingBrush = Brushes.Black;

        public PointD? DrawOffset;
        public PointD? DrawMultiplier;

        public DeviceTracing () {
        }

        public DeviceTracing (App? app) {
            Instance = app;
        }

        ~DeviceTracing () {
        }

        public void UpdateScale () {
            if (Strip?.CanScale ?? false) {
                Label lblScaleMin = this.FindControl<Label> ("lblScaleMin");
                Label lblScaleMax = this.FindControl<Label> ("lblScaleMax");

                lblScaleMin.Foreground = TracingBrush;
                lblScaleMax.Foreground = TracingBrush;

                lblScaleMin.Content = Strip.ScaleMin.ToString ();
                lblScaleMax.Content = Strip.ScaleMax.ToString ();
            }
        }

        public void CalculateOffsets () {
            Image imgTracing = this.FindControl<Image> ("imgTracing");

            II.Rhythm.Tracing.CalculateOffsets (Strip,
               imgTracing.Bounds.Width, imgTracing.Bounds.Height,
               ref DrawOffset, ref DrawMultiplier);
        }

        public async Task DrawTracing ()
            => await Draw (Strip, TracingBrush, 1);

        public Task Draw (Strip? _Strip, IBrush? _Brush, double? _Thickness) {
            if (_Strip is null)
                return Task.CompletedTask;

            Image imgTracing = this.FindControl<Image> ("imgTracing");

            PixelSize size = new (    // Must use a size > 0
                imgTracing.Bounds.Width > 0 ? (int)imgTracing.Bounds.Width : 100,
                imgTracing.Bounds.Height > 0 ? (int)imgTracing.Bounds.Height : 100);

            Tracing = new RenderTargetBitmap (size);

            TracingPen.Brush = _Brush ?? Brushes.Black;
            TracingPen.Thickness = _Thickness ?? 1d;

            Trace.DrawPath (_Strip, Tracing, TracingPen, DrawOffset, DrawMultiplier);

            imgTracing.Source = Tracing;

            return Task.CompletedTask;
        }
    }
}