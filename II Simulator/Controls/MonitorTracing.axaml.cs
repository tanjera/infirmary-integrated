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
        
        private List<II.Settings.Device.Numeric>? Transducers_Zeroed {
            set { 
                if (Instance?.Scenario?.DeviceMonitor is not null)
                    Instance.Scenario.DeviceMonitor.Transducers_Zeroed = value ?? new List<II.Settings.Device.Numeric> (); 
            }
            get { return Instance?.Scenario?.DeviceMonitor.Transducers_Zeroed; }
        }
        
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
            this.GetControl<Image> ("imgTracing").ContextMenu = menuContext;
            this.GetControl<Label> ("lblLead").ContextMenu = menuContext;

            uiMenuZeroTransducer = new MenuItem ();
            uiMenuZeroTransducer.Header = Instance?.Language.Localize ("MENU:MenuZeroTransducer");
            uiMenuZeroTransducer.Classes.Add ("item");
            uiMenuZeroTransducer.Click += MenuZeroTransducer_Click;
            menuContext.Items.Add (uiMenuZeroTransducer);
            menuContext.Items.Add (new Separator ());

            MenuItem menuAddTracing = new ();
            menuAddTracing.Header = Instance?.Language.Localize ("MENU:MenuAddTracing");
            menuAddTracing.Classes.Add ("item");
            menuAddTracing.Click += MenuAddTracing_Click;
            menuContext.Items.Add (menuAddTracing);

            MenuItem menuRemoveTracing = new ();
            menuRemoveTracing.Header = Instance?.Language.Localize ("MENU:MenuRemoveTracing");
            menuRemoveTracing.Classes.Add ("item");
            menuRemoveTracing.Click += MenuRemoveTracing_Click;
            menuContext.Items.Add (menuRemoveTracing);
            menuContext.Items.Add (new Separator ());

            MenuItem menuMoveUp = new ();
            menuMoveUp.Header = Instance?.Language.Localize ("MENU:MoveUp");
            menuMoveUp.Click += MenuMoveUp_Click;
            menuContext.Items.Add (menuMoveUp);

            MenuItem menuMoveDown = new ();
            menuMoveDown.Header = Instance?.Language.Localize ("MENU:MoveDown");
            menuMoveDown.Click += MenuMoveDown_Click;
            menuContext.Items.Add (menuMoveDown);
            menuContext.Items.Add (new Separator ());
            
            MenuItem menuIncreaseAmplitude = new ();
            menuIncreaseAmplitude.Header = Instance?.Language.Localize ("MENU:IncreaseAmplitude");
            menuIncreaseAmplitude.Classes.Add ("item");
            menuIncreaseAmplitude.Click += MenuIncreaseAmplitude_Click;
            menuContext.Items.Add (menuIncreaseAmplitude);

            MenuItem menuDecreaseAmplitude = new ();
            menuDecreaseAmplitude.Header = Instance?.Language.Localize ("MENU:DecreaseAmplitude");
            menuDecreaseAmplitude.Classes.Add ("item");
            menuDecreaseAmplitude.Click += MenuDecreaseAmplitude_Click;
            menuContext.Items.Add (menuDecreaseAmplitude);
            menuContext.Items.Add (new Separator ());

            uiMenuToggleAutoScale = new MenuItem ();
            uiMenuToggleAutoScale.Header = Instance?.Language.Localize ("MENU:ToggleAutoScaling");
            uiMenuToggleAutoScale.Classes.Add ("item");
            uiMenuToggleAutoScale.Click += MenuToggleAutoScale_Click;
            menuContext.Items.Add (uiMenuToggleAutoScale);
            menuContext.Items.Add (new Separator ());

            MenuItem menuSelectInput = new (),
                     menuECGLeads = new ();

            menuSelectInput.Header = Instance?.Language.Localize ("MENU:MenuSelectInputSource");
            menuSelectInput.Classes.Add ("item");

            menuECGLeads.Header = Instance?.Language.Localize ("TRACING:ECG");
            menuECGLeads.Classes.Add ("item");

            menuSelectInput.Items.Add (menuECGLeads);

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
                    menuECGLeads.Items.Add (mi);
                else
                    menuSelectInput.Items.Add (mi);
            }

            menuContext.Items.Add (menuSelectInput);
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

                Border borderTracing = this.GetControl<Border> ("borderTracing");
                Label lblLead = this.GetControl<Label> ("lblLead");
                Label lblScaleAuto = this.GetControl<Label> ("lblScaleAuto");
                Label lblScaleMin = this.GetControl<Label> ("lblScaleMin");
                Label lblScaleMax = this.GetControl<Label> ("lblScaleMax");

                borderTracing.BorderBrush = TracingBrush;

                lblLead.Foreground = TracingBrush;
                lblLead.Content = Instance?.Language.Localize (Lead.LookupString (Lead.Value));

                if (uiMenuZeroTransducer is not null)
                    uiMenuZeroTransducer.IsEnabled = Strip?.Lead?.IsTransduced () ?? false;

                if (uiMenuToggleAutoScale is not null)
                    uiMenuToggleAutoScale.IsEnabled = Strip?.CanScale ?? false;

                lblScaleAuto.IsVisible = Strip?.CanScale ?? false;
                lblScaleMin.IsVisible = Strip?.CanScale ?? false;
                lblScaleMax.IsVisible = Strip?.CanScale ?? false;

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
            switch (Lead?.Value) {
                case Lead.Values.ABP: 
                    if (!(Transducers_Zeroed?.Contains(II.Settings.Device.Numeric.ABP) ?? true))
                        Transducers_Zeroed.Add (II.Settings.Device.Numeric.ABP);
                    return;
                
                case Lead.Values.CVP: 
                    if (!(Transducers_Zeroed?.Contains(II.Settings.Device.Numeric.CVP) ?? true))
                        Transducers_Zeroed.Add (II.Settings.Device.Numeric.CVP);
                    return;

                case Lead.Values.PA: 
                    if (!(Transducers_Zeroed?.Contains(II.Settings.Device.Numeric.PA) ?? true))
                        Transducers_Zeroed.Add (II.Settings.Device.Numeric.PA);
                    return;

                case Lead.Values.ICP: 
                    if (!(Transducers_Zeroed?.Contains(II.Settings.Device.Numeric.ICP) ?? true))
                        Transducers_Zeroed.Add (II.Settings.Device.Numeric.ICP);
                    return;

                case Lead.Values.IAP: 
                    if (!(Transducers_Zeroed?.Contains(II.Settings.Device.Numeric.IAP) ?? true))
                        Transducers_Zeroed.Add (II.Settings.Device.Numeric.IAP);
                    return;
            }
        }

        private void MenuAddTracing_Click (object? sender, RoutedEventArgs e)
            => Instance?.Device_Monitor?.AddTracing ();

        private void MenuRemoveTracing_Click (object? sender, RoutedEventArgs e)
            => Instance?.Device_Monitor?.RemoveTracing (this);

        private void MenuMoveUp_Click (object? sender, RoutedEventArgs e)
            => Instance?.Device_Monitor?.MoveTracing_Up (this);
        
        private void MenuMoveDown_Click (object? sender, RoutedEventArgs e) 
            => Instance?.Device_Monitor?.MoveTracing_Down (this);
        
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
            Strip?.Add_Beat__Cardiac_Baseline (Instance?.Physiology);
            Strip?.Add_Breath__Respiratory_Baseline (Instance?.Physiology);

            CalculateOffsets ();
            UpdateInterface ();
        }
    }
}