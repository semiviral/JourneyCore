using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using JourneyCore.Lib.System;
using JourneyCore.Lib.System.Event;
using Serilog;

namespace JourneyCore.Client.Net
{
    public class ServerSynchronizer
    {
        /// <summary>
        /// </summary>
        /// <param name="tickRate">Time interval in milliseconds to dequeue all state updates</param>
        public ServerSynchronizer(int tickRate)
        {
            TickRate = tickRate;
            AutoReset = new AutoResetEvent(false);
            TickTimer = new Timer(OnTickTimerElapsed, AutoReset, TickRate, 0);

            UpdatePackages = new List<UpdatePackage>();
            Watch = new Stopwatch();
        }

        private Timer TickTimer { get; }
        private AutoResetEvent AutoReset { get; }
        private List<UpdatePackage> UpdatePackages { get; }
        private Stopwatch Watch { get; }

        public int TickRate { get; }
        
        public event AsyncEventHandler<UpdatePackage[]> SyncCallback;

        private void OnTickTimerElapsed(object state)
        {
            Watch.Restart();

            if (UpdatePackages.Count > 0)
            {
                SendStatePackage();
            }

            Watch.Stop();

            ((AutoResetEvent)state).Set();

            long nextTickDue = Watch.ElapsedMilliseconds == 0 ? TickRate : Watch.ElapsedMilliseconds % TickRate;

            TickTimer.Change(nextTickDue, 0);
        }

        private void SendStatePackage()
        {
            Log.Information($"Sending state package with {UpdatePackages.Count} items.");

            SyncCallback?.Invoke(this, UpdatePackages.ToArray());

            UpdatePackages.Clear();
        }

        public void AllocateStateUpdate(StateUpdateType packageType, params object[] args)
        {
            UpdatePackages.Add(new UpdatePackage(packageType, args));
        }
    }
}