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

            if ((DateTime.Now - Last).TotalSeconds * 1000 > Interval) {
                Last = DateTime.Now;
                Tick?.Invoke (this, new EventArgs());
            }
        }

        public void Process (object sender, EventArgs e) => Process();

    }
}
