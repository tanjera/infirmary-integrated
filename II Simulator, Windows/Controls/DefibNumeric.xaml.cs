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

using II;
using II.Drawing;
using II.Localization;
using II.Rhythm;

namespace IISIM.Controls {

    /// <summary>
    /// Interaction logic for DefibNumeric.xaml
    /// </summary>
    public partial class DefibNumeric : UserControl {
        public App? Instance { get; set; }
        private Windows.DeviceDefib? Device;

        /* Drawing variables, offsets and multipliers */
        public Color.Schemes? ColorScheme;
        public Brush? TracingBrush = Brushes.Black;

        /* Variables controlling for visual alarms */
        public II.Timer? AlarmTimer = new ();
        public bool? AlarmIterator = false;

        public bool? AlarmLine1;
        public bool? AlarmLine2;
        public bool? AlarmLine3;

        /* State machines, flags, properties, and utilities */

        public ControlTypes? ControlType;

        private MenuItem? uiMenuZeroTransducer;

        public class ControlTypes {
            public Values Value;

            public ControlTypes (Values v) {
                Value = v;
            }

            public enum Values {
                ECG, T, RR, ETCO2,
                SPO2, NIBP, ABP, CVP,
                PA, DEFIB
            }

            public Color.Leads GetLead_Color {
                get { return SwitchLead_Color (this.Value); }
            }

            private static Color.Leads SwitchLead_Color (Values value) => value switch {
                ControlTypes.Values.ECG => Color.Leads.ECG,
                ControlTypes.Values.T => Color.Leads.T,
                ControlTypes.Values.RR => Color.Leads.RR,
                ControlTypes.Values.ETCO2 => Color.Leads.ETCO2,
                ControlTypes.Values.SPO2 => Color.Leads.SPO2,
                ControlTypes.Values.NIBP => Color.Leads.NIBP,
                ControlTypes.Values.ABP => Color.Leads.ABP,
                ControlTypes.Values.CVP => Color.Leads.CVP,
                ControlTypes.Values.PA => Color.Leads.PA,
                ControlTypes.Values.DEFIB => Color.Leads.DEFIB,
                _ => Color.Leads.ECG
            };

            public static string LookupString (Values value) {
                return String.Format ("NUMERIC:{0}", Enum.GetValues (typeof (Values)).GetValue ((int)value)?.ToString ());
            }

            public static List<string> MenuItem_Formats {
                get {
                    List<string> o = new ();
                    foreach (Values v in Enum.GetValues (typeof (Values)))
                        o.Add (String.Format ("{0}: {1}", v.ToString (), LookupString (v)));
                    return o;
                }
            }
        }

        public DefibNumeric () {
            InitializeComponent ();
        }

        public DefibNumeric (App? app, Windows.DeviceDefib device, ControlTypes.Values v, Color.Schemes? cs) {
            InitializeComponent ();
            DataContext = this;

            Device = device;
            Instance = app;

            LayoutUpdated += this.UpdateInterface;

            InitTimers ();
            InitAlarm ();

            ControlType = new ControlTypes (v);
            ColorScheme = cs;

            InitInterface ();
            UpdateInterface ();
        }

        ~DefibNumeric () {
            AlarmTimer?.Dispose ();
        }

        public virtual void InitTimers () {
            if (Instance is null || AlarmTimer is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitTimers)}");
                return;
            }

            Instance.Timer_Main.Elapsed += AlarmTimer.Process;

            AlarmTimer.Tick += (s, e) => {
                App.Current.Dispatcher.Invoke (() => {
                    OnTick_Alarm (s, e);
                });
            };

            AlarmTimer.Set (1000);
            AlarmTimer.Start ();
        }

        public virtual void InitAlarm () {
            AlarmLine1 = false;
            AlarmLine2 = false;
            AlarmLine3 = false;
        }

        private void InitInterface () {
            // Context Menu (right-click menu!)
            ContextMenu contextMenu = new ContextMenu ();

            // Note: children inherit the context menu (e.g. TextBlocks on the Grid)
            layoutGrid.ContextMenu = contextMenu;

            uiMenuZeroTransducer = new ();
            uiMenuZeroTransducer.Header = Instance?.Language.Localize ("MENU:MenuZeroTransducer");
            uiMenuZeroTransducer.Click += MenuZeroTransducer_Click;
            contextMenu.Items.Add (uiMenuZeroTransducer);
            contextMenu.Items.Add (new Separator ());

            MenuItem menuAddNumeric = new ();
            menuAddNumeric.Header = Instance?.Language.Localize ("MENU:MenuAddNumeric");
            menuAddNumeric.Click += MenuAddNumeric_Click;
            contextMenu.Items.Add (menuAddNumeric);

            MenuItem menuRemoveNumeric = new ();
            menuRemoveNumeric.Header = Instance?.Language.Localize ("MENU:MenuRemoveNumeric");
            menuRemoveNumeric.Click += MenuRemoveNumeric_Click;
            contextMenu.Items.Add (menuRemoveNumeric);
            contextMenu.Items.Add (new Separator ());

            MenuItem menuSelectInput = new ();
            menuSelectInput.Header = Instance?.Language.Localize ("MENU:MenuSelectInputSource");
            contextMenu.Items.Add (menuSelectInput);

            foreach (ControlTypes.Values v in Enum.GetValues (typeof (ControlTypes.Values))) {
                MenuItem mi = new ();
                mi.Header = Instance?.Language.Localize (ControlTypes.LookupString (v));
                mi.Name = v.ToString ();
                mi.Click += MenuSelectInputSource;
                menuSelectInput.Items.Add (mi);
            }
        }

        public void SetColorScheme (Color.Schemes scheme) {
            ColorScheme = scheme;
            UpdateInterface ();
        }

        private void UpdateInterface ()
            => UpdateInterface (this, new EventArgs ());

        private void UpdateInterface (object? sender, EventArgs e) {
            App.Current.Dispatcher.InvokeAsync (async () => {
                if (ControlType?.Value == ControlTypes.Values.DEFIB && ColorScheme == Color.Schemes.Dark) {
                    // Defib numeric in dark more (colorful) gets specific colors based on status

                    // Default color scheme settings are still utilized as "default" state
                    Brush statusColor = Color.GetLead (ControlType?.GetLead_Color, ColorScheme);

                    if (Device?.Mode == Windows.DeviceDefib.Modes.PACER) {
                        statusColor = Brushes.Orange;
                    } else if (Device?.Analyze != Windows.DeviceDefib.AnalyzeStates.Inactive) {
                        statusColor = Device?.Analyze switch {
                            Windows.DeviceDefib.AnalyzeStates.Analyzing => Brushes.YellowGreen,
                            _ => statusColor
                        };
                    } else if (Device?.Charge != Windows.DeviceDefib.ChargeStates.Discharged) {
                        statusColor = Device?.Charge switch {
                            Windows.DeviceDefib.ChargeStates.Charging => Brushes.Yellow,
                            Windows.DeviceDefib.ChargeStates.Charged => Brushes.Red,
                            _ => statusColor
                        };
                    }

                    borderNumeric.BorderBrush = statusColor;
                    lblNumType.Foreground = statusColor;
                    lblLine1.Foreground = statusColor;
                    lblLine2.Foreground = statusColor;
                    lblLine3.Foreground = statusColor;
                } else {
                    // All other numerics and color modes get colors set by the color scheme
                    borderNumeric.BorderBrush = Color.GetLead (ControlType?.GetLead_Color, ColorScheme);
                    lblNumType.Foreground = Color.GetLead (ControlType?.GetLead_Color, ColorScheme);
                    lblLine1.Foreground = Color.GetLead (ControlType?.GetLead_Color, ColorScheme);
                    lblLine2.Foreground = Color.GetLead (ControlType?.GetLead_Color, ColorScheme);
                    lblLine3.Foreground = Color.GetLead (ControlType?.GetLead_Color, ColorScheme);
                }
                lblLine1.Visibility = Visibility.Visible;
                lblLine2.Visibility = Visibility.Visible;
                lblLine3.Visibility = Visibility.Visible;

                lblNumType.Text = Instance?.Language.Localize (ControlTypes.LookupString (ControlType.Value));

                /* Set lines to be visible/hidden as appropriate */
                switch (ControlType?.Value) {
                    default:
                    case ControlTypes.Values.NIBP:
                    case ControlTypes.Values.ABP:
                    case ControlTypes.Values.PA:
                    case ControlTypes.Values.DEFIB:
                        lblLine1.FontSize = 30;
                        lblLine2.FontSize = 30;
                        lblLine3.FontSize = 20;
                        break;

                    case ControlTypes.Values.SPO2:
                    case ControlTypes.Values.ETCO2:
                        lblLine1.FontSize = 30;
                        lblLine2.FontSize = 20;
                        lblLine3.Visibility = Visibility.Hidden;
                        break;

                    case ControlTypes.Values.ECG:
                    case ControlTypes.Values.T:
                    case ControlTypes.Values.RR:
                    case ControlTypes.Values.CVP:
                        lblLine1.FontSize = 30;
                        lblLine2.Visibility = Visibility.Hidden;
                        lblLine3.Visibility = Visibility.Hidden;
                        break;
                }

                /* Set menu items enabled/disabled accordingly */
                switch (ControlType?.Value) {
                    default:
                        if (uiMenuZeroTransducer is not null)
                            uiMenuZeroTransducer.IsEnabled = false;
                        break;

                    case ControlTypes.Values.ABP:
                    case ControlTypes.Values.CVP:
                    case ControlTypes.Values.PA:
                        if (uiMenuZeroTransducer is not null)
                            uiMenuZeroTransducer.IsEnabled = true;
                        break;
                }
            });
        }

        public virtual void OnTick_Alarm (object? sender, EventArgs e) {
            // TODO: Implement (from base class???)
        }

        public void UpdateVitals () {
            // TODO: Implement
        }

        private void MenuZeroTransducer_Click (object? sender, RoutedEventArgs e) {
            if (Instance?.Physiology is not null) {
                switch (ControlType?.Value) {
                    case ControlTypes.Values.ABP: Instance.Physiology.TransducerZeroed_ABP = true; return;
                    case ControlTypes.Values.CVP: Instance.Physiology.TransducerZeroed_CVP = true; return;
                    case ControlTypes.Values.PA: Instance.Physiology.TransducerZeroed_PA = true; return;
                }
            }
        }

        private void MenuAddNumeric_Click (object? sender, RoutedEventArgs e)
            => Instance?.Device_Defib?.AddNumeric ();

        private void MenuRemoveNumeric_Click (object? sender, RoutedEventArgs e)
            => Instance?.Device_Defib?.RemoveNumeric (this);

        private void MenuSelectInputSource (object? sender, RoutedEventArgs e) {
            ControlTypes.Values selectedValue;
            if (!Enum.TryParse<ControlTypes.Values> ((sender as MenuItem)?.Name, out selectedValue))
                return;

            if (ControlType is not null)
                ControlType.Value = selectedValue;

            UpdateInterface ();
            UpdateVitals ();
        }
    }
}