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
using Avalonia.Threading;

using II;
using II.Localization;
using II.Rhythm;

namespace IISIM.Controls {

    public partial class DefibNumeric : DeviceNumeric {
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

        public DefibNumeric (App? app, DeviceDefib parent, ControlTypes.Values v, Color.Schemes cs) : base (app) {
            InitializeComponent ();

            DeviceParent = parent;
            Instance = ((DeviceDefib)DeviceParent).Instance;
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
            List<object> menuitemsContext = new ();

            this.FindControl<Grid> ("layoutGrid").ContextMenu = contextMenu;
            this.FindControl<TextBlock> ("lblNumType").ContextMenu = contextMenu;
            this.FindControl<TextBlock> ("lblLine1").ContextMenu = contextMenu;
            this.FindControl<TextBlock> ("lblLine2").ContextMenu = contextMenu;
            this.FindControl<TextBlock> ("lblLine3").ContextMenu = contextMenu;
            this.FindControl<Viewbox> ("vbLine1").ContextMenu = contextMenu;
            this.FindControl<Viewbox> ("vbLine2").ContextMenu = contextMenu;
            this.FindControl<Viewbox> ("vbLine3").ContextMenu = contextMenu;

            uiMenuZeroTransducer = new MenuItem ();
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

        public void SetColorScheme (Color.Schemes scheme) {
            ColorScheme = scheme;
            UpdateInterface ();
        }

        private void UpdateInterface ()
            => UpdateInterface (this, new EventArgs ());

        private void UpdateInterface (object sender, EventArgs e) {
            Dispatcher.UIThread.InvokeAsync (() => {
                Border borderNumeric = this.FindControl<Border> ("borderNumeric");
                TextBlock lblNumType = this.FindControl<TextBlock> ("lblNumType");
                TextBlock lblLine1 = this.FindControl<TextBlock> ("lblLine1");
                TextBlock lblLine2 = this.FindControl<TextBlock> ("lblLine2");
                TextBlock lblLine3 = this.FindControl<TextBlock> ("lblLine3");

                borderNumeric.BorderBrush = Color.GetLead (ControlType?.GetLead_Color, ColorScheme);

                lblNumType.Foreground = Color.GetLead (ControlType?.GetLead_Color, ColorScheme);
                lblLine1.Foreground = Color.GetLead (ControlType?.GetLead_Color, ColorScheme);
                lblLine2.Foreground = Color.GetLead (ControlType?.GetLead_Color, ColorScheme);
                lblLine3.Foreground = Color.GetLead (ControlType?.GetLead_Color, ColorScheme);

                lblLine1.IsVisible = true;
                lblLine2.IsVisible = true;
                lblLine3.IsVisible = true;

                lblNumType.Text = Instance?.Language.Localize (ControlTypes.LookupString (ControlType.Value));

                /* Set lines to be visible/hidden as appropriate */
                switch (ControlType?.Value) {
                    default:
                    case ControlTypes.Values.NIBP:
                    case ControlTypes.Values.ABP:
                    case ControlTypes.Values.PA:
                    case ControlTypes.Values.DEFIB:
                        break;

                    case ControlTypes.Values.ECG:
                    case ControlTypes.Values.T:
                    case ControlTypes.Values.RR:
                    case ControlTypes.Values.CVP:
                        lblLine3.IsVisible = false;
                        break;

                    case ControlTypes.Values.SPO2:
                    case ControlTypes.Values.ETCO2:
                        lblLine2.IsVisible = false;
                        lblLine3.IsVisible = false;
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

        public void UpdateVitals () {
            if (Instance?.Physiology == null)
                return;

            TextBlock lblLine1 = this.FindControl<TextBlock> ("lblLine1");
            TextBlock lblLine2 = this.FindControl<TextBlock> ("lblLine2");
            TextBlock lblLine3 = this.FindControl<TextBlock> ("lblLine3");

            switch (ControlType?.Value) {
                default:
                case ControlTypes.Values.ECG:
                    lblLine1.Text = String.Format ("{0:0}", Instance.Physiology.MeasureHR_ECG (
                        Strip.DefaultLength, Strip.DefaultLength * Strip.DefaultBufferLength));
                    break;

                case ControlTypes.Values.T:
                    lblLine1.Text = String.Format ("{0:0.0}", Instance.Physiology.T);
                    break;

                case ControlTypes.Values.SPO2:
                    lblLine1.Text = String.Format ("{0:0}", II.Math.RandomPercentRange (Instance.Physiology.SPO2, 0.01f));
                    lblLine2.Text = String.Format ("@ {0:0}", Instance.Physiology.MeasureHR_SPO2 (
                        Strip.DefaultLength, Strip.DefaultLength * Strip.DefaultBufferLength));
                    break;

                case ControlTypes.Values.RR:
                    lblLine1.Text = String.Format ("{0:0}", Instance.Physiology.MeasureRR (
                        Strip.DefaultLength * Strip.DefaultRespiratoryCoefficient, Strip.DefaultLength * Strip.DefaultBufferLength));
                    break;

                case ControlTypes.Values.ETCO2:
                    lblLine1.Text = String.Format ("{0:0}", II.Math.RandomPercentRange (Instance.Physiology.ETCO2, 0.02f));
                    lblLine2.Text = String.Format ("@ {0:0}", Instance.Physiology.MeasureRR (
                        Strip.DefaultLength * Strip.DefaultRespiratoryCoefficient, Strip.DefaultLength * Strip.DefaultBufferLength));
                    break;

                case ControlTypes.Values.NIBP:
                    lblLine1.Text = String.Format ("{0:0}", Instance.Physiology.NSBP);
                    lblLine2.Text = String.Format ("/ {0:0}", Instance.Physiology.NDBP);
                    lblLine3.Text = String.Format ("({0:0})", Instance.Physiology.NMAP);
                    break;

                case ControlTypes.Values.ABP:
                    if (Instance.Physiology.TransducerZeroed_ABP) {
                        lblLine1.Text = String.Format ("{0:0}", II.Math.RandomPercentRange (Instance.Physiology.ASBP, 0.02f));
                        lblLine2.Text = String.Format ("/ {0:0}", II.Math.RandomPercentRange (
                            (Instance.Physiology.IABP_Active ? Instance.Physiology.IABP_DBP : Instance.Physiology.ADBP), 0.02f));
                        lblLine3.Text = String.Format ("({0:0})", II.Math.RandomPercentRange (Instance.Physiology.AMAP, 0.02f));
                    } else {
                        lblLine1.Text = Utility.WrapString (Instance.Language.Localize ("NUMERIC:ZeroTransducer"));
                        lblLine2.Text = "";
                        lblLine3.Text = "";
                    }
                    break;

                case ControlTypes.Values.CVP:
                    if (Instance.Physiology.TransducerZeroed_CVP)
                        lblLine1.Text = String.Format ("{0:0}", II.Math.RandomPercentRange (Instance.Physiology.CVP, 0.02f));
                    else
                        lblLine1.Text = Instance.Language.Localize ("NUMERIC:ZeroTransducer");
                    break;

                case ControlTypes.Values.PA:
                    if (Instance.Physiology.TransducerZeroed_PA) {
                        lblLine1.Text = String.Format ("{0:0}", II.Math.RandomPercentRange (Instance.Physiology.PSP, 0.02f));
                        lblLine2.Text = String.Format ("/ {0:0}", II.Math.RandomPercentRange (Instance.Physiology.PDP, 0.02f));
                        lblLine3.Text = String.Format ("({0:0})", II.Math.RandomPercentRange (Instance.Physiology.PMP, 0.02f));
                    } else {
                        lblLine1.Text = Instance.Language.Localize ("NUMERIC:ZeroTransducer");
                        lblLine2.Text = "";
                        lblLine3.Text = "";
                    }
                    break;

                case ControlTypes.Values.DEFIB:
                    if (DeviceParent is not null && DeviceParent is DeviceDefib device) {
                        switch (device.Mode) {
                            default:
                            case DeviceDefib.Modes.DEFIB:

                                lblLine1.Text = Instance.Language.Localize ("DEFIB:Defibrillation");
                                lblLine2.Text = String.Format ("{0:0} {1}", device.DefibEnergy, Instance.Language.Localize ("DEFIB:Joules"));
                                if (device.Charging)
                                    lblLine3.Text = Instance.Language.Localize ("DEFIB:Charging");
                                else if (device.Charged)
                                    lblLine3.Text = Instance.Language.Localize ("DEFIB:Charged");
                                else if (device.Analyzed) {
                                    switch (Instance.Physiology.Cardiac_Rhythm.Value) {
                                        default:
                                            lblLine3.Text = Instance.Language.Localize ("DEFIB:NoShockAdvised");
                                            break;

                                        case Cardiac_Rhythms.Values.Ventricular_Fibrillation_Coarse:
                                        case Cardiac_Rhythms.Values.Ventricular_Fibrillation_Fine:
                                        case Cardiac_Rhythms.Values.Ventricular_Tachycardia_Monomorphic_Pulsed:
                                        case Cardiac_Rhythms.Values.Ventricular_Tachycardia_Monomorphic_Pulseless:
                                        case Cardiac_Rhythms.Values.Ventricular_Tachycardia_Polymorphic:
                                            lblLine3.Text = Instance.Language.Localize ("DEFIB:ShockAdvised");
                                            break;
                                    }
                                } else
                                    lblLine3.Text = "";
                                break;

                            case DeviceDefib.Modes.SYNC:
                                lblLine1.Text = Instance.Language.Localize ("DEFIB:Synchronized");
                                lblLine2.Text = String.Format ("{0:0} {1}", device.DefibEnergy, Instance.Language.Localize ("DEFIB:Joules"));
                                lblLine3.Text = device.Charged ? Instance.Language.Localize ("DEFIB:Charged") : "";
                                break;

                            case DeviceDefib.Modes.PACER:
                                lblLine1.Text = Instance.Language.Localize ("DEFIB:Pacing");
                                lblLine2.Text = String.Format ("{0:0} {1}", device.PacerEnergy, Instance.Language.Localize ("DEFIB:Milliamps"));
                                lblLine3.Text = String.Format ("{0}: {1:0}", Instance.Language.Localize ("DEFIB:Rate"), device.PacerRate);
                                break;
                        }
                    }
                    break;
            }
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