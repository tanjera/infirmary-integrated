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

using IISIM.Windows;

namespace IISIM.Controls {

    /// <summary>
    /// Interaction logic for MonitorNumeric.xaml
    /// </summary>
    public partial class MonitorNumeric : UserControl {
        public App? Instance { get; set; }
        private Windows.DeviceMonitor? Device;

        /* Drawing variables, offsets and multipliers */
        public Color.Schemes? ColorScheme;
        public Brush? TracingBrush = Brushes.Black;

        /* Variables controlling for visual alarms */
        public Alarm? AlarmActive;
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
                CO, PA, ICP, IAP
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
                ControlTypes.Values.CO => Color.Leads.CO,
                ControlTypes.Values.PA => Color.Leads.PA,
                ControlTypes.Values.ICP => Color.Leads.ICP,
                ControlTypes.Values.IAP => Color.Leads.IAP,
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

        public MonitorNumeric () {
            InitializeComponent ();
        }

        public MonitorNumeric (App? app, Windows.DeviceMonitor device, ControlTypes.Values v, Color.Schemes? cs) {
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

        ~MonitorNumeric () {
            AlarmTimer?.Dispose ();
        }

        public virtual void InitTimers () {
            if (Instance is null || AlarmTimer is null) {
                Debug.WriteLine ($"Null return at {this.Name}.{nameof (InitTimers)}");
                return;
            }

            Instance.Timer_Main.Elapsed += AlarmTimer.Process;

            AlarmTimer.Tick += (s, e) =>
            Application.Current.Dispatcher.Invoke (() => {
                OnTick_Alarm (s, e);
            });

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
            this.ContextMenu = contextMenu;

            // Note: Background must be set to receive right-clicks for triggering ContextMenu
            this.Background = Brushes.Transparent;

            borderNumeric.BorderBrush = Color.GetLead (ControlType?.GetLead_Color, ColorScheme);
            lblNumType.Foreground = Color.GetLead (ControlType?.GetLead_Color, ColorScheme);
            lblLine1.Foreground = Color.GetLead (ControlType?.GetLead_Color, ColorScheme);
            lblLine2.Foreground = Color.GetLead (ControlType?.GetLead_Color, ColorScheme);
            lblLine3.Foreground = Color.GetLead (ControlType?.GetLead_Color, ColorScheme);

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
            
            MenuItem menuMoveUp = new ();
            menuMoveUp.Header = Instance?.Language.Localize ("MENU:MoveUp");
            menuMoveUp.Click += MenuMoveUp_Click;
            contextMenu.Items.Add (menuMoveUp);

            MenuItem menuMoveDown = new ();
            menuMoveDown.Header = Instance?.Language.Localize ("MENU:MoveDown");
            menuMoveDown.Click += MenuMoveDown_Click;
            contextMenu.Items.Add (menuMoveDown);
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
            Application.Current.Dispatcher.Invoke (InitInterface);
        }

        private void UpdateInterface ()
            => UpdateInterface (this, new EventArgs ());

        private void UpdateInterface (object? sender, EventArgs e) {
            App.Current.Dispatcher.InvokeAsync (() => {
                lblLine1.Visibility = Visibility.Visible;
                lblLine2.Visibility = Visibility.Visible;
                lblLine3.Visibility = Visibility.Visible;

                if (ControlType is not null)
                    lblNumType.Text = Instance?.Language.Localize (ControlTypes.LookupString (ControlType.Value));

                /* Set lines to be visible/hidden as appropriate */
                switch (ControlType?.Value) {
                    default:
                    case ControlTypes.Values.NIBP:
                    case ControlTypes.Values.ABP:
                    case ControlTypes.Values.PA:
                        lblLine1.FontSize = 30;
                        lblLine2.FontSize = 30;
                        lblLine3.FontSize = 20;
                        break;

                    case ControlTypes.Values.SPO2:
                    case ControlTypes.Values.ETCO2:
                    case ControlTypes.Values.ICP:
                        lblLine1.FontSize = 30;
                        lblLine2.FontSize = 20;
                        lblLine3.Visibility = Visibility.Hidden;
                        break;

                    case ControlTypes.Values.ECG:
                    case ControlTypes.Values.T:
                    case ControlTypes.Values.RR:
                    case ControlTypes.Values.CVP:
                    case ControlTypes.Values.CO:
                    case ControlTypes.Values.IAP:
                        lblLine1.FontSize = 36;
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
                    case ControlTypes.Values.IAP:
                    case ControlTypes.Values.ICP:
                    case ControlTypes.Values.PA:
                        if (uiMenuZeroTransducer is not null)
                            uiMenuZeroTransducer.IsEnabled = true;
                        break;
                }
            });
        }

        public void UpdateVitals () {
            if (Instance?.Physiology == null)
                return;

            switch (ControlType?.Value) {
                default:
                case ControlTypes.Values.ECG:
                    int? hr = Instance?.Physiology.MeasureHR_ECG (Strip.DefaultLength, Strip.DefaultLength * Strip.DefaultBufferLength);
                    lblLine1.Text = String.Format ("{0:0}", hr);

                    AlarmActive = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.HR);
                    AlarmLine1 = (AlarmActive is not null && AlarmActive.IsSet && AlarmActive.IsEnabled && AlarmActive.ActivateAlarm (hr));
                    break;

                case ControlTypes.Values.T:
                    lblLine1.Text = String.Format ("{0:0.0}", Instance?.Physiology.T);
                    break;

                case ControlTypes.Values.SPO2:
                    int? spo2 = (int)II.Math.RandomPercentRange (Instance?.Physiology?.SPO2 ?? 0, 0.01f);
                    lblLine1.Text = String.Format ("{0:0} %", spo2);
                    lblLine2.Text = String.Format ("@ {0:0}", Instance?.Physiology.MeasureHR_SPO2 (
                        Strip.DefaultLength, Strip.DefaultLength * Strip.DefaultBufferLength));

                    AlarmActive = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.SPO2);
                    AlarmLine1 = (AlarmActive is not null && AlarmActive.IsSet && AlarmActive.IsEnabled && AlarmActive.ActivateAlarm (spo2));
                    break;

                case ControlTypes.Values.RR:
                    int? rr = Instance?.Physiology.MeasureRR (
                        Strip.DefaultLength * Strip.DefaultRespiratoryCoefficient, Strip.DefaultLength * Strip.DefaultBufferLength);

                    lblLine1.Text = String.Format ("{0:0}", rr);

                    AlarmActive = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.RR);
                    AlarmLine1 = (AlarmActive is not null && AlarmActive.IsSet && AlarmActive.IsEnabled && AlarmActive.ActivateAlarm (rr ?? 0));
                    break;

                case ControlTypes.Values.ETCO2:
                    int? etco2 = (int)II.Math.RandomPercentRange (Instance?.Physiology?.ETCO2 ?? 0, 0.02f);

                    lblLine1.Text = String.Format ("{0:0}", etco2);
                    lblLine2.Text = String.Format ("@ {0:0}", Instance?.Physiology.MeasureRR (
                        Strip.DefaultLength * Strip.DefaultRespiratoryCoefficient, Strip.DefaultLength * Strip.DefaultBufferLength));

                    AlarmActive = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.ETCO2);
                    AlarmLine1 = (AlarmActive is not null && AlarmActive.IsSet && AlarmActive.IsEnabled && AlarmActive.ActivateAlarm (etco2));
                    break;

                case ControlTypes.Values.NIBP:
                    lblLine1.Text = String.Format ("{0:0}", Instance?.Physiology.NSBP);
                    lblLine2.Text = String.Format ("/ {0:0}", Instance?.Physiology.NDBP);
                    lblLine3.Text = String.Format ("({0:0})", Instance?.Physiology.NMAP);

                    AlarmActive = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.NSBP);
                    AlarmLine1 = (AlarmActive is not null && AlarmActive.IsSet && AlarmActive.IsEnabled && AlarmActive.ActivateAlarm (Instance?.Physiology.NSBP));

                    AlarmActive = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.NDBP);
                    AlarmLine2 = (AlarmActive is not null && AlarmActive.IsSet && AlarmActive.IsEnabled && AlarmActive.ActivateAlarm (Instance?.Physiology.NDBP));

                    AlarmActive = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.NMAP);
                    AlarmLine3 = (AlarmActive is not null && AlarmActive.IsSet && AlarmActive.IsEnabled && AlarmActive.ActivateAlarm (Instance?.Physiology.NMAP));
                    break;

                case ControlTypes.Values.ABP:
                    if (Instance?.Physiology.TransducerZeroed_ABP ?? false) {
                        int asbp = (int)II.Math.RandomPercentRange (Instance?.Physiology?.ASBP, 0.02f);
                        int adbp = (int)II.Math.RandomPercentRange ((Instance?.Physiology?.IABP_Active ?? false ? Instance?.Physiology.IABP_DBP : Instance?.Physiology.ADBP), 0.02f);
                        int amap = (int)II.Math.RandomPercentRange (Instance?.Physiology?.AMAP, 0.02f);

                        lblLine1.Text = String.Format ("{0:0}", asbp);
                        lblLine2.Text = String.Format ("/ {0:0}", adbp);
                        lblLine3.Text = String.Format ("({0:0})", amap);

                        AlarmActive = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.ASBP);
                        AlarmLine1 = (AlarmActive is not null && AlarmActive.IsSet && AlarmActive.IsEnabled && AlarmActive.ActivateAlarm (asbp));

                        AlarmActive = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.ADBP);
                        AlarmLine2 = (AlarmActive is not null && AlarmActive.IsSet && AlarmActive.IsEnabled && AlarmActive.ActivateAlarm (adbp));

                        AlarmActive = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.AMAP);
                        AlarmLine3 = (AlarmActive is not null && AlarmActive.IsSet && AlarmActive.IsEnabled && AlarmActive.ActivateAlarm (amap));
                    } else {
                        lblLine1.Text = Utility.WrapString (Instance?.Language.Localize ("NUMERIC:ZeroTransducer"));
                        lblLine2.Text = "";
                        lblLine3.Text = "";

                        AlarmLine1 = false;
                        AlarmLine2 = false;
                        AlarmLine3 = false;
                    }
                    break;

                case ControlTypes.Values.CVP:
                    if (Instance?.Physiology.TransducerZeroed_CVP ?? false) {
                        int cvp = (int)II.Math.RandomPercentRange (Instance?.Physiology.CVP, 0.02f);

                        lblLine1.Text = String.Format ("{0:0}", cvp);

                        AlarmActive = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.CVP);
                        AlarmLine1 = (AlarmActive is not null && AlarmActive.IsSet && AlarmActive.IsEnabled && AlarmActive.ActivateAlarm (cvp));
                    } else {
                        lblLine1.Text = Instance?.Language.Localize ("NUMERIC:ZeroTransducer");

                        AlarmLine1 = false;
                    }
                    break;

                case ControlTypes.Values.CO:
                    lblLine1.Text = String.Format ("{0:0.0}", Instance?.Physiology.CO);
                    break;

                case ControlTypes.Values.PA:
                    if (Instance?.Physiology.TransducerZeroed_PA ?? false) {
                        int psp = (int)II.Math.RandomPercentRange (Instance?.Physiology.PSP, 0.02f);
                        int pdp = (int)II.Math.RandomPercentRange (Instance?.Physiology.PDP, 0.02f);
                        int pmp = (int)II.Math.RandomPercentRange (Instance?.Physiology.PMP, 0.02f);

                        lblLine1.Text = String.Format ("{0:0}", psp);
                        lblLine2.Text = String.Format ("/ {0:0}", pdp);
                        lblLine3.Text = String.Format ("({0:0})", pmp);

                        AlarmActive = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.PSP);
                        AlarmLine1 = (AlarmActive is not null && AlarmActive.IsSet && AlarmActive.IsEnabled && AlarmActive.ActivateAlarm (psp));

                        AlarmActive = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.PDP);
                        AlarmLine2 = (AlarmActive is not null && AlarmActive.IsSet && AlarmActive.IsEnabled && AlarmActive.ActivateAlarm (pdp));

                        AlarmActive = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.PMP);
                        AlarmLine3 = (AlarmActive is not null && AlarmActive.IsSet && AlarmActive.IsEnabled && AlarmActive.ActivateAlarm (pmp));
                    } else {
                        lblLine1.Text = Instance?.Language.Localize ("NUMERIC:ZeroTransducer");
                        lblLine2.Text = "";
                        lblLine3.Text = "";

                        AlarmLine1 = false;
                        AlarmLine2 = false;
                        AlarmLine3 = false;
                    }
                    break;

                case ControlTypes.Values.ICP:
                    if (Instance?.Physiology.TransducerZeroed_ICP ?? false) {
                        int icp = (int)II.Math.RandomPercentRange (Instance?.Physiology.ICP, 0.02f);

                        lblLine1.Text = String.Format ("{0:0}", icp);
                        lblLine2.Text = String.Format ("({0:0})", Physiology.CalculateCPP (Instance?.Physiology.ICP, Instance?.Physiology.AMAP));

                        AlarmActive = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.ICP);
                        AlarmLine1 = (AlarmActive is not null && AlarmActive.IsSet && AlarmActive.IsEnabled && AlarmActive.ActivateAlarm (icp));
                    } else {
                        lblLine1.Text = Instance?.Language.Localize ("NUMERIC:ZeroTransducer");
                        lblLine2.Text = "";

                        AlarmLine1 = false;
                    }
                    break;

                case ControlTypes.Values.IAP:
                    if (Instance?.Physiology.TransducerZeroed_IAP ?? false) {
                        int iap = (int)II.Math.RandomPercentRange (Instance?.Physiology.IAP, 0.02f);

                        lblLine1.Text = String.Format ("{0:0}", iap);

                        AlarmActive = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.IAP);
                        AlarmLine1 = (AlarmActive is not null && AlarmActive.IsSet && AlarmActive.IsEnabled && AlarmActive.ActivateAlarm (iap));
                    } else {
                        lblLine1.Text = Instance?.Language.Localize ("NUMERIC:ZeroTransducer");

                        AlarmLine1 = false;
                    }
                    break;
            }
        }

        public void OnTick_Alarm (object? sender, EventArgs e) {
            AlarmIterator = !AlarmIterator;

            int time = (AlarmActive?.Priority ?? Alarm.Priorities.Low) switch {
                Alarm.Priorities.Low => 10000,
                Alarm.Priorities.Medium => 5000,
                Alarm.Priorities.High => 1000,
                _ => 10000,
            };

            _ = AlarmTimer?.ResetStart (time);

            if (ControlType is not null) {
                if (AlarmLine1 ?? false)
                    lblLine1.Foreground = AlarmIterator ?? false
                        ? Color.GetLead (ControlType.GetLead_Color, ColorScheme)
                        : Color.GetAlarm (ControlType.GetLead_Color, ColorScheme);
                else
                    lblLine1.Foreground = Color.GetLead (ControlType.GetLead_Color, ColorScheme);

                if (AlarmLine2 ?? false)
                    lblLine2.Foreground = AlarmIterator ?? false
                        ? Color.GetLead (ControlType.GetLead_Color, ColorScheme)
                        : Color.GetAlarm (ControlType.GetLead_Color, ColorScheme);
                else
                    lblLine2.Foreground = Color.GetLead (ControlType.GetLead_Color, ColorScheme);

                if (AlarmLine3 ?? false)
                    lblLine3.Foreground = AlarmIterator ?? false
                        ? Color.GetLead (ControlType.GetLead_Color, ColorScheme)
                        : Color.GetAlarm (ControlType.GetLead_Color, ColorScheme);
                else
                    lblLine3.Foreground = Color.GetLead (ControlType.GetLead_Color, ColorScheme);
            }
        }

        private void MenuZeroTransducer_Click (object? sender, RoutedEventArgs e) {
            if (Instance?.Physiology is not null) {
                switch (ControlType?.Value) {
                    case ControlTypes.Values.ABP: Instance.Physiology.TransducerZeroed_ABP = true; return;
                    case ControlTypes.Values.CVP: Instance.Physiology.TransducerZeroed_CVP = true; return;
                    case ControlTypes.Values.PA: Instance.Physiology.TransducerZeroed_PA = true; return;
                    case ControlTypes.Values.ICP: Instance.Physiology.TransducerZeroed_ICP = true; return;
                    case ControlTypes.Values.IAP: Instance.Physiology.TransducerZeroed_IAP = true; return;
                }
            }
        }

        private void MenuAddNumeric_Click (object? sender, RoutedEventArgs e)
            => Instance?.Device_Monitor?.AddNumeric ();

        private void MenuRemoveNumeric_Click (object? sender, RoutedEventArgs e)
            => Instance?.Device_Monitor?.RemoveNumeric (this);

        private void MenuMoveUp_Click (object? sender, RoutedEventArgs e)
            => Instance?.Device_Monitor?.MoveNumeric_Up (this);
        
        private void MenuMoveDown_Click (object? sender, RoutedEventArgs e) 
            => Instance?.Device_Monitor?.MoveNumeric_Down (this);
        
        private void MenuSelectInputSource (object? sender, RoutedEventArgs e) {
            ControlTypes.Values selectedValue;
            if (!Enum.TryParse<ControlTypes.Values> ((sender as MenuItem)?.Name, out selectedValue))
                return;

            if (ControlType is not null)
                ControlType.Value = selectedValue;

            InitInterface ();
            UpdateInterface ();
            UpdateVitals ();
        }
    }
}