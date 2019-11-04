using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Ookii.Dialogs.Wpf;

namespace Waveform_Dictionary_Builder {

    public partial class Dictionary_Builder : Window {
        private BackgroundWorker bgWorker = new BackgroundWorker ();
        private StringBuilder dictOut = new StringBuilder ();

        public Dictionary_Builder () {
            InitializeComponent ();

            // Set up "Load Folder" dialog
            VistaFolderBrowserDialog dlgLoad = new VistaFolderBrowserDialog ();
            dlgLoad.Description = "Select 'Waveforms' folder";

            // Set up "Save File" dialog
            Microsoft.Win32.SaveFileDialog dlgSave = new Microsoft.Win32.SaveFileDialog ();
            dlgSave.Title = "Destination to save Waveform.Dictionary.Plots.cs";
            dlgSave.FileName = "Waveform.Dictionary.Plots"; // Default file name
            dlgSave.DefaultExt = ".cs"; // Default file extension
            dlgSave.Filter = "C# File (*.cs)|*.cs|All files (*.*)|*.*";
            dlgSave.FilterIndex = 1;
            dlgSave.OverwritePrompt = true;
            dlgSave.RestoreDirectory = true;

            // Set up BackgroundWorker to report process and finish output
            bgWorker.WorkerReportsProgress = true;

            bgWorker.DoWork += new DoWorkEventHandler (ProcessFolder);

            bgWorker.ProgressChanged += (s, e) =>
                txtOutput.AppendText (e.UserState.ToString ());

            // Run the program!
            txtOutput.Clear ();
            if (dlgLoad.ShowDialog () == true) {
                bgWorker.RunWorkerAsync (dlgLoad.SelectedPath);
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

        private void ProcessFolder (object sender, DoWorkEventArgs e) {
            BackgroundWorker worker = sender as BackgroundWorker;

            dictOut.Append (
              "using System;\n"
            + "using System.Collections.Generic;\n\n"

            + "namespace II.Waveform {\n"
            + "\tpublic static partial class Dictionary {\n");

            worker.ReportProgress (1, "Processing waveform files.\n");
            string [] files = Directory.GetFiles (e.Argument.ToString (), "*.iiwf");

            // Iterate all files, populate respective dictionary
            for (int i = 0; i < files.Length; i++) {
                worker.ReportProgress (1, String.Format ("Processing file {0:000}: {1}\n", i, files [i]));

                string WaveName = "";
                int DrawResolution = 0;
                int IndexOffset = 0;
                List<Vertex> Vertices = new List<Vertex> ();

                /* Load individual .iiwf file */
                StreamReader sr = new StreamReader (files [i]);
                string file = sr.ReadToEnd ().Trim ();
                sr.Close ();

                StringReader sRead = new StringReader (file);

                try {
                    string line;
                    while ((line = sRead.ReadLine ()) != null) {
                        if (line.Contains (":")) {
                            string pName = line.Substring (0, line.IndexOf (':')),
                                    pValue = line.Substring (line.IndexOf (':') + 1).Trim ();
                            switch (pName) {
                                default: break;

                                case "WaveName": WaveName = pValue; break;
                                case "DrawResolution": DrawResolution = int.Parse (pValue); break;
                                case "IndexOffset": IndexOffset = int.Parse (pValue); break;

                                case "Vertices":
                                    Vertices = new List<Vertex> ();
                                    while (pValue.Length > 0) {
                                        if (!pValue.Trim ().StartsWith ("(") || !pValue.Contains (")"))
                                            break;

                                        /* Pull current coordinate set from string of coordinates */
                                        string coord = pValue.Trim ().Substring (0, pValue.IndexOf (")") + 1);
                                        pValue = pValue.Substring (coord.Length).Trim ();   // And remove the current coordinate from pValue

                                        /* Process the current coordinate and add to Vertices */
                                        string [] coords = coord.Trim ('(', ')').Split (' ');
                                        int x = int.Parse (coords [0]);
                                        double y = double.Parse (coords [1]);

                                        if (Vertices.Count == x)
                                            Vertices.Add (new Vertex (y));
                                    }
                                    break;
                            }
                        }
                    }

                    /* Write .iiwf as PlotData to dictOut */

                    dictOut.AppendLine (String.Format ("\t\tpublic static Plot {0} = new Plot () {{", WaveName));
                    dictOut.AppendLine (String.Format ("\t\t\tDrawResolution = {0},", DrawResolution));
                    dictOut.AppendLine (String.Format ("\t\t\tIndexOffset = {0},", IndexOffset));
                    dictOut.AppendLine (String.Format ("\t\t\tVertices = new float[] {{", IndexOffset));

                    for (int v = 0; v < Vertices.Count; v++) {
                        dictOut.Append (String.Format ("{0}{1}{2}f{3}",
                            (v > 0 && v % 15 == 0 ? "\n" : ""),
                            (v % 15 == 0 ? "\t\t\t\t" : ""),
                            Vertices [v].Y,
                            (v < Vertices.Count - 1 ? ", " : "")));
                    }

                    dictOut.AppendLine ("\n\t\t\t}\n\t\t};\n");
                } catch {
                } finally {
                    sRead.Close ();
                }
            }

            dictOut.AppendLine ("\t}\n}");
        }

        private void txtOutput_TextChanged (object sender, System.Windows.Controls.TextChangedEventArgs e) {
            txtOutput.ScrollToEnd ();
        }
    }
}