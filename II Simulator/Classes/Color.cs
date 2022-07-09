using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Media;

namespace IISIM {

    public class Color {

        public enum Schemes {
            Light,
            Dark,
            Grid
        }

        public enum Devices {
            DeviceMonitor,
            DeviceDefib,
            DeviceECG,
            DeviceIABP,
            DeviceEFM
        }

        public enum Leads {
            ECG,
            T,
            RR,
            ETCO2,
            SPO2,
            NIBP,
            ABP,
            CVP,
            CO,
            PA,
            ICP,
            IAP,
            IABP,
            DEFIB,
            EFM
        }

        // Color lookup table for Background for all devices and color schemes
        public static IBrush [] [] Background = new IBrush [] [] {     // Indexed by [Devices] [Schemes]
            new IBrush[] { Brushes.White, Brushes.Black, Brushes.White }, // Monitor
            new IBrush[] { Brushes.White, Brushes.Black, Brushes.White }, // Defib
            new IBrush[] { Brushes.White, Brushes.Black, Brushes.White }, // ECG
            new IBrush[] { Brushes.White, Brushes.Black, Brushes.White }, // IABP
            new IBrush[] { Brushes.White, Brushes.Black, Brushes.White } // EFM
        };

        // Color lookup table for Numerics and Tracings for all devices and color schemes
        public static IBrush [] [] Lead = new IBrush [] [] { // Indexed by [Lead] [Schemes]
            new IBrush[] { Brushes.Black, Brushes.Green, Brushes.Black}, // ECG

            new IBrush[] { Brushes.Black, Brushes.LightGray, Brushes.Black}, // T
            new IBrush[] { Brushes.Black, Brushes.Salmon, Brushes.Black}, // RR
            new IBrush[] { Brushes.Black, Brushes.Aqua, Brushes.Black}, // ETCO2
            new IBrush[] { Brushes.Black, Brushes.Orange, Brushes.Black}, // SPO2

            new IBrush[] { Brushes.Black, Brushes.White, Brushes.Black}, // NIBP
            new IBrush[] { Brushes.Black, Brushes.Red, Brushes.Black}, // ABP

            new IBrush[] { Brushes.Black, Brushes.Blue, Brushes.Black}, // CVP
            new IBrush[] { Brushes.Black, Brushes.Brown, Brushes.Black}, // CO

            new IBrush[] { Brushes.Black, Brushes.Yellow, Brushes.Black}, // PA
            new IBrush[] { Brushes.Black, Brushes.Khaki, Brushes.Black}, // ICP
            new IBrush[] { Brushes.Black, Brushes.Aquamarine, Brushes.Black}, // IAP
            new IBrush[] { Brushes.Black, Brushes.SkyBlue, Brushes.Black}, // IABP

            new IBrush[] { Brushes.Black, Brushes.Turquoise, Brushes.Black}, // DEFIB
            new IBrush[] { Brushes.Black, Brushes.White, Brushes.Black}, // EFM
        };

        public static IBrush GetBackground (Devices device, Schemes scheme) {
            return Background [device.GetHashCode ()] [scheme.GetHashCode ()];
        }

        public static IBrush GetLead (II.Lead.Values lead, Schemes scheme) {
            return GetLead (SwitchLead (lead), scheme);
        }

        public static IBrush GetLead (Leads lead, Schemes scheme) {
            return Lead [lead.GetHashCode ()] [scheme.GetHashCode ()];
        }

        public static IBrush GetAlarm (Leads lead, Schemes scheme) {
            return Lead [lead.GetHashCode ()] [scheme.GetHashCode ()].Equals (Brushes.Red) ? Brushes.Yellow : Brushes.Red;
        }

        public static Leads SwitchLead (II.Lead.Values lead) => lead switch {
            II.Lead.Values.ECG_I => Leads.ECG,
            II.Lead.Values.ECG_II => Leads.ECG,
            II.Lead.Values.ECG_III => Leads.ECG,
            II.Lead.Values.ECG_AVF => Leads.ECG,
            II.Lead.Values.ECG_AVL => Leads.ECG,
            II.Lead.Values.ECG_AVR => Leads.ECG,
            II.Lead.Values.ECG_V1 => Leads.ECG,
            II.Lead.Values.ECG_V2 => Leads.ECG,
            II.Lead.Values.ECG_V3 => Leads.ECG,
            II.Lead.Values.ECG_V4 => Leads.ECG,
            II.Lead.Values.ECG_V5 => Leads.ECG,
            II.Lead.Values.ECG_V6 => Leads.ECG,
            II.Lead.Values.SPO2 => Leads.SPO2,
            II.Lead.Values.RR => Leads.RR,
            II.Lead.Values.ETCO2 => Leads.ETCO2,
            II.Lead.Values.CVP => Leads.CVP,
            II.Lead.Values.ABP => Leads.ABP,
            II.Lead.Values.PA => Leads.PA,
            II.Lead.Values.ICP => Leads.ICP,
            II.Lead.Values.IAP => Leads.IAP,
            II.Lead.Values.IABP => Leads.IABP,
            II.Lead.Values.FHR => Leads.EFM,
            II.Lead.Values.TOCO => Leads.EFM,
            _ => Leads.ECG
        };
    }
}