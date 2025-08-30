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
    /// Interaction logic for DefibTracing.xaml
    /// </summary>
    public partial class DefibTracing : UserControl {
        public App? Instance { get; set; }

        public Strip? Strip;
        public Lead? Lead { get { return Strip?.Lead; } }

        /* Drawing variables, offsets and multipliers */
        public Color.Schemes? ColorScheme;
        public Brush? TracingBrush = Brushes.Black;

        public PointD? DrawOffset = new (0, 0);
        public PointD? DrawMultiplier = new (1, 1);

        /* State machines, flags, properties, and utilities */

        private MenuItem? uiMenuZeroTransducer;
        private MenuItem? uiMenuToggleAutoScale;

        public DefibTracing () {
            InitializeComponent ();
        }

        public DefibTracing (App? app, Strip? strip, Color.Schemes? cs) {
            InitializeComponent ();
            DataContext = this;

            LayoutUpdated += UpdateInterface;

            Instance = app;
            Strip = strip;
            ColorScheme = cs;

            InitInterface ();
            UpdateInterface ();
        }

        ~DefibTracing () {
            Strip?.Points?.Clear ();
        }

        public void InitInterface () {
            // Context Menu (right-click menu!)
            ContextMenu menuContext = new ();

            // Note: children inherit the context menu (e.g. TextBlocks on the Grid)
            this.ContextMenu = menuContext;

            // Note: Background must be set to receive right-clicks for triggering ContextMenu
            this.Background = Brushes.Transparent;

            uiMenuZeroTransducer = new MenuItem ();
            uiMenuZeroTransducer.Header = Instance?.Language.Localize ("MENU:MenuZeroTransducer");
            uiMenuZeroTransducer.Click += MenuZeroTransducer_Click;
            menuContext.Items.Add (uiMenuZeroTransducer);

            menuContext.Items.Add (new Separator ());

            MenuItem menuAddTracing = new ();
            menuAddTracing.Header = Instance?.Language.Localize ("MENU:MenuAddTracing");
            menuAddTracing.Click += MenuAddTracing_Click;
            menuContext.Items.Add (menuAddTracing);

            MenuItem menuRemoveTracing = new ();
            menuRemoveTracing.Header = Instance?.Language.Localize ("MENU:MenuRemoveTracing");
            menuRemoveTracing.Click += MenuRemoveTracing_Click;
            menuContext.Items.Add (menuRemoveTracing);

            menuContext.Items.Add (new Separator ());

            MenuItem menuIncreaseAmplitude = new ();
            menuIncreaseAmplitude.Header = Instance?.Language.Localize ("MENU:IncreaseAmplitude");
            menuIncreaseAmplitude.Click += MenuIncreaseAmplitude_Click;
            menuContext.Items.Add (menuIncreaseAmplitude);

            MenuItem menuDecreaseAmplitude = new ();
            menuDecreaseAmplitude.Header = Instance?.Language.Localize ("MENU:DecreaseAmplitude");
            menuDecreaseAmplitude.Click += MenuDecreaseAmplitude_Click;
            menuContext.Items.Add (menuDecreaseAmplitude);

            menuContext.Items.Add (new Separator ());

            uiMenuToggleAutoScale = new MenuItem ();
            uiMenuToggleAutoScale.Header = Instance?.Language.Localize ("MENU:ToggleAutoScaling");
            uiMenuToggleAutoScale.Click += MenuToggleAutoScale_Click;
            menuContext.Items.Add (uiMenuToggleAutoScale);

            menuContext.Items.Add (new Separator ());

            MenuItem menuSelectInput = new (),
                     menuECGLeads = new ();

            menuSelectInput.Header = Instance?.Language.Localize ("MENU:MenuSelectInputSource");
            menuECGLeads.Header = Instance?.Language.Localize ("TRACING:ECG");

            menuSelectInput.Items.Add (menuECGLeads);

            foreach (Lead.Values v in Enum.GetValues (typeof (Lead.Values))) {
                // Only include certain leads- e.g. bedside monitors don't interface with IABP or EFM
                string el = v.ToString ();
                if (!el.StartsWith ("ECG") && el != "SPO2" && el != "CVP" && el != "ABP"
                    && el != "PA" && el != "RR" && el != "ETCO2")
                    continue;

                MenuItem mi = new ();
                mi.Header = Instance?.Language.Localize (Lead.LookupString (v));
                mi.Name = v.ToString ();
                mi.Click += MenuSelectInputSource;
                if (mi.Name.StartsWith ("ECG"))
                    menuECGLeads.Items.Add (mi);
                else
                    menuSelectInput.Items.Add (mi);
            }

            menuContext.Items.Add (menuSelectInput);
        }

        private void UpdateInterface ()
            => UpdateInterface (this, new EventArgs ());

        private void UpdateInterface (object? sender, EventArgs e) {
            App.Current.Dispatcher.InvokeAsync (() => {
                TracingBrush = Color.GetLead (Lead?.Value ?? Lead.Values.ECG_I, ColorScheme ?? Color.Schemes.Light);

                brdTracing.BorderBrush = TracingBrush;

                lblLead.Foreground = TracingBrush;
                lblLead.Content = Instance?.Language.Localize (Lead.LookupString (Lead.Value));

                if (uiMenuZeroTransducer is not null)
                    uiMenuZeroTransducer.IsEnabled = Strip?.Lead?.IsTransduced () ?? false;
                if (uiMenuToggleAutoScale is not null)
                    uiMenuToggleAutoScale.IsEnabled = Strip?.CanScale ?? false;

                lblScaleAuto.Visibility = Strip?.CanScale ?? false ? Visibility.Visible : Visibility.Hidden;
                lblScaleMin.Visibility = Strip?.CanScale ?? false ? Visibility.Visible : Visibility.Hidden;
                lblScaleMax.Visibility = Strip?.CanScale ?? false ? Visibility.Visible : Visibility.Hidden;

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

        public void UpdateScale () {
            if (Strip?.CanScale ?? false) {
                lblScaleMin.Foreground = TracingBrush;
                lblScaleMax.Foreground = TracingBrush;

                lblScaleMin.Content = Strip.ScaleMin.ToString ();
                lblScaleMax.Content = Strip.ScaleMax.ToString ();
            }
        }

        public void SetColorScheme (Color.Schemes scheme) {
            ColorScheme = scheme;
            UpdateInterface ();
        }

        public void CalculateOffsets () {
            if (Strip is null)
                return;

            DrawOffset ??= new PointD (0, 0);
            DrawMultiplier ??= new PointD (1, 1);

            DrawOffset.X = 0;
            DrawMultiplier.X = (float)(cnvTracing.ActualWidth / Strip.DisplayLength);

            switch (Strip.Offset) {
                case Strip.Offsets.Center:
                    DrawOffset.Y = (int)(cnvTracing.ActualHeight / 2f);
                    DrawMultiplier.Y = (float)((-cnvTracing.ActualHeight / 2f) * Strip.Amplitude);
                    break;

                case Strip.Offsets.Stretch:
                    DrawOffset.Y = (int)(cnvTracing.ActualHeight * (1 - (Strip.ScaleMargin / 2)));
                    DrawMultiplier.Y = (float)(-cnvTracing.ActualHeight * (1 - Strip.ScaleMargin) * Strip.Amplitude);
                    break;

                case Strip.Offsets.Scaled:
                    DrawOffset.Y = (int)(cnvTracing.ActualHeight * (1 - Strip.ScaleMargin));
                    DrawOffset.Y = -(int)cnvTracing.ActualHeight;
                    break;
            }
        }

        public void DrawTracing () {
            plTracing.Points.Clear ();
            plTracing.Stroke = TracingBrush;
            plTracing.StrokeThickness = 1d;

            if (Strip is not null && Strip.Points is not null && Strip.Points.Count > 1) {
                lock (Strip.lockPoints) {
                    /* clipX: Off-screen multiplier to clip for start- and end-points
                     * Generally works well at 1.25 with minimal functional artifact; performance gains at 2.0 are still
                     * incredibly valuable; will keep at a decent middle ground... 1.5?
                     */
                    double clipX = 1.5;

                    double x, y;
                    double maxX = cnvTracing.ActualWidth;

                    foreach (var p in Strip.Points) {
                        x = (p.X * DrawMultiplier?.X ?? 1) + DrawOffset?.X ?? 0;
                        y = (p.Y * DrawMultiplier?.Y ?? 1) + DrawOffset?.Y ?? 0;

                        /* Only add the Strip.Point[] to the PolyLine's Point stack if it is within visible Canvas
                         * bounds, for performance related to instantiating a System.Windows.Point() and popping it
                         * to the stack even if it is an irrelevant point (e.g. has scrolled off the screen!), even though
                         * cnvTracing.ClipToBounds functions well, it gives a massive performance boost.
                         * ... but some clipped points do need to be added to the stack to define start- and end-points for
                         * some line segments that hit the edge of the visible box!!
                         * ...
                         * Note: 12L ECG holds more tracings in memory than any other device, so keep this tuned for performance
                         * whenever possible specifically for 12 L ECG ... may be able to add to a user option variable??
                         */

                        if (x >= 0 - (maxX * clipX) && x <= (maxX * clipX))
                            plTracing.Points.Add (new System.Windows.Point (x, y));
                    }
                }
            }
        }

        private void MenuZeroTransducer_Click (object? sender, RoutedEventArgs e) {
            if (Instance is null || Instance.Physiology is null)
                return;

            switch (Lead?.Value) {
                case Lead.Values.ABP: Instance.Physiology.TransducerZeroed_ABP = true; return;
                case Lead.Values.CVP: Instance.Physiology.TransducerZeroed_CVP = true; return;
                case Lead.Values.PA: Instance.Physiology.TransducerZeroed_PA = true; return;
                case Lead.Values.ICP: Instance.Physiology.TransducerZeroed_ICP = true; return;
                case Lead.Values.IAP: Instance.Physiology.TransducerZeroed_IAP = true; return;
            }
        }

        private void MenuAddTracing_Click (object? sender, RoutedEventArgs e)
            => Instance?.Device_Defib?.AddTracing ();

        private void MenuRemoveTracing_Click (object? sender, RoutedEventArgs e)
            => Instance?.Device_Defib?.RemoveTracing (this);

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
            if (sender is null || !Enum.TryParse<Lead.Values> (((MenuItem)sender).Name, out Lead.Values selectedValue))
                return;

            Strip?.SetLead (selectedValue);
            Strip?.Reset ();

            CalculateOffsets ();
            UpdateInterface ();
        }
    }
}