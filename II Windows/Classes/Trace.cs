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

        public static void BitmapToImage (Bitmap bitmap, System.Windows.Controls.Image image) {
            if (bitmap == null || image == null)
                return;

            // DEBUG: saves every bitmap render to temp folder
            //bitmap.Save (Path.Combine (II.File.GetTempDirPath (), Guid.NewGuid () + ".bmp"));

            BitmapImage bmpi = new BitmapImage ();
            bmpi.BeginInit ();

            MemoryStream ms = new MemoryStream ();
            bitmap.Save (ms, System.Drawing.Imaging.ImageFormat.Bmp);
            MemoryStream msbs = new MemoryStream (ms.ToArray ());
            bmpi.StreamSource = msbs;

            bmpi.EndInit ();
            image.Source = bmpi;

            ms.Close ();
            ms.Dispose ();
            msbs.Close ();
            msbs.Dispose ();
        }
    }
}