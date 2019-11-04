using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace II {
    public static class Screenshot {
        private static int headerMargin = 30;

        private static int marginLeft = 50;
        private static int marginRight = 50;
        private static int marginTop = 80;
        private static int marginBottom = 50;

        public static PdfDocument AssemblePdf (string bitpath, string title, string header) {
            XImage bitmap = XImage.FromFile (bitpath);

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
            int maxWidth = (int)(pg.Width - marginLeft - marginRight);
            int maxHeight = (int)(pg.Height - marginTop - marginBottom);

            // Find the ratio to scale the image to fit it to the page
            double fitRatio = System.Math.Min (
                (double)maxWidth / (double)bitmap.PixelWidth,
                (double)maxHeight / (double)bitmap.PixelHeight);

            // Find the desired image size with scaling, maintaining aspect ratio
            int desiredWidth = (int)(bitmap.PixelWidth * fitRatio);
            int desiredHeight = (int)(bitmap.PixelHeight * fitRatio);

            XGraphics gfx = XGraphics.FromPdfPage (pg);

            // Draw the image, padding the "short" side
            gfx.DrawImage (bitmap,
                marginLeft + ((maxWidth - desiredWidth) / 2),
                marginTop + ((maxHeight - desiredHeight) / 2),
                desiredWidth,
                desiredHeight);

            // Draw the header to the top left
            gfx.DrawString (String.IsNullOrEmpty (header) ? "" : header,
                new XFont ("Verdana", 8, XFontStyle.Regular),
                XBrushes.Black,
                new XRect (marginLeft, marginTop - headerMargin, maxWidth, 30),
                XStringFormats.BottomLeft);

            // Draw the title to the top right
            gfx.DrawString (String.IsNullOrEmpty (title) ? "" : title,
                new XFont ("Verdana", 10, XFontStyle.Bold),
                XBrushes.Black,
                new XRect (marginLeft, marginTop - headerMargin, maxWidth, 30),
                XStringFormats.TopRight);

            gfx.DrawString (Utility.DateTime_ToString (DateTime.Now),
                new XFont ("Verdana", 8, XFontStyle.Regular),
                XBrushes.Black,
                new XRect (marginLeft, marginTop - headerMargin, maxWidth, 30),
                XStringFormats.BottomRight);

            return doc;
        }

        public static void SavePdf (PdfDocument doc, string filepath)
            => doc.Save (filepath);

        public static void PrintPdf (PdfDocument doc) {
            string filepath = II.File.GetTempDirPath ()
                + Utility.DateTime_ToString_FilePath (DateTime.Now) + ".pdf";
            SavePdf (doc, filepath);

            Process.Start (filepath);
        }
    }
}