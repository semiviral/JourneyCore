using System.Threading.Tasks;
using JourneyCore.Lib.System.Net.Security;
using JourneyCore.Server.Net.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;
using SFML.System;

namespace JourneyCore.Server.Net.SignalR.Contexts
{
    public class GameClientContext : IGameClientContext
    {
        public GameClientContext(IHubContext<GameClientHub> gameClientHub)
        {
            HubContext = gameClientHub;
        }

        public IHubContext<GameClientHub> HubContext { get; }

        public async Task SendServerStatus(string connectionId, bool serverStatus)
        {
            await HubContext.Clients.Client(connectionId).SendAsync("ReceiveServerStatus", serverStatus);
        }

        public async Task SendConnectionId(string connectionId)
        {
            await HubContext.Clients.Client(connectionId).SendAsync("ReceiveConnectionId", connectionId);
        }

        public async Task MovePlayer(string connectionId, Vector2f movement)
        {
            await HubContext.Clients.Client(connectionId).SendAsync("ReceivePlayerMovement", movement);
        }
    }
}