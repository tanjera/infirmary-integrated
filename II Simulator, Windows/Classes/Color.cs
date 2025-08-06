using II;
using II.Rhythm;
using II.Waveform;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
        public static Brush [] [] Background = {     // Indexed by [Devices] [Schemes]
            [Brushes.White, Brushes.Black, Brushes.White], // Monitor
            [Brushes.White, Brushes.Black, Brushes.White], // Defib
            [Brushes.White, Brushes.Black, Brushes.White], // ECG
            [Brushes.White, Brushes.Black, Brushes.White], // IABP
            [Brushes.White, Brushes.Black, Brushes.White] // EFM
        };

        // Color lookup table for Numerics and Tracings for all devices and color schemes
        public static Brush [] [] Lead = new Brush [] [] { // Indexed by [Lead] [Schemes]
            [Brushes.Black, Brushes.Green, Brushes.Black], // ECG

            [Brushes.Black, Brushes.LightGray, Brushes.Black], // T
            [Brushes.Black, Brushes.Salmon, Brushes.Black], // RR
            [Brushes.Black, Brushes.Aqua, Brushes.Black], // ETCO2
            [Brushes.Black, Brushes.Orange, Brushes.Black], // SPO2

            [Brushes.Black, Brushes.White, Brushes.Black], // NIBP
            [Brushes.Black, Brushes.Red, Brushes.Black], // ABP

            [Brushes.Black, Brushes.Blue, Brushes.Black], // CVP
            [Brushes.Black, Brushes.Brown, Brushes.Black], // CO

            [Brushes.Black, Brushes.Yellow, Brushes.Black], // PA
            [Brushes.Black, Brushes.Khaki, Brushes.Black], // ICP
            [Brushes.Black, Brushes.Aquamarine, Brushes.Black], // IAP
            [Brushes.Black, Brushes.SkyBlue, Brushes.Black], // IABP

            [Brushes.Black, Brushes.Turquoise, Brushes.Black], // DEFIB
            [Brushes.Black, Brushes.White, Brushes.Black], // EFM
        };

        public static Brush GetBackground (Devices? device, Schemes? scheme) {
            return Background [device?.GetHashCode () ?? 0] [scheme?.GetHashCode () ?? 0];
        }

        public static Brush GetLead (II.Lead.Values? lead, Schemes? scheme) {
            return GetLead (SwitchLead (lead ?? II.Lead.Values.ECG_I), scheme ?? Schemes.Light);
        }

        public static Brush GetLead (Leads? lead, Schemes? scheme) {
            return Lead [lead?.GetHashCode () ?? 0] [scheme?.GetHashCode () ?? 0];
        }

        public static Brush GetAlarm (Leads? lead, Schemes? scheme) {
            return Lead [lead?.GetHashCode () ?? 0] [scheme?.GetHashCode () ?? 0].Equals (Brushes.Red) ? Brushes.Yellow : Brushes.Red;
        }

        public static Leads SwitchLead (II.Lead.Values? lead) => lead switch {
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