using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;

using Excel = Microsoft.Office.Interop.Excel;

namespace Dictionary_Builder {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DictionaryBuild : Window {
        private BackgroundWorker bgWorker = new BackgroundWorker ();
        private StringBuilder dictOut = new StringBuilder ();

        public DictionaryBuild () {
            InitializeComponent ();

            // Set up "Load File" dialog
            Microsoft.Win32.OpenFileDialog dlgLoad = new Microsoft.Win32.OpenFileDialog ();
            dlgLoad.Title = "Select location of Localization Strings.xlsx";
            dlgLoad.FileName = "Localization Strings";
            dlgLoad.DefaultExt = ".xlsx";
            dlgLoad.Filter = "Excel Spreadsheet (*.xlsx)|*.xlsx|All files (*.*)|*.*";
            dlgLoad.FilterIndex = 1;
            dlgLoad.RestoreDirectory = true;

            // Set up "Load File" dialog
            Microsoft.Win32.SaveFileDialog dlgSave = new Microsoft.Win32.SaveFileDialog ();
            dlgSave.Title = "Destination to save Localization.Dictionary.cs";
            dlgSave.FileName = "Localization.Dictionary"; // Default file name
            dlgSave.DefaultExt = ".cs"; // Default file extension
            dlgSave.Filter = "C# File (*.cs)|*.cs|All files (*.*)|*.*";
            dlgSave.FilterIndex = 1;
            dlgSave.OverwritePrompt = true;
            dlgSave.RestoreDirectory = true;

            // Set up BackgroundWorker to report process and finish output
            bgWorker.WorkerReportsProgress = true;

            bgWorker.DoWork += new DoWorkEventHandler (ProcessSpreadsheet);

            bgWorker.ProgressChanged += (s, e) =>
                txtOutput.AppendText (e.UserState.ToString ());

            // Run the program!
            txtOutput.Clear ();
            if (dlgLoad.ShowDialog () == true) {
                bgWorker.RunWorkerAsync (dlgLoad.FileName);
            }

            bgWorker.RunWorkerCompleted += (s, e) => {
                if (dlgSave.ShowDialog () == true) {
                    StreamWriter outFile = new StreamWriter (dlgSave.FileName, false);
                    outFile.Write (dictOut.ToString ());
                    outFile.Close ();
                    txtOutput.AppendText (String.Format ("\n\nOutput written to {0}\n", dlgSave.FileName));
                    txtOutput.AppendText ("You may now close this program.");
                }
            };
        }

        private void ProcessSpreadsheet (object sender, DoWorkEventArgs e) {
            BackgroundWorker worker = sender as BackgroundWorker;

            List<string> Languages = new List<string> ();
            List<Dictionary<string, string>> Dictionaries = new List<Dictionary<string, string>> ();

            Excel.Application xApp = new Excel.Application ();
            Excel.Workbook xWorkbook = xApp.Workbooks.Open (e.Argument.ToString (), 0, true, 5, "", "", true, Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            Excel.Worksheet xWorksheet = (Excel.Worksheet)xWorkbook.Sheets [1];

            Excel.Range xRange = xWorksheet.UsedRange;
            int rowCount = xRange.Rows.Count;
            int colCount = xRange.Columns.Count;

            string key;
            Excel.Range range;

            worker.ReportProgress (1, "Processing language codes.\n");

            // $B1 -> $...1: language codes
            for (int j = 2; j <= colCount; j++) {
                Languages.Add ((xWorksheet.Cells [1, j] as Excel.Range).Value.ToString ().ToUpper ().Substring (0, 3));
                Dictionaries.Add (new Dictionary<string, string> ());
            }

            // Iterate all rows, populate respective dictionaries
            for (int i = 2; i <= rowCount; i++) {
                range = xWorksheet.Cells [i, 1] as Excel.Range;
                if (range == null || range.Value == null)
                    continue;

                key = range.Value.ToString ();
                worker.ReportProgress (1, String.Format ("Processing row {0:000}: {1}\n", i, key));

                for (int j = 2; j <= colCount; j++) {
                    range = xWorksheet.Cells [i, j] as Excel.Range;
                    Dictionaries [j - 2].Add (key, range.Value == null ? "" : range.Value.ToString ());
                }
            }

            xWorkbook.Close ();
            xApp.Quit ();

            // Compile dictionaries into C# localization code
            dictOut.Append (
              "using System;\n"
            + "using System.Collections.Generic;\n"
            + "using System.Globalization;\n"
            + "using System.Text;\n\n"
            + "namespace II.Localization {\n\n"
            + "\tpublic partial class Language {\n\n");

            for (int i = 0; i < Languages.Count; i++) {
                dictOut.AppendLine (String.Format ("\t\tstatic Dictionary<string, string> {0} = new Dictionary<string, string> () {{", Languages [i]));

                foreach (KeyValuePair<string, string> pair in Dictionaries [i])
                    dictOut.AppendLine (String.Format ("\t\t\t{{{0,-60} {1}}},",
                        String.Format ("\"{0}\",", pair.Key),
                        String.Format ("\"{0}\"", pair.Value)));

                dictOut.AppendLine ("\t\t};\n");
            }

            dictOut.AppendLine ("\t}\n}");
        }

        private void txtOutput_TextChanged (object sender, System.Windows.Controls.TextChangedEventArgs e) {
            txtOutput.ScrollToEnd ();
        }
    }
}