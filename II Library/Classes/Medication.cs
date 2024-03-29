﻿/* Medication.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera), (c) 2023
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace II {
    public class Medication {
        public class Dose {
            public string? OrderUUID;

            public DateTime? ScheduledTime;

            public bool Administered = false;
            public string Comment = "";

            public Task Load (string inc) {
                using StringReader sRead = new (inc);

                try {
                    string? line;
                    while (!String.IsNullOrEmpty (line = sRead.ReadLine ())) {
                        line = line.Trim ();

                        if (line.Contains (':')) {
                            string pName = line.Substring (0, line.IndexOf (':')),
                                    pValue = line.Substring (line.IndexOf (':') + 1).Trim ();
                            switch (pName) {
                                default: break;

                                // Patient/scenario information
                                case "OrderUUID": OrderUUID = pValue; break;

                                case "ScheduledTime": ScheduledTime = Utility.DateTime_FromString (pValue); break;
                                case "Administered": Administered = bool.Parse (pValue); break;
                                case "Comment": Comment = pValue; break;
                            }
                        }
                    }
                } catch {
                    /* If the load fails... just bail on the actual value parsing and continue the load process */
                }

                sRead.Close ();

                return Task.CompletedTask;
            }

            public string Save (int indent = 1) {
                string dent = Utility.Indent (indent);
                StringBuilder sWrite = new ();

                // File/scenario information
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "OrderUUID", OrderUUID));

                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "ScheduledTime", Utility.DateTime_ToString (ScheduledTime)));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Administered", Administered));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Comment", Comment));

                return sWrite.ToString ();
            }
        }

        public class Order {
            public string? UUID;
            public string? DrugName;

            public double? DoseAmount;
            public DoseUnits.Values? DoseUnit;
            public Routes.Values? Route;

            public PeriodTypes.Values? PeriodType;
            public int? PeriodAmount = 1;
            public PeriodUnits.Values? PeriodUnit;
            public int? TotalDoses = 10;

            public Priorities.Values? Priority;

            public DateTime? StartTime;
            public DateTime? EndTime;

            public string? Indication;
            public string? Notes;

            public Order () {
                UUID = Guid.NewGuid ().ToString ();
            }

            public Order (Record chart) {
                UUID = Guid.NewGuid ().ToString ();

                StartTime = chart.CurrentTime;
                EndTime = chart.CurrentTime + new TimeSpan (7, 0, 0, 0);
            }

            public bool IsScheduled {
                get {
                    return PeriodType == PeriodTypes.Values.Once || PeriodType == PeriodTypes.Values.Repeats;
                }
            }

            public bool IsComplete {
                get {
                    return DrugName is not null
                        && DoseAmount is not null && DoseAmount > 0
                        && DoseUnit is not null
                        && Route is not null
                        && (PeriodType == PeriodTypes.Values.Once
                            || (PeriodType is not null
                                && PeriodAmount is not null
                                && PeriodUnit is not null));
                }
            }

            public class Routes {
                public Values? Value;

                public static string LookupString (Values value) {
                    return String.Format ("ENUM:Routes:{0}", Enum.GetValues (typeof (Values)).GetValue ((int)value)?.ToString ());
                }

                public enum Values {
                    PO,
                    IV,
                    IM,
                    Subcut,
                    Inhaled
                }
            }

            public class DoseUnits {
                public Values? Value;

                public static string LookupString (Values value) {
                    return String.Format ("ENUM:DoseUnits:{0}", Enum.GetValues (typeof (Values)).GetValue ((int)value)?.ToString ());
                }

                public enum Values {
                    // Volume
                    L,

                    ML,

                    G,
                    MG,
                    MCG,

                    MEQ,

                    IU,

                    Puff,

                    Drop,

                    // Volume/time
                    ML_HR,

                    MCG_HR,

                    // Volume/kg/time
                    ML_KG_HR,

                    MCG_KG_MIN
                }
            }

            public class Priorities {
                public Values? Value;

                public static string LookupString (Values value) {
                    return String.Format ("ENUM:Priorities:{0}", Enum.GetValues (typeof (Values)).GetValue ((int)value)?.ToString ());
                }

                public enum Values {
                    Routine,
                    Now,
                    Stat
                }
            }

            public class PeriodTypes {
                public Values? Value;

                public static string LookupString (Values value) {
                    return String.Format ("ENUM:PeriodTypes:{0}", Enum.GetValues (typeof (Values)).GetValue ((int)value)?.ToString ());
                }

                public enum Values {
                    Repeats,
                    PRN,
                    Once
                }
            }

            public class PeriodUnits {
                public Values? Value;

                public static string LookupString (Values value) {
                    return String.Format ("ENUM:PeriodUnits:{0}", Enum.GetValues (typeof (Values)).GetValue ((int)value)?.ToString ());
                }

                public enum Values {
                    Minute,
                    Hour,
                    Day,
                    Week
                }
            }

            public Task Load (string inc) {
                using StringReader sRead = new (inc);

                try {
                    string? line;
                    while (!String.IsNullOrEmpty (line = sRead.ReadLine ())) {
                        line = line.Trim ();

                        if (line.Contains (':')) {
                            string pName = line.Substring (0, line.IndexOf (':')),
                                    pValue = line.Substring (line.IndexOf (':') + 1).Trim ();
                            switch (pName) {
                                default: break;

                                // Patient/scenario information
                                case "UUID": UUID = pValue; break;
                                case "DrugName": DrugName = pValue; break;

                                case "DoseAmount": DoseAmount = double.Parse (pValue); break;
                                case "DoseUnit": DoseUnit = (DoseUnits.Values)Enum.Parse (typeof (DoseUnits.Values), pValue); break;
                                case "Route": Route = (Routes.Values)Enum.Parse (typeof (Routes.Values), pValue); break;

                                case "PeriodType": PeriodType = (PeriodTypes.Values)Enum.Parse (typeof (PeriodTypes.Values), pValue); break;
                                case "PeriodAmount": PeriodAmount = int.Parse (pValue); break;
                                case "PeriodUnit": PeriodUnit = (PeriodUnits.Values)Enum.Parse (typeof (PeriodUnits.Values), pValue); break;
                                case "TotalDoses": TotalDoses = int.Parse (pValue); break;

                                case "Priority": Priority = (Priorities.Values)Enum.Parse (typeof (Priorities.Values), pValue); break;

                                case "StartTime": StartTime = Utility.DateTime_FromString (pValue); break;
                                case "EndTime": EndTime = Utility.DateTime_FromString (pValue); break;

                                case "Indication": Indication = pValue; break;
                                case "Notes": Notes = pValue; break;
                            }
                        }
                    }
                } catch {
                    /* If the load fails... just bail on the actual value parsing and continue the load process */
                }

                sRead.Close ();

                return Task.CompletedTask;
            }

            public string Save (int indent = 1) {
                string dent = Utility.Indent (indent);
                StringBuilder sWrite = new ();

                // File/scenario information
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "UUID", UUID));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "DrugName", DrugName));

                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "DoseAmount", DoseAmount));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "DoseUnit", DoseUnit));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Route", Route));

                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "PeriodType", PeriodType));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "PeriodAmount", PeriodAmount));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "PeriodUnit", PeriodUnit));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "TotalDoses", TotalDoses));

                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Priority", Priority));

                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "StartTime", Utility.DateTime_ToString (StartTime)));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "EndTime", Utility.DateTime_ToString (EndTime)));

                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Indication", Indication));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Notes", Notes));

                return sWrite.ToString ();
            }
        }
    }
}