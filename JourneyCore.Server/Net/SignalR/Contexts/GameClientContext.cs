using System.Threading.Tasks;
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

        public async Task PlayerPositionModification(string connectionId, Vector2f newPosition)
        {
            await HubContext.Clients.Client(connectionId).SendAsync("ReceivePlayerMovementModification", newPosition);
        }

        public async Task PlayerRotationModification(string connectionId, float newRotation)
        {
            await HubContext.Clients.Client(connectionId).SendAsync("ReceivePlayerRotationModification", newRotation);
        }
    }
}