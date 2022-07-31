using System;
using System.Collections.Generic;
using System.Text;

namespace II {

    public class Medication {

        public class Dose {
            public Order? Order;

            public DateTime? ScheduledTime;

            public AdministrationStatuses.Values? AdministrationStatus;
            public TimeStatuses.Values? TimeStatus;

            public string? AdministrationNotes;

            public class AdministrationStatuses {
                public Values? Value;

                public static string LookupString (Values value) {
                    return String.Format ("ENUM:AdministrationStatuses:{0}", Enum.GetValues (typeof (Values)).GetValue ((int)value)?.ToString ());
                }

                public enum Values {
                    Administered,
                    NotAdministered
                }
            }

            public class TimeStatuses {
                public Values? Value;

                public static string LookupString (Values value) {
                    return String.Format ("ENUM:TimeStatuses:{0}", Enum.GetValues (typeof (Values)).GetValue ((int)value)?.ToString ());
                }

                public enum Values {
                    Pending,
                    Late
                }
            }
        }

        public class Order {
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
                StartTime = DateTime.Now;
                EndTime = DateTime.Now + new TimeSpan (1, 0, 0, 0);
            }

            public Order (Chart chart) {
                StartTime = chart.CurrentTime;
                EndTime = chart.CurrentTime + new TimeSpan (1, 0, 0, 0);
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
                    IV,
                    PO
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
        }
    }
}