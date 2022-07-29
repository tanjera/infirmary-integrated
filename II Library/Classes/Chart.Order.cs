using System;
using System.Collections.Generic;
using System.Text;

namespace II {
    public partial class Chart {
        public class Order {
            public class Drug {
                public string? DrugName { get; set; }

                public double? DoseAmount { get; set; }
                public DoseUnits? DoseUnit { get; set; }
                public Routes? Route { get; set; }

                public PeriodTypes? PeriodType { get; set; }
                public int? PeriodAmount { get; set; }
                public PeriodUnits? PeriodUnit { get; set; }
                public int? TotalDoses { get; set; }

                public Priorities? Priority { get; set; }

                public string? Notes { get; set; }

                public TimeOnly? StartTime { get; set; }
                public TimeOnly? EndTime { get; set; }

                public enum Routes {
                    Oral,
                    Intravenous
                }

                public enum DoseUnits {
                    // Volume
                    L,

                    ML,

                    G,
                    MG,
                    MCG,

                    MEQ,

                    IU,

                    PUFF,

                    DROP,

                    // Volume/time
                    ML_HR,

                    MCG_HR,

                    // Volume/kg/time
                    ML_KG_HR,

                    MCG_KG_MIN
                }

                public enum Priorities {
                    Stat,
                    Now,
                    Routine
                }

                public enum PeriodTypes {
                    PRN,
                    Once,
                    Repeats
                }

                public enum PeriodUnits {
                    Hour,
                    Day,
                    Week
                }

                public bool IsScheduled {
                    get {
                        return PeriodType == PeriodTypes.Once || PeriodType == PeriodTypes.Repeats;
                    }
                }
            }
        }
    }
}