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

namespace IISIM.Controls {

    public partial class IABPTracing : DeviceTracing {

        public IABPTracing () {
            InitializeComponent ();
        }

        public IABPTracing (App? app, Strip? strip, Color.Schemes? cs) {
            InitializeComponent ();
            DataContext = this;

            Instance = app;
            Strip = strip;
            ColorScheme = cs;

            UpdateInterface ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public void SetColorScheme (Color.Schemes scheme) {
            ColorScheme = scheme;
            UpdateInterface ();
        }

        private void UpdateInterface ()
            => UpdateInterface (this, new EventArgs ());

        private void UpdateInterface (object sender, EventArgs e) {
            Dispatcher.UIThread.InvokeAsync (() => {
                TracingBrush = Color.GetLead (Lead?.Value ?? Lead.Values.ECG_I, ColorScheme ?? Color.Schemes.Light);

                Border borderTracing = this.FindControl<Border> ("borderTracing");
                Label lblLead = this.FindControl<Label> ("lblLead");
                Label lblScaleAuto = this.FindControl<Label> ("lblScaleAuto");
                Label lblScaleMin = this.FindControl<Label> ("lblScaleMin");
                Label lblScaleMax = this.FindControl<Label> ("lblScaleMax");

                borderTracing.BorderBrush = TracingBrush;

                lblLead.Foreground = TracingBrush;
                lblLead.Content = Instance?.Language.Localize (Lead.LookupString (Lead.Value));

                if (Strip?.CanScale ?? false) {
                    lblScaleAuto.Foreground = TracingBrush;
                    lblScaleMin.Foreground = TracingBrush;
                    lblScaleMax.Foreground = TracingBrush;

                    lblScaleAuto.Content = Strip.ScaleAuto
                        ? Instance?.Language.Localize ("TRACING:Auto")
                        : Instance?.Language.Localize ("TRACING:Fixed");
                    lblScaleMin.Content = Strip.ScaleMin.ToString ();
                    lblScaleMax.Content = Strip.ScaleMax.ToString ();
                }

                CalculateOffsets ();
            });
        }
    }
}