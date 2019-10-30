using II;
using II.Rhythm;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace II_Windows.Controls {

    /// <summary>
    /// Interaction logic for Numeric.xaml
    /// </summary>
    public partial class DefibNumeric : UserControl {
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

            public Brush Color { get { return Coloring [(int)Value]; } }

            public static List<string> MenuItem_Formats {
                get {
                    List<string> o = new List<string> ();
                    foreach (Values v in Enum.GetValues (typeof (Values)))
                        o.Add (String.Format ("{0}: {1}", v.ToString (), LookupString (v)));
                    return o;
                }
            }

            public static Brush [] Coloring = new Brush [] {
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

        public DefibNumeric (ControlType.Values v) {
            InitializeComponent ();

            InitInterface ();

            controlType = new ControlType (v);
            UpdateInterface ();
        }

        private void InitInterface () {

            // Context Menu (right-click menu!)
            ContextMenu contextMenu = new ContextMenu ();
            layoutGrid.ContextMenu = contextMenu;
            lblNumType.ContextMenu = contextMenu;
            lblLine1.ContextMenu = contextMenu;
            lblLine2.ContextMenu = contextMenu;
            lblLine3.ContextMenu = contextMenu;
            vbLine1.ContextMenu = contextMenu;
            vbLine2.ContextMenu = contextMenu;
            vbLine3.ContextMenu = contextMenu;

            menuZeroTransducer = new MenuItem ();
            menuZeroTransducer.Header = App.Language.Localize ("MENU:MenuZeroTransducer");
            menuZeroTransducer.Click += MenuZeroTransducer_Click;
            contextMenu.Items.Add (menuZeroTransducer);

            contextMenu.Items.Add (new Separator ());

            MenuItem menuAddNumeric = new MenuItem ();
            menuAddNumeric.Header = App.Language.Localize ("MENU:MenuAddNumeric");
            menuAddNumeric.Click += MenuAddNumeric_Click;
            contextMenu.Items.Add (menuAddNumeric);

            MenuItem menuRemoveNumeric = new MenuItem ();
            menuRemoveNumeric.Header = App.Language.Localize ("MENU:MenuRemoveNumeric");
            menuRemoveNumeric.Click += MenuRemoveNumeric_Click;
            contextMenu.Items.Add (menuRemoveNumeric);

            contextMenu.Items.Add (new Separator ());

            MenuItem menuSelectInput = new MenuItem ();
            menuSelectInput.Header = App.Language.Localize ("MENU:MenuSelectInputSource");

            foreach (ControlType.Values v in Enum.GetValues (typeof (ControlType.Values))) {
                MenuItem mi = new MenuItem ();
                mi.Header = App.Language.Localize (ControlType.LookupString (v));
                mi.Name = v.ToString ();
                mi.Click += MenuSelectInputSource;
                menuSelectInput.Items.Add (mi);
            }

            contextMenu.Items.Add (menuSelectInput);
        }

        private void UpdateInterface () {
            borderNumeric.BorderBrush = controlType.Color;

            lblNumType.Foreground = controlType.Color;
            lblLine1.Foreground = controlType.Color;
            lblLine2.Foreground = controlType.Color;
            lblLine3.Foreground = controlType.Color;

            lblLine1.Visibility = Visibility.Visible;
            lblLine2.Visibility = Visibility.Visible;
            lblLine3.Visibility = Visibility.Visible;

            lblNumType.Text = App.Language.Localize (ControlType.LookupString (controlType.Value));

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
                    lblLine2.Visibility = Visibility.Hidden;
                    lblLine3.Visibility = Visibility.Hidden;
                    break;

                case ControlType.Values.SPO2:
                case ControlType.Values.ETCO2:
                    lblLine3.Visibility = Visibility.Hidden;
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

        public void UpdateVitals (DeviceDefib Device) {
            if (App.Patient == null)
                return;

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

                    switch (Device.Mode) {
                        default:
                        case DeviceDefib.Modes.DEFIB:
                            lblLine1.Visibility = Visibility.Visible;
                            lblLine2.Visibility = Visibility.Visible;
                            lblLine3.Visibility = Visibility.Visible;

                            lblLine1.Text = App.Language.Localize ("DEFIB:Defibrillation");
                            lblLine2.Text = String.Format ("{0:0} {1}", Device.Energy, App.Language.Localize ("DEFIB:Joules"));
                            if (Device.Charging)
                                lblLine3.Text = App.Language.Localize ("DEFIB:Charging");
                            else if (Device.Charged)
                                lblLine3.Text = App.Language.Localize ("DEFIB:Charged");
                            else if (Device.Analyzed) {
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
                            lblLine1.Visibility = Visibility.Visible;
                            lblLine2.Visibility = Visibility.Visible;
                            lblLine3.Visibility = Visibility.Visible;

                            lblLine1.Text = App.Language.Localize ("DEFIB:Synchronized");
                            lblLine2.Text = String.Format ("{0:0} {1}", Device.Energy, App.Language.Localize ("DEFIB:Joules"));
                            lblLine3.Text = Device.Charged ? App.Language.Localize ("DEFIB:Charged") : "";
                            break;

                        case DeviceDefib.Modes.PACER:
                            lblLine1.Visibility = Visibility.Visible;
                            lblLine2.Visibility = Visibility.Visible;
                            lblLine3.Visibility = Visibility.Visible;

                            lblLine1.Text = App.Language.Localize ("DEFIB:Pacing");
                            lblLine2.Text = String.Format ("{0:0} {1}", Device.PacerEnergy, App.Language.Localize ("DEFIB:Milliamps"));
                            lblLine3.Text = String.Format ("{0}: {1:0}", App.Language.Localize ("DEFIB:Rate"), Device.PacerRate);
                            break;
                    }
                    break;
            }
        }

        private void MenuZeroTransducer_Click (object sender, RoutedEventArgs e) {
            switch (controlType.Value) {
                case ControlType.Values.ABP: App.Patient.TransducerZeroed_ABP = true; return;
                case ControlType.Values.CVP: App.Patient.TransducerZeroed_CVP = true; return;
                case ControlType.Values.PA: App.Patient.TransducerZeroed_PA = true; return;
            }
        }

        private void MenuAddNumeric_Click (object sender, RoutedEventArgs e)
            => App.Device_Defib.AddNumeric ();

        private void MenuRemoveNumeric_Click (object sender, RoutedEventArgs e)
            => App.Device_Defib.RemoveNumeric (this);

        private void MenuSelectInputSource (object sender, RoutedEventArgs e) {
            ControlType.Values selectedValue;
            if (!Enum.TryParse<ControlType.Values> (((MenuItem)sender).Name, out selectedValue))
                return;

            controlType.Value = selectedValue;

            UpdateInterface ();
            UpdateVitals (new DeviceDefib ());
        }
    }
}