using System;
using System.Drawing;
using System.Windows.Forms;

namespace Infirmary_Integrated.Controls {
    public partial class Values_BP : UserControl {

        public enum ControlType {
            NIBP,
            ART,
            PA
        };

        public Values_BP () {
            InitializeComponent ();
        }

        public void Update (Patient p, ControlType t) {
            labelType.ForeColor = Color.Red;
            labelSBP.ForeColor = Color.Red;
            labelDBP.ForeColor = Color.Red;
            labelMAP.ForeColor = Color.Red;

            labelSBP.Text = p.SBP.ToString ();
            labelDBP.Text = String.Format("/ {0}", p.DBP.ToString ());
            labelMAP.Text = String.Format("({0})", p.MAP.ToString ());
            
            switch (t) {
                default:
                case ControlType.NIBP:
                    labelType.Text = "NIBP";
                    break;

                case ControlType.ART:
                    labelType.Text = "ART";
                    break;

                case ControlType.PA:
                    labelType.Text = "NIBP";
                    break;
            }
            
        }
    }
}
