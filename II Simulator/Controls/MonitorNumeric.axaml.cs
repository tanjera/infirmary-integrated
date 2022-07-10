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

    public partial class MonitorNumeric : UserControl {
        public ControlType? controlType;
        public Color.Schemes colorScheme;

        private Timer timerAlarm = new ();
        private bool iterAlarm = false;

        private MenuItem? menuZeroTransducer;

        private Alarm? alarmRef;
        private bool alarmLine1;
        private bool alarmLine2;
        private bool alarmLine3;

        public class ControlType {
            public Values Value;

            public ControlType (Values v) {
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
                ControlType.Values.ECG => Color.Leads.ECG,
                ControlType.Values.T => Color.Leads.T,
                ControlType.Values.RR => Color.Leads.RR,
                ControlType.Values.ETCO2 => Color.Leads.ETCO2,
                ControlType.Values.SPO2 => Color.Leads.SPO2,
                ControlType.Values.NIBP => Color.Leads.NIBP,
                ControlType.Values.ABP => Color.Leads.ABP,
                ControlType.Values.CVP => Color.Leads.CVP,
                ControlType.Values.CO => Color.Leads.CO,
                ControlType.Values.PA => Color.Leads.PA,
                ControlType.Values.ICP => Color.Leads.ICP,
                ControlType.Values.IAP => Color.Leads.IAP,
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

        public MonitorNumeric (ControlType.Values v, Color.Schemes cs) {
            InitializeComponent ();

            controlType = new ControlType (v);
            colorScheme = cs;

            InitInterface ();
            InitTimers ();
            InitAlarm ();

            UpdateInterface ();
        }

        ~MonitorNumeric () {
            timerAlarm.Dispose ();
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

            menuZeroTransducer = new ();
            menuZeroTransducer.Header = App.Language.Localize ("MENU:MenuZeroTransducer");
            menuZeroTransducer.Classes.Add ("item");
            menuZeroTransducer.Click += MenuZeroTransducer_Click;
            menuitemsContext.Add (menuZeroTransducer);

            menuitemsContext.Add (new Separator ());

            MenuItem menuAddNumeric = new ();
            menuAddNumeric.Header = App.Language.Localize ("MENU:MenuAddNumeric");
            menuAddNumeric.Classes.Add ("item");
            menuAddNumeric.Click += MenuAddNumeric_Click;
            menuitemsContext.Add (menuAddNumeric);

            MenuItem menuRemoveNumeric = new ();
            menuRemoveNumeric.Header = App.Language.Localize ("MENU:MenuRemoveNumeric");
            menuRemoveNumeric.Classes.Add ("item");
            menuRemoveNumeric.Click += MenuRemoveNumeric_Click;
            menuitemsContext.Add (menuRemoveNumeric);

            menuitemsContext.Add (new Separator ());

            MenuItem menuSelectInput = new ();
            List<object> menuitemsSelectInput = new ();
            menuSelectInput.Header = App.Language.Localize ("MENU:MenuSelectInputSource");
            menuSelectInput.Classes.Add ("item");

            foreach (ControlType.Values v in Enum.GetValues (typeof (ControlType.Values))) {
                MenuItem mi = new ();
                mi.Header = App.Language.Localize (ControlType.LookupString (v));
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
            App.Timer_Main.Elapsed += timerAlarm.Process;

            timerAlarm.Tick += (s, e) => { Dispatcher.UIThread.InvokeAsync (() => { OnTick_Alarm (s, e); }); };

            timerAlarm.Set (1000);
            timerAlarm.Start ();
        }

        private void InitAlarm () {
            alarmLine1 = false;
            alarmLine2 = false;
            alarmLine3 = false;
        }

        private void OnTick_Alarm (object? sender, EventArgs e) {
            TextBlock lblLine1 = this.FindControl<TextBlock> ("lblLine1");
            TextBlock lblLine2 = this.FindControl<TextBlock> ("lblLine2");
            TextBlock lblLine3 = this.FindControl<TextBlock> ("lblLine3");

            iterAlarm = !iterAlarm;

            int time = (alarmRef?.Priority ?? Alarm.Priorities.Low) switch {
                Alarm.Priorities.Low => 10000,
                Alarm.Priorities.Medium => 5000,
                Alarm.Priorities.High => 1000,
                _ => 10000,
            };

            _ = timerAlarm.ResetAuto (time);

            if (controlType is not null) {
                if (alarmLine1)
                    lblLine1.Foreground = iterAlarm
                        ? Color.GetLead (controlType.GetLead_Color, colorScheme)
                        : Color.GetAlarm (controlType.GetLead_Color, colorScheme);
                else
                    lblLine1.Foreground = Color.GetLead (controlType.GetLead_Color, colorScheme);

                if (alarmLine2)
                    lblLine2.Foreground = iterAlarm
                        ? Color.GetLead (controlType.GetLead_Color, colorScheme)
                        : Color.GetAlarm (controlType.GetLead_Color, colorScheme);
                else
                    lblLine2.Foreground = Color.GetLead (controlType.GetLead_Color, colorScheme);

                if (alarmLine3)
                    lblLine3.Foreground = iterAlarm
                        ? Color.GetLead (controlType.GetLead_Color, colorScheme)
                        : Color.GetAlarm (controlType.GetLead_Color, colorScheme);
                else
                    lblLine3.Foreground = Color.GetLead (controlType.GetLead_Color, colorScheme);
            }
        }

        public void SetColorScheme (Color.Schemes scheme) {
            colorScheme = scheme;
            UpdateInterface ();
        }

        private void UpdateInterface () {
            if (controlType is null)
                return;

            Dispatcher.UIThread.InvokeAsync (() => {
                Border borderNumeric = this.FindControl<Border> ("borderNumeric");
                TextBlock lblNumType = this.FindControl<TextBlock> ("lblNumType");
                TextBlock lblLine1 = this.FindControl<TextBlock> ("lblLine1");
                TextBlock lblLine2 = this.FindControl<TextBlock> ("lblLine2");
                TextBlock lblLine3 = this.FindControl<TextBlock> ("lblLine3");

                borderNumeric.BorderBrush = Color.GetLead (controlType.GetLead_Color, colorScheme);

                lblNumType.Foreground = Color.GetLead (controlType.GetLead_Color, colorScheme);
                lblLine1.Foreground = Color.GetLead (controlType.GetLead_Color, colorScheme);
                lblLine2.Foreground = Color.GetLead (controlType.GetLead_Color, colorScheme);
                lblLine3.Foreground = Color.GetLead (controlType.GetLead_Color, colorScheme);

                lblLine1.IsVisible = true;
                lblLine2.IsVisible = true;
                lblLine3.IsVisible = true;

                lblNumType.Text = App.Language.Localize (ControlType.LookupString (controlType.Value));

                /* Set lines to be visible/hidden as appropriate */
                switch (controlType.Value) {
                    default:
                    case ControlType.Values.NIBP:
                    case ControlType.Values.ABP:
                    case ControlType.Values.PA:
                        break;

                    case ControlType.Values.SPO2:
                    case ControlType.Values.ETCO2:
                    case ControlType.Values.ICP:
                        lblLine3.IsVisible = false;
                        break;

                    case ControlType.Values.ECG:
                    case ControlType.Values.T:
                    case ControlType.Values.RR:
                    case ControlType.Values.CVP:
                    case ControlType.Values.CO:
                    case ControlType.Values.IAP:
                        lblLine2.IsVisible = false;
                        lblLine3.IsVisible = false;
                        break;
                }

                /* Set menu items enabled/disabled accordingly */
                switch (controlType.Value) {
                    default:
                        if (menuZeroTransducer is not null)
                            menuZeroTransducer.IsEnabled = false;
                        break;

                    case ControlType.Values.ABP:
                    case ControlType.Values.CVP:
                    case ControlType.Values.IAP:
                    case ControlType.Values.ICP:
                    case ControlType.Values.PA:
                        if (menuZeroTransducer is not null)
                            menuZeroTransducer.IsEnabled = true;
                        break;
                }
            });
        }

        public void UpdateVitals () {
            if (App.Patient == null)
                return;

            TextBlock lblLine1 = this.FindControl<TextBlock> ("lblLine1");
            TextBlock lblLine2 = this.FindControl<TextBlock> ("lblLine2");
            TextBlock lblLine3 = this.FindControl<TextBlock> ("lblLine3");

            switch (controlType?.Value) {
                default:
                case ControlType.Values.ECG:
                    int hr = App.Patient.MeasureHR_ECG (Strip.DefaultLength, Strip.DefaultLength * Strip.DefaultBufferLength);
                    lblLine1.Text = String.Format ("{0:0}", hr);

                    alarmRef = App.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.HR);
                    alarmLine1 = (alarmRef is not null && alarmRef.IsSet && alarmRef.IsEnabled && alarmRef.ShouldAlarm (hr));
                    break;

                case ControlType.Values.T:
                    lblLine1.Text = String.Format ("{0:0.0}", App.Patient.T);
                    break;

                case ControlType.Values.SPO2:
                    int spo2 = (int)II.Math.RandomPercentRange (App.Patient.SPO2, 0.01f);
                    lblLine1.Text = String.Format ("{0:0}", spo2);
                    lblLine2.Text = String.Format ("@ {0:0}", App.Patient.MeasureHR_SPO2 (
                        Strip.DefaultLength, Strip.DefaultLength * Strip.DefaultBufferLength));

                    alarmRef = App.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.SPO2);
                    alarmLine1 = (alarmRef is not null && alarmRef.IsSet && alarmRef.IsEnabled && alarmRef.ShouldAlarm (spo2));
                    break;

                case ControlType.Values.RR:
                    int rr = App.Patient.MeasureRR (
                        Strip.DefaultLength * Strip.DefaultRespiratoryCoefficient, Strip.DefaultLength * Strip.DefaultBufferLength);

                    lblLine1.Text = String.Format ("{0:0}", rr);

                    alarmRef = App.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.RR);
                    alarmLine1 = (alarmRef is not null && alarmRef.IsSet && alarmRef.IsEnabled && alarmRef.ShouldAlarm (rr));
                    break;

                case ControlType.Values.ETCO2:
                    int etco2 = (int)II.Math.RandomPercentRange (App.Patient.ETCO2, 0.02f);

                    lblLine1.Text = String.Format ("{0:0}", etco2);
                    lblLine2.Text = String.Format ("@ {0:0}", App.Patient.MeasureRR (
                        Strip.DefaultLength * Strip.DefaultRespiratoryCoefficient, Strip.DefaultLength * Strip.DefaultBufferLength));

                    alarmRef = App.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.ETCO2);
                    alarmLine1 = (alarmRef is not null && alarmRef.IsSet && alarmRef.IsEnabled && alarmRef.ShouldAlarm (etco2));
                    break;

                case ControlType.Values.NIBP:
                    lblLine1.Text = String.Format ("{0:0}", App.Patient.NSBP);
                    lblLine2.Text = String.Format ("/ {0:0}", App.Patient.NDBP);
                    lblLine3.Text = String.Format ("({0:0})", App.Patient.NMAP);

                    alarmRef = App.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.NSBP);
                    alarmLine1 = (alarmRef is not null && alarmRef.IsSet && alarmRef.IsEnabled && alarmRef.ShouldAlarm (App.Patient.NSBP));

                    alarmRef = App.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.NDBP);
                    alarmLine2 = (alarmRef is not null && alarmRef.IsSet && alarmRef.IsEnabled && alarmRef.ShouldAlarm (App.Patient.NDBP));

                    alarmRef = App.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.NMAP);
                    alarmLine3 = (alarmRef is not null && alarmRef.IsSet && alarmRef.IsEnabled && alarmRef.ShouldAlarm (App.Patient.NMAP));
                    break;

                case ControlType.Values.ABP:
                    if (App.Patient.TransducerZeroed_ABP) {
                        int asbp = (int)II.Math.RandomPercentRange (App.Patient.ASBP, 0.02f);
                        int adbp = (int)II.Math.RandomPercentRange ((App.Patient.IABP_Active ? App.Patient.IABP_DBP : App.Patient.ADBP), 0.02f);
                        int amap = (int)II.Math.RandomPercentRange (App.Patient.AMAP, 0.02f);

                        lblLine1.Text = String.Format ("{0:0}", asbp);
                        lblLine2.Text = String.Format ("/ {0:0}", adbp);
                        lblLine3.Text = String.Format ("({0:0})", amap);

                        alarmRef = App.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.ASBP);
                        alarmLine1 = (alarmRef is not null && alarmRef.IsSet && alarmRef.IsEnabled && alarmRef.ShouldAlarm (asbp));

                        alarmRef = App.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.ADBP);
                        alarmLine2 = (alarmRef is not null && alarmRef.IsSet && alarmRef.IsEnabled && alarmRef.ShouldAlarm (adbp));

                        alarmRef = App.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.AMAP);
                        alarmLine3 = (alarmRef is not null && alarmRef.IsSet && alarmRef.IsEnabled && alarmRef.ShouldAlarm (amap));
                    } else {
                        lblLine1.Text = Utility.WrapString (App.Language.Localize ("NUMERIC:ZeroTransducer"));
                        lblLine2.Text = "";
                        lblLine3.Text = "";

                        alarmLine1 = false;
                        alarmLine2 = false;
                        alarmLine3 = false;
                    }
                    break;

                case ControlType.Values.CVP:
                    if (App.Patient.TransducerZeroed_CVP) {
                        int cvp = (int)II.Math.RandomPercentRange (App.Patient.CVP, 0.02f);

                        lblLine1.Text = String.Format ("{0:0}", cvp);

                        alarmRef = App.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.CVP);
                        alarmLine1 = (alarmRef is not null && alarmRef.IsSet && alarmRef.IsEnabled && alarmRef.ShouldAlarm (cvp));
                    } else {
                        lblLine1.Text = App.Language.Localize ("NUMERIC:ZeroTransducer");

                        alarmLine1 = false;
                    }
                    break;

                case ControlType.Values.CO:
                    lblLine1.Text = String.Format ("{0:0.0}", App.Patient.CO);
                    break;

                case ControlType.Values.PA:
                    if (App.Patient.TransducerZeroed_PA) {
                        int psp = (int)II.Math.RandomPercentRange (App.Patient.PSP, 0.02f);
                        int pdp = (int)II.Math.RandomPercentRange (App.Patient.PDP, 0.02f);
                        int pmp = (int)II.Math.RandomPercentRange (App.Patient.PMP, 0.02f);

                        lblLine1.Text = String.Format ("{0:0}", psp);
                        lblLine2.Text = String.Format ("/ {0:0}", pdp);
                        lblLine3.Text = String.Format ("({0:0})", pmp);

                        alarmRef = App.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.PSP);
                        alarmLine1 = (alarmRef is not null && alarmRef.IsSet && alarmRef.IsEnabled && alarmRef.ShouldAlarm (psp));

                        alarmRef = App.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.PDP);
                        alarmLine2 = (alarmRef is not null && alarmRef.IsSet && alarmRef.IsEnabled && alarmRef.ShouldAlarm (pdp));

                        alarmRef = App.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.PMP);
                        alarmLine3 = (alarmRef is not null && alarmRef.IsSet && alarmRef.IsEnabled && alarmRef.ShouldAlarm (pmp));
                    } else {
                        lblLine1.Text = App.Language.Localize ("NUMERIC:ZeroTransducer");
                        lblLine2.Text = "";
                        lblLine3.Text = "";

                        alarmLine1 = false;
                        alarmLine2 = false;
                        alarmLine3 = false;
                    }
                    break;

                case ControlType.Values.ICP:
                    if (App.Patient.TransducerZeroed_ICP) {
                        int icp = (int)II.Math.RandomPercentRange (App.Patient.ICP, 0.02f);

                        lblLine1.Text = String.Format ("{0:0}", icp);
                        lblLine2.Text = String.Format ("({0:0})", Patient.CalculateCPP (App.Patient.ICP, App.Patient.AMAP));

                        alarmRef = App.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.ICP);
                        alarmLine1 = (alarmRef is not null && alarmRef.IsSet && alarmRef.IsEnabled && alarmRef.ShouldAlarm (icp));
                    } else {
                        lblLine1.Text = App.Language.Localize ("NUMERIC:ZeroTransducer");
                        lblLine2.Text = "";

                        alarmLine1 = false;
                    }
                    break;

                case ControlType.Values.IAP:
                    if (App.Patient.TransducerZeroed_IAP) {
                        int iap = (int)II.Math.RandomPercentRange (App.Patient.IAP, 0.02f);

                        lblLine1.Text = String.Format ("{0:0}", iap);

                        alarmRef = App.Scenario?.DeviceMonitor.Alarms.Find (a => a.Parameter == Alarm.Parameters.IAP);
                        alarmLine1 = (alarmRef is not null && alarmRef.IsSet && alarmRef.IsEnabled && alarmRef.ShouldAlarm (iap));
                    } else {
                        lblLine1.Text = App.Language.Localize ("NUMERIC:ZeroTransducer");

                        alarmLine1 = false;
                    }
                    break;
            }
        }

        private void MenuZeroTransducer_Click (object? sender, RoutedEventArgs e) {
            if (App.Patient is null)
                return;

            switch (controlType?.Value) {
                case ControlType.Values.ABP: App.Patient.TransducerZeroed_ABP = true; return;
                case ControlType.Values.CVP: App.Patient.TransducerZeroed_CVP = true; return;
                case ControlType.Values.PA: App.Patient.TransducerZeroed_PA = true; return;
                case ControlType.Values.ICP: App.Patient.TransducerZeroed_ICP = true; return;
                case ControlType.Values.IAP: App.Patient.TransducerZeroed_IAP = true; return;
            }
        }

        private void MenuAddNumeric_Click (object? sender, RoutedEventArgs e)
            => App.Device_Monitor?.AddNumeric ();

        private void MenuRemoveNumeric_Click (object? sender, RoutedEventArgs e)
            => App.Device_Monitor?.RemoveNumeric (this);

        private void MenuSelectInputSource (object? sender, RoutedEventArgs e) {
            if (sender is not MenuItem)
                return;

            if (!Enum.TryParse<ControlType.Values> (((MenuItem)sender).Name, out ControlType.Values selectedValue))
                return;

            if (controlType is null)
                controlType = new (selectedValue);
            else
                controlType.Value = selectedValue;

            UpdateInterface ();
            UpdateVitals ();
        }
    }
}