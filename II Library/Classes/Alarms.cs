/* Alarms.cs
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
    public class Alarms {
        private Dictionary<Alarm.Parameters, Alarm?> Listing;

        public static readonly Alarm [] DefaultListing_Adult =  {
            new Alarm(Alarm.Parameters.HR,true, 50, 130, Alarm.Severities.High),
            new Alarm(Alarm.Parameters.RR, true, 8, 24, Alarm.Severities.Medium),
            new Alarm(Alarm.Parameters.SPO2,true, 92, null, Alarm.Severities.Low),
            new Alarm(Alarm.Parameters.ETCO2,true, 30, 50, Alarm.Severities.Medium),
            new Alarm(Alarm.Parameters.CVP,true, 0, 12, Alarm.Severities.Low),
            new Alarm(Alarm.Parameters.ICP,true, null, 20, Alarm.Severities.Medium),
            new Alarm(Alarm.Parameters.IAP,true, null, 20, Alarm.Severities.Medium),

            new Alarm(Alarm.Parameters.NSBP,true, 90, 160, Alarm.Severities.Medium),
            new Alarm(Alarm.Parameters.NDBP,true, 45, 100, Alarm.Severities.Medium),
            new Alarm(Alarm.Parameters.NMAP, true, 65, 120, Alarm.Severities.Medium),
            new Alarm(Alarm.Parameters.ASBP,true, 90, 160, Alarm.Severities.Medium),
            new Alarm(Alarm.Parameters.ADBP,true, 45, 100, Alarm.Severities.Medium),
            new Alarm(Alarm.Parameters.AMAP,true, 65, 120, Alarm.Severities.Medium),
            new Alarm(Alarm.Parameters.PSP,true, 15, 40, Alarm.Severities.Medium),
            new Alarm(Alarm.Parameters.PDP,true, 5, 15, Alarm.Severities.Medium),
            new Alarm( Alarm.Parameters.PMP,true, 10, 25, Alarm.Severities.Medium),
        };

        public Alarms () {
            Listing = new ();

            foreach (Alarm.Parameters param in Enum.GetValues (typeof (Alarm.Parameters)))
                Listing [param] = new Alarm () { Enabled = false };
        }

        public Task<Alarm?> Get (Alarm.Parameters param) {
            if (Listing.ContainsKey (param))
                return Task.FromResult (Listing [param]);
            else
                return Task.FromResult ((Alarm?)null);
        }

        public Task Set (Alarm.Parameters param, Alarm? alarm) {
            if (Listing.ContainsKey (param))
                Listing [param] = alarm;
            else
                Listing.Add (param, alarm);

            return Task.CompletedTask;
        }

        public Task<bool> IsSet (Alarm.Parameters param) {
            return Task.FromResult (Listing.ContainsKey (param)
                && Listing [param] is not null && Listing [param]?.Severity is not null
                && (Listing [param]?.Low is not null || Listing [param]?.High is not null));
        }

        public async Task<bool> IsEnabled (Alarm.Parameters param) {
            return (await IsSet (param)) && (Listing [param]?.Enabled ?? false);
        }

        public async Task<bool> ShouldAlarm (Alarm.Parameters param, int value) {
            if (!await IsEnabled (param))
                return false;
            else
                return (Listing [param]?.Low is not null && Listing [param]?.Low > 0 && value < Listing [param]?.Low)
                    || (Listing [param]?.High is not null && Listing [param]?.High > 0 && value > Listing [param]?.High);
        }

        public Task Clear () {
            Listing = new ();

            return Task.CompletedTask;
        }

        public async Task Import (Alarm [] list) {
            await Clear ();

            foreach (Alarm a in list) {
                if (a.Parameter is not null)
                    Listing.Add ((Alarm.Parameters)a.Parameter, a);
            }
        }

        public async Task Load (string inc) {
            await Clear ();

            using StringReader sRead = new (inc);
            string? line;

            try {
                while (!String.IsNullOrEmpty (line = await sRead.ReadLineAsync ())) {
                    Alarm a = new ();
                    if (await a.Load (line.Trim ()) && a.Parameter is not null)
                        Listing.Add ((Alarm.Parameters)a.Parameter, a);
                }
            } catch {
                /* If the load fails... just bail on the actual value parsing and continue the load process */
            }

            sRead.Close ();
        }

        public async Task<string> Save (int indent = 1) {
            StringBuilder sb = new ();
            string? line;

            foreach (var l in Listing) {
                if (l.Value is not null) {
                    l.Value.Parameter = l.Key;                                      // Ensure the object's index matches the Dictionary's index

                    if (!String.IsNullOrEmpty (line = await l.Value.Save (indent)))
                        sb.AppendLine (line);
                }
            }

            return sb.ToString ();
        }

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

            public enum Severities {
                Low,
                Medium,
                High
            }

            public Alarm.Parameters? Parameter { get; set; }

            public bool? Enabled { get; set; }

            public int? High { get; set; }
            public int? Low { get; set; }

            public Severities? Severity { get; set; }

            public Alarm () {
                Enabled = false;
            }

            public Alarm (Alarm.Parameters? param, bool? enabled, int? high, int? low, Severities? severity) {
                Parameter = param;
                Enabled = enabled;
                High = high;
                Low = low;
                Severity = severity;
            }

            public Task<bool> Load (string inc) {
                try {
                    string [] parts = inc.Trim ().Split (' ');
                    if (parts.Length == 5) {
                        Parameter = String.IsNullOrEmpty (parts [0]) ? null : (Alarm.Parameters)Enum.Parse (typeof (Alarm.Parameters), parts [0]);
                        Enabled = String.IsNullOrEmpty (parts [1]) ? null : bool.Parse (parts [1]);
                        High = String.IsNullOrEmpty (parts [2]) ? null : int.Parse (parts [2]);
                        Low = String.IsNullOrEmpty (parts [3]) ? null : int.Parse (parts [3]);
                        Severity = String.IsNullOrEmpty (parts [4]) ? null : (Severities)Enum.Parse (typeof (Severities), parts [4]);

                        return Task.FromResult (true);
                    }
                } catch {
                    /* If the load fails... just bail on the actual value parsing and continue the load process */
                }

                return Task.FromResult (false);
            }

            public Task<string> Save (int indent = 1) {
                string dent = Utility.Indent (indent);
                return Task.FromResult (String.Format ("{0} {1} {2} {3} {4} {5}", dent, Parameter.ToString (), Enabled, High, Low, Severity.ToString ()));
            }
        }
    }
}