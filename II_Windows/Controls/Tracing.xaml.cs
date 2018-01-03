using System;
using System.Collections.Generic;
using System.Linq;
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

using II;
using II.Rhythm;

namespace II_Windows.Controls {
    /// <summary>
    /// Interaction logic for Tracing.xaml
    /// </summary>
    public partial class Tracing : UserControl {

        Strip rStrip;

        // Tracing Point offsets and multipliers
        int offX, offY;
        double multX, multY;

        public event EventHandler<TracingEdited_EventArgs> TracingEdited;
        public class TracingEdited_EventArgs : EventArgs {
            public Leads Lead { get; set; }
            public TracingEdited_EventArgs (Leads lead) { Lead = lead; }
        }

        public Tracing (ref Strip strip) {
            InitializeComponent ();

            rStrip = strip;
        }

        public void Draw () {
            Pen pen;
            //e.Graphics.Clear (Color.Black);
            //pen = new Pen (pcolor, 1f);


            offX = 0;
            //offY = panel.Height / 2;
            //multX = panel.Width / rStrip.Length;
            //multY = -panel.Height / 2;

            if (rStrip.Points.Count < 2)
                return;

            rStrip.RemoveNull ();
            rStrip.Sort ();

            System.Drawing.Point lastPoint = new System.Drawing.Point (
                    (int)(rStrip.Points [0].X * multX) + offX,
                    (int)(rStrip.Points [0].Y * multY) + offY);

            for (int i = 1; i < rStrip.Points.Count; i++) {
                if (rStrip.Points [i].X > rStrip.Length * 2)
                    continue;

                System.Drawing.Point thisPoint = new System.Drawing.Point (
                    (int)(rStrip.Points [i].X * multX) + offX,
                    (int)(rStrip.Points [i].Y * multY) + offY);

                //e.Graphics.DrawLine (pen, lastPoint, thisPoint);
                lastPoint = thisPoint;
            }
        }
    }
}
