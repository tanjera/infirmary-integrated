using System;
using System.Threading.Tasks;

namespace II {

    public class Timer {
        private int _Interval = 0;
        private bool _Locked = false;

        private DateTime Last;
        private bool Running = false;

        public bool IsRunning { get { return Running; } }

        public event EventHandler<EventArgs> Tick;

        ~Timer () => Dispose ();

        public void Dispose () {
            if (Tick != null) {
                foreach (Delegate d in Tick.GetInvocationList ()) {
                    Tick -= (EventHandler<EventArgs>)d;
                }
            }
        }

        public bool IsLocked { get => _Locked; }
        public int Interval { get => _Interval; }
        public int Elapsed { get => (int)((DateTime.Now - Last).TotalSeconds * 1000); }
        public int Remainder { get => _Interval - (int)((DateTime.Now - Last).TotalSeconds * 1000); }

        public void Lock () => _Locked = true;

        public void Unlock () => _Locked = false;

        public Task Start () {
            Running = true;

            return Task.CompletedTask;
        }

        public Task Continue (int interval) {
            _Interval = interval;
            Running = true;

            return Task.CompletedTask;
        }

        public Task Stop () {
            Running = false;

            return Task.CompletedTask;
        }

        public Task Set (int interval) {
            _Interval = interval;
            Last = DateTime.Now;

            return Task.CompletedTask;
        }

        public Task Reset () {
            Last = DateTime.Now;

            return Task.CompletedTask;
        }

        public async Task Reset (int interval)
            => await Set (interval);

        public async Task ResetAuto () {
            await Reset ();
            await Start ();
        }

        public async Task ResetAuto (int interval) {
            await Reset (interval);
            await Start ();
        }

        public async Task ResetAuto (float interval)
            => await ResetAuto ((int)interval);

        public void Process () {
            if (!Running)
                return;

            if ((DateTime.Now - Last).TotalSeconds * 1000 > _Interval) {
                Last = DateTime.Now;
                Tick?.Invoke (this, new EventArgs ());
            }
        }

        public void Process (object? sender, EventArgs e)
            => Process ();
    }
}