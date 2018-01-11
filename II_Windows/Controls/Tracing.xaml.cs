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
using II.Localization;

namespace II_Windows.Controls {
    /// <summary>
    /// Interaction logic for Tracing.xaml
    /// </summary>
    public partial class Tracing : UserControl {

        public Strip rStrip;
        Brush tBrush;

        // Tracing Point offsets and multipliers
        int offX, offY;
        double multX, multY;

        public event EventHandler<TracingEdited_EventArgs> TracingEdited;
        public class TracingEdited_EventArgs : EventArgs {
            public Leads Lead { get; set; }
            public TracingEdited_EventArgs (Leads lead) { Lead = lead; }
        }

        public Tracing (Strip strip) {
            InitializeComponent ();

            rStrip = strip;
            SetLead (rStrip.Lead.Value);
        }


        public void SetLead (Leads.Values l) {
            rStrip.Lead.Value = l;
            lblLead.Content = Strings.Lookup(App.Language.Value, Leads.LookupString(l));

            switch (l) {
                default: tBrush = Brushes.Green; break;
                case Leads.Values.ABP: tBrush = Brushes.Red; break;
                case Leads.Values.CVP: tBrush = Brushes.Blue; break;
                case Leads.Values.PA: tBrush = Brushes.Yellow; break;
                case Leads.Values.RR: tBrush = Brushes.Salmon; break;
                case Leads.Values.ETCO2: tBrush = Brushes.Aqua; break;
                case Leads.Values.SPO2: tBrush = Brushes.Orange; break;
            }

            lblLead.Foreground = tBrush;
        }

        public void Draw () {
            offX = 0;
            offY = (int)canvasTracing.ActualHeight / 2;
            multX = (int)canvasTracing.ActualWidth / rStrip.Length;
            multY = -(int)canvasTracing.ActualHeight / 2;

            if (rStrip.Points.Count < 2)
                return;

            rStrip.RemoveNull ();
            rStrip.Sort ();

            Path sp = new Path { Stroke = tBrush, StrokeThickness = 1 };
            StreamGeometry sg = new StreamGeometry { FillRule = FillRule.EvenOdd };

            using (StreamGeometryContext sgc = sg.Open()) {
                sgc.BeginFigure (new System.Windows.Point (
                    (int)(rStrip.Points [0].X * multX) + offX,
                    (int)(rStrip.Points [0].Y * multY) + offY),
                    true, false);

                for (int i = 1; i < rStrip.Points.Count; i++) {
                    if (rStrip.Points [i].X > rStrip.Length * 2)
                        continue;

                    sgc.LineTo(new System.Windows.Point (
                        (int)(rStrip.Points [i].X * multX) + offX,
                        (int)(rStrip.Points [i].Y * multY) + offY),
                        true, true);
                }
            }

            sg.Freeze ();
            sp.Data = sg;

            canvasTracing.Children.Clear ();
            canvasTracing.Children.Add (sp);
        }
    }
}
