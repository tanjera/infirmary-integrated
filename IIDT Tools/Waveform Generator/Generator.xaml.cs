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

namespace Waveform_Generator {

    public partial class Generator : Window {
        public static int DrawResolution = 10;

        private BackgroundWorker bgWorker = new BackgroundWorker ();
        private StringBuilder dictOut = new StringBuilder ();

        public Generator () {
            InitializeComponent ();

            // Set up BackgroundWorker to report process and finish output
            bgWorker.WorkerReportsProgress = true;

            bgWorker.DoWork += new DoWorkEventHandler (ProcessFunction);

            bgWorker.ProgressChanged += (s, e) =>
                txtOutput.AppendText (e.UserState.ToString ());

            // Run the program!
            txtOutput.Clear ();
            bgWorker.RunWorkerAsync ();

            bgWorker.RunWorkerCompleted += (s, e) => {
                txtOutput.AppendText ("Waveform generated and outputted.\n");
                txtOutput.AppendText ("You may now close this program.");
                Application.Current.Shutdown ();
            };
        }

        private void ProcessFunction (object sender, DoWorkEventArgs e) {
            BackgroundWorker worker = sender as BackgroundWorker;

            string WaveName = "";
            double DrawLength = 0.0;
            int IndexOffset = 0;
            List<Vertex> Vertices = new List<Vertex> ();

            /* Generate waveform into List<Point> via Waveform.cs */
            List<Point> Wave = Waveform.Generate (DrawResolution, out WaveName);
            WaveName = WaveName.Trim ().Replace (' ', '_');

            /* Convert List<Point> to List<Vertex> and calculate associated WaveData parameters */
            DrawLength = Math.Round (Wave.Last ().X, 1);

            for (int i = 0; i < Wave.Count; i++) {        // NOTE: MAY NEED TO SCALE X AXIS TO EACH X POINT @ DRAWRESOLUTION
                Vertices.Add (new Vertex (Wave [i].Y));
            }

            if (String.IsNullOrEmpty (WaveName)) {
                MessageBox.Show (
                        "You must enter a name for the Waveform in order to save.",
                        "Waveform Name Required", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Stream s;
            Microsoft.Win32.SaveFileDialog dlgSave = new Microsoft.Win32.SaveFileDialog ();
            dlgSave.FileName = String.Format ("{0}.iiwf", WaveName);
            dlgSave.Filter = "Infirmary Integrated waveform files (*.iiwf)|*.iiwf|All files (*.*)|*.*";
            dlgSave.FilterIndex = 1;
            dlgSave.RestoreDirectory = true;

            if (dlgSave.ShowDialog () == true) {
                if ((s = dlgSave.OpenFile ()) != null) {
                    StringBuilder sb = new StringBuilder ();

                    sb.AppendLine (String.Format ("{0}:{1}", "WaveName", WaveName));
                    sb.AppendLine (String.Format ("{0}:{1}", "DrawResolution", DrawResolution));
                    sb.AppendLine (String.Format ("{0}:{1}", "IndexOffset", IndexOffset));

                    StringBuilder sbVert = new StringBuilder ();
                    for (int i = 0; i < Vertices.Count; i++)
                        sbVert.Append (String.Format ("({0} {1}) ", i, Math.Round (Vertices [i].Y, 2)));

                    sb.AppendLine (String.Format ("{0}:{1}", "Vertices", sbVert.ToString ().Trim ()));

                    StreamWriter sw = new StreamWriter (s);
                    sw.Write (sb.ToString ().Trim ());

                    sw.Close ();
                    s.Close ();
                }
            }
        }

        private void txtOutput_TextChanged (object sender, System.Windows.Controls.TextChangedEventArgs e) {
            txtOutput.ScrollToEnd ();
        }
    }
}