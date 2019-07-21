using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JourneyCore.Lib.System.Event;

namespace JourneyCore.Lib.System.Net
{
    public class AutoResetTimer
    {
        /// <summary>
        /// </summary>
        /// <param name="tickRate">Time interval in milliseconds to dequeue all state updates</param>
        /// <param name="updateType"></param>
        public AutoResetTimer(int tickRate)
        {
            // todo
            //      move to a design whereby the server tells the client
            //      it is ready to receive updates
            //      this will allow for automatic synchronization.
            //      
            //      when the client receives the update callback,
            //      wait until the next frame update to begin sending them
            //                      maybe?????

            TickRate = tickRate;
            AutoReset = new AutoResetEvent(false);
            TickTimer = new Timer(OnTimerTickElapsed, AutoReset, TickRate, 0);
            Watch = new Stopwatch();
        }

        private Timer TickTimer { get; }
        private AutoResetEvent AutoReset { get; }
        private Stopwatch Watch { get; }

        public int TickRate { get; }

        public event AsyncEventHandler<float> ElapsedAsync;

        public void OnTimerTickElapsed(object state)
        {
            Task.Run(() => OnTickTimerElapsedAsyncRespective(state));
        }

        private async Task OnTickTimerElapsedAsyncRespective(object state)
        {
            Watch.Restart();

            if (ElapsedAsync != null)
            {
                await ElapsedAsync.Invoke(state, Watch.ElapsedMilliseconds);
            }

            Watch.Stop();

            ((AutoResetEvent) state).Set();

            long nextTickDue = Watch.ElapsedMilliseconds == 0 ? TickRate : Watch.ElapsedMilliseconds % TickRate;

            TickTimer.Change(nextTickDue, 0);
        }
    }
}