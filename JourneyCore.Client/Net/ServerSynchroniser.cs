using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using JourneyCore.Lib.System;
using Microsoft.AspNetCore.SignalR.Client;

namespace JourneyCore.Client.Net
{
    public class ServerSynchroniser
    {
        /// <summary>
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tickInterval">Time intreval in milliseconds to dequeue all state updates</param>
        public ServerSynchroniser(HubConnection connection, int tickInterval)
        {
            TickInterval = tickInterval;
            AutoReset = new AutoResetEvent(false);
            TickTimer = new Timer(OnTickTimerElapsed, AutoReset, TickInterval, 0);

            UpdatePackages = new List<UpdatePackage>();
            Connection = connection;
            Watch = new Stopwatch();
        }

        private int TickInterval { get; }
        private Timer TickTimer { get; }
        private AutoResetEvent AutoReset { get; }
        private List<UpdatePackage> UpdatePackages { get; set; }
        private HubConnection Connection { get; }
        private Stopwatch Watch { get; }

        private void OnTickTimerElapsed(object state)
        {
            Watch.Restart();

            if (UpdatePackages.Any())
            {
                SendStatePackage();
            }

            Watch.Stop();

            ((AutoResetEvent)state).Set();

            long nextTickDue = Watch.ElapsedMilliseconds == 0 ? TickInterval : Watch.ElapsedMilliseconds % TickInterval;

            TickTimer.Change(nextTickDue, 0);
        }

        private void SendStatePackage()
        {
            List<UpdatePackage> updatePackages = new List<UpdatePackage>(UpdatePackages);

            UpdatePackages = new List<UpdatePackage>();

            Connection?.InvokeAsync("ReceiveUpdatePackages", updatePackages);
        }

        public void AllocateStateUpdate(StateUpdateType packageType, params object[] args)
        {
            UpdatePackages.Add(new UpdatePackage { UpdateType = packageType, Args = args });
        }
    }
}