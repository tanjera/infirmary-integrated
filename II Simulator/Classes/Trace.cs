using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static void DrawWaveform (Strip? strip, RenderTargetBitmap bitmap,
                Pen pen, PointD? offset, PointD? multiplier) {
            if (strip is null) {
                Debug.WriteLine ($"Null return at Trace.{nameof (DrawWaveform)} d/t null Strip or Strip.Points; normal on initialization.");
                return;
            }

            if (bitmap == null) {    // Can't initiate Bitmap here; don't have width/height
                Debug.WriteLine ($"Null return at Trace.{nameof (DrawWaveform)} d/t null bitmap; normal on initialization.");
                return;
            }

            offset ??= new ();
            multiplier ??= new (1d, 1d);

            using (DrawingContext ctx = bitmap.CreateDrawingContext (false)) {
                var sg = new StreamGeometry ();

                using (var sgc = sg.Open ()) {
                    if (strip?.Points is not null && strip?.Points?.Count > 1) {
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
                }

                ctx.DrawGeometry (Brushes.Transparent, pen, sg.Clone ());
            }
        }
        
        public static void DrawEvents (Strip? strip, RenderTargetBitmap bitmap,
                Pen pen, PointD? offset, PointD? multiplier) {
            if (strip is null) {
                Debug.WriteLine ($"Null return at Trace.{nameof (DrawWaveform)} d/t null Strip or Strip.Points; normal on initialization.");
                return;
            }

            if (bitmap == null) {    // Can't initiate Bitmap here; don't have width/height
                Debug.WriteLine ($"Null return at Trace.{nameof (DrawWaveform)} d/t null bitmap; normal on initialization.");
                return;
            }

            offset ??= new ();
            multiplier ??= new (1d, 1d);
            
            double spikeEnd = bitmap.Size.Height / 4;
            double syncStart = bitmap.Size.Height / 2;
            double syncEnd = bitmap.Size.Height / 8;

            using (DrawingContext ctx = bitmap.CreateDrawingContext (false)) {
                var sg = new StreamGeometry ();

                using (var sgc = sg.Open ()) {
                    lock (strip.lockEvents) {
                        for (int i = 0; i < strip?.Events?.Count; i++) {
                            if (strip?.Events [i].Type == Event.Types.Pacer) {
                                sgc.BeginFigure (new Point (
                                        (strip.Events [i].X * multiplier.X) + offset.X,
                                        0 + offset.Y),
                                    false);

                                sgc.LineTo (new Point (
                                    (strip.Events [i].X * multiplier.X) + offset.X,
                                    -spikeEnd + offset.Y
                                ));
                            } else if (strip?.Events [i].Type == Event.Types.Sync) {
                                sgc.BeginFigure (new Point (
                                        (strip.Events [i].X * multiplier.X) + offset.X,
                                        -syncStart + offset.Y),
                                    false);

                                sgc.LineTo (new Point (
                                    (strip.Events [i].X * multiplier.X) + offset.X,
                                    -syncStart + syncEnd + offset.Y
                                ));
                            }

                            sgc.EndFigure (false);
                        }
                    }
                }

                ctx.DrawGeometry (Brushes.Transparent, pen, sg.Clone ());
            }
        }
    }
}