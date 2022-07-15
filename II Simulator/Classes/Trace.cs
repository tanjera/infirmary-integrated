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

namespace IISIM {

    public class Trace {

        public static void DrawPath (Strip? strip, RenderTargetBitmap bitmap,
                Pen pen, PointD? offset, PointD? multiplier) {
            if (strip is null || strip?.Points is null || strip?.Points?.Count < 2)
                return;

            if (bitmap == null)     // Can't initiate Bitmap here; don't have width/height
                return;

            if (offset is null)
                offset = new ();

            if (multiplier is null)
                multiplier = new (1d, 1d);

            using (IDrawingContextImpl ctx = bitmap.CreateDrawingContext (null)) {
                var sg = new StreamGeometry ();

                using (var sgc = sg.Open ()) {
                    lock (strip.lockPoints) {
                        sgc.BeginFigure (new Point (
                            (strip.Points [0].X * multiplier.X) + offset.X,
                            (strip.Points [0].Y * multiplier.Y) + offset.Y),
                            false);

                        for (int i = 1; i < strip.Points.Count; i++) {
                            sgc.LineTo (new Point (
                                (strip.Points [i].X * multiplier.X) + offset.X,
                                (strip.Points [i].Y * multiplier.Y) + offset.Y
                                ));
                        }
                    }
                    sgc.EndFigure (false);
                }

                ctx.DrawGeometry (Brushes.Transparent, pen, sg.PlatformImpl);
            }
        }
    }
}