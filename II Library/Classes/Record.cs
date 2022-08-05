using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace II {
    public class Record {
        public DateTime? CurrentTime;

        public string? Name;
        public DateOnly DOB;
        public string? MRN;

        public CodeStatuses.Values CodeStatus;
        public List<Allergy> Allergies = new ();

        public string? HomeAddress;
        public string? TelephoneNumber;
        public string? InsuranceProvider;
        public string? InsuranceAccount;

        public List<Medication.Order> RxOrders = new ();
        public List<Medication.Dose> RxDoses = new ();

        public Record () {
            CurrentTime = DateTime.Now;

            /* Initialize values that would otherwise cause functions to crash (e.g. Save/Load) if left blank */
            CodeStatus = CodeStatuses.Values.FullCode;
            DOB = new DateOnly (2000, 1, 1);
        }

        public class CodeStatuses {
            public Values? Value;

            public static string LookupString (Values value) {
                return String.Format ("ENUM:CodeStatuses:{0}", Enum.GetValues (typeof (Values)).GetValue ((int)value)?.ToString ());
            }

            public enum Values {
                FullCode,
                NoIntubation,
                NoResuscitation
            }
        }

        public async Task Load (string inc) {
            using StringReader sRead = new (inc);
            string? line, pline;
            StringBuilder pbuffer;

            Allergies.Clear ();
            RxOrders.Clear ();
            RxDoses.Clear ();

            try {
                while (!String.IsNullOrEmpty (line = await sRead.ReadLineAsync ())) {
                    line = line.Trim ();

                    if (line == "> Begin: Allergy") {
                        pbuffer = new ();

                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                && pline.Trim () != "> End: Allergy")
                            pbuffer.AppendLine (pline);

                        Allergy allergy = new ();
                        await allergy.Load (pbuffer.ToString ());
                        Allergies.Add (allergy);
                    } else if (line == "> Begin: Medication.Order") {
                        pbuffer = new ();

                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                && pline.Trim () != "> End: Medication.Order")
                            pbuffer.AppendLine (pline);

                        Medication.Order order = new ();
                        await order.Load (pbuffer.ToString ());
                        RxOrders.Add (order);
                    } else if (line == "> Begin: Medication.Dose") {
                        pbuffer = new ();

                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                && pline.Trim () != "> End: Medication.Dose")
                            pbuffer.AppendLine (pline);

                        Medication.Dose dose = new ();
                        await dose.Load (pbuffer.ToString ());
                        RxDoses.Add (dose);
                    } else if (line.Contains (':')) {
                        string pName = line.Substring (0, line.IndexOf (':')),
                                pValue = line.Substring (line.IndexOf (':') + 1).Trim ();
                        switch (pName) {
                            default: break;

                            // Patient/scenario information
                            case "CurrentTime": CurrentTime = Utility.DateTime_FromString (pValue); break;

                            case "Name": Name = pValue; break;
                            case "DOB": DOB = Utility.DateOnly_FromString (pValue); break;
                            case "MRN": MRN = pValue; break;

                            case "CodeStatus": CodeStatus = (CodeStatuses.Values)Enum.Parse (typeof (CodeStatuses.Values), pValue); break;

                            case "HomeAddress": HomeAddress = pValue; break;
                            case "TelephoneNumber": TelephoneNumber = pValue; break;
                            case "InsuranceProvider": InsuranceProvider = pValue; break;
                            case "InsuranceAccount": InsuranceAccount = pValue; break;
                        }
                    }
                }
            } catch {
                /* If the load fails... just bail on the actual value parsing and continue the load process */
            }

            sRead.Close ();
        }

        public string Save (int indent = 1) {
            string dent = Utility.Indent (indent);
            StringBuilder sWrite = new ();

            // File/scenario information
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "CurrentTime", Utility.DateTime_ToString (CurrentTime)));

            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Name", Name));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "DOB", Utility.DateOnly_ToString (DOB)));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "MRN", MRN));

            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "CodeStatus", CodeStatus));

            for (int i = 0; i < Allergies.Count; i++) {
                sWrite.AppendLine ($"{dent}> Begin: Allergy");
                sWrite.Append (Allergies [i].Save (indent + 1));
                sWrite.AppendLine ($"{dent}> End: Allergy");
            }

            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "HomeAddress", HomeAddress));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "TelephoneNumber", TelephoneNumber));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "InsuranceProvider", InsuranceProvider));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "InsuranceAccount", InsuranceAccount));

            for (int i = 0; i < RxOrders.Count; i++) {
                sWrite.AppendLine ($"{dent}> Begin: Medication.Order");
                sWrite.Append (RxOrders [i].Save (indent + 1));
                sWrite.AppendLine ($"{dent}> End: Medication.Order");
            }

            for (int i = 0; i < RxDoses.Count; i++) {
                sWrite.AppendLine ($"{dent}> Begin: Medication.Dose");
                sWrite.Append (RxDoses [i].Save (indent + 1));
                sWrite.AppendLine ($"{dent}> End: Medication.Dose");
            }

            return sWrite.ToString ();
        }
    }
}