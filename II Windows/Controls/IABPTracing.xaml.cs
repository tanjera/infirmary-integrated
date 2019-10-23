using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

using II;
using II.Rhythm;

namespace II_Windows.Controls {

    /// <summary>
    /// Interaction logic for Tracing.xaml
    /// </summary>
    public partial class IABPTracing : UserControl {
        public Strip Strip;
        public Lead Lead { get { return Strip.Lead; } }

        /* Drawing variables, offsets and multipliers */
        private Brush tracingBrush = Brushes.Black;
        private Brush referenceBrush = Brushes.DarkGray;

        private StreamGeometry drawGeometry;
        private StreamGeometryContext drawContext;
        private int drawXOffset, drawYOffset;
        private double drawXMultiplier, drawYMultiplier;

        public IABPTracing (Strip strip) {
            InitializeComponent ();
            DataContext = this;

            Strip = strip;

            UpdateInterface (null, null);
        }

        private void UpdateInterface (object sender, SizeChangedEventArgs e) {
            switch (Strip.Lead.Value) {
                default: tracingBrush = Brushes.Green; break;
                case Lead.Values.ABP: tracingBrush = Brushes.Red; break;
                case Lead.Values.IABP: tracingBrush = Brushes.SkyBlue; break;
            }

            borderTracing.BorderBrush = tracingBrush;

            lblLead.Foreground = tracingBrush;
            lblLead.Content = App.Language.Dictionary [Lead.LookupString (Lead.Value)];

            if (Strip.CanScale) {
                lblScaleAuto.Foreground = tracingBrush;
                lblScaleMin.Foreground = tracingBrush;
                lblScaleMax.Foreground = tracingBrush;

                lblScaleAuto.Content = Strip.ScaleAuto
                    ? App.Language.Dictionary ["TRACING:Auto"]
                    : App.Language.Dictionary ["TRACING:Fixed"];
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

        public void CalculateOffsets () {
            drawXOffset = 0;
            drawXMultiplier = (int)canvasTracing.ActualWidth / Strip.DisplayLength;

            switch (Strip.Offset) {
                case Strip.Offsets.Center:
                    drawYOffset = (int)(canvasTracing.ActualHeight / 2);
                    drawYMultiplier = (-(int)canvasTracing.ActualHeight / 2) * Strip.Amplitude;
                    break;

                case Strip.Offsets.Stretch:
                    drawYOffset = (int)(canvasTracing.ActualHeight * 0.9);
                    drawYMultiplier = -(int)canvasTracing.ActualHeight * 0.8 * Strip.Amplitude;
                    break;

                case Strip.Offsets.Scaled:
                    drawYOffset = (int)(canvasTracing.ActualHeight * 0.9);
                    drawYMultiplier = -(int)canvasTracing.ActualHeight;
                    break;
            }
        }

        public void DrawTracing ()
            => DrawPath (drawPath, Strip.Points, tracingBrush, 1);

        public void DrawReference ()
            => DrawPath (drawReference, Strip.Reference, referenceBrush, 1);

        public void DrawPath (Path _Path, List<II.Waveform.Point> _Points, Brush _Brush, double _Thickness) {
            if (_Points.Count < 2)
                return;

            _Path.Stroke = _Brush;
            _Path.StrokeThickness = _Thickness;
            drawGeometry = new StreamGeometry { FillRule = FillRule.EvenOdd };

            using (drawContext = drawGeometry.Open ()) {
                drawContext.BeginFigure (new System.Windows.Point (
                    (int)(_Points [0].X * drawXMultiplier) + drawXOffset,
                    (int)(_Points [0].Y * drawYMultiplier) + drawYOffset),
                    true, false);

                for (int i = 1; i < _Points.Count - 1; i++) {
                    drawContext.LineTo (new System.Windows.Point (
                        (int)(_Points [i].X * drawXMultiplier) + drawXOffset,
                        (int)(_Points [i].Y * drawYMultiplier) + drawYOffset),
                        true, true);
                }

                drawContext.LineTo (new System.Windows.Point (
                        (int)(_Points [_Points.Count - 1].X * drawXMultiplier) + drawXOffset,
                        (int)(_Points [_Points.Count - 1].Y * drawYMultiplier) + drawYOffset),
                        true, true);
            }

            drawGeometry.Freeze ();
            _Path.Data = drawGeometry;
        }

        private void canvasTracing_SizeChanged (object sender, SizeChangedEventArgs e) {
            CalculateOffsets ();

            //DrawReference ();
        }
    }
}