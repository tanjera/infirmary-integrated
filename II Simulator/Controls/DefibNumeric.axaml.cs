using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;

using II;
using II.Localization;
using II.Rhythm;
using II.Settings;

namespace IISIM.Controls {

    public partial class DefibNumeric : DeviceNumeric {
        public ControlTypes? ControlType;

        private MenuItem? uiMenuZeroTransducer;

        private List<II.Settings.Device.Numeric>? Transducers_Zeroed {
            set { 
                if (Instance?.Scenario?.DeviceDefib is not null)
                    Instance.Scenario.DeviceDefib.Transducers_Zeroed = value ?? new List<II.Settings.Device.Numeric> (); 
            }
            get { return Instance?.Scenario?.DeviceDefib.Transducers_Zeroed; }
        }
        
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
            ContextMenu contextMenu = new ContextMenu ();
            
            // Note: children inherit the context menu (e.g. TextBlocks on the Grid)
            this.GetControl<Grid> ("layoutGrid").ContextMenu = contextMenu;

            uiMenuZeroTransducer = new();
            uiMenuZeroTransducer.Header = Instance?.Language.Localize ("MENU:MenuZeroTransducer");
            uiMenuZeroTransducer.Click += MenuZeroTransducer_Click;
            contextMenu.Items.Add (uiMenuZeroTransducer);
            contextMenu.Items.Add (new Separator ());

            MenuItem menuAddNumeric = new();
            menuAddNumeric.Header = Instance?.Language.Localize ("MENU:MenuAddNumeric");
            menuAddNumeric.Click += MenuAddNumeric_Click;
            contextMenu.Items.Add (menuAddNumeric);

            MenuItem menuRemoveNumeric = new();
            menuRemoveNumeric.Header = Instance?.Language.Localize ("MENU:MenuRemoveNumeric");
            menuRemoveNumeric.Click += MenuRemoveNumeric_Click;
            contextMenu.Items.Add (menuRemoveNumeric);
            contextMenu.Items.Add (new Separator ());
            
            MenuItem menuMoveLeft = new ();
            menuMoveLeft.Header = Instance?.Language.Localize ("MENU:MoveLeft");
            menuMoveLeft.Click += MenuMoveLeft_Click;
            contextMenu.Items.Add (menuMoveLeft);

            MenuItem menuMoveRight = new ();
            menuMoveRight.Header = Instance?.Language.Localize ("MENU:MoveRight");
            menuMoveRight.Click += MenuMoveRight_Click;
            contextMenu.Items.Add (menuMoveRight);
            contextMenu.Items.Add (new Separator ());
            
            MenuItem menuSelectInput = new();
            menuSelectInput.Header = Instance?.Language.Localize ("MENU:MenuSelectInputSource");
            contextMenu.Items.Add (menuSelectInput);

            foreach (ControlTypes.Values v in Enum.GetValues (typeof (ControlTypes.Values))) {
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

        private void UpdateInterface ()
            => UpdateInterface (this, new EventArgs ());

        private void UpdateInterface (object sender, EventArgs e) {
            Dispatcher.UIThread.InvokeAsync (() => {
                Border borderNumeric = this.GetControl<Border> ("borderNumeric");
                TextBlock lblNumType = this.GetControl<TextBlock> ("lblNumType");
                TextBlock lblLine1 = this.GetControl<TextBlock> ("lblLine1");
                TextBlock lblLine2 = this.GetControl<TextBlock> ("lblLine2");
                TextBlock lblLine3 = this.GetControl<TextBlock> ("lblLine3");

                if (ControlType?.Value == ControlTypes.Values.DEFIB && ColorScheme == Color.Schemes.Dark) {
                    // Defib numeric in dark more (colorful) gets specific colors based on status

                    // Default color scheme settings are still utilized as "default" state
                    IBrush statusColor = Color.GetLead (ControlType?.GetLead_Color, ColorScheme);

                    if (((DeviceDefib)DeviceParent).Mode == DeviceDefib.Modes.PACER) {
                        statusColor = Brushes.Orange;
                    } else if (((DeviceDefib)DeviceParent).Analyze != DeviceDefib.AnalyzeStates.Inactive) {
                        statusColor = ((DeviceDefib)DeviceParent).Analyze switch {
                            DeviceDefib.AnalyzeStates.Analyzing => Brushes.YellowGreen,
                            _ => statusColor
                        };
                    } else if (((DeviceDefib)DeviceParent).Charge != DeviceDefib.ChargeStates.Discharged) {
                        statusColor = ((DeviceDefib)DeviceParent).Charge switch {
                            DeviceDefib.ChargeStates.Charging => Brushes.Yellow,
                            DeviceDefib.ChargeStates.Charged => Brushes.Red,
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
                        lblLine1.FontSize = 30;
                        lblLine2.FontSize = 30;
                        lblLine3.FontSize = 20;
                        break;

                    case ControlTypes.Values.SPO2:
                    case ControlTypes.Values.ETCO2:
                        lblLine1.FontSize = 30;
                        lblLine2.FontSize = 20;
                        lblLine3.IsVisible = false;
                        break;

                    case ControlTypes.Values.ECG:
                    case ControlTypes.Values.T:
                    case ControlTypes.Values.RR:
                    case ControlTypes.Values.CVP:
                        lblLine1.FontSize = 30;
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

            TextBlock lblLine1 = this.GetControl<TextBlock> ("lblLine1");
            TextBlock lblLine2 = this.GetControl<TextBlock> ("lblLine2");
            TextBlock lblLine3 = this.GetControl<TextBlock> ("lblLine3");

            switch (ControlType?.Value) {
                default:
                case ControlTypes.Values.ECG:
                    lblLine1.Text = String.Format ("{0:0}", Instance.Physiology.MeasureHR_ECG (
                        Strip.DefaultLength * Strip.DefaultBufferLength));
                    break;

                case ControlTypes.Values.T:
                    lblLine1.Text = String.Format ("{0:0.0}", Instance.Physiology.T);
                    break;

                case ControlTypes.Values.SPO2:
                    lblLine1.Text = String.Format ("{0:0} %", II.Math.RandomPercentRange (Instance.Physiology.SPO2, 0.01f));
                    lblLine2.Text = String.Format ("@ {0:0}", Instance.Physiology.MeasureHR_SPO2 (
                        Strip.DefaultLength * Strip.DefaultBufferLength));
                    break;

                case ControlTypes.Values.RR:
                    lblLine1.Text = String.Format ("{0:0}", Instance.Physiology.MeasureRR (
                        Strip.DefaultLength * Strip.DefaultBufferLength));
                    break;

                case ControlTypes.Values.ETCO2:
                    lblLine1.Text = String.Format ("{0:0}", II.Math.RandomPercentRange (Instance.Physiology.ETCO2, 0.02f));
                    lblLine2.Text = String.Format ("@ {0:0}", Instance.Physiology.MeasureRR (
                        Strip.DefaultLength * Strip.DefaultBufferLength));
                    break;

                case ControlTypes.Values.NIBP:
                    lblLine1.Text = String.Format ("{0:0}", Instance.Physiology.NSBP);
                    lblLine2.Text = String.Format ("/ {0:0}", Instance.Physiology.NDBP);
                    lblLine3.Text = String.Format ("({0:0})", Instance.Physiology.NMAP);
                    break;

                case ControlTypes.Values.ABP:
                    if (Transducers_Zeroed?.Contains(Device.Numeric.ABP) ?? false) {
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
                    if (Transducers_Zeroed?.Contains(Device.Numeric.CVP) ?? false)
                        lblLine1.Text = String.Format ("{0:0}", II.Math.RandomPercentRange (Instance.Physiology.CVP, 0.02f));
                    else
                        lblLine1.Text = Instance.Language.Localize ("NUMERIC:ZeroTransducer");
                    break;

                case ControlTypes.Values.PA:
                    if (Transducers_Zeroed?.Contains(Device.Numeric.PA) ?? false) {
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

                                if (device.Analyze == DeviceDefib.AnalyzeStates.Analyzing) {
                                    lblLine3.Text = Instance.Language.Localize ("DEFIB:Analyzing");
                                } else if (device.Analyze == DeviceDefib.AnalyzeStates.Analyzed) {
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
                                } else {
                                    lblLine3.Text = device.Charge switch {
                                        DeviceDefib.ChargeStates.Charging => Instance.Language.Localize ("DEFIB:Charging"),
                                        DeviceDefib.ChargeStates.Charged => Instance.Language.Localize ("DEFIB:Charged"),
                                        _ => ""
                                    };
                                }

                                break;

                            case DeviceDefib.Modes.SYNC:
                                lblLine1.Text = Instance.Language.Localize ("DEFIB:Synchronized");
                                lblLine2.Text = String.Format ("{0:0} {1}", device.DefibEnergy, Instance.Language.Localize ("DEFIB:Joules"));
                                lblLine3.Text = device.Charge switch {
                                    DeviceDefib.ChargeStates.Charging => Instance.Language.Localize ("DEFIB:Charging"),
                                    DeviceDefib.ChargeStates.Charged => Instance.Language.Localize ("DEFIB:Charged"),
                                    _ => ""
                                };
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
                }
            }
        }

        private void MenuAddNumeric_Click (object? sender, RoutedEventArgs e)
            => Instance?.Device_Defib?.AddNumeric ();

        private void MenuRemoveNumeric_Click (object? sender, RoutedEventArgs e)
            => Instance?.Device_Defib?.RemoveNumeric (this);
        
        private void MenuMoveLeft_Click (object? sender, RoutedEventArgs e)
            => Instance?.Device_Defib?.MoveNumeric_Left (this);
        
        private void MenuMoveRight_Click (object? sender, RoutedEventArgs e) 
            => Instance?.Device_Defib?.MoveNumeric_Right (this);

        
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