using System.Threading.Tasks;
using JourneyCore.Server.Net.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;
using SFML.System;

namespace JourneyCore.Server.Net.SignalR.Contexts
{
    public interface IGameClientContext
    {
        IHubContext<GameClientHub> HubContext { get; }

        Task MovePlayer(string connectionId, Vector2f movement);
        Task SendServerStatus(string connectionId, bool serverReady);
    }
}