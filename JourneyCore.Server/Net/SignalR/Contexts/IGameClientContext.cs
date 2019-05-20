using System.Threading.Tasks;
using JourneyCore.Lib.Graphics.Rendering.Environment.Tiling;
using JourneyCore.Server.Net.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;
using SFML.System;

namespace JourneyCore.Server.Net.SignalR.Contexts
{
    public interface IGameClientContext
    {
        IHubContext<GameClientHub> HubContext { get; }

        Task MovePlayer(string connectionId, Vector2f movement);
        Task SendServerStatus(bool serverReady);
        Task SendTexture(string connectionId, string key, byte[] texture);
        Task SendMap(string connectionId, string textureName, TileMap map, Tile[] usedTiles);
    }
}