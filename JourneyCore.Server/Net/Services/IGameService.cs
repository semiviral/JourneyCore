using System.Collections.Generic;
using System.Threading.Tasks;
using JourneyCore.Lib.Game.Environment.Mapping;
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

        Task ReceiveUpdatePackages(string connectionId, List<UpdatePackage> updatePackages);
        Task RegisterEncryptedConnection(string connectionId, byte[] clientPublicKey);
        
        Task<DiffieHellmanMessagePackage> GetImage(string id, byte[] remotePublicKey, byte[] textureNameEncrypted);

        Task<DiffieHellmanMessagePackage> GetTileSetMetadata(string id, byte[] remotePublicKey,
            byte[] tileSetNameEncrypted);

        Task<DiffieHellmanMessagePackage> GetMapMetadata(string id, byte[] remotePublicKey, byte[] mapNameEncrypted);

        Task<DiffieHellmanMessagePackage> GetChunk(string id, byte[] remotePublicKey, byte[] mapNameEncrypted,
            byte[] coordsEncrypted);

        Task<DiffieHellmanMessagePackage> GetPlayer(string id, byte[] remotePublicKey);
    }
}