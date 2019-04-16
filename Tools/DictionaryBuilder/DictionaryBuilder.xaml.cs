using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

using Excel = Microsoft.Office.Interop.Excel;


namespace DictionaryBuilder {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DictionaryBuild : Window {
        public DictionaryBuild () {
            InitializeComponent ();

            // Hard-coded... too lazy to make a file picker
            txtFilepath.Text = @"C:\Users\Ibi\Documents\Infirmary Integrated\Tools\DictionaryBuilder\bin\Debug\Localization Strings.xlsx";
        }

        private void OnClick_ProcessSpreadsheet(object sender, RoutedEventArgs e) {
            List<string> Languages = new List<string>();
            List<Dictionary<string, string>> Dictionaries = new List<Dictionary<string, string>>();
            StringBuilder sbOut = new StringBuilder();

            txtOutput.Clear();

            Excel.Application xApp = new Excel.Application();
            Excel.Workbook xWorkbook = xApp.Workbooks.Open(txtFilepath.Text, 0, true, 5, "", "", true, Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            Excel.Worksheet xWorksheet = (Excel.Worksheet)xWorkbook.Sheets[1];

            Excel.Range xRange = xWorksheet.UsedRange;
            int rowCount = xRange.Rows.Count;
            int colCount = xRange.Columns.Count;

            string key;
            Excel.Range range;


            // $B1 -> $...1: language codes
            for (int j = 2; j <= colCount; j++) {
                Languages.Add((xWorksheet.Cells[1, j] as Excel.Range).Value.ToString().ToUpper().Substring(0, 3));
                Dictionaries.Add(new Dictionary<string, string>());
            }

            // Iterate all rows, populate respective dictionaries
            for (int i = 2; i <= rowCount; i++) {
                range = xWorksheet.Cells[i, 1] as Excel.Range;
                if (range == null || range.Value == null)
                    continue;

                key = range.Value.ToString();

                for (int j = 2; j <= colCount; j++) {
                    range = xWorksheet.Cells[i, j] as Excel.Range;
                    Dictionaries[j - 2].Add(key, range.Value == null ? "" : range.Value.ToString());
                }
            }

            xWorkbook.Close();
            xApp.Quit();

            // Compile dictionaries into C# localization code
            for (int i = 0; i < Languages.Count; i++) {
                sbOut.AppendLine(String.Format("\t\tstatic Dictionary<string, string> {0} = new Dictionary<string, string> () {{", Languages[i]));

                foreach (KeyValuePair<string, string> pair in Dictionaries[i])
                    sbOut.AppendLine(String.Format("\t\t\t{{{0,-60} {1}}},",
                        String.Format("\"{0}\",", pair.Key),
                        String.Format("\"{0}\"", pair.Value)));

                sbOut.AppendLine("\t\t};\n\n");
            }

            txtOutput.Text = sbOut.ToString ();
            Clipboard.SetText (txtOutput.Text);
        }
    }
}
