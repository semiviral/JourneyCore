using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using JourneyCore.Lib.System;
using Microsoft.AspNetCore.SignalR.Client;

namespace JourneyCore.Client
{
    public class ServerSynchroniser
    {
        private int TickIntreval { get; }
        private Timer TickTimer { get; }
        private AutoResetEvent AutoReset { get; }
        private List<UpdatePackage> UpdatePackages { get; set; }
        private HubConnection Connection { get; }
        private Stopwatch Watch { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tickIntreval">Time intreval in milliseconds to dequeue all state updates</param>
        public ServerSynchroniser(HubConnection connection, int tickIntreval)
        {
            TickIntreval = tickIntreval;
            AutoReset = new AutoResetEvent(false);
            TickTimer = new Timer(OnTickTimerElapsed, AutoReset, TickIntreval, 0);

            UpdatePackages = new List<UpdatePackage>();
            Connection = connection;
            Watch = new Stopwatch();
        }

        private void OnTickTimerElapsed(object state)
        {
            Watch.Restart();

            if (UpdatePackages.Count() > 0)
            {
                SendStatePackage();
            }

            Watch.Stop();

            ((AutoResetEvent)state).Set();

            long nextTickDue = Watch.ElapsedMilliseconds == 0 ? TickIntreval : Watch.ElapsedMilliseconds % TickIntreval;

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
