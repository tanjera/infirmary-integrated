using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace II.Forms
{
    public partial class Dialog_License : Form
    {
        public Dialog_License ()
        {
            /* License added to Textbox on Dec 21, 2017 IK */

            InitializeComponent ();
        }

        private void ButtonAccept_Click (object sender, EventArgs e) {
            this.DialogResult = DialogResult.OK;
            this.Close ();
        }

        private void ButtonDecline_Click (object sender, EventArgs e) {
            this.DialogResult = DialogResult.Cancel;
            this.Close ();
        }
    }
}
