using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

using II;
using II.Rhythm;

namespace II_Windows.Controls {

    public static class Tracings {

        public static void DrawPath (Path _Path, List<II.Waveform.Point> _Points, Brush _Brush, double _Thickness,
            StreamGeometry drawGeometry, StreamGeometryContext drawContext,
            int drawXOffset, int drawYOffset,
            double drawXMultiplier, double drawYMultiplier
            ) {
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
    }
}