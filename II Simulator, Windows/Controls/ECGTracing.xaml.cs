using II;
using II.Drawing;
using II.Localization;
using II.Rhythm;

using II.Rhythm;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IISIM.Controls {

    /// <summary>
    /// Interaction logic for ECGTracing.xaml
    /// </summary>
    public partial class ECGTracing : UserControl {
        public App? Instance { get; set; }

        public Strip? Strip;
        public Lead? Lead { get { return Strip?.Lead; } }
        public RenderTargetBitmap? Tracing;

        /* Drawing variables, offsets and multipliers */
        public Color.Schemes? ColorScheme;
        public Pen? TracingPen = new ();
        public Brush? TracingBrush = Brushes.Black;

        public PointD? DrawOffset;
        public PointD? DrawMultiplier;

        public ECGTracing () {
            InitializeComponent ();
        }

        public ECGTracing (App? app, Strip? strip, Color.Schemes? cs) {
            InitializeComponent ();
            DataContext = this;

            Instance = app;
            Strip = strip;
            ColorScheme = cs;

            // TODO: Implement
            //UpdateInterface ();
        }

        ~ECGTracing () {
            Strip?.Points?.Clear ();
        }

        /* TODO: Implement
        public void UpdateScale () {
            if (Strip?.CanScale ?? false) {
                Label lblScaleMin = this.GetControl<Label> ("lblScaleMin");
                Label lblScaleMax = this.GetControl<Label> ("lblScaleMax");

                lblScaleMin.Foreground = TracingBrush;
                lblScaleMax.Foreground = TracingBrush;

                lblScaleMin.Content = Strip.ScaleMin.ToString ();
                lblScaleMax.Content = Strip.ScaleMax.ToString ();
            }
        }

        public void CalculateOffsets () {
            Image imgTracing = this.GetControl<Image> ("imgTracing");

            II.Rhythm.Tracing.CalculateOffsets (Strip,
               imgTracing.Bounds.Width, imgTracing.Bounds.Height,
               ref DrawOffset, ref DrawMultiplier);
        }

        public async Task DrawTracing ()
            => await Draw (Strip, TracingBrush, 1);

        public Task Draw (Strip? _Strip, IBrush? _Brush, double? _Thickness) {
            if (_Strip is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (Draw)}");
                return Task.CompletedTask;
            }

            Image imgTracing = this.GetControl<Image> ("imgTracing");

            PixelSize size = new (    // Must use a size > 0
                imgTracing.Bounds.Width > 0 ? (int)imgTracing.Bounds.Width : 100,
                imgTracing.Bounds.Height > 0 ? (int)imgTracing.Bounds.Height : 100);

            Tracing = new RenderTargetBitmap (size);

            if (TracingPen is not null) {
                TracingPen.Brush = _Brush ?? Brushes.Black;
                TracingPen.Thickness = _Thickness ?? 1d;

                Trace.DrawPath (_Strip, Tracing, TracingPen, DrawOffset, DrawMultiplier);
            }

            imgTracing.Source = Tracing;

            return Task.CompletedTask;
        }
        */
    }
}