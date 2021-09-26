using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;

using II;
using II.Drawing;
using II.Rhythm;
using II.Waveform;

namespace II_Avalonia {

    public class Trace {

        public static async Task DrawPath (List<PointD> points, RenderTargetBitmap bitmap,
                Pen pen, PointD offset, PointD multiplier) {
            if (points.Count < 2)
                return;

            if (bitmap == null)     // Can't initiate Bitmap here; don't have width/height
                return;

            using (IDrawingContextImpl ctx = bitmap.CreateDrawingContext (null)) {
                var sg = new StreamGeometry ();

                using (var sgc = sg.Open ()) {
                    sgc.BeginFigure (new Point (
                        (points [0].X * multiplier.X) + offset.X,
                        (points [0].Y * multiplier.Y) + offset.Y),
                        false);

                    for (int i = 1; i < points.Count; i++) {
                        sgc.LineTo (new Point (
                            (points [i].X * multiplier.X) + offset.X,
                            (points [i].Y * multiplier.Y) + offset.Y
                            ));
                    }

                    sgc.EndFigure (false);
                }

                ctx.DrawGeometry (Brushes.Transparent, pen, sg.PlatformImpl);
            }
        }
    }
}