using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using II;

using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace II_Windows {

    public static class ScreenshotPdf {
        private static int headerMargin = 30;
        private static Thickness pageMargin = new Thickness (50, 80, 50, 50);

        public static PdfDocument AssemblePdf (BitmapSource bitmap, string title) {
            PdfDocument doc = new PdfDocument ();
            doc.Info.Title = title;

            PdfPage pg = doc.AddPage ();

            // Calclate image aspect ratio, determine if image is wide (landscape) or tall (portrait)
            double aspectRatio = (double)bitmap.PixelWidth / (double)bitmap.PixelHeight;
            bool isLandscape = aspectRatio > 1;

            pg.Orientation = isLandscape                      // Orient the .pdf page
                ? PageOrientation.Landscape
                : PageOrientation.Portrait;

            // Calculate the maximum allowable size for an image with printer margins
            int maxWidth = (int)(pg.Width - pageMargin.Left - pageMargin.Right);
            int maxHeight = (int)(pg.Height - pageMargin.Top - pageMargin.Bottom);

            // Find the ratio to scale the image to fit it to the page
            double fitRatio = System.Math.Min (
                (double)maxWidth / (double)bitmap.PixelWidth,
                (double)maxHeight / (double)bitmap.PixelHeight);

            // Find the desired image size with scaling, maintaining aspect ratio
            int desiredWidth = (int)(bitmap.PixelWidth * fitRatio);
            int desiredHeight = (int)(bitmap.PixelHeight * fitRatio);

            XGraphics gfx = XGraphics.FromPdfPage (pg);
            XImage img = XImage.FromBitmapSource (bitmap);

            // Draw the image, padding the "short" side
            gfx.DrawImage (img,
                pageMargin.Left + ((maxWidth - desiredWidth) / 2),
                pageMargin.Top + ((maxHeight - desiredHeight) / 2),
                desiredWidth,
                desiredHeight);

            // Draw the title to the top right
            gfx.DrawString (title,
                new XFont ("Verdana", 10, XFontStyle.Bold),
                XBrushes.Black,
                new XRect (pageMargin.Left, pageMargin.Top - headerMargin, maxWidth, 30),
                XStringFormats.TopRight);

            gfx.DrawString (Utility.DateTime_ToString (DateTime.Now),
                new XFont ("Verdana", 8, XFontStyle.Regular),
                XBrushes.Black,
                new XRect (pageMargin.Left, pageMargin.Top - headerMargin, maxWidth, 30),
                XStringFormats.BottomRight);

            return doc;
        }

        public static void SavePdf (PdfDocument doc, string filepath)
            => doc.Save (filepath);

        public static void SavePdf (BitmapSource bitsource, string title) {
            /* Initiate IO stream, show Save File dialog to select file destination */
            Stream s;
            Microsoft.Win32.SaveFileDialog dlgSave = new Microsoft.Win32.SaveFileDialog ();

            dlgSave.Filter = "Portable Document Format (*.pdf)|*.pdf|All files (*.*)|*.*";
            dlgSave.FilterIndex = 1;
            dlgSave.RestoreDirectory = true;

            if (dlgSave.ShowDialog () == true) {
                ScreenshotPdf.SavePdf (
                    ScreenshotPdf.AssemblePdf (bitsource, title),
                    dlgSave.FileName);
            }
        }

        public static void PrintPdf (PdfDocument doc) {
            string filepath = II.File.GetTempDirPath ()
                + Utility.DateTime_ToString_FilePath (DateTime.Now) + ".pdf";
            SavePdf (doc, filepath);

            Process.Start (filepath);
        }
    }
}