using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace II_Windows {

    public static class Screenshot {

        public static BitmapSource GetBitmap (this UIElement source, double scale) {
            double actualHeight = source.RenderSize.Height;
            double actualWidth = source.RenderSize.Width;

            double renderHeight = actualHeight * scale;
            double renderWidth = actualWidth * scale;

            RenderTargetBitmap renderTarget = new RenderTargetBitmap ((int)renderWidth, (int)renderHeight, 96, 96, PixelFormats.Pbgra32);
            VisualBrush sourceBrush = new VisualBrush (source);

            DrawingVisual drawingVisual = new DrawingVisual ();
            DrawingContext drawingContext = drawingVisual.RenderOpen ();

            using (drawingContext) {
                drawingContext.PushTransform (new ScaleTransform (scale, scale));
                drawingContext.DrawRectangle (sourceBrush, null, new Rect (new System.Windows.Point (0, 0), new System.Windows.Point (actualWidth, actualHeight)));
            }
            renderTarget.Render (drawingVisual);

            return (BitmapSource)renderTarget;
        }

        public static byte [] GetJPG (BitmapSource bitmap, int quality) {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder ();
            encoder.QualityLevel = quality;
            encoder.Frames.Add (BitmapFrame.Create (bitmap));

            Byte [] array;
            using (MemoryStream ms = new MemoryStream ()) {
                encoder.Save (ms);
                array = ms.ToArray ();
            }

            return array;
        }

        public static byte [] GetPNG (BitmapSource bitmap) {
            PngBitmapEncoder encoder = new PngBitmapEncoder ();
            encoder.Interlace = PngInterlaceOption.On;
            encoder.Frames.Add (BitmapFrame.Create (bitmap));

            Byte [] array;
            using (MemoryStream ms = new MemoryStream ()) {
                encoder.Save (ms);
                array = ms.ToArray ();
            }

            return array;
        }
    }
}