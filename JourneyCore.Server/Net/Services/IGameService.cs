using System.Collections.Generic;
using System.Threading.Tasks;
using JourneyCore.Lib.Game.Environment.Mapping;
using JourneyCore.Lib.Game.Environment.Metadata;
using JourneyCore.Lib.Game.Net;
using JourneyCore.Lib.Game.Object.Entity;
using JourneyCore.Lib.System;
using Microsoft.Extensions.Hosting;
using SFML.System;

namespace JourneyCore.Server.Net.Services
{
    public interface IGameService : IHostedService
    {
        int TickRate { get; }
        bool Status { get; }
        List<Player> Players { get; }
        Dictionary<string, Map> TileMaps { get; }

        Task ReceiveUpdatePackages(List<UpdatePackage> updatePackages);

        byte[] GetImage(string textureName);
        TileSetMetadata GetTileSetMetadata(string tileSetName);
        MapMetadata GetMapMetadata(string mapName);
        IEnumerable<Chunk> GetChunk(string mapName, Vector2i chunkCoords);
    }
}