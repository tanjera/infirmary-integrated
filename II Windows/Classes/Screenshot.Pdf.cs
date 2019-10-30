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

using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace II_Windows {

    public static class ScreenshotPdf {
        private static int offsetVertical = 75;
        private static int offsetHorizontal = 50;

        public static PdfDocument AssemblePdf (BitmapSource bitmap, string title, bool landscape = true) {
            PdfDocument doc = new PdfDocument ();
            doc.Info.Title = title;

            PdfPage pg = doc.AddPage ();
            pg.Orientation = landscape
                ? PageOrientation.Landscape
                : PageOrientation.Portrait;

            XGraphics gfx = XGraphics.FromPdfPage (pg);
            XImage img = XImage.FromBitmapSource (bitmap);

            gfx.DrawImage (img,
                offsetHorizontal,
                offsetVertical,
                pg.Width - (offsetHorizontal * 2),
                pg.Height - (offsetVertical * 2));

            return doc;
        }

        public static void SavePdf (PdfDocument doc, string filepath)
            => doc.Save (filepath);

        public static void SavePdf (BitmapSource bitsource) {
            /* Initiate IO stream, show Save File dialog to select file destination */
            Stream s;
            Microsoft.Win32.SaveFileDialog dlgSave = new Microsoft.Win32.SaveFileDialog ();

            dlgSave.Filter = "Portable Document Format (*.pdf)|*.pdf|All files (*.*)|*.*";
            dlgSave.FilterIndex = 1;
            dlgSave.RestoreDirectory = true;

            if (dlgSave.ShowDialog () == true) {
                ScreenshotPdf.SavePdf (ScreenshotPdf.AssemblePdf (
                    bitsource, "Infirmary Integrated, 12 Lead ECG"),
                    dlgSave.FileName);
            }
        }

        public static void PrintPdf (PdfDocument doc) {
            string filepath = Path.GetTempPath () + Guid.NewGuid ().ToString () + ".pdf";
            SavePdf (doc, filepath);

            Process.Start (filepath, "--print");
        }
    }
}