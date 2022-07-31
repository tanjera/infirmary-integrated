using System;
using System.Collections.Generic;
using System.Text;

namespace II {
    public class Chart {
        public DateTime? CurrentTime;

        public List<Medication.Order> MedicationOrders = new ();
        public List<Medication.Dose> MedicationDoses = new ();

        public Chart () {
            CurrentTime = DateTime.Now;
        }
    }
}