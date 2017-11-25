using System;
using System.Drawing;
using System.Windows.Forms;

namespace II.Controls {
    public partial class Values : UserControl {

        public ControlType cType;

        public enum ControlType {
            ECG,
            SPO2,
            NIBP,
            ART,
            CVP,
            PA
        };

        static public Color valueColors(ControlType c) {
            switch (c) {
                default:
                case ControlType.ECG: return Color.Green;
                case ControlType.SPO2: return Color.Orange;
                case ControlType.NIBP: return Color.White;
                case ControlType.ART: return Color.Red;
                case ControlType.CVP: return Color.Blue;
                case ControlType.PA: return Color.Yellow;
            }            
        }

        public Values (ControlType t) {
            InitializeComponent ();

            cType = t;
        }

        public void Update (Patient p) {
            labelType.ForeColor = valueColors(cType);
            label1.ForeColor = valueColors (cType);
            label2.ForeColor = valueColors (cType);
            label3.ForeColor = valueColors (cType);

            switch (cType) {
                default:
                case ControlType.ECG:
                    labelType.Text = "ECG";
                    label1.Text = p.HR.ToString ();
                    label2.ForeColor = Color.Black;
                    label3.ForeColor = Color.Black;
                    break;

                case ControlType.SPO2:
                    labelType.Text = "SpO2";
                    label1.Text = p.SpO2.ToString ();
                    label2.ForeColor = Color.Black;
                    label3.ForeColor = Color.Black;
                    break;


                case ControlType.NIBP:
                    labelType.Text = "NIBP";
                    label1.Text = p.SBP.ToString ();
                    label2.Text = String.Format ("/ {0}", p.DBP.ToString ());
                    label3.Text = String.Format ("({0})", p.MAP.ToString ());
                    break;

                case ControlType.ART:
                    labelType.Text = "ART";
                    label1.Text = p.SBP.ToString ();
                    label2.Text = String.Format ("/ {0}", p.DBP.ToString ());
                    label3.Text = String.Format ("({0})", p.MAP.ToString ());
                    break;

                case ControlType.CVP:
                    labelType.Text = "CVP";
                    label1.Text = p.CVP.ToString ();
                    label2.ForeColor = Color.Black;
                    label3.ForeColor = Color.Black;
                    break;

                case ControlType.PA:
                    labelType.Text = "PA";
                    label1.Text = p.PSP.ToString ();
                    label2.Text = String.Format ("/ {0}", p.PDP.ToString ());
                    label3.Text = String.Format ("({0})", p.PMP.ToString ());
                    break;
            }

            
        }
    }
}
