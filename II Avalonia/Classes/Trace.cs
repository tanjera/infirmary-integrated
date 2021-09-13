using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace II_Avalonia {

    public static class Trace {

        public static Bitmap? BitmapToImageSource (System.Drawing.Bitmap bitmap) {
            if (bitmap == null)
                return null;

            using (MemoryStream ms = new MemoryStream ()) {
                bitmap.Save (ms, System.Drawing.Imaging.ImageFormat.Png);   // PNG supports transparency!
                return new Bitmap (ms);
            }
        }
    }
}