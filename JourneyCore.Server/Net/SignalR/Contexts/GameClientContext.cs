using Microsoft.AspNetCore.SignalR;
using JourneyCore.Server.Net.SignalR.Hubs;
using System.Threading.Tasks;
using System.Collections.Generic;
using JourneyCore.Lib.Graphics.Rendering.Environment.Tiling;
using SFML.System;

namespace JourneyCore.Server.Net.SignalR.Contexts
{
    public class GameClientContext : IGameClientContext
    {
        public IHubContext<GameClientHub> HubContext { get; }

        public GameClientContext(IHubContext<GameClientHub> gameClientHub)
        {
            HubContext = gameClientHub;
        }

        public async Task SendServerStatus(bool serverStatus)
        {
            await HubContext.Clients.All.SendAsync("ReceiveServerStatus", serverStatus);
        }

        public async Task SendTexture(string connectionId, string key, byte[] texture)
        {
            await HubContext.Clients.Client(connectionId).SendAsync("ReceiveTexture", key, texture);
        }

        public async Task SendTileMap(string connectionId, TileMap tileMap)
        {
            await HubContext.Clients.Client(connectionId).SendAsync("ReceiveTileMap", tileMap);
        }

        public async Task MovePlayer(string connectionId, Vector2f movement)
        {
            await HubContext.Clients.Client(connectionId).SendAsync("ReceivePlayerMovement", movement);
        }
    }
}
