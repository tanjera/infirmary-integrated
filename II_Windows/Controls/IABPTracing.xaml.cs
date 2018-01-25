using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;


using II;
using II.Rhythm;
using II.Localization;

namespace II_Windows.Controls {
    /// <summary>
    /// Interaction logic for Tracing.xaml
    /// </summary>
    public partial class IABPTracing : UserControl {

        public Strip wfStrip;
        public Leads Lead { get { return wfStrip.Lead; } }

        // Drawing variables, offsets and multipliers
        Path drawPath;
        Brush drawBrush;
        StreamGeometry drawGeometry;
        StreamGeometryContext drawContext;
        int drawXOffset, drawYOffset;
        double drawXMultiplier, drawYMultiplier;


        public IABPTracing (Strip strip) {
            InitializeComponent ();
            DataContext = this;

            wfStrip = strip;

            UpdateInterface (null, null);
        }

        private void UpdateInterface (object sender, SizeChangedEventArgs e) {
            switch (wfStrip.Lead.Value) {
                default: drawBrush = Brushes.Green; break;
                case Leads.Values.ABP: drawBrush = Brushes.Red; break;
                case Leads.Values.IABP: drawBrush = Brushes.SkyBlue; break;
            }

            borderTracing.BorderBrush = drawBrush;

            lblLead.Foreground = drawBrush;
            lblLead.Content = App.Language.Dictionary[Leads.LookupString (Lead.Value)];
        }

        public void Scroll () => wfStrip.Scroll ();
        public void Unpause () => wfStrip.Unpause ();
        public void ClearFuture () => wfStrip.ClearFuture ();
        public void Add_Beat__Cardiac_Baseline (Patient P) => wfStrip.Add_Beat__Cardiac_Baseline (P);
        public void Add_Beat__Cardiac_Atrial (Patient P) {
            if (Lead.IsTransduced () && !Leads.IsZeroed (Lead.Value, P))
                return;
            else
                wfStrip.Add_Beat__Cardiac_Atrial (P);
        }
        public void Add_Beat__Cardiac_Ventricular (Patient P) {
            if (Lead.IsTransduced () && !Leads.IsZeroed (Lead.Value, P))
                return;
            else
                wfStrip.Add_Beat__Cardiac_Ventricular (P);
        }

        public void Draw () {
            if (Lead.IsTransduced () && !Leads.IsZeroed (Lead.Value, App.Patient)) {
                wfStrip.Reset ();
                canvasTracing.Children.Clear ();
                return;
            }

            drawXOffset = 0;
            drawYOffset = (int)canvasTracing.ActualHeight / 2;
            drawXMultiplier = (int)canvasTracing.ActualWidth / wfStrip.lengthSeconds;
            drawYMultiplier = -(int)canvasTracing.ActualHeight / 2;

            if (wfStrip.Points.Count < 2)
                return;

            wfStrip.RemoveNull ();
            wfStrip.Sort ();

            drawPath = new Path { Stroke = drawBrush, StrokeThickness = 1 };
            drawGeometry = new StreamGeometry { FillRule = FillRule.EvenOdd };

            using (drawContext = drawGeometry.Open ()) {
                drawContext.BeginFigure (new System.Windows.Point (
                    (int)(wfStrip.Points [0].X * drawXMultiplier) + drawXOffset,
                    (int)(wfStrip.Points [0].Y * drawYMultiplier) + drawYOffset),
                    true, false);

                for (int i = 1; i < wfStrip.Points.Count; i++) {
                    if (wfStrip.Points [i].X > wfStrip.lengthSeconds * 2)
                        continue;

                    drawContext.LineTo (new System.Windows.Point (
                        (int)(wfStrip.Points [i].X * drawXMultiplier) + drawXOffset,
                        (int)(wfStrip.Points [i].Y * drawYMultiplier) + drawYOffset),
                        true, true);
                }
            }

            drawGeometry.Freeze ();
            drawPath.Data = drawGeometry;

            canvasTracing.Children.Clear ();
            canvasTracing.Children.Add (drawPath);
        }
    }
}
