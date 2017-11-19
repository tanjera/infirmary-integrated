using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using Infirmary_Integrated.Rhythms;

namespace Infirmary_Integrated
{
    public partial class Device_Monitor : Form
    {
        Strip stripECG;
        Strip.Renderer renderECG;

        Patient _Patient = new Patient();


        public Device_Monitor()
        {
            InitializeComponent();

            timerDraw.Interval = _.Draw_Refresh;

            stripECG = new Strip ();
            renderECG = new Strip.Renderer (ecgTracing, ref stripECG, new Pen (Color.Green, 1f));
        }

        private void onTick (object sender, EventArgs e) {
            stripECG.Scroll ();
            ecgTracing.Invalidate ();
            
            // HARDCODE: populate strip with NSR
            Rhythm.EKG_Rhythm__Normal_Sinus (ref stripECG, 80, 0f);
        }

        private void ECGTracing_Paint (object sender, PaintEventArgs e) {
            renderECG.Draw (e);
        }

    }
}
