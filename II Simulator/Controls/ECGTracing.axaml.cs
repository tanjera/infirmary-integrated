using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;

using II;
using II.Drawing;
using II.Localization;
using II.Rhythm;

namespace II_Simulator.Controls {

    public partial class ECGTracing : UserControl {
        public Strip Strip;
        public Lead Lead { get { return Strip.Lead; } }
        public RenderTargetBitmap Tracing;

        /* Drawing variables, offsets and multipliers */
        public Color.Schemes colorScheme;
        private Pen tracingPen = new Pen ();
        private IBrush tracingBrush = Brushes.Green;

        private PointD drawOffset;
        private PointD drawMultiplier;

        public ECGTracing () {
            InitializeComponent ();
        }

        public ECGTracing (Strip strip, Color.Schemes cs) {
            InitializeComponent ();
            DataContext = this;

            Strip = strip;
            colorScheme = cs;

            UpdateInterface ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public void SetColorScheme (Color.Schemes scheme) {
            colorScheme = scheme;
            UpdateInterface ();
        }

        private void UpdateInterface ()
            => UpdateInterface (this, new EventArgs ());

        private void UpdateInterface (object sender, EventArgs e) {
            Dispatcher.UIThread.InvokeAsync (() => {
                tracingBrush = Color.GetLead (Lead.Value, colorScheme);

                Label lblLead = this.FindControl<Label> ("lblLead");

                lblLead.Foreground = tracingBrush;
                lblLead.Content = App.Language.Localize (Lead.LookupString (Lead.Value, true));

                CalculateOffsets ();
            });
        }

        public void CalculateOffsets () {
            Image imgTracing = this.FindControl<Image> ("imgTracing");

            II.Rhythm.Tracing.CalculateOffsets (Strip,
               imgTracing.Bounds.Width, imgTracing.Bounds.Height,
               ref drawOffset, ref drawMultiplier);
        }

        public async Task DrawTracing ()
            => await Draw (Strip, tracingBrush, 1);

        public Task Draw (Strip _Strip, IBrush _Brush, double _Thickness) {
            Image imgTracing = this.FindControl<Image> ("imgTracing");

            PixelSize size = new PixelSize (    // Must use a size > 0
                imgTracing.Bounds.Width > 0 ? (int)imgTracing.Bounds.Width : 100,
                imgTracing.Bounds.Height > 0 ? (int)imgTracing.Bounds.Height : 100);

            Tracing = new RenderTargetBitmap (size);

            tracingPen.Brush = _Brush;
            tracingPen.Thickness = _Thickness;

            Trace.DrawPath (_Strip, Tracing, tracingPen, drawOffset, drawMultiplier);

            imgTracing.Source = Tracing;

            return Task.CompletedTask;
        }
    }
}