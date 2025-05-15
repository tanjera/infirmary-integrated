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

    public partial class IABPNumeric : DeviceNumeric {
        public ControlTypes? ControlType;

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

        public IABPNumeric (App? app, DeviceIABP parent, ControlTypes.Values v, Color.Schemes cs) : base (app) {
            InitializeComponent ();

            DeviceParent = parent;
            ControlType = new ControlTypes (v);
            ColorScheme = cs;

            UpdateInterface ();
        }

        private void InitializeComponent () {
            AvaloniaXamlLoader.Load (this);
        }

        public void SetColorScheme (Color.Schemes scheme) {
            ColorScheme = scheme;
            UpdateInterface ();
        }

        private void UpdateInterface () {
            Dispatcher.UIThread.InvokeAsync (() => {
                Border borderNumeric = this.GetControl<Border> ("borderNumeric");
                TextBlock lblNumType = this.GetControl<TextBlock> ("lblNumType");
                TextBlock lblLine1 = this.GetControl<TextBlock> ("lblLine1");
                TextBlock lblLine2 = this.GetControl<TextBlock> ("lblLine2");
                TextBlock lblLine3 = this.GetControl<TextBlock> ("lblLine3");

                borderNumeric.BorderBrush = Color.GetLead (ControlType?.GetLead_Color, ColorScheme);

                lblNumType.Foreground = Color.GetLead (ControlType?.GetLead_Color, ColorScheme);
                lblLine1.Foreground = Color.GetLead (ControlType?.GetLead_Color, ColorScheme);
                lblLine2.Foreground = Color.GetLead (ControlType?.GetLead_Color, ColorScheme);
                lblLine3.Foreground = Color.GetLead (ControlType?.GetLead_Color, ColorScheme);

                lblLine1.IsVisible = true;
                lblLine2.IsVisible = true;
                lblLine3.IsVisible = true;

                lblNumType.Text = Instance?.Language.Localize (ControlTypes.LookupString (ControlType?.Value ?? ControlTypes.Values.ECG));

                /* Set lines to be visible/hidden as appropriate */
                switch (ControlType?.Value) {
                    default:
                    case ControlTypes.Values.ABP:
                    case ControlTypes.Values.IABP_AP:
                        lblLine1.FontSize = 30;
                        lblLine2.FontSize = 30;
                        lblLine3.FontSize = 20;
                        break;

                    case ControlTypes.Values.ECG:
                        lblLine1.FontSize = 30;
                        lblLine2.IsVisible = false;
                        lblLine3.IsVisible = false;
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
                    lblLine1.Text = String.Format ("{0:0}", Instance?.Physiology.MeasureHR_ECG (
                        Strip.DefaultLength, Strip.DefaultLength * Strip.DefaultBufferLength));
                    break;

                case ControlTypes.Values.ABP:
                    if (Instance.Physiology.TransducerZeroed_ABP) {
                        lblLine1.Text = String.Format ("{0:0}", II.Math.RandomPercentRange (Instance.Physiology.ASBP, 0.02f));
                        lblLine2.Text = String.Format ("/ {0:0}", II.Math.RandomPercentRange (
                            (!Instance?.Device_IABP?.Running ?? false
                            ? Instance?.Physiology?.ADBP ?? 0
                            : Instance?.Physiology?.IABP_DBP ?? 0)
                            , 0.02f));

                        // IABP shows MAP calculated by IABP!! Different from how monitors calculate MAP...
                        lblLine3.Text = String.Format ("({0:0})", II.Math.RandomPercentRange (Instance?.Physiology?.IABP_MAP ?? 0, 0.02f));
                    } else {
                        lblLine1.Text = Utility.WrapString (Instance?.Language.Localize ("NUMERIC:ZeroTransducer") ?? "");
                        lblLine2.Text = "";
                        lblLine3.Text = "";
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

        public override void OnTick_Alarm (object? sender, EventArgs e) {
            AlarmIterator = !AlarmIterator;
            _ = AlarmTimer?.ResetStart (1000);

            switch (ControlType?.Value) {
                default: break;

                case ControlTypes.Values.IABP_AP:
                    if ((Instance?.Device_IABP?.Running ?? false)
                            && Instance?.Physiology?.IABP_AP < Instance?.Device_IABP?.AugmentationAlarm) {
                        AlarmLine1 = true;
                        this.GetControl<TextBlock> ("lblLine1").Foreground = (AlarmIterator ?? false) ? Brushes.Red : Brushes.SkyBlue;
                    } else {
                        AlarmLine1 = false;
                    }
                    break;
            }
        }
    }
}