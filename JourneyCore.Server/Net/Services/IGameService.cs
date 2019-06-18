using System.Collections.Generic;
using System.Threading.Tasks;
using JourneyCore.Lib.Game.Environment.Mapping;
using JourneyCore.Lib.Game.Environment.Metadata;
using JourneyCore.Lib.Game.Object.Entity;
using JourneyCore.Lib.System.Net;
using JourneyCore.Lib.System.Net.Security;
using Microsoft.Extensions.Hosting;

namespace JourneyCore.Server.Net.Services
{
    public interface IGameService : IHostedService
    {
        int TickRate { get; }
        bool Status { get; }
        List<Player> Players { get; }
        Dictionary<string, Map> TileMaps { get; }

        Task ReceiveUpdatePackages(List<UpdatePackage> updatePackages);

        DiffieHellmanKeyPackage RegisterDiffieHellman(string guid, byte[] clientPublicKey);
        Task<byte[]> GetImage(string guid, byte[] remotePublicKey, byte[] textureNameEncrypted);
        Task<TileSetMetadata> GetTileSetMetadata(string guid, byte[] remotePublicKey, byte[] tileSetNameEncrypted);
        Task<MapMetadata> GetMapMetadata(string guid, byte[] remotePublicKey, byte[] mapNameEncrypted);
        Task<List<Chunk>> GetChunk(string guid, byte[] remotePublicKey, byte[] mapNameEncrypted, byte[] coordsEncrypted);
    }
}