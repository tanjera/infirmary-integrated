using II;
using II.Rhythm;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace II_Windows.Controls {

    /// <summary>
    /// Interaction logic for Tracing.xaml
    /// </summary>
    public partial class EFMTracing : UserControl {
        public Strip Strip;

        // Drawing variables, offsets and multipliers

        private Brush drawBrush;
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
            drawBrush = Brushes.Green;

            lblLead.Foreground = drawBrush;
            lblLead.Content = App.Language.Dictionary [Lead.LookupString (Strip.Lead.Value, true)];
        }

        public void Draw () {
            drawXOffset = 0;
            drawYOffset = (int)canvasTracing.ActualHeight / 2;
            drawXMultiplier = (int)canvasTracing.ActualWidth / Strip.lengthSeconds;
            drawYMultiplier = -(int)canvasTracing.ActualHeight / 2;

            if (Strip.Points.Count < 2)
                return;

            Strip.RemoveNull ();
            Strip.Sort ();

            drawPath.Stroke = drawBrush;
            drawPath.StrokeThickness = 1;
            drawGeometry = new StreamGeometry { FillRule = FillRule.EvenOdd };

            using (drawContext = drawGeometry.Open ()) {
                drawContext.BeginFigure (new System.Windows.Point (
                    (int)(Strip.Points [0].X * drawXMultiplier) + drawXOffset,
                    (int)(Strip.Points [0].Y * drawYMultiplier) + drawYOffset),
                    true, false);

                for (int i = 1; i < Strip.Points.Count; i++) {
                    if (Strip.Points [i].X > Strip.lengthSeconds * 2)
                        continue;

                    drawContext.LineTo (new System.Windows.Point (
                        (int)(Strip.Points [i].X * drawXMultiplier) + drawXOffset,
                        (int)(Strip.Points [i].Y * drawYMultiplier) + drawYOffset),
                        true, true);
                }
            }

            drawGeometry.Freeze ();
            drawPath.Data = drawGeometry;
        }
    }
}