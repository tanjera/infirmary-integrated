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
    public partial class EFMTracing : UserControl {
        public Strip Strip;
        public Lead Lead { get { return Strip.Lead; } }

        /* Drawing variables, offsets and multipliers */
        private System.Drawing.Color bgColor = System.Drawing.Color.Black;
        private System.Drawing.Brush tracingPen = System.Drawing.Brushes.Green;
        private System.Windows.Media.Brush tracingBrush = System.Windows.Media.Brushes.Green;
        private System.Drawing.Brush referencePen = System.Drawing.Brushes.DarkGray;

        private System.Drawing.Point drawOffset;
        private System.Drawing.PointF drawMultiplier;

        public EFMTracing (Strip strip) {
            InitializeComponent ();
            DataContext = this;

            Strip = strip;

            InitInterface (null, null);
        }

        private void InitInterface (object sender, SizeChangedEventArgs e) {
            tracingPen = System.Drawing.Brushes.Black;
            referencePen = System.Drawing.Brushes.DarkGray;
            tracingBrush = System.Windows.Media.Brushes.Black;
            bgColor = System.Drawing.Color.Transparent;

            lblLead.Foreground = tracingBrush;
            lblLead.Content = App.Language.Localize (Lead.LookupString (Lead.Value));
        }

        public void CalculateOffsets ()
            => Tracing.CalculateOffsets (Strip,
                cnvTracing.ActualWidth, cnvTracing.ActualHeight,
                ref drawOffset, ref drawMultiplier);

        public void DrawTracing ()
            => DrawPath (Strip.Points, tracingPen, 2);

        public void DrawReference ()
            => DrawPath (Strip.Reference, referencePen, 1);

        public void DrawPath (List<PointF> _Points, System.Drawing.Brush _Brush, float _Thickness) {
            Tracing.Init (ref Strip.Tracing, (int)cnvTracing.ActualWidth, (int)cnvTracing.ActualHeight);

            Tracing.DrawPath (_Points, Strip.Tracing, new System.Drawing.Pen (_Brush, _Thickness),
                bgColor, drawOffset, drawMultiplier);

            imgTracing.Source = Trace.BitmapToImageSource (Strip.Tracing);
        }

        private void cnvTracing_SizeChanged (object sender, SizeChangedEventArgs e) {
            CalculateOffsets ();

            //DrawReference ();
        }
    }
}