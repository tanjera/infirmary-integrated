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
    public partial class EFMTracing : UserControl {
        public Strip Strip;

        /* Drawing variables, offsets and multipliers */
        private Brush tracingBrush = Brushes.Black;
        private Brush referenceBrush = Brushes.DarkGray;

        private StreamGeometry drawGeometry;
        private StreamGeometryContext drawContext;
        private int drawXOffset, drawYOffset;
        private double drawXMultiplier, drawYMultiplier;

        public EFMTracing (Strip strip) {
            InitializeComponent ();
            DataContext = this;

            Strip = strip;
            UpdateInterface (null, null);
        }

        private void UpdateInterface (object sender, SizeChangedEventArgs e) {
            tracingBrush = Brushes.Green;

            lblLead.Foreground = tracingBrush;
            lblLead.Content = App.Language.Dictionary [Lead.LookupString (Strip.Lead.Value, true)];
        }

        public void CalculateOffsets () {
            drawXOffset = 0;
            drawYOffset = (int)(canvasTracing.ActualHeight / 2)
               - (int)(canvasTracing.ActualHeight / 2 * Strip.Offset);
            drawXMultiplier = (int)canvasTracing.ActualWidth / Strip.DisplayLength;
            drawYMultiplier = (-(int)canvasTracing.ActualHeight / 2) * Strip.Amplitude;
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

                for (int i = 1; i < _Points.Count; i++) {
                    drawContext.LineTo (new System.Windows.Point (
                        (int)(_Points [i].X * drawXMultiplier) + drawXOffset,
                        (int)(_Points [i].Y * drawYMultiplier) + drawYOffset),
                        true, true);
                }
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