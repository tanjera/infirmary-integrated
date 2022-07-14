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

    public partial class EFMTracing : DeviceTracing {

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

                Label lblLead = this.FindControl<Label> ("lblLead");

                lblLead.Foreground = TracingBrush;
                lblLead.Content = Instance?.Language.Localize (Lead.LookupString (Lead?.Value ?? Lead.Values.ECG_I, true));

                CalculateOffsets ();
            });
        }
    }
}