using System;
using System.Collections.Generic;
using System.Text;

namespace II
{
    public class Timer
    {
        public int Interval = 0;

        DateTime Last;
        bool Running = false;

        public event EventHandler<EventArgs> Tick;

        public void Start () {
            Running = true;
        }

        public void Stop () {
            Running = false;
        }

        public void Process () {
            if (!Running)
                return;

            if ((DateTime.Now - Last).TotalSeconds > Interval) {
                Last = DateTime.Now;
                Tick (this, new EventArgs());
            }
        }
    }
}
