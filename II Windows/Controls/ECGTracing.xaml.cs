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
    public partial class ECGTracing : UserControl {
        public Strip Strip;
        public Lead Lead { get { return Strip.Lead; } }

        /* Drawing variables, offsets and multipliers */
        private DeviceECG.ColorSchemes colorScheme;
        private bool showGrid = false;
        private Brush tracingBrush = Brushes.Green;
        private Brush referenceBrush = Brushes.DarkGray;

        private StreamGeometry drawGeometry;
        private StreamGeometryContext drawContext;
        private int drawXOffset, drawYOffset;
        private double drawXMultiplier, drawYMultiplier;

        public ECGTracing (Strip strip) {
            InitializeComponent ();
            DataContext = this;

            Strip = strip;

            UpdateInterface (null, null);
        }

        private void UpdateInterface (object sender, SizeChangedEventArgs e) {
            lblLead.Foreground = tracingBrush;
            lblLead.Content = App.Language.Localize (Lead.LookupString (Lead.Value, true));
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

        public void SetColors (DeviceECG.ColorSchemes scheme, bool grid) {
            colorScheme = scheme;
            showGrid = grid;

            switch (scheme) {
                default:
                case DeviceECG.ColorSchemes.Light:
                    tracingBrush = Brushes.Black;
                    referenceBrush = Brushes.DarkGray;
                    canvasTracing.Background = showGrid
                        ? Brushes.Transparent
                        : Brushes.White;
                    break;

                case DeviceECG.ColorSchemes.Dark:
                    tracingBrush = Brushes.Green;
                    referenceBrush = Brushes.DarkGray;
                    canvasTracing.Background = showGrid
                        ? Brushes.Transparent
                        : Brushes.Black;
                    break;
            }

            UpdateInterface (null, null);
        }

        private void canvasTracing_SizeChanged (object sender, SizeChangedEventArgs e) {
            CalculateOffsets ();

            //DrawReference ();
        }
    }
}