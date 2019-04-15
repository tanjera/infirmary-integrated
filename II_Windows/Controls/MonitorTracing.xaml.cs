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
    public partial class MonitorTracing : UserControl {

        public Strip wfStrip;
        public Leads Lead { get { return wfStrip.Lead; } }
        public double Amplitude = 1.0;

        // Drawing variables, offsets and multipliers
        Path drawPath;
        Brush drawBrush;
        StreamGeometry drawGeometry;
        StreamGeometryContext drawContext;
        int drawXOffset, drawYOffset;
        double drawXMultiplier, drawYMultiplier;


        public MonitorTracing (Strip strip) {
            InitializeComponent ();
            DataContext = this;

            wfStrip = strip;

            InitInterface ();
            UpdateInterface (null, null);
        }

        private void InitInterface () {
            // Populate UI strings per language selection
            Languages.Values l = App.Language.Value;

            // Context Menu (right-click menu!)
            ContextMenu contextMenu = new ContextMenu ();
            canvasTracing.ContextMenu = contextMenu;
            lblLead.ContextMenu = contextMenu;

            MenuItem menuZeroTransducer = new MenuItem ();
            menuZeroTransducer.Header = App.Language.Dictionary["MENU:MenuZeroTransducer"];
            menuZeroTransducer.Click += MenuZeroTransducer_Click;
            contextMenu.Items.Add (menuZeroTransducer);

            contextMenu.Items.Add (new Separator ());

            MenuItem menuAddTracing = new MenuItem ();
            menuAddTracing.Header = App.Language.Dictionary["MENU:MenuAddTracing"];
            menuAddTracing.Click += MenuAddTracing_Click;
            contextMenu.Items.Add (menuAddTracing);

            MenuItem menuRemoveTracing = new MenuItem ();
            menuRemoveTracing.Header = App.Language.Dictionary["MENU:MenuRemoveTracing"];
            menuRemoveTracing.Click += MenuRemoveTracing_Click;
            contextMenu.Items.Add (menuRemoveTracing);

            contextMenu.Items.Add (new Separator());

            MenuItem menuIncreaseAmplitude = new MenuItem();
            menuIncreaseAmplitude.Header = App.Language.Dictionary["MENU:IncreaseAmplitude"];
            menuIncreaseAmplitude.Click += MenuIncreaseAmplitude_Click;
            contextMenu.Items.Add(menuIncreaseAmplitude);

            MenuItem menuDecreaseAmplitude = new MenuItem();
            menuDecreaseAmplitude.Header = App.Language.Dictionary["MENU:DecreaseAmplitude"];
            menuDecreaseAmplitude.Click += MenuDecreaseAmplitude_Click;
            contextMenu.Items.Add(menuDecreaseAmplitude);

            contextMenu.Items.Add(new Separator());

            MenuItem menuSelectInput = new MenuItem (),
                     menuECGLeads = new MenuItem();
            menuSelectInput.Header = App.Language.Dictionary["MENU:MenuSelectInputSource"];
            menuECGLeads.Header = App.Language.Dictionary["TRACING:ECG"];
            menuSelectInput.Items.Add (menuECGLeads);

            foreach (Leads.Values v in Enum.GetValues(typeof(Leads.Values))) {
                // Only include certain leads- e.g. bedside monitors don't interface with IABP or EFM
                string el = v.ToString ();
                if (!el.StartsWith ("ECG") && el != "SPO2" && el != "RR" && el != "ETCO2"
                    && el != "CVP" && el != "ABP" && el != "PA"
                    && el != "ICP" && el != "IAP")
                    continue;

                MenuItem mi = new MenuItem ();
                mi.Header = App.Language.Dictionary[Leads.LookupString (v)];
                mi.Name = v.ToString ();
                mi.Click += MenuSelectInputSource;
                if (mi.Name.StartsWith ("ECG"))
                    menuECGLeads.Items.Add (mi);
                else
                    menuSelectInput.Items.Add (mi);
            }

            contextMenu.Items.Add (menuSelectInput);
        }

        private void UpdateInterface (object sender, SizeChangedEventArgs e) {
            switch (Lead.Value) {
                default: drawBrush = Brushes.Green; break;
                case Leads.Values.SPO2: drawBrush = Brushes.Orange; break;
                case Leads.Values.RR: drawBrush = Brushes.Salmon; break;
                case Leads.Values.ETCO2: drawBrush = Brushes.Aqua; break;
                case Leads.Values.ABP: drawBrush = Brushes.Red; break;
                case Leads.Values.CVP: drawBrush = Brushes.Blue; break;
                case Leads.Values.PA: drawBrush = Brushes.Yellow; break;
                case Leads.Values.ICP: drawBrush = Brushes.Khaki; break;
                case Leads.Values.IAP: drawBrush = Brushes.Aquamarine; break;
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
        public void Add_Beat__Cardiac_Atrial (Patient P) => wfStrip.Add_Beat__Cardiac_Atrial (P);
        public void Add_Beat__Cardiac_Ventricular (Patient P) => wfStrip.Add_Beat__Cardiac_Ventricular (P);
        public void Add_Beat__Respiratory_Baseline (Patient P) => wfStrip.Add_Beat__Respiratory_Baseline (P);
        public void Add_Beat__Respiratory_Inspiration (Patient P) => wfStrip.Add_Beat__Respiratory_Inspiration (P);
        public void Add_Beat__Respiratory_Expiration (Patient P) => wfStrip.Add_Beat__Respiratory_Expiration (P);

        public void Draw () {
            drawXOffset = 0;
            drawYOffset = (int)canvasTracing.ActualHeight / 2;
            drawXMultiplier = (int)canvasTracing.ActualWidth / wfStrip.lengthSeconds;
            drawYMultiplier = (-(int)canvasTracing.ActualHeight / 2) * Amplitude;

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

        private void MenuZeroTransducer_Click (object sender, RoutedEventArgs e) {
            switch (Lead.Value) {
                case Leads.Values.ABP: App.Patient.TransducerZeroed_ABP = true; return;
                case Leads.Values.CVP: App.Patient.TransducerZeroed_CVP = true; return;
                case Leads.Values.PA: App.Patient.TransducerZeroed_PA = true; return;
                case Leads.Values.ICP: App.Patient.TransducerZeroed_ICP = true; return;
                case Leads.Values.IAP: App.Patient.TransducerZeroed_IAP = true; return;
            }
        }

        private void MenuAddTracing_Click (object sender, RoutedEventArgs e)
            => App.Device_Monitor.AddTracing ();
        private void MenuRemoveTracing_Click (object sender, RoutedEventArgs e)
            => App.Device_Monitor.RemoveTracing (this);
        private void MenuIncreaseAmplitude_Click (object sender, RoutedEventArgs e)
            => Amplitude = Utility.Clamp(Amplitude + 0.2, 0.2, 2.0);
        private void MenuDecreaseAmplitude_Click(object sender, RoutedEventArgs e)
            => Amplitude = Utility.Clamp(Amplitude - 0.2, 0.2, 2.0);

        private void MenuSelectInputSource (object sender, RoutedEventArgs e) {
            Leads.Values selectedValue;
            if (!Enum.TryParse<Leads.Values> (((MenuItem)sender).Name, out selectedValue))
                return;

            wfStrip.Lead.Value = selectedValue;
            wfStrip.Reset ();
            Add_Beat__Cardiac_Baseline (App.Patient);
            Add_Beat__Respiratory_Baseline (App.Patient);

            UpdateInterface (null, null);
        }
    }
}
