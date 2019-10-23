﻿using System.Collections.Generic;
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
            lblLead.Content = App.Language.Dictionary [Lead.LookupString (Lead.Value, true)];
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
                    if (_Points [i].Y == _Points [i - 1].Y && _Points [i].Y == _Points [i + 1].Y)
                        continue;
                    else
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