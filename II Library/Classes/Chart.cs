using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace II {
    public class Chart {
        public DateTime? CurrentTime;

        public List<Medication.Order> RxOrders = new ();
        public List<Medication.Dose> RxDoses = new ();

        public Chart () {
            CurrentTime = DateTime.Now;
        }

        public async Task Load (string inc) {
            using StringReader sRead = new (inc);
            string? line, pline;
            StringBuilder pbuffer;

            RxOrders.Clear ();
            RxDoses.Clear ();

            try {
                while (!String.IsNullOrEmpty (line = await sRead.ReadLineAsync ())) {
                    line = line.Trim ();

                    if (line == "> Begin: Medication.Order") {
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