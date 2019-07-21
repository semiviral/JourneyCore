using System;
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

        public async Task SendConnectionId(string connectionId)
        {
            await HubContext.Clients.Client(connectionId).SendAsync("ReceiveConnectionId", connectionId);
        }

        public async Task PlayerPositionModification(string connectionId, Vector2f positionModification)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
            {
                throw new NullReferenceException("Parameter `connectionId` cannot be null or whitespace.");
            }

            await HubContext.Clients.Client(connectionId)
                .SendAsync("ReceivePlayerPositionModification", positionModification);
        }

        public async Task PlayerRotationModification(string connectionId, float rotationModification)
        {
            await HubContext.Clients.Client(connectionId)
                .SendAsync("ReceivePlayerRotationModification", rotationModification);
        }
    }
}