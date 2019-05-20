using System.Threading.Tasks;
using JourneyCore.Lib.Graphics.Rendering.Environment.Tiling;
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

        public async Task SendServerStatus(bool serverStatus)
        {
            await HubContext.Clients.All.SendAsync("ReceiveServerStatus", serverStatus);
        }

        public async Task SendTexture(string connectionId, string key, byte[] texture)
        {
            await HubContext.Clients.Client(connectionId).SendAsync("ReceiveTexture", key, texture);
        }

        public async Task SendMap(string connectionId, string textureName, TileMap map, Tile[] usedTiles)
        {
            await HubContext.Clients.Client(connectionId).SendAsync("ReceiveMap", textureName, map, usedTiles);
        }

        public async Task MovePlayer(string connectionId, Vector2f movement)
        {
            await HubContext.Clients.Client(connectionId).SendAsync("ReceivePlayerMovement", movement);
        }
    }
}