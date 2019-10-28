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

        public void CalculateOffsets ()
            => Tracing.CalculateOffsets (Strip, canvasTracing.ActualWidth, canvasTracing.ActualHeight,
                ref drawXOffset, ref drawYOffset, ref drawXMultiplier, ref drawYMultiplier);

        public void DrawTracing ()
            => DrawPath (drawPath, Strip.Points, tracingBrush, 1);

        public void DrawReference ()
            => DrawPath (drawReference, Strip.Reference, referenceBrush, 1);

        public void DrawPath (Path _Path, List<II.Waveform.Point> _Points, Brush _Brush, double _Thickness)
            => Tracings.DrawPath (_Path, _Points, _Brush, _Thickness,
                drawGeometry, drawContext, drawXOffset, drawYOffset, drawXMultiplier, drawYMultiplier);

        private void canvasTracing_SizeChanged (object sender, SizeChangedEventArgs e) {
            CalculateOffsets ();

            //DrawReference ();
        }
    }
}