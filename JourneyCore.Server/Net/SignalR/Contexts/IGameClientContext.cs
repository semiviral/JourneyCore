using System.Threading.Tasks;
using JourneyCore.Lib.System.Net.Security;
using JourneyCore.Server.Net.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;
using SFML.System;

namespace JourneyCore.Server.Net.SignalR.Contexts
{
    public interface IGameClientContext
    {
        IHubContext<GameClientHub> HubContext { get; }

        Task SendServerStatus(string connectionId, bool serverReady);
        Task SendConnectionId(string connectionId);
        Task MovePlayer(string connectionId, Vector2f movement);
    }
}