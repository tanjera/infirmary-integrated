using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

namespace II_Windows {

    public static class Trace {

        public static BitmapImage BitmapToImageSource (Bitmap bitmap) {
            using (MemoryStream ms = new MemoryStream ()) {
                bitmap.Save (ms, System.Drawing.Imaging.ImageFormat.Png);   // PNG supports transparency!
                ms.Position = 0;

                BitmapImage bi = new BitmapImage ();
                bi.BeginInit ();
                bi.StreamSource = ms;
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit ();

                return bi;
            }
        }
    }
}