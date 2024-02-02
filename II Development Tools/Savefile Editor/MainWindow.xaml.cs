using System.IO;
using System.Text;
using System.Text.Unicode;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using II;

using Microsoft.Win32;

namespace Savefile_Editor {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public MainWindow () {
            InitializeComponent ();
        }

        private void btnSelectFile_Click (object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog ();
            if (openFileDialog.ShowDialog () == true)
                tbFilepath.Text = openFileDialog.FileName;
        }

        private void btnDecode_Click (object sender, RoutedEventArgs e) {
            if (!File.Exists (tbFilepath.Text)) {
                MessageBox.Show ("File does not exist!", "Invalid File", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }

            try {
                using StreamReader sr = new StreamReader (tbFilepath.Text);
                string metadata = sr?.ReadLine ()?.Trim () ?? "";
                string hash = sr?.ReadLine ()?.Trim () ?? "";
                string file = sr?.ReadLine ()?.Trim () ?? "";
                sr?.Close ();

                string outfile;

                string outfp = System.IO.Path.ChangeExtension (tbFilepath.Text, "txt");

                using StreamWriter sw = new StreamWriter (outfp);
                if (!String.IsNullOrWhiteSpace (file))
                    sw.Write (Encryption.DecryptAES (file));
                else
                    throw new FileFormatException ();

                MessageBox.Show ($"File decoding completed. File outputted to {outfp}", "Decode Complete", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
            } catch (Exception ex) {
                MessageBox.Show ($"Error occurred. Error message: {ex.Message}", "Error Occurred", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            }
        }

        private void btnEncodeT1_Click (object sender, RoutedEventArgs e) {
            if (!File.Exists (tbFilepath.Text)) {
                MessageBox.Show ("File does not exist!", "Invalid File", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }

            try {
                using StreamReader sr = new StreamReader (tbFilepath.Text);
                string file = sr?.ReadToEnd () ?? "";
                sr?.Close ();

                string outfp = System.IO.Path.ChangeExtension (tbFilepath.Text, "ii");

                using StreamWriter sw = new StreamWriter (outfp);
                sw.WriteLine (".ii:t1");
                sw.WriteLine (Encryption.HashSHA256 (file));
                sw.WriteLine (Encryption.EncryptAES (file));

                MessageBox.Show ($"File encoding completed. File outputted to {outfp}", "Encode Complete", MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
            } catch (Exception ex) {
                MessageBox.Show ($"Error occurred. Error message: {ex.Message}", "Error Occurred", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            }
        }
    }
}