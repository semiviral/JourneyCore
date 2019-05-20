using System.Collections.Generic;
using System.Threading.Tasks;
using JourneyCore.Lib.Graphics.Rendering.Environment.Tiling;
using JourneyCore.Lib.System;
using Microsoft.Extensions.Hosting;

namespace JourneyCore.Server.Net.SignalR.Services
{
    public interface IGameService : IHostedService
    {
        List<Instance> WorldInstances { get; }
        Dictionary<string, TileMap> TileMaps { get; }

        Instance GetInstanceById(string id);
        Task ReceiveUpdatePackages(List<UpdatePackage> updatePackages);
        Task SendServerStatus(string connectionId);
        Task SendTextureList(string connectionId);
        Task SendMap(string connectionId, string mapName);
    }
}