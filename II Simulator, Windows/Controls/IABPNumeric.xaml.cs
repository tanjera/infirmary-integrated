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
    /// Interaction logic for IABPNumeric.xaml
    /// </summary>
    public partial class IABPNumeric : UserControl {
        public App? Instance { get; set; }
        private Windows.DeviceIABP? Device;

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
                ECG, ABP, IABP_AP
            }

            public Color.Leads GetLead_Color {
                get { return SwitchLead_Color (this.Value); }
            }

            private static Color.Leads SwitchLead_Color (Values value) => value switch {
                ControlTypes.Values.ECG => Color.Leads.ECG,
                ControlTypes.Values.ABP => Color.Leads.ABP,
                ControlTypes.Values.IABP_AP => Color.Leads.IABP,
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

        public IABPNumeric () {
            InitializeComponent ();
        }

        public IABPNumeric (App? app, Windows.DeviceIABP device, ControlTypes.Values v, Color.Schemes? cs) {
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

        ~IABPNumeric () {
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
            // Note: Background must be set to receive right-clicks for triggering ContextMenu
            this.Background = Brushes.Transparent;

            borderNumeric.BorderBrush = Color.GetLead (ControlType?.GetLead_Color, ColorScheme);
            lblNumType.Foreground = Color.GetLead (ControlType?.GetLead_Color, ColorScheme);
            lblLine1.Foreground = Color.GetLead (ControlType?.GetLead_Color, ColorScheme);
            lblLine2.Foreground = Color.GetLead (ControlType?.GetLead_Color, ColorScheme);
            lblLine3.Foreground = Color.GetLead (ControlType?.GetLead_Color, ColorScheme);
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
                    case ControlTypes.Values.ABP:
                        lblLine1.FontSize = 30;
                        lblLine2.FontSize = 30;
                        lblLine3.FontSize = 20;
                        break;

                    case ControlTypes.Values.IABP_AP:
                        lblLine1.FontSize = 30;
                        lblLine2.FontSize = 20;
                        lblLine3.FontSize = 20;
                        break;

                    case ControlTypes.Values.ECG:
                        lblLine1.FontSize = 40;
                        lblLine2.Visibility = Visibility.Hidden;
                        lblLine3.Visibility = Visibility.Hidden;
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
                    int? hr = Instance?.Physiology.MeasureHR_ECG (
                        Strip.DefaultLength, 
                        Strip.DefaultLength * Strip.DefaultBufferLength);
                    lblLine1.Text = String.Format ("{0:0}", hr);

                    AlarmActive = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.HR);
                    AlarmLine1 = (AlarmActive is not null && AlarmActive.IsSet && AlarmActive.IsEnabled && AlarmActive.ActivateAlarm (hr));
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

                case ControlTypes.Values.IABP_AP:
                    // Flash augmentation pressure reading if below alarm limit
                    lblLine1.Foreground = (AlarmLine1 ?? false) && (AlarmIterator ?? false)
                        ? Brushes.Red : Brushes.SkyBlue;

                    lblLine1.Text = Instance?.Device_IABP?.Running ?? false
                        ? String.Format ("{0:0}", Instance?.Physiology.IABP_AP) : "";

                    lblLine2.Text = String.Format ("{0:0}%", Instance?.Device_IABP?.Augmentation);
                    lblLine3.Text = String.Format ("{0}: {1:0}", Instance?.Language.Localize ("IABP:Alarm"),
                        Instance?.Device_IABP?.AugmentationAlarm);
                    break;
            }
        }

        public void OnTick_Alarm (object? sender, EventArgs e) {
            AlarmIterator = !AlarmIterator;
            _ = AlarmTimer?.ResetStart (1000);

            switch (ControlType?.Value) {
                default: break;

                case ControlTypes.Values.IABP_AP:
                    if ((Instance?.Device_IABP?.Running ?? false)
                            && Instance?.Physiology?.IABP_AP < Instance?.Device_IABP?.AugmentationAlarm) {
                        AlarmLine1 = true;
                        lblLine1.Foreground = (AlarmIterator ?? false) ? Brushes.Red : Brushes.SkyBlue;
                    } else {
                        AlarmLine1 = false;
                    }
                    break;
            }
        }
    }
}