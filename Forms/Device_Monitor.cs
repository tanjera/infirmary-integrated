using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Infirmary_Integrated.Rhythms;

namespace Infirmary_Integrated
{
    public partial class Device_Monitor : Form
    {
        Strip stripECG = new Strip ();
        Strip_Renderer renderECG;

        Patient _Patient;

        public Device_Monitor()
        {
            InitializeComponent();

            renderECG = new Strip_Renderer (ecgTracing.CreateGraphics (), ref stripECG,
                new Pen (Color.Green, 1f));           
        }

        public void onTick () {
            stripECG.Scroll ();
            renderECG.Draw ();
        }

        private void ecgTracing_Paint (object sender, PaintEventArgs e) {
            renderECG.Draw ();
        }

        private void ecgNumerics_Paint (object sender, PaintEventArgs e) {
        }
    }
}
