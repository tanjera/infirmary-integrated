using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Threading.Tasks;

using II.Drawing;

namespace II.Rhythm {

    public class Tracing {

        public static void CalculateOffsets (
            Strip? strip,
            double width, double height,
            ref PointD? drawOffset,
            ref PointD? drawMultiplier) {
            if (strip is null)
                return;
            if (drawOffset is null)
                drawOffset = new PointD ();
            if (drawMultiplier is null)
                drawMultiplier = new PointD ();

            drawOffset.X = 0;
            drawMultiplier.X = (int)width / strip.DisplayLength;

            switch (strip.Offset) {
                case Strip.Offsets.Center:
                    drawOffset.Y = (int)(height / 2f);
                    drawMultiplier.Y = (-(int)height / 2f) * strip.Amplitude;
                    break;

                case Strip.Offsets.Stretch:
                    drawOffset.Y = (int)(height * (1 - (strip.ScaleMargin / 2)));
                    drawMultiplier.Y = -(int)height * (1 - strip.ScaleMargin) * strip.Amplitude;
                    break;

                case Strip.Offsets.Scaled:
                    drawOffset.Y = (int)(height * (1 - strip.ScaleMargin));
                    drawMultiplier.Y = -(int)height;
                    break;
            }
        }
    }
}