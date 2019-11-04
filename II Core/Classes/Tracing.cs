using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;

namespace II.Rhythm {
    public class Tracing {
        public static void Init (ref Bitmap bitmap, int width, int height) {
            if (width == 0 || height == 0)
                return;

            if (bitmap == null || bitmap.Width != width || bitmap.Height != height)
                bitmap = new Bitmap (width, height);
        }

        public static void CalculateOffsets (Strip strip, double width, double height,
            ref Point drawOffset, ref PointF drawMultiplier) {
            drawOffset.X = 0;
            drawMultiplier.X = (int)width / strip.DisplayLength;

            switch (strip.Offset) {
                case Strip.Offsets.Center:
                    drawOffset.Y = (int)(height / 2f);
                    drawMultiplier.Y = (-(int)height / 2f) * strip.Amplitude;
                    break;

                case Strip.Offsets.Stretch:
                    drawOffset.Y = (int)(height * 0.9f);
                    drawMultiplier.Y = -(int)height * 0.8f * strip.Amplitude;
                    break;

                case Strip.Offsets.Scaled:
                    drawOffset.Y = (int)(height * 0.9f);
                    drawMultiplier.Y = -(int)height;
                    break;
            }
        }

        public static void DrawPath (List<PointF> points, Bitmap bitmap,
                Pen pen, Color background, PointF offset, PointF multiplier) {
            if (points.Count < 2)
                return;

            if (bitmap == null)     // Can't initiate Bitmap here; don't have width/height
                return;

            using (Graphics g = Graphics.FromImage (bitmap)) {
                pen.LineJoin = LineJoin.Miter;
                pen.MiterLimit = 0;

                g.Clear (background);

                GraphicsPath gp = new GraphicsPath ();

                for (int i = 1; i < points.Count; i++) {
                    gp.AddLine (
                        new PointF (
                            (points [i - 1].X * multiplier.X) + offset.X,
                            (points [i - 1].Y * multiplier.Y) + offset.Y),
                        new PointF (
                            (points [i].X * multiplier.X) + offset.X,
                            (points [i].Y * multiplier.Y) + offset.Y));
                }

                g.DrawPath (pen, gp);
            }
        }
    }
}