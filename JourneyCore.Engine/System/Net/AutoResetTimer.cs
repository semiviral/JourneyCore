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
        public AutoResetTimer(int tickRate)
        {
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

        private void OnTimerTickElapsed(object state)
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

            long _nextTickDue = Watch.ElapsedMilliseconds == 0 ? TickRate : Watch.ElapsedMilliseconds % TickRate;

            TickTimer.Change(_nextTickDue, 0);
        }
    }
}