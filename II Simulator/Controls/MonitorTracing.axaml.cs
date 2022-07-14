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

    public partial class MonitorTracing : DeviceTracing {
        private MenuItem? uiMenuZeroTransducer;
        private MenuItem? uiMenuToggleAutoScale;

        public MonitorTracing () {
            InitializeComponent ();
        }

        public MonitorTracing (App? app, Strip? strip, Color.Schemes? cs) {
            InitializeComponent ();
            DataContext = this;

            Instance = app;
            Strip = strip;
            ColorScheme = cs;

            InitInterface ();
            UpdateInterface ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private void InitInterface () {
            // Context Menu (right-click menu!)
            ContextMenu menuContext = new ();
            List<object> menuitemsContext = new ();
            this.FindControl<Image> ("imgTracing").ContextMenu = menuContext;
            this.FindControl<Label> ("lblLead").ContextMenu = menuContext;

            uiMenuZeroTransducer = new MenuItem ();
            uiMenuZeroTransducer.Header = Instance?.Language.Localize ("MENU:MenuZeroTransducer");
            uiMenuZeroTransducer.Classes.Add ("item");
            uiMenuZeroTransducer.Click += MenuZeroTransducer_Click;
            menuitemsContext.Add (uiMenuZeroTransducer);

            menuitemsContext.Add (new Separator ());

            MenuItem menuAddTracing = new ();
            menuAddTracing.Header = Instance?.Language.Localize ("MENU:MenuAddTracing");
            menuAddTracing.Classes.Add ("item");
            menuAddTracing.Click += MenuAddTracing_Click;
            menuitemsContext.Add (menuAddTracing);

            MenuItem menuRemoveTracing = new ();
            menuRemoveTracing.Header = Instance?.Language.Localize ("MENU:MenuRemoveTracing");
            menuRemoveTracing.Classes.Add ("item");
            menuRemoveTracing.Click += MenuRemoveTracing_Click;
            menuitemsContext.Add (menuRemoveTracing);

            menuitemsContext.Add (new Separator ());

            MenuItem menuIncreaseAmplitude = new ();
            menuIncreaseAmplitude.Header = Instance?.Language.Localize ("MENU:IncreaseAmplitude");
            menuIncreaseAmplitude.Classes.Add ("item");
            menuIncreaseAmplitude.Click += MenuIncreaseAmplitude_Click;
            menuitemsContext.Add (menuIncreaseAmplitude);

            MenuItem menuDecreaseAmplitude = new ();
            menuDecreaseAmplitude.Header = Instance?.Language.Localize ("MENU:DecreaseAmplitude");
            menuDecreaseAmplitude.Classes.Add ("item");
            menuDecreaseAmplitude.Click += MenuDecreaseAmplitude_Click;
            menuitemsContext.Add (menuDecreaseAmplitude);

            menuitemsContext.Add (new Separator ());

            uiMenuToggleAutoScale = new MenuItem ();
            uiMenuToggleAutoScale.Header = Instance?.Language.Localize ("MENU:ToggleAutoScaling");
            uiMenuToggleAutoScale.Classes.Add ("item");
            uiMenuToggleAutoScale.Click += MenuToggleAutoScale_Click;
            menuitemsContext.Add (uiMenuToggleAutoScale);

            menuitemsContext.Add (new Separator ());

            MenuItem menuSelectInput = new (),
                     menuECGLeads = new ();
            List<object> menuitemsSelectInput = new (),
                menuitemsECGLeads = new ();

            menuSelectInput.Header = Instance?.Language.Localize ("MENU:MenuSelectInputSource");
            menuSelectInput.Classes.Add ("item");

            menuECGLeads.Header = Instance?.Language.Localize ("TRACING:ECG");
            menuECGLeads.Classes.Add ("item");

            menuitemsSelectInput.Add (menuECGLeads);

            foreach (Lead.Values v in Enum.GetValues (typeof (Lead.Values))) {
                // Only include certain leads- e.g. bedside monitors don't interface with IABP or EFM
                string el = v.ToString ();
                if (!el.StartsWith ("ECG") && el != "SPO2" && el != "RR" && el != "ETCO2"
                    && el != "CVP" && el != "ABP" && el != "PA"
                    && el != "ICP" && el != "IAP")
                    continue;

                MenuItem mi = new ();
                mi.Header = Instance?.Language.Localize (Lead.LookupString (v));
                mi.Classes.Add ("item");
                mi.Name = v.ToString ();
                mi.Click += MenuSelectInputSource;
                if (mi.Name.StartsWith ("ECG"))
                    menuitemsECGLeads.Add (mi);
                else
                    menuitemsSelectInput.Add (mi);
            }

            menuitemsContext.Add (menuSelectInput);

            menuSelectInput.Items = menuitemsSelectInput;
            menuECGLeads.Items = menuitemsECGLeads;
            menuContext.Items = menuitemsContext;
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

                if (uiMenuZeroTransducer is not null)
                    uiMenuZeroTransducer.IsEnabled = Strip?.Lead?.IsTransduced () ?? false;

                if (uiMenuToggleAutoScale is not null)
                    uiMenuToggleAutoScale.IsEnabled = Strip?.CanScale ?? false;

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

        private void MenuZeroTransducer_Click (object? sender, RoutedEventArgs e) {
            if (Instance?.Patient == null)
                return;

            switch (Lead?.Value) {
                case Lead.Values.ABP: Instance.Patient.TransducerZeroed_ABP = true; return;
                case Lead.Values.CVP: Instance.Patient.TransducerZeroed_CVP = true; return;
                case Lead.Values.PA: Instance.Patient.TransducerZeroed_PA = true; return;
                case Lead.Values.ICP: Instance.Patient.TransducerZeroed_ICP = true; return;
                case Lead.Values.IAP: Instance.Patient.TransducerZeroed_IAP = true; return;
            }
        }

        private void MenuAddTracing_Click (object? sender, RoutedEventArgs e)
            => Instance?.Device_Monitor?.AddTracing ();

        private void MenuRemoveTracing_Click (object? sender, RoutedEventArgs e)
            => Instance?.Device_Monitor?.RemoveTracing (this);

        private void MenuIncreaseAmplitude_Click (object? sender, RoutedEventArgs e) {
            Strip?.IncreaseAmplitude ();
            CalculateOffsets ();
        }

        private void MenuDecreaseAmplitude_Click (object? sender, RoutedEventArgs e) {
            Strip?.DecreaseAmplitude ();
            CalculateOffsets ();
        }

        private void MenuToggleAutoScale_Click (object? sender, RoutedEventArgs e) {
            if (Strip is not null)
                Strip.ScaleAuto = !Strip.ScaleAuto;

            UpdateInterface ();
        }

        private void MenuSelectInputSource (object? sender, RoutedEventArgs e) {
            if (sender == null || sender is not MenuItem || !Enum.TryParse<Lead.Values> (((MenuItem)sender).Name, out Lead.Values selectedValue))
                return;

            Strip?.SetLead (selectedValue);
            Strip?.Reset ();
            Strip?.Add_Beat__Cardiac_Baseline (Instance?.Patient);
            Strip?.Add_Breath__Respiratory_Baseline (Instance?.Patient);

            CalculateOffsets ();
            UpdateInterface ();
        }
    }
}