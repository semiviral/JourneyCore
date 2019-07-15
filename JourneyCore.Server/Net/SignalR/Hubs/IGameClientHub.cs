using System.Collections.Generic;
using System.Threading.Tasks;
using JourneyCore.Lib.System.Net;

namespace JourneyCore.Server.Net.SignalR.Hubs
{
    public interface IGameClientHub
    {
        Task RequestReadyStatus();
        Task RequestConnectionId();
        Task ReceiveUpdatePackages(List<UpdatePackage> updatePackages);
    }
}