/* Timer.cs
 * Infirmary Integrated
 * By Ibi Keller (Tanjera), (c) 2023
 */

using System;
using System.Threading.Tasks;

namespace II {
    public class Timer {
        private int _Interval = 0;
        private bool _Locked = false;

        private DateTime Last;
        private bool Running = false;

        public bool IsRunning { get { return Running; } }

        public event EventHandler<EventArgs>? Tick;

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

        /// <summary>
        /// Starts a Timer (or continues a running Timer).
        /// Does not alter time since last tick.
        /// </summary>
        /// <returns></returns>
        public Task Start () {
            Running = true;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Adjusts Timer Interval without starting/stopping/resetting Timer.
        /// Does not alter time since last tick.
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public Task Adjust (int interval) {
            _Interval = interval;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Adjusts Timer Interval while continuing a running Timer (or starting a stopped Timer!)
        /// Does not alter time since last tick.
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public Task Continue (int interval) {
            _Interval = interval;
            Running = true;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops a Timer.
        /// Does not alter time since last tick.
        /// </summary>
        /// <returns></returns>
        public Task Stop () {
            Running = false;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets a Timer's Interval and resets time since last tick.
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public Task Set (int interval) {
            _Interval = interval;
            Last = DateTime.Now;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Resets a Timer's time since last tick. Otherwise does not alter Timer state.
        /// </summary>
        /// <returns></returns>
        public Task Reset () {
            Last = DateTime.Now;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets a Timer's Interval and resets time since last tick.
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public async Task Reset (int interval)
            => await Set (interval);

        /// <summary>
        /// Macro for Reset() and Start()
        /// </summary>
        /// <returns></returns>
        public async Task ResetStart () {
            await Reset ();
            await Start ();
        }

        /// <summary>
        /// Macro for Reset(interval) and Start()
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public async Task ResetStart (int interval) {
            await Reset (interval);
            await Start ();
        }

        /// <summary>
        /// Overload for Reset(int interval) and Start()
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public async Task ResetStart (float interval)
            => await ResetStart ((int)interval);

        /// <summary>
        /// Macro for Reset() and Stop()
        /// </summary>
        /// <returns></returns>
        public async Task ResetStop () {
            await Reset ();
            await Stop ();
        }

        /// <summary>
        /// Macro for Reset(interval) and Stop()
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public async Task ResetStop (int interval) {
            await Reset (interval);
            await Stop ();
        }

        /// <summary>
        /// Overload for Reset(int interval) and Stop()
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public async Task ResetStop (float interval)
            => await ResetStop ((int)interval);

        /// <summary>
        /// Invokes the Timer's Tick event
        /// </summary>
        /// <returns></returns>
        public Task Trigger () {
            Tick?.Invoke (this, new EventArgs ());
            return Task.CompletedTask;
        }

        /// <summary>
        /// "Runs" the Timer: checks time since last Tick and, if greater than Interval, invokes the
        /// Tick event and then resets time since the last tick.
        /// </summary>
        public void Process () {
            if (!Running)
                return;

            if ((DateTime.Now - Last).TotalSeconds * 1000 > _Interval) {
                Last = DateTime.Now;
                Tick?.Invoke (this, new EventArgs ());
            }
        }

        /// <summary>
        /// Overload for Process()
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Process (object? sender, EventArgs e)
            => Process ();
    }
}