using II;
using II.Drawing;
using II.Localization;
using II.Rhythm;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IISIM.Controls {

    /// <summary>
    /// Interaction logic for EFMTracing.xaml
    /// </summary>
    public partial class EFMTracing : UserControl {
        public App? Instance { get; set; }

        public Strip? Strip;
        public Lead? Lead { get { return Strip?.Lead; } }

        /* Drawing variables, offsets and multipliers */
        public Color.Schemes? ColorScheme;
        public Brush? TracingBrush = Brushes.Black;

        public PointD? DrawOffset = new (0, 0);
        public PointD? DrawMultiplier = new (1, 1);

        public EFMTracing () {
            InitializeComponent ();
        }

        public EFMTracing (App? app, Strip? strip, Color.Schemes? cs) {
            InitializeComponent ();
            DataContext = this;

            Instance = app;
            Strip = strip;
            ColorScheme = cs;
            UpdateInterface ();
        }

        ~EFMTracing () {
            Strip?.Points?.Clear ();
        }

        public void SetColorScheme (Color.Schemes scheme) {
            ColorScheme = scheme;
            UpdateInterface ();
        }

        private void UpdateInterface ()
            => UpdateInterface (this, new EventArgs ());

        private void UpdateInterface (object sender, EventArgs e) {
            App.Current.Dispatcher.InvokeAsync (() => {
                TracingBrush = Color.GetLead (Lead?.Value ?? Lead.Values.ECG_I, ColorScheme ?? Color.Schemes.Light);

                lblLead.Foreground = TracingBrush;
                lblLead.Content = Instance?.Language.Localize (Lead.LookupString (Lead?.Value ?? Lead.Values.ECG_I, true));

                CalculateOffsets ();
            });
        }

        public void CalculateOffsets () {
            if (Strip is null)
                return;

            DrawOffset ??= new PointD (0, 0);
            DrawMultiplier ??= new PointD (1, 1);

            DrawOffset.X = 0;
            DrawMultiplier.X = (float)(cnvTracing.ActualWidth / Strip.DisplayLength);

            /* EFM Tracing is a Strip type Stretch */
            DrawOffset.Y = (int)(cnvTracing.ActualHeight * (1 - (Strip.ScaleMargin / 2)));
            DrawMultiplier.Y = (float)(-cnvTracing.ActualHeight * (1 - Strip.ScaleMargin) * Strip.Amplitude);
        }

        public void DrawTracing () {
            plTracing.Points.Clear ();
            plTracing.Stroke = TracingBrush;
            plTracing.StrokeThickness = 1d;

            if (Strip is not null && Strip.Points is not null && Strip.Points.Count > 1) {
                lock (Strip.lockPoints) {
                    double x, y;

                    foreach (var p in Strip.Points) {
                        x = (p.X * DrawMultiplier?.X ?? 1) + DrawOffset?.X ?? 0;
                        y = (p.Y * DrawMultiplier?.Y ?? 1) + DrawOffset?.Y ?? 0;

                        plTracing.Points.Add (new System.Windows.Point (x, y));
                    }
                }
            }
        }
    }
}