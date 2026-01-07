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
using Avalonia.Threading;

using II;
using II.Localization;
using II.Rhythm;
using II.Settings;

namespace IISIM.Controls {

    public partial class MonitorNumeric : DeviceNumeric {
        public ControlTypes? ControlType;

        private MenuItem? uiMenuZeroTransducer;

        /* Variables controlling for visual alarms */
        public Alarm? AlarmRef;
        
        private List<II.Settings.Device.Numeric>? Transducers_Zeroed {
            set { 
                if (Instance?.Scenario?.DeviceMonitor is not null)
                    Instance.Scenario.DeviceMonitor.Transducers_Zeroed = value ?? new List<II.Settings.Device.Numeric> (); 
            }
            get { return Instance?.Scenario?.DeviceMonitor.Transducers_Zeroed; }
        }
        
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

        public MonitorNumeric (App? app, ControlTypes.Values v, Color.Schemes cs) : base (app) {
            InitializeComponent ();

            ControlType = new ControlTypes (v);
            ColorScheme = cs;

            InitInterface ();
            UpdateInterface ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private void InitInterface () {
            // Context Menu (right-click menu!)
            ContextMenu contextMenu = new ();

            this.GetControl<Grid> ("layoutGrid").ContextMenu = contextMenu;

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
            
            foreach (ControlTypes.Values v in Enum.GetValues (typeof(ControlTypes.Values))) {
                MenuItem mi = new();
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

        private void UpdateInterface () {
            if (ControlType is null)
                return;

            Dispatcher.UIThread.InvokeAsync (() => {
                Border borderNumeric = this.GetControl<Border> ("borderNumeric");
                TextBlock lblNumType = this.GetControl<TextBlock> ("lblNumType");
                TextBlock lblLine1 = this.GetControl<TextBlock> ("lblLine1");
                TextBlock lblLine2 = this.GetControl<TextBlock> ("lblLine2");
                TextBlock lblLine3 = this.GetControl<TextBlock> ("lblLine3");

                borderNumeric.BorderBrush = Color.GetLead (ControlType.GetLead_Color, ColorScheme);

                lblNumType.Foreground = Color.GetLead (ControlType.GetLead_Color, ColorScheme);
                lblLine1.Foreground = Color.GetLead (ControlType.GetLead_Color, ColorScheme);
                lblLine2.Foreground = Color.GetLead (ControlType.GetLead_Color, ColorScheme);
                lblLine3.Foreground = Color.GetLead (ControlType.GetLead_Color, ColorScheme);

                lblLine1.IsVisible = true;
                lblLine2.IsVisible = true;
                lblLine3.IsVisible = true;

                lblNumType.Text = Instance?.Language.Localize (ControlTypes.LookupString (ControlType.Value));

                /* Set lines to be visible/hidden as appropriate */
                switch (ControlType.Value) {
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
                        lblLine3.IsVisible = false;
                        break;

                    case ControlTypes.Values.ECG:
                    case ControlTypes.Values.T:
                    case ControlTypes.Values.RR:
                    case ControlTypes.Values.CVP:
                    case ControlTypes.Values.CO:
                    case ControlTypes.Values.IAP:
                        lblLine1.FontSize = 30;
                        lblLine2.IsVisible = false;
                        lblLine3.IsVisible = false;
                        break;
                }

                /* Set menu items enabled/disabled accordingly */
                switch (ControlType.Value) {
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

            TextBlock lblLine1 = this.GetControl<TextBlock> ("lblLine1");
            TextBlock lblLine2 = this.GetControl<TextBlock> ("lblLine2");
            TextBlock lblLine3 = this.GetControl<TextBlock> ("lblLine3");

            switch (ControlType?.Value) {
                default:
                case ControlTypes.Values.ECG:
                    int? hr = Instance?.Physiology.MeasureHR_ECG (Strip.DefaultLength, Strip.DefaultLength * Strip.DefaultBufferLength);
                    lblLine1.Text = String.Format ("{0:0}", hr);

                    AlarmRef = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.HR);
                    AlarmLine1 = (AlarmRef is not null && AlarmRef.IsSet && AlarmRef.IsEnabled && AlarmRef.ActivateAlarm (hr));
                    break;

                case ControlTypes.Values.T:
                    lblLine1.Text = String.Format ("{0:0.0}", Instance?.Physiology.T);
                    break;

                case ControlTypes.Values.SPO2:
                    int? spo2 = (int)II.Math.RandomPercentRange (Instance?.Physiology?.SPO2 ?? 0, 0.01f);
                    lblLine1.Text = String.Format ("{0:0} %", spo2);
                    lblLine2.Text = String.Format ("@ {0:0}", Instance?.Physiology.MeasureHR_SPO2 (
                        Strip.DefaultLength, Strip.DefaultLength * Strip.DefaultBufferLength));

                    AlarmRef = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.SPO2);
                    AlarmLine1 = (AlarmRef is not null && AlarmRef.IsSet && AlarmRef.IsEnabled && AlarmRef.ActivateAlarm (spo2));
                    break;

                case ControlTypes.Values.RR:
                    int? rr = Instance?.Physiology.MeasureRR (
                        Strip.DefaultLength * Strip.DefaultRespiratoryCoefficient, Strip.DefaultLength * Strip.DefaultBufferLength);

                    lblLine1.Text = String.Format ("{0:0}", rr);

                    AlarmRef = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.RR);
                    AlarmLine1 = (AlarmRef is not null && AlarmRef.IsSet && AlarmRef.IsEnabled && AlarmRef.ActivateAlarm (rr ?? 0));
                    break;

                case ControlTypes.Values.ETCO2:
                    int? etco2 = (int)II.Math.RandomPercentRange (Instance?.Physiology?.ETCO2 ?? 0, 0.02f);

                    lblLine1.Text = String.Format ("{0:0}", etco2);
                    lblLine2.Text = String.Format ("@ {0:0}", Instance?.Physiology.MeasureRR (
                        Strip.DefaultLength * Strip.DefaultRespiratoryCoefficient, Strip.DefaultLength * Strip.DefaultBufferLength));

                    AlarmRef = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.ETCO2);
                    AlarmLine1 = (AlarmRef is not null && AlarmRef.IsSet && AlarmRef.IsEnabled && AlarmRef.ActivateAlarm (etco2));
                    break;

                case ControlTypes.Values.NIBP:
                    lblLine1.Text = String.Format ("{0:0}", Instance?.Physiology.NSBP);
                    lblLine2.Text = String.Format ("/ {0:0}", Instance?.Physiology.NDBP);
                    lblLine3.Text = String.Format ("({0:0})", Instance?.Physiology.NMAP);

                    AlarmRef = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.NSBP);
                    AlarmLine1 = (AlarmRef is not null && AlarmRef.IsSet && AlarmRef.IsEnabled && AlarmRef.ActivateAlarm (Instance?.Physiology.NSBP));

                    AlarmRef = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.NDBP);
                    AlarmLine2 = (AlarmRef is not null && AlarmRef.IsSet && AlarmRef.IsEnabled && AlarmRef.ActivateAlarm (Instance?.Physiology.NDBP));

                    AlarmRef = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.NMAP);
                    AlarmLine3 = (AlarmRef is not null && AlarmRef.IsSet && AlarmRef.IsEnabled && AlarmRef.ActivateAlarm (Instance?.Physiology.NMAP));
                    break;

                case ControlTypes.Values.ABP:
                    if (Transducers_Zeroed?.Contains(Device.Numeric.ABP) ?? false) {
                        int asbp = (int)II.Math.RandomPercentRange (Instance?.Physiology?.ASBP, 0.02f);
                        int adbp = (int)II.Math.RandomPercentRange ((Instance?.Physiology?.IABP_Active ?? false ? Instance?.Physiology.IABP_DBP : Instance?.Physiology?.ADBP), 0.02f);
                        int amap = (int)II.Math.RandomPercentRange (Instance?.Physiology?.AMAP, 0.02f);

                        lblLine1.Text = String.Format ("{0:0}", asbp);
                        lblLine2.Text = String.Format ("/ {0:0}", adbp);
                        lblLine3.Text = String.Format ("({0:0})", amap);

                        AlarmRef = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.ASBP);
                        AlarmLine1 = (AlarmRef is not null && AlarmRef.IsSet && AlarmRef.IsEnabled && AlarmRef.ActivateAlarm (asbp));

                        AlarmRef = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.ADBP);
                        AlarmLine2 = (AlarmRef is not null && AlarmRef.IsSet && AlarmRef.IsEnabled && AlarmRef.ActivateAlarm (adbp));

                        AlarmRef = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.AMAP);
                        AlarmLine3 = (AlarmRef is not null && AlarmRef.IsSet && AlarmRef.IsEnabled && AlarmRef.ActivateAlarm (amap));
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
                    if (Transducers_Zeroed?.Contains(Device.Numeric.CVP)?? false) {
                        int cvp = (int)II.Math.RandomPercentRange (Instance?.Physiology.CVP, 0.02f);

                        lblLine1.Text = String.Format ("{0:0}", cvp);

                        AlarmRef = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.CVP);
                        AlarmLine1 = (AlarmRef is not null && AlarmRef.IsSet && AlarmRef.IsEnabled && AlarmRef.ActivateAlarm (cvp));
                    } else {
                        lblLine1.Text = Instance?.Language.Localize ("NUMERIC:ZeroTransducer");

                        AlarmLine1 = false;
                    }
                    break;

                case ControlTypes.Values.CO:
                    lblLine1.Text = String.Format ("{0:0.0}", Instance?.Physiology.CO);
                    break;

                case ControlTypes.Values.PA:
                    if (Transducers_Zeroed?.Contains(Device.Numeric.PA) ?? false) {
                        int psp = (int)II.Math.RandomPercentRange (Instance?.Physiology.PSP, 0.02f);
                        int pdp = (int)II.Math.RandomPercentRange (Instance?.Physiology.PDP, 0.02f);
                        int pmp = (int)II.Math.RandomPercentRange (Instance?.Physiology.PMP, 0.02f);

                        lblLine1.Text = String.Format ("{0:0}", psp);
                        lblLine2.Text = String.Format ("/ {0:0}", pdp);
                        lblLine3.Text = String.Format ("({0:0})", pmp);

                        AlarmRef = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.PSP);
                        AlarmLine1 = (AlarmRef is not null && AlarmRef.IsSet && AlarmRef.IsEnabled && AlarmRef.ActivateAlarm (psp));

                        AlarmRef = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.PDP);
                        AlarmLine2 = (AlarmRef is not null && AlarmRef.IsSet && AlarmRef.IsEnabled && AlarmRef.ActivateAlarm (pdp));

                        AlarmRef = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.PMP);
                        AlarmLine3 = (AlarmRef is not null && AlarmRef.IsSet && AlarmRef.IsEnabled && AlarmRef.ActivateAlarm (pmp));
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
                    if (Transducers_Zeroed?.Contains(Device.Numeric.ICP) ?? false) {
                        int icp = (int)II.Math.RandomPercentRange (Instance?.Physiology.ICP, 0.02f);

                        lblLine1.Text = String.Format ("{0:0}", icp);
                        lblLine2.Text = String.Format ("({0:0})", Physiology.CalculateCPP (Instance?.Physiology.ICP, Instance?.Physiology.AMAP));

                        AlarmRef = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.ICP);
                        AlarmLine1 = (AlarmRef is not null && AlarmRef.IsSet && AlarmRef.IsEnabled && AlarmRef.ActivateAlarm (icp));
                    } else {
                        lblLine1.Text = Instance?.Language.Localize ("NUMERIC:ZeroTransducer");
                        lblLine2.Text = "";

                        AlarmLine1 = false;
                    }
                    break;

                case ControlTypes.Values.IAP:
                    if (Transducers_Zeroed?.Contains(Device.Numeric.IAP) ?? false) {
                        int iap = (int)II.Math.RandomPercentRange (Instance?.Physiology.IAP, 0.02f);

                        lblLine1.Text = String.Format ("{0:0}", iap);

                        AlarmRef = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.IAP);
                        AlarmLine1 = (AlarmRef is not null && AlarmRef.IsSet && AlarmRef.IsEnabled && AlarmRef.ActivateAlarm (iap));
                    } else {
                        lblLine1.Text = Instance?.Language.Localize ("NUMERIC:ZeroTransducer");

                        AlarmLine1 = false;
                    }
                    break;
            }
        }

        public override void OnTick_Alarm (object? sender, EventArgs e) {
            TextBlock lblLine1 = this.GetControl<TextBlock> ("lblLine1");
            TextBlock lblLine2 = this.GetControl<TextBlock> ("lblLine2");
            TextBlock lblLine3 = this.GetControl<TextBlock> ("lblLine3");

            AlarmIterator = !AlarmIterator;

            int time = (AlarmRef?.Priority ?? Alarm.Priorities.Low) switch {
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
            if (Instance?.Physiology is null)
                return;

            switch (ControlType?.Value) {
                case ControlTypes.Values.ABP:
                    if (!(Transducers_Zeroed?.Contains (II.Settings.Device.Numeric.ABP) ?? true))
                        Transducers_Zeroed.Add (II.Settings.Device.Numeric.ABP);
                    return;

                case ControlTypes.Values.CVP:
                    if (!(Transducers_Zeroed?.Contains (II.Settings.Device.Numeric.CVP) ?? true))
                        Transducers_Zeroed.Add (II.Settings.Device.Numeric.CVP);
                    return;

                case ControlTypes.Values.PA:
                    if (!(Transducers_Zeroed?.Contains (II.Settings.Device.Numeric.PA) ?? true))
                        Transducers_Zeroed.Add (II.Settings.Device.Numeric.PA);
                    return;

                case ControlTypes.Values.ICP:
                    if (!(Transducers_Zeroed?.Contains (II.Settings.Device.Numeric.ICP) ?? true))
                        Transducers_Zeroed.Add (II.Settings.Device.Numeric.ICP);
                    return;

                case ControlTypes.Values.IAP:
                    if (!(Transducers_Zeroed?.Contains (II.Settings.Device.Numeric.IAP) ?? true))
                        Transducers_Zeroed.Add (II.Settings.Device.Numeric.IAP);
                    return;
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
            if (sender is not MenuItem)
                return;

            if (!Enum.TryParse<ControlTypes.Values> (((MenuItem)sender).Name, out ControlTypes.Values selectedValue))
                return;

            if (ControlType is null)
                ControlType = new (selectedValue);
            else
                ControlType.Value = selectedValue;

            Dispatcher.UIThread.InvokeAsync (UpdateInterface);
            Dispatcher.UIThread.InvokeAsync (UpdateVitals);
        }

    }
}