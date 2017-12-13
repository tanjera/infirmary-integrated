using System.Windows.Forms;

namespace II.Forms {
    public partial class Dialog_About : Form {
        public Dialog_About () {
            InitializeComponent ();

            labelVersion.Text = string.Format("Version {0}", _.Version);
        }
    }
}
