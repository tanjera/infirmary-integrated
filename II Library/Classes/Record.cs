/* Record.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera), (c) 2024
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace II {

    public class Record {
        public DateTime? CurrentTime;

        public string? Name;
        public DateOnly? DOB;
        public string? Sex;
        public string? MRN;

        public CodeStatuses.Values? CodeStatus;
        public List<Allergy> Allergies = new ();

        public string? HomeAddress;
        public string? TelephoneNumber;
        public string? InsuranceProvider;
        public string? InsuranceAccount;
        public string? DemographicNotes;

        public List<Note> Notes = new List<Note> ();

        public List<Medication.Order> RxOrders = new ();
        public List<Medication.Dose> RxDoses = new ();

        public int? Age {
            get {
                if (DOB is null)
                    return null;

                int age = DateTime.Today.Year - DOB.Value.Year;

                if (DateTime.Today.Month < DOB.Value.Month
                    || (DateTime.Today.Month == DOB.Value.Month && DateTime.Today.Day < DOB.Value.Day))
                    age -= 1;

                return age;
            }
        }

        public class Note {
            public string? UUID;

            public DateTime? Timestamp;
            public string? Title;
            public string? Author;
            public string? Content;

            public Note () {
                UUID = Guid.NewGuid ().ToString ();
            }

            public async Task Load (string inc) {
                using StringReader sRead = new (inc);
                string? line, pline;
                StringBuilder pbuffer;

                try {
                    while (!String.IsNullOrEmpty (line = sRead.ReadLine ())) {
                        line = line.Trim ();

                        if (line == "> Begin: Record.Note.Content >>>") {
                            pbuffer = new ();

                            while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                    && pline.Trim () != "> End: Record.Note.Content <<<")
                                pbuffer.AppendLine (pline);

                            Content = pbuffer.ToString ().Trim ();
                        } else if (line.Contains (':')) {
                            string pName = line.Substring (0, line.IndexOf (':')),
                                    pValue = line.Substring (line.IndexOf (':') + 1).Trim ();
                            switch (pName) {
                                default: break;

                                case "UUID": UUID = pValue; break;

                                case "Timestamp": Timestamp = Utility.DateTime_FromString (pValue); break;
                                case "Title": Title = pValue; break;
                                case "Author": Author = pValue; break;
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
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "UUID", UUID));

                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Timestamp", Utility.DateTime_ToString (Timestamp)));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Title", Title));
                sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Author", Author));

                // Allow for multi-line note content
                sWrite.AppendLine ($"{dent}> Begin: Record.Note.Content >>>");
                sWrite.AppendLine (Content?.Trim ());
                sWrite.AppendLine ($"{dent}> End: Record.Note.Content <<<");

                return sWrite.ToString ();
            }
        }

        public Record () {
            CurrentTime = DateTime.Now;
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
                    } else if (line == "> Begin: Record.Note") {
                        pbuffer = new ();

                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                && pline.Trim () != "> End: Record.Note")
                            pbuffer.AppendLine (pline);

                        Note note = new ();
                        await note.Load (pbuffer.ToString ());
                        Notes.Add (note);
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
                    } else if (line == "> Begin: Demographic.Notes >>>") {
                        pbuffer = new ();

                        while ((pline = (await sRead.ReadLineAsync ())?.Trim ()) != null
                                && pline.Trim () != "> End: Demographic.Notes <<<")
                            pbuffer.AppendLine (pline);

                        DemographicNotes = pbuffer.ToString ().Trim ();
                    } else if (line.Contains (':')) {
                        string pName = line.Substring (0, line.IndexOf (':')),
                                pValue = line.Substring (line.IndexOf (':') + 1).Trim ();
                        switch (pName) {
                            default: break;

                            // Patient/scenario information
                            case "CurrentTime": CurrentTime = Utility.DateTime_FromString (pValue); break;

                            case "Name": Name = pValue; break;
                            case "DOB": DOB = Utility.DateOnly_FromString (pValue); break;
                            case "Sex": Sex = pValue; break;
                            case "MRN": MRN = pValue; break;

                            case "CodeStatus":
                                CodeStatus = String.IsNullOrEmpty (pValue) ? null
                                    : (CodeStatuses.Values)Enum.Parse (typeof (CodeStatuses.Values), pValue);
                                break;

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
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "DOB", Utility.DateOnly_ToString (DOB) ?? ""));
            sWrite.AppendLine (String.Format ("{0}{1}:{2}", dent, "Sex", Sex));
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

            // Allow for multi-line demographic notes
            sWrite.AppendLine ($"{dent}> Begin: Demographic.Notes >>>");
            sWrite.AppendLine (DemographicNotes?.Trim ());
            sWrite.AppendLine ($"{dent}> End: Demographic.Notes <<<");

            // Notes
            for (int i = 0; i < Notes.Count; i++) {
                sWrite.AppendLine ($"{dent}> Begin: Record.Note");
                sWrite.Append (Notes [i].Save (indent + 1));
                sWrite.AppendLine ($"{dent}> End: Record.Note");
            }

            // Medication Orders
            for (int i = 0; i < RxOrders.Count; i++) {
                sWrite.AppendLine ($"{dent}> Begin: Medication.Order");
                sWrite.Append (RxOrders [i].Save (indent + 1));
                sWrite.AppendLine ($"{dent}> End: Medication.Order");
            }

            // Medication Doses
            for (int i = 0; i < RxDoses.Count; i++) {
                sWrite.AppendLine ($"{dent}> Begin: Medication.Dose");
                sWrite.Append (RxDoses [i].Save (indent + 1));
                sWrite.AppendLine ($"{dent}> End: Medication.Dose");
            }

            return sWrite.ToString ();
        }
    }
}