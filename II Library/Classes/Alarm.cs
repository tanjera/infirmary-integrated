/* Alarm.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera), (c) 2022
 *
 * Handling and storage of Devices' Alarm parameters
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace II {

    public class Alarm {

        public enum Parameters {
            HR,
            RR,
            SPO2,
            ETCO2,
            CVP,
            ICP,
            IAP,
            NSBP, NDBP, NMAP,
            ASBP, ADBP, AMAP,
            PSP, PDP, PMP,
        }

        public enum Priorities {
            Low,
            Medium,
            High
        }

        public Parameters? Parameter { get; set; }

        public bool? Alarming { get; set; }
        public bool? Enabled { get; set; }

        public int? High { get; set; }
        public int? Low { get; set; }

        public Priorities? Priority { get; set; }

        public static readonly Alarm [] DefaultListing_Adult =  {
            new Alarm(Alarm.Parameters.HR,true, 50, 130, Alarm.Priorities.High),
            new Alarm(Alarm.Parameters.RR, true, 8, 24, Alarm.Priorities.Medium),
            new Alarm(Alarm.Parameters.SPO2,true, 92, null, Alarm.Priorities.Low),
            new Alarm(Alarm.Parameters.ETCO2,true, 30, 50, Alarm.Priorities.Medium),
            new Alarm(Alarm.Parameters.CVP,true, 0, 12, Alarm.Priorities.Low),
            new Alarm(Alarm.Parameters.ICP,true, null, 20, Alarm.Priorities.Medium),
            new Alarm(Alarm.Parameters.IAP,true, null, 20, Alarm.Priorities.Medium),

            new Alarm(Alarm.Parameters.NSBP,true, 90, 160, Alarm.Priorities.Medium),
            new Alarm(Alarm.Parameters.NDBP,true, 45, 100, Alarm.Priorities.Medium),
            new Alarm(Alarm.Parameters.NMAP, true, 65, 120, Alarm.Priorities.Medium),
            new Alarm(Alarm.Parameters.ASBP,true, 90, 160, Alarm.Priorities.Medium),
            new Alarm(Alarm.Parameters.ADBP,true, 45, 100, Alarm.Priorities.Medium),
            new Alarm(Alarm.Parameters.AMAP,true, 65, 120, Alarm.Priorities.Medium),
            new Alarm(Alarm.Parameters.PSP,true, 15, 40, Alarm.Priorities.Medium),
            new Alarm(Alarm.Parameters.PDP,true, 5, 15, Alarm.Priorities.Medium),
            new Alarm( Alarm.Parameters.PMP,true, 10, 25, Alarm.Priorities.Medium),
        };

        public bool IsSet {
            get { return Parameter is not null && Priority is not null && (Low is not null || High is not null); }
        }

        public bool IsEnabled {
            get { return IsSet && (Enabled ?? false); }
        }

        public bool ActivateAlarm (int? value) {
            Alarming = ShouldAlarm (value);
            return Alarming ?? false;
        }

        public bool ShouldAlarm (int? value) {
            return value is not null && IsSet && IsEnabled
                && ((Low is not null && Low > 0 && value < Low) || (High is not null && High > 0 && value > High));
        }

        public Alarm () {
            Enabled = false;
        }

        public Alarm (Alarm.Parameters? param, bool? enabled, int? low, int? high, Priorities? priority) {
            Parameter = param;
            Enabled = enabled ?? false;
            Low = low ?? 0;
            High = high ?? 0;
            Priority = priority ?? Priorities.Low;
        }

        public Task Set (Alarm.Parameters? param, bool? enabled, int? low, int? high, Priorities? priority) {
            Parameter = param;
            Enabled = enabled ?? false;
            Low = low ?? 0;
            High = high ?? 0;
            Priority = priority ?? Priorities.Low;

            return Task.CompletedTask;
        }

        public Task Load (string inc) {
            try {
                string [] parts = inc.Trim ().Split (' ');

                if (parts.Length == 5) {
                    Parameter = String.IsNullOrEmpty (parts [0]) ? null : (Alarm.Parameters)Enum.Parse (typeof (Alarm.Parameters), parts [0]);
                    Enabled = String.IsNullOrEmpty (parts [1]) ? false : bool.Parse (parts [1]);
                    Low = String.IsNullOrEmpty (parts [2]) ? 0 : int.Parse (parts [2]);
                    High = String.IsNullOrEmpty (parts [3]) ? 0 : int.Parse (parts [3]);
                    Priority = String.IsNullOrEmpty (parts [4]) ? Priorities.Low : (Priorities)Enum.Parse (typeof (Priorities), parts [4]);
                }
            } catch {
                /* If the load fails... just bail on the actual value parsing and continue the load process */
            }

            return Task.CompletedTask;
        }

        public Task<string> Save (int indent = 1) {
            string dent = Utility.Indent (indent);
            return Task.FromResult (String.Format ("{0} {1} {2} {3} {4} {5}", dent, Parameter.ToString (), Enabled, Low, High, Priority.ToString ()));
        }
    }
}