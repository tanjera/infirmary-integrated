using System;
using System.Drawing;
using System.Windows.Forms;

namespace Infirmary_Integrated.Controls {
    public partial class Values_HR : UserControl {

        public enum ControlType {
            HR,
            SpO2
        };

        public Values_HR () {
            InitializeComponent ();
        }

        public void Update(Patient p, ControlType t) {
            switch (t) {
                default:
                case ControlType.HR:
                    labelType.ForeColor = Color.Green;
                    labelHR.ForeColor = Color.Green;

                    labelType.Text = "ECG";
                    labelHR.Text = p.HR.ToString ();
                    return;

                case ControlType.SpO2:
                    labelType.ForeColor = Color.Yellow;
                    labelHR.ForeColor = Color.Yellow;

                    labelType.Text = "SpO2";
                    labelHR.Text = p.SpO2.ToString ();
                    return;
            }
        }
    }
}
