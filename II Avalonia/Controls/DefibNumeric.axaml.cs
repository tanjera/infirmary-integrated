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

    public partial class DefibNumeric : UserControl {
        /* Properties for applying DPI scaling options */
        public double UIScale { get { return App.Settings.UIScale; } }
        public int FontScale { get { return (int)(14 * App.Settings.UIScale); } }

        public DeviceDefib deviceParent;
        public ControlType controlType;

        private MenuItem menuZeroTransducer;

        public class ControlType {
            public Values Value;

            public ControlType (Values v) {
                Value = v;
            }

            public enum Values {
                ECG, T, RR, ETCO2,
                SPO2, NIBP, ABP, CVP,
                PA, DEFIB
            }

            public static string LookupString (Values value) {
                return String.Format ("NUMERIC:{0}", Enum.GetValues (typeof (Values)).GetValue ((int)value).ToString ());
            }

            public IBrush Color { get { return Coloring [(int)Value]; } }

            public static List<string> MenuItem_Formats {
                get {
                    List<string> o = new List<string> ();
                    foreach (Values v in Enum.GetValues (typeof (Values)))
                        o.Add (String.Format ("{0}: {1}", v.ToString (), LookupString (v)));
                    return o;
                }
            }

            public static IBrush [] Coloring = new IBrush [] {
                Brushes.Green,
                Brushes.LightGray,
                Brushes.Salmon,
                Brushes.Aqua,
                Brushes.Orange,
                Brushes.White,
                Brushes.Red,
                Brushes.Blue,
                Brushes.Yellow,
                Brushes.Turquoise
            };
        }

        public DefibNumeric () {
            InitializeComponent ();
        }

        public DefibNumeric (DeviceDefib parent, ControlType.Values v) {
            InitializeComponent ();

            InitInterface ();

            deviceParent = parent;
            controlType = new ControlType (v);

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

        private void UpdateInterface () {
            Border borderNumeric = this.FindControl<Border> ("borderNumeric");
            TextBlock lblNumType = this.FindControl<TextBlock> ("lblNumType");
            TextBlock lblLine1 = this.FindControl<TextBlock> ("lblLine1");
            TextBlock lblLine2 = this.FindControl<TextBlock> ("lblLine2");
            TextBlock lblLine3 = this.FindControl<TextBlock> ("lblLine3");

            borderNumeric.BorderBrush = controlType.Color;

            lblNumType.Foreground = controlType.Color;
            lblLine1.Foreground = controlType.Color;
            lblLine2.Foreground = controlType.Color;
            lblLine3.Foreground = controlType.Color;

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
                case ControlType.Values.DEFIB:
                    break;

                case ControlType.Values.ECG:
                case ControlType.Values.T:
                case ControlType.Values.RR:
                case ControlType.Values.CVP:
                    lblLine3.IsVisible = false;
                    break;

                case ControlType.Values.SPO2:
                case ControlType.Values.ETCO2:
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

                case ControlType.Values.DEFIB:

                    switch (deviceParent.Mode) {
                        default:
                        case DeviceDefib.Modes.DEFIB:

                            lblLine1.Text = App.Language.Localize ("DEFIB:Defibrillation");
                            lblLine2.Text = String.Format ("{0:0} {1}", deviceParent.Energy, App.Language.Localize ("DEFIB:Joules"));
                            if (deviceParent.Charging)
                                lblLine3.Text = App.Language.Localize ("DEFIB:Charging");
                            else if (deviceParent.Charged)
                                lblLine3.Text = App.Language.Localize ("DEFIB:Charged");
                            else if (deviceParent.Analyzed) {
                                switch (App.Patient.Cardiac_Rhythm.Value) {
                                    default:
                                        lblLine3.Text = App.Language.Localize ("DEFIB:NoShockAdvised");
                                        break;

                                    case Cardiac_Rhythms.Values.Ventricular_Fibrillation_Coarse:
                                    case Cardiac_Rhythms.Values.Ventricular_Fibrillation_Fine:
                                    case Cardiac_Rhythms.Values.Ventricular_Tachycardia_Monomorphic_Pulsed:
                                    case Cardiac_Rhythms.Values.Ventricular_Tachycardia_Monomorphic_Pulseless:
                                    case Cardiac_Rhythms.Values.Ventricular_Tachycardia_Polymorphic:
                                        lblLine3.Text = App.Language.Localize ("DEFIB:ShockAdvised");
                                        break;
                                }
                            } else
                                lblLine3.Text = "";
                            break;

                        case DeviceDefib.Modes.SYNC:
                            lblLine1.Text = App.Language.Localize ("DEFIB:Synchronized");
                            lblLine2.Text = String.Format ("{0:0} {1}", deviceParent.Energy, App.Language.Localize ("DEFIB:Joules"));
                            lblLine3.Text = deviceParent.Charged ? App.Language.Localize ("DEFIB:Charged") : "";
                            break;

                        case DeviceDefib.Modes.PACER:
                            lblLine1.Text = App.Language.Localize ("DEFIB:Pacing");
                            lblLine2.Text = String.Format ("{0:0} {1}", deviceParent.PacerEnergy, App.Language.Localize ("DEFIB:Milliamps"));
                            lblLine3.Text = String.Format ("{0}: {1:0}", App.Language.Localize ("DEFIB:Rate"), deviceParent.PacerRate);
                            break;
                    }
                    break;
            }
        }

        private void MenuZeroTransducer_Click (object? sender, RoutedEventArgs e) {
            switch (controlType.Value) {
                case ControlType.Values.ABP: App.Patient.TransducerZeroed_ABP = true; return;
                case ControlType.Values.CVP: App.Patient.TransducerZeroed_CVP = true; return;
                case ControlType.Values.PA: App.Patient.TransducerZeroed_PA = true; return;
            }
        }

        private void MenuAddNumeric_Click (object? sender, RoutedEventArgs e)
            => App.Device_Defib.AddNumeric ();

        private void MenuRemoveNumeric_Click (object? sender, RoutedEventArgs e)
            => App.Device_Defib.RemoveNumeric (this);

        private void MenuSelectInputSource (object? sender, RoutedEventArgs e) {
            ControlType.Values selectedValue;
            if (!Enum.TryParse<ControlType.Values> ((sender as MenuItem)?.Name, out selectedValue))
                return;

            controlType.Value = selectedValue;

            UpdateInterface ();
            UpdateVitals ();
        }
    }
}