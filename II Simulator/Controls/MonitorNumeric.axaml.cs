using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

using II;
using II.Localization;
using II.Rhythm;

namespace II_Avalonia.Controls {

    public partial class MonitorNumeric : UserControl {
        public ControlType controlType;
        public Color.Schemes colorScheme;

        private MenuItem menuZeroTransducer;

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
                return String.Format ("NUMERIC:{0}", Enum.GetValues (typeof (Values)).GetValue ((int)value).ToString ());
            }

            public static List<string> MenuItem_Formats {
                get {
                    List<string> o = new List<string> ();
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

            InitInterface ();

            controlType = new ControlType (v);
            colorScheme = cs;

            UpdateInterface ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        private void InitInterface () {
            // Context Menu (right-click menu!)
            ContextMenu contextMenu = new ContextMenu ();
            List<object> menuitemsContext = new List<object> ();

            this.FindControl<Grid> ("layoutGrid").ContextMenu = contextMenu;
            this.FindControl<TextBlock> ("lblNumType").ContextMenu = contextMenu;
            this.FindControl<TextBlock> ("lblLine1").ContextMenu = contextMenu;
            this.FindControl<TextBlock> ("lblLine2").ContextMenu = contextMenu;
            this.FindControl<TextBlock> ("lblLine3").ContextMenu = contextMenu;
            this.FindControl<Viewbox> ("vbLine1").ContextMenu = contextMenu;
            this.FindControl<Viewbox> ("vbLine2").ContextMenu = contextMenu;
            this.FindControl<Viewbox> ("vbLine3").ContextMenu = contextMenu;

            menuZeroTransducer = new MenuItem ();
            menuZeroTransducer.Header = App.Language.Localize ("MENU:MenuZeroTransducer");
            menuZeroTransducer.Classes.Add ("item");
            menuZeroTransducer.Click += MenuZeroTransducer_Click;
            menuitemsContext.Add (menuZeroTransducer);

            menuitemsContext.Add (new Separator ());

            MenuItem menuAddNumeric = new MenuItem ();
            menuAddNumeric.Header = App.Language.Localize ("MENU:MenuAddNumeric");
            menuAddNumeric.Classes.Add ("item");
            menuAddNumeric.Click += MenuAddNumeric_Click;
            menuitemsContext.Add (menuAddNumeric);

            MenuItem menuRemoveNumeric = new MenuItem ();
            menuRemoveNumeric.Header = App.Language.Localize ("MENU:MenuRemoveNumeric");
            menuRemoveNumeric.Classes.Add ("item");
            menuRemoveNumeric.Click += MenuRemoveNumeric_Click;
            menuitemsContext.Add (menuRemoveNumeric);

            menuitemsContext.Add (new Separator ());

            MenuItem menuSelectInput = new MenuItem ();
            List<object> menuitemsSelectInput = new List<object> ();
            menuSelectInput.Header = App.Language.Localize ("MENU:MenuSelectInputSource");
            menuSelectInput.Classes.Add ("item");

            foreach (ControlType.Values v in Enum.GetValues (typeof (ControlType.Values))) {
                MenuItem mi = new MenuItem ();
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

        public void SetColorScheme (Color.Schemes scheme) {
            colorScheme = scheme;
            UpdateInterface ();
        }

        private void UpdateInterface () {
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
                    menuZeroTransducer.IsEnabled = false;
                    break;

                case ControlType.Values.ABP:
                case ControlType.Values.CVP:
                case ControlType.Values.IAP:
                case ControlType.Values.ICP:
                case ControlType.Values.PA:
                    menuZeroTransducer.IsEnabled = true;
                    break;
            }
        }

        public void UpdateVitals () {
            if (App.Patient == null)
                return;

            TextBlock lblLine1 = this.FindControl<TextBlock> ("lblLine1");
            TextBlock lblLine2 = this.FindControl<TextBlock> ("lblLine2");
            TextBlock lblLine3 = this.FindControl<TextBlock> ("lblLine3");

            switch (controlType.Value) {
                default:
                case ControlType.Values.ECG:
                    lblLine1.Text = String.Format ("{0:0}", App.Patient.MeasureHR_ECG (
                        Strip.DefaultLength, Strip.DefaultLength * Strip.DefaultBufferLength));
                    break;

                case ControlType.Values.T:
                    lblLine1.Text = String.Format ("{0:0.0}", App.Patient.T);
                    break;

                case ControlType.Values.SPO2:
                    lblLine1.Text = String.Format ("{0:0}", II.Math.RandomPercentRange (App.Patient.SPO2, 0.01f));
                    lblLine2.Text = String.Format ("@ {0:0}", App.Patient.MeasureHR_SPO2 (
                        Strip.DefaultLength, Strip.DefaultLength * Strip.DefaultBufferLength));
                    break;

                case ControlType.Values.RR:
                    lblLine1.Text = String.Format ("{0:0}", App.Patient.MeasureRR (
                        Strip.DefaultLength * Strip.DefaultRespiratoryCoefficient, Strip.DefaultLength * Strip.DefaultBufferLength));
                    break;

                case ControlType.Values.ETCO2:
                    lblLine1.Text = String.Format ("{0:0}", II.Math.RandomPercentRange (App.Patient.ETCO2, 0.02f));
                    lblLine2.Text = String.Format ("@ {0:0}", App.Patient.MeasureRR (
                        Strip.DefaultLength * Strip.DefaultRespiratoryCoefficient, Strip.DefaultLength * Strip.DefaultBufferLength));
                    break;

                case ControlType.Values.NIBP:
                    lblLine1.Text = String.Format ("{0:0}", App.Patient.NSBP);
                    lblLine2.Text = String.Format ("/ {0:0}", App.Patient.NDBP);
                    lblLine3.Text = String.Format ("({0:0})", App.Patient.NMAP);
                    break;

                case ControlType.Values.ABP:
                    if (App.Patient.TransducerZeroed_ABP) {
                        lblLine1.Text = String.Format ("{0:0}", II.Math.RandomPercentRange (App.Patient.ASBP, 0.02f));
                        lblLine2.Text = String.Format ("/ {0:0}", II.Math.RandomPercentRange (
                            (App.Patient.IABP_Active ? App.Patient.IABP_DBP : App.Patient.ADBP), 0.02f));
                        lblLine3.Text = String.Format ("({0:0})", II.Math.RandomPercentRange (App.Patient.AMAP, 0.02f));
                    } else {
                        lblLine1.Text = Utility.WrapString (App.Language.Localize ("NUMERIC:ZeroTransducer"));
                        lblLine2.Text = "";
                        lblLine3.Text = "";
                    }
                    break;

                case ControlType.Values.CVP:
                    if (App.Patient.TransducerZeroed_CVP)
                        lblLine1.Text = String.Format ("{0:0}", II.Math.RandomPercentRange (App.Patient.CVP, 0.02f));
                    else
                        lblLine1.Text = App.Language.Localize ("NUMERIC:ZeroTransducer");
                    break;

                case ControlType.Values.CO:
                    lblLine1.Text = String.Format ("{0:0.0}", App.Patient.CO);
                    break;

                case ControlType.Values.PA:
                    if (App.Patient.TransducerZeroed_PA) {
                        lblLine1.Text = String.Format ("{0:0}", II.Math.RandomPercentRange (App.Patient.PSP, 0.02f));
                        lblLine2.Text = String.Format ("/ {0:0}", II.Math.RandomPercentRange (App.Patient.PDP, 0.02f));
                        lblLine3.Text = String.Format ("({0:0})", II.Math.RandomPercentRange (App.Patient.PMP, 0.02f));
                    } else {
                        lblLine1.Text = App.Language.Localize ("NUMERIC:ZeroTransducer");
                        lblLine2.Text = "";
                        lblLine3.Text = "";
                    }
                    break;

                case ControlType.Values.ICP:
                    if (App.Patient.TransducerZeroed_ICP) {
                        lblLine1.Text = String.Format ("{0:0}", II.Math.RandomPercentRange (App.Patient.ICP, 0.02f));
                        lblLine2.Text = String.Format ("({0:0})", Patient.CalculateCPP (App.Patient.ICP, App.Patient.AMAP));
                    } else {
                        lblLine1.Text = App.Language.Localize ("NUMERIC:ZeroTransducer");
                        lblLine2.Text = "";
                    }
                    break;

                case ControlType.Values.IAP:
                    if (App.Patient.TransducerZeroed_IAP)
                        lblLine1.Text = String.Format ("{0:0}", II.Math.RandomPercentRange (App.Patient.IAP, 0.02f));
                    else
                        lblLine1.Text = App.Language.Localize ("NUMERIC:ZeroTransducer");
                    break;
            }
        }

        private void MenuZeroTransducer_Click (object? sender, RoutedEventArgs e) {
            switch (controlType.Value) {
                case ControlType.Values.ABP: App.Patient.TransducerZeroed_ABP = true; return;
                case ControlType.Values.CVP: App.Patient.TransducerZeroed_CVP = true; return;
                case ControlType.Values.PA: App.Patient.TransducerZeroed_PA = true; return;
                case ControlType.Values.ICP: App.Patient.TransducerZeroed_ICP = true; return;
                case ControlType.Values.IAP: App.Patient.TransducerZeroed_IAP = true; return;
            }
        }

        private void MenuAddNumeric_Click (object? sender, RoutedEventArgs e)
            => App.Device_Monitor.AddNumeric ();

        private void MenuRemoveNumeric_Click (object? sender, RoutedEventArgs e)
            => App.Device_Monitor.RemoveNumeric (this);

        private void MenuSelectInputSource (object? sender, RoutedEventArgs e) {
            ControlType.Values selectedValue;
            if (!Enum.TryParse<ControlType.Values> (((MenuItem)sender).Name, out selectedValue))
                return;

            controlType.Value = selectedValue;

            UpdateInterface ();
            UpdateVitals ();
        }
    }
}