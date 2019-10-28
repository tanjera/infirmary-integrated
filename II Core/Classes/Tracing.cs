using System;
using System.Collections.Generic;
using System.Text;

namespace II.Rhythm {
    public class Tracing {
        public static void CalculateOffsets (Strip strip, double width, double height,
            ref int drawXOffset, ref int drawYOffset, ref double drawXMultiplier, ref double drawYMultiplier) {
            drawXOffset = 0;
            drawXMultiplier = (int)width / strip.DisplayLength;

            switch (strip.Offset) {
                case Strip.Offsets.Center:
                    drawYOffset = (int)(height / 2);
                    drawYMultiplier = (-(int)height / 2) * strip.Amplitude;
                    break;

                case Strip.Offsets.Stretch:
                    drawYOffset = (int)(height * 0.9);
                    drawYMultiplier = -(int)height * 0.8 * strip.Amplitude;
                    break;

                case Strip.Offsets.Scaled:
                    drawYOffset = (int)(height * 0.9);
                    drawYMultiplier = -(int)height;
                    break;
            }
        }
    }
}