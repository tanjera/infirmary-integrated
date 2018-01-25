using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;


using II;
using II.Rhythm;
using II.Localization;

namespace II_Windows.Controls {
    /// <summary>
    /// Interaction logic for Tracing.xaml
    /// </summary>
    public partial class EFMTracing : UserControl {

        public Strip wfStrip;

        // Drawing variables, offsets and multipliers
        Path drawPath;
        Brush drawBrush;
        StreamGeometry drawGeometry;
        StreamGeometryContext drawContext;
        int drawXOffset, drawYOffset;
        double drawXMultiplier, drawYMultiplier;


        public EFMTracing (Strip strip) {
            InitializeComponent ();
            DataContext = this;

            wfStrip = strip;
            UpdateInterface (null, null);
        }

        private void UpdateInterface (object sender, SizeChangedEventArgs e) {
            drawBrush = Brushes.Green;

            lblLead.Foreground = drawBrush;
            lblLead.Content = App.Language.Dictionary[Leads.LookupString (wfStrip.Lead.Value, true)];
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
                    if (wfStrip.Points [i].X > wfStrip.lengthSeconds * 2)
                        continue;

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
