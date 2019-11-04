using System.Collections.Generic;
using System.Drawing;
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
        private System.Drawing.Brush tracingPen = System.Drawing.Brushes.Black;
        private System.Windows.Media.Brush tracingBrush = System.Windows.Media.Brushes.Black;
        private System.Drawing.Brush referencePen = System.Drawing.Brushes.DarkGray;

        private System.Drawing.Point drawOffset;
        private System.Drawing.PointF drawMultiplier;

        public IABPTracing (Strip strip) {
            InitializeComponent ();
            DataContext = this;

            Strip = strip;

            UpdateInterface (null, null);
        }

        private void UpdateInterface (object sender, SizeChangedEventArgs e) {
            switch (Strip.Lead.Value) {
                default:
                    tracingPen = System.Drawing.Brushes.Green;
                    tracingBrush = System.Windows.Media.Brushes.Green;
                    break;

                case Lead.Values.ABP:
                    tracingPen = System.Drawing.Brushes.Red;
                    tracingBrush = System.Windows.Media.Brushes.Red;
                    break;

                case Lead.Values.IABP:
                    tracingPen = System.Drawing.Brushes.SkyBlue;
                    tracingBrush = System.Windows.Media.Brushes.SkyBlue;
                    break;
            }

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
                lblScaleMin.Foreground = tracingBrush;
                lblScaleMax.Foreground = tracingBrush;

                lblScaleMin.Content = Strip.ScaleMin.ToString ();
                lblScaleMax.Content = Strip.ScaleMax.ToString ();
            }
        }

        public void CalculateOffsets ()
                    => Tracing.CalculateOffsets (Strip,
                        cnvTracing.ActualWidth, cnvTracing.ActualHeight,
                        ref drawOffset, ref drawMultiplier);

        public void DrawTracing ()
            => DrawPath (Strip.Points, tracingPen, 1);

        public void DrawReference ()
            => DrawPath (Strip.Reference, referencePen, 1);

        public void DrawPath (List<PointF> _Points, System.Drawing.Brush _Brush, float _Thickness) {
            Tracing.Init (ref Strip.Tracing, (int)cnvTracing.ActualWidth, (int)cnvTracing.ActualHeight);

            Tracing.DrawPath (_Points, Strip.Tracing, new System.Drawing.Pen (_Brush, _Thickness),
                System.Drawing.Color.Black, drawOffset, drawMultiplier);

            imgTracing.Source = Trace.BitmapToImageSource (Strip.Tracing);
        }

        private void cnvTracing_SizeChanged (object sender, SizeChangedEventArgs e) {
            CalculateOffsets ();

            //DrawReference ();
        }
    }
}