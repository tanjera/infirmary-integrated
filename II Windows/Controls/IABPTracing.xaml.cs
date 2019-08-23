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
    public partial class IABPTracing : UserControl {
        public Strip wfStrip;
        public Leads Lead { get { return wfStrip.Lead; } }

        // Drawing variables, offsets and multipliers
        private Path drawPath;

        private Brush drawBrush;
        private StreamGeometry drawGeometry;
        private StreamGeometryContext drawContext;
        private int drawXOffset, drawYOffset;
        private double drawXMultiplier, drawYMultiplier;

        public IABPTracing (Strip strip) {
            InitializeComponent ();
            DataContext = this;

            wfStrip = strip;

            UpdateInterface (null, null);
        }

        private void UpdateInterface (object sender, SizeChangedEventArgs e) {
            switch (wfStrip.Lead.Value) {
                default: drawBrush = Brushes.Green; break;
                case Leads.Values.ABP: drawBrush = Brushes.Red; break;
                case Leads.Values.IABP: drawBrush = Brushes.SkyBlue; break;
            }

            borderTracing.BorderBrush = drawBrush;

            lblLead.Foreground = drawBrush;
            lblLead.Content = App.Language.Dictionary [Leads.LookupString (Lead.Value)];
        }

        public void Draw () {
            drawXOffset = 0;
            drawYOffset = (int)canvasTracing.ActualHeight / 2;
            drawXMultiplier = (int)canvasTracing.ActualWidth / wfStrip.lengthSeconds;
            drawYMultiplier = -(int)canvasTracing.ActualHeight / 2;

            if (wfStrip.Points.Count < 2)
                return;

            wfStrip.RemoveNull ();
            wfStrip.Sort ();

            drawPath = new Path { Stroke = drawBrush, StrokeThickness = 1 };
            drawGeometry = new StreamGeometry { FillRule = FillRule.EvenOdd };

            using (drawContext = drawGeometry.Open ()) {
                drawContext.BeginFigure (new System.Windows.Point (
                    (int)(wfStrip.Points [0].X * drawXMultiplier) + drawXOffset,
                    (int)(wfStrip.Points [0].Y * drawYMultiplier) + drawYOffset),
                    true, false);

                for (int i = 1; i < wfStrip.Points.Count; i++) {
                    drawContext.LineTo (new System.Windows.Point (
                        (int)(wfStrip.Points [i].X * drawXMultiplier) + drawXOffset,
                        (int)(wfStrip.Points [i].Y * drawYMultiplier) + drawYOffset),
                        true, true);
                }
            }

            drawGeometry.Freeze ();
            drawPath.Data = drawGeometry;

            canvasTracing.Children.Clear ();
            canvasTracing.Children.Add (drawPath);
        }
    }
}