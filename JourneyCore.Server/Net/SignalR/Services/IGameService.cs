using System.Collections.Generic;
using System.Threading.Tasks;
using JourneyCore.Lib.Game.Context.Entities;
using JourneyCore.Lib.Game.Environment.Mapping;
using JourneyCore.Lib.Game.Environment.Metadata;
using JourneyCore.Lib.System;
using Microsoft.Extensions.Hosting;
using SFML.System;

namespace JourneyCore.Server.Net.SignalR.Services
{
    public interface IGameService : IHostedService
    {
        bool Status { get; }
        List<Entity> Players { get; }
        Dictionary<string, Map> TileMaps { get; }

        Task ReceiveUpdatePackages(List<UpdatePackage> updatePackages);

        byte[] GetTexture(string textureName);
        MapMetadata GetMapMetadata(string mapName);
        IEnumerable<Chunk> GetChunk(string mapName, Vector2i chunkCoords);
    }
}