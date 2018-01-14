﻿using System;
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

            InitInterface ();

            wfStrip = strip;
            UpdateInterface (null, null);
        }

        private void InitInterface () {
            // Populate UI strings per language selection
            Languages.Values l = App.Language.Value;

            // Context Menu (right-click menu!)
            ContextMenu contextMenu = new ContextMenu ();
            canvasTracing.ContextMenu = contextMenu;
            lblLead.ContextMenu = contextMenu;

            MenuItem menuAddTracing = new MenuItem ();
            menuAddTracing.Header = Strings.Lookup (l, "CM:MenuAddTracing");
            menuAddTracing.Click += MenuAddTracing_Click;
            contextMenu.Items.Add (menuAddTracing);

            MenuItem menuRemoveTracing = new MenuItem ();
            menuRemoveTracing.Header = Strings.Lookup (l, "CM:MenuRemoveTracing");
            menuRemoveTracing.Click += MenuRemoveTracing_Click;
            contextMenu.Items.Add (menuRemoveTracing);

            contextMenu.Items.Add (new Separator());

            MenuItem menuSelectInput = new MenuItem (),
                     menuECGLeads = new MenuItem();
            menuSelectInput.Header = Strings.Lookup (l, "CM:MenuSelectInputSource");
            menuECGLeads.Header = Strings.Lookup (l, "TRACING:ECG");
            menuSelectInput.Items.Add (menuECGLeads);

            foreach (Leads.Values v in Enum.GetValues(typeof(Leads.Values))) {
                MenuItem mi = new MenuItem ();
                mi.Header = Strings.Lookup (l, Leads.LookupString (v));
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
            switch (wfStrip.Lead.Value) {
                default: drawBrush = Brushes.Green; break;
                case Leads.Values.ABP: drawBrush = Brushes.Red; break;
                case Leads.Values.CVP: drawBrush = Brushes.Blue; break;
                case Leads.Values.PA: drawBrush = Brushes.Yellow; break;
                case Leads.Values.RR: drawBrush = Brushes.Salmon; break;
                case Leads.Values.ETCO2: drawBrush = Brushes.Aqua; break;
                case Leads.Values.SPO2: drawBrush = Brushes.Orange; break;
            }

            borderTracing.BorderBrush = drawBrush;

            lblLead.Foreground = drawBrush;
            lblLead.Content = Strings.Lookup (App.Language.Value, Leads.LookupString (wfStrip.Lead.Value));
        }

        public void Draw () {
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

        private void MenuAddTracing_Click (object sender, RoutedEventArgs e)
            => App.Device_Monitor.AddTracing ();
        private void MenuRemoveTracing_Click (object sender, RoutedEventArgs e)
            => App.Device_Monitor.RemoveTracing (this);

        private void MenuSelectInputSource (object sender, RoutedEventArgs e) {
            Leads.Values selectedValue;
            if (!Enum.TryParse<Leads.Values> (((MenuItem)sender).Name, out selectedValue))
                return;

            wfStrip.Lead.Value = selectedValue;
            wfStrip.Reset ();
            wfStrip.Add_Beat__Cardiac_Baseline (App.Patient);
            wfStrip.Add_Beat__Respiratory_Baseline (App.Patient);

            UpdateInterface (null, null);
        }
    }
}
