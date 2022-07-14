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

namespace IISIM.Controls {

    public partial class MonitorNumeric : DeviceNumeric {
        public ControlTypes? ControlType;

        private MenuItem? uiMenuZeroTransducer;

        /* Variables controlling for visual alarms */
        public Alarm? AlarmRef;
        private Timer? AlarmTimer = new ();
        private bool? AlarmIterator = false;

        private bool? AlarmLine1;
        private bool? AlarmLine2;
        private bool? AlarmLine3;

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

        public MonitorNumeric (App? app, ControlTypes.Values v, Color.Schemes cs) {
            InitializeComponent ();

            Instance = app;
            ControlType = new ControlTypes (v);
            ColorScheme = cs;

            InitInterface ();
            InitTimers ();
            InitAlarm ();

            UpdateInterface ();
        }

        ~MonitorNumeric () {
            AlarmTimer?.Dispose ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private void InitInterface () {
            // Context Menu (right-click menu!)
            ContextMenu contextMenu = new ();
            List<object> menuitemsContext = new ();

            this.FindControl<Grid> ("layoutGrid").ContextMenu = contextMenu;
            this.FindControl<TextBlock> ("lblNumType").ContextMenu = contextMenu;
            this.FindControl<TextBlock> ("lblLine1").ContextMenu = contextMenu;
            this.FindControl<TextBlock> ("lblLine2").ContextMenu = contextMenu;
            this.FindControl<TextBlock> ("lblLine3").ContextMenu = contextMenu;
            this.FindControl<Viewbox> ("vbLine1").ContextMenu = contextMenu;
            this.FindControl<Viewbox> ("vbLine2").ContextMenu = contextMenu;
            this.FindControl<Viewbox> ("vbLine3").ContextMenu = contextMenu;

            uiMenuZeroTransducer = new ();
            uiMenuZeroTransducer.Header = Instance?.Language.Localize ("MENU:MenuZeroTransducer");
            uiMenuZeroTransducer.Classes.Add ("item");
            uiMenuZeroTransducer.Click += MenuZeroTransducer_Click;
            menuitemsContext.Add (uiMenuZeroTransducer);

            menuitemsContext.Add (new Separator ());

            MenuItem menuAddNumeric = new ();
            menuAddNumeric.Header = Instance?.Language.Localize ("MENU:MenuAddNumeric");
            menuAddNumeric.Classes.Add ("item");
            menuAddNumeric.Click += MenuAddNumeric_Click;
            menuitemsContext.Add (menuAddNumeric);

            MenuItem menuRemoveNumeric = new ();
            menuRemoveNumeric.Header = Instance?.Language.Localize ("MENU:MenuRemoveNumeric");
            menuRemoveNumeric.Classes.Add ("item");
            menuRemoveNumeric.Click += MenuRemoveNumeric_Click;
            menuitemsContext.Add (menuRemoveNumeric);

            menuitemsContext.Add (new Separator ());

            MenuItem menuSelectInput = new ();
            List<object> menuitemsSelectInput = new ();
            menuSelectInput.Header = Instance?.Language.Localize ("MENU:MenuSelectInputSource");
            menuSelectInput.Classes.Add ("item");

            foreach (ControlTypes.Values v in Enum.GetValues (typeof (ControlTypes.Values))) {
                MenuItem mi = new ();
                mi.Header = Instance?.Language.Localize (ControlTypes.LookupString (v));
                mi.Classes.Add ("item");
                mi.Name = v.ToString ();
                mi.Click += MenuSelectInputSource;
                menuitemsSelectInput.Add (mi);
            }

            menuSelectInput.Items = menuitemsSelectInput;

            menuitemsContext.Add (menuSelectInput);
            contextMenu.Items = menuitemsContext;
        }

        private void InitTimers () {
            if (Instance is null || AlarmTimer is null)
                return;

            Instance.Timer_Main.Elapsed += AlarmTimer.Process;

            AlarmTimer.Tick += (s, e) => { Dispatcher.UIThread.InvokeAsync (() => { OnTick_Alarm (s, e); }); };

            AlarmTimer.Set (1000);
            AlarmTimer.Start ();
        }

        private void InitAlarm () {
            AlarmLine1 = false;
            AlarmLine2 = false;
            AlarmLine3 = false;
        }

        private void OnTick_Alarm (object? sender, EventArgs e) {
            TextBlock lblLine1 = this.FindControl<TextBlock> ("lblLine1");
            TextBlock lblLine2 = this.FindControl<TextBlock> ("lblLine2");
            TextBlock lblLine3 = this.FindControl<TextBlock> ("lblLine3");

            AlarmIterator = !AlarmIterator;

            int time = (AlarmRef?.Priority ?? Alarm.Priorities.Low) switch {
                Alarm.Priorities.Low => 10000,
                Alarm.Priorities.Medium => 5000,
                Alarm.Priorities.High => 1000,
                _ => 10000,
            };

            _ = AlarmTimer?.ResetAuto (time);

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

        public void SetColorScheme (Color.Schemes scheme) {
            ColorScheme = scheme;
            UpdateInterface ();
        }

        private void UpdateInterface () {
            if (ControlType is null)
                return;

            Dispatcher.UIThread.InvokeAsync (() => {
                Border borderNumeric = this.FindControl<Border> ("borderNumeric");
                TextBlock lblNumType = this.FindControl<TextBlock> ("lblNumType");
                TextBlock lblLine1 = this.FindControl<TextBlock> ("lblLine1");
                TextBlock lblLine2 = this.FindControl<TextBlock> ("lblLine2");
                TextBlock lblLine3 = this.FindControl<TextBlock> ("lblLine3");

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
                        break;

                    case ControlTypes.Values.SPO2:
                    case ControlTypes.Values.ETCO2:
                    case ControlTypes.Values.ICP:
                        lblLine3.IsVisible = false;
                        break;

                    case ControlTypes.Values.ECG:
                    case ControlTypes.Values.T:
                    case ControlTypes.Values.RR:
                    case ControlTypes.Values.CVP:
                    case ControlTypes.Values.CO:
                    case ControlTypes.Values.IAP:
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
            if (Instance?.Patient == null)
                return;

            TextBlock lblLine1 = this.FindControl<TextBlock> ("lblLine1");
            TextBlock lblLine2 = this.FindControl<TextBlock> ("lblLine2");
            TextBlock lblLine3 = this.FindControl<TextBlock> ("lblLine3");

            switch (ControlType?.Value) {
                default:
                case ControlTypes.Values.ECG:
                    int? hr = Instance?.Patient.MeasureHR_ECG (Strip.DefaultLength, Strip.DefaultLength * Strip.DefaultBufferLength);
                    lblLine1.Text = String.Format ("{0:0}", hr);

                    AlarmRef = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.HR);
                    AlarmLine1 = (AlarmRef is not null && AlarmRef.IsSet && AlarmRef.IsEnabled && AlarmRef.ActivateAlarm (hr));
                    break;

                case ControlTypes.Values.T:
                    lblLine1.Text = String.Format ("{0:0.0}", Instance?.Patient.T);
                    break;

                case ControlTypes.Values.SPO2:
                    int? spo2 = (int)II.Math.RandomPercentRange (Instance?.Patient?.SPO2 ?? 0, 0.01f);
                    lblLine1.Text = String.Format ("{0:0}", spo2);
                    lblLine2.Text = String.Format ("@ {0:0}", Instance?.Patient.MeasureHR_SPO2 (
                        Strip.DefaultLength, Strip.DefaultLength * Strip.DefaultBufferLength));

                    AlarmRef = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.SPO2);
                    AlarmLine1 = (AlarmRef is not null && AlarmRef.IsSet && AlarmRef.IsEnabled && AlarmRef.ActivateAlarm (spo2));
                    break;

                case ControlTypes.Values.RR:
                    int? rr = Instance?.Patient.MeasureRR (
                        Strip.DefaultLength * Strip.DefaultRespiratoryCoefficient, Strip.DefaultLength * Strip.DefaultBufferLength);

                    lblLine1.Text = String.Format ("{0:0}", rr);

                    AlarmRef = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.RR);
                    AlarmLine1 = (AlarmRef is not null && AlarmRef.IsSet && AlarmRef.IsEnabled && AlarmRef.ActivateAlarm (rr ?? 0));
                    break;

                case ControlTypes.Values.ETCO2:
                    int? etco2 = (int)II.Math.RandomPercentRange (Instance?.Patient?.ETCO2 ?? 0, 0.02f);

                    lblLine1.Text = String.Format ("{0:0}", etco2);
                    lblLine2.Text = String.Format ("@ {0:0}", Instance?.Patient.MeasureRR (
                        Strip.DefaultLength * Strip.DefaultRespiratoryCoefficient, Strip.DefaultLength * Strip.DefaultBufferLength));

                    AlarmRef = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.ETCO2);
                    AlarmLine1 = (AlarmRef is not null && AlarmRef.IsSet && AlarmRef.IsEnabled && AlarmRef.ActivateAlarm (etco2));
                    break;

                case ControlTypes.Values.NIBP:
                    lblLine1.Text = String.Format ("{0:0}", Instance?.Patient.NSBP);
                    lblLine2.Text = String.Format ("/ {0:0}", Instance?.Patient.NDBP);
                    lblLine3.Text = String.Format ("({0:0})", Instance?.Patient.NMAP);

                    AlarmRef = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.NSBP);
                    AlarmLine1 = (AlarmRef is not null && AlarmRef.IsSet && AlarmRef.IsEnabled && AlarmRef.ActivateAlarm (Instance?.Patient.NSBP));

                    AlarmRef = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.NDBP);
                    AlarmLine2 = (AlarmRef is not null && AlarmRef.IsSet && AlarmRef.IsEnabled && AlarmRef.ActivateAlarm (Instance?.Patient.NDBP));

                    AlarmRef = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.NMAP);
                    AlarmLine3 = (AlarmRef is not null && AlarmRef.IsSet && AlarmRef.IsEnabled && AlarmRef.ActivateAlarm (Instance?.Patient.NMAP));
                    break;

                case ControlTypes.Values.ABP:
                    if (Instance?.Patient.TransducerZeroed_ABP ?? false) {
                        int asbp = (int)II.Math.RandomPercentRange (Instance?.Patient?.ASBP, 0.02f);
                        int adbp = (int)II.Math.RandomPercentRange ((Instance?.Patient?.IABP_Active ?? false ? Instance?.Patient.IABP_DBP : Instance?.Patient.ADBP), 0.02f);
                        int amap = (int)II.Math.RandomPercentRange (Instance?.Patient?.AMAP, 0.02f);

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
                    if (Instance?.Patient.TransducerZeroed_CVP ?? false) {
                        int cvp = (int)II.Math.RandomPercentRange (Instance?.Patient.CVP, 0.02f);

                        lblLine1.Text = String.Format ("{0:0}", cvp);

                        AlarmRef = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.CVP);
                        AlarmLine1 = (AlarmRef is not null && AlarmRef.IsSet && AlarmRef.IsEnabled && AlarmRef.ActivateAlarm (cvp));
                    } else {
                        lblLine1.Text = Instance?.Language.Localize ("NUMERIC:ZeroTransducer");

                        AlarmLine1 = false;
                    }
                    break;

                case ControlTypes.Values.CO:
                    lblLine1.Text = String.Format ("{0:0.0}", Instance?.Patient.CO);
                    break;

                case ControlTypes.Values.PA:
                    if (Instance?.Patient.TransducerZeroed_PA ?? false) {
                        int psp = (int)II.Math.RandomPercentRange (Instance?.Patient.PSP, 0.02f);
                        int pdp = (int)II.Math.RandomPercentRange (Instance?.Patient.PDP, 0.02f);
                        int pmp = (int)II.Math.RandomPercentRange (Instance?.Patient.PMP, 0.02f);

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
                    if (Instance?.Patient.TransducerZeroed_ICP ?? false) {
                        int icp = (int)II.Math.RandomPercentRange (Instance?.Patient.ICP, 0.02f);

                        lblLine1.Text = String.Format ("{0:0}", icp);
                        lblLine2.Text = String.Format ("({0:0})", Patient.CalculateCPP (Instance?.Patient.ICP, Instance?.Patient.AMAP));

                        AlarmRef = Instance?.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.ICP);
                        AlarmLine1 = (AlarmRef is not null && AlarmRef.IsSet && AlarmRef.IsEnabled && AlarmRef.ActivateAlarm (icp));
                    } else {
                        lblLine1.Text = Instance?.Language.Localize ("NUMERIC:ZeroTransducer");
                        lblLine2.Text = "";

                        AlarmLine1 = false;
                    }
                    break;

                case ControlTypes.Values.IAP:
                    if (Instance?.Patient.TransducerZeroed_IAP ?? false) {
                        int iap = (int)II.Math.RandomPercentRange (Instance?.Patient.IAP, 0.02f);

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

        private void MenuZeroTransducer_Click (object? sender, RoutedEventArgs e) {
            if (Instance?.Patient is null)
                return;

            switch (ControlType?.Value) {
                case ControlTypes.Values.ABP: Instance.Patient.TransducerZeroed_ABP = true; return;
                case ControlTypes.Values.CVP: Instance.Patient.TransducerZeroed_CVP = true; return;
                case ControlTypes.Values.PA: Instance.Patient.TransducerZeroed_PA = true; return;
                case ControlTypes.Values.ICP: Instance.Patient.TransducerZeroed_ICP = true; return;
                case ControlTypes.Values.IAP: Instance.Patient.TransducerZeroed_IAP = true; return;
            }
        }

        private void MenuAddNumeric_Click (object? sender, RoutedEventArgs e)
            => Instance?.Device_Monitor?.AddNumeric ();

        private void MenuRemoveNumeric_Click (object? sender, RoutedEventArgs e)
            => Instance?.Device_Monitor?.RemoveNumeric (this);

        private void MenuSelectInputSource (object? sender, RoutedEventArgs e) {
            if (sender is not MenuItem)
                return;

            if (!Enum.TryParse<ControlTypes.Values> (((MenuItem)sender).Name, out ControlTypes.Values selectedValue))
                return;

            if (ControlType is null)
                ControlType = new (selectedValue);
            else
                ControlType.Value = selectedValue;

            UpdateInterface ();
            UpdateVitals ();
        }
    }
}