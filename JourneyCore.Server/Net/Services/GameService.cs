using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using JourneyCore.Lib.Game.Environment.Mapping;
using JourneyCore.Lib.Game.Environment.Metadata;
using JourneyCore.Lib.Game.Environment.Tiling;
using JourneyCore.Lib.Game.Object.Entity;
using JourneyCore.Lib.System.Components.Loaders;
using JourneyCore.Lib.System.Net;
using JourneyCore.Lib.System.Net.Security;
using JourneyCore.Server.Net.SignalR.Contexts;
using Newtonsoft.Json;
using SFML.System;

namespace JourneyCore.Server.Net.Services
{
    public class GameService : IGameService
    {
        public GameService(IGameClientContext gameClientContext)
        {
            GameClientContext = gameClientContext;

            CryptoServices = new Dictionary<string, DiffieHellman>();
            TextureImages = new Dictionary<string, byte[]>();
            TileSets = new Dictionary<string, TileSet>();
            TileMaps = new Dictionary<string, Map>();
            Players = new List<Player>();

            TickRate = (int)(1f / 30f * 1000f);
        }


        #region CLIENT-TO-SERVER REQUESTS

        public Task ReceiveUpdatePackages(List<UpdatePackage> updatePackages)
        {
            return Task.CompletedTask;
        }

        #endregion


        #region VARIABLES

        private IGameClientContext GameClientContext { get; }
        private Dictionary<string, DiffieHellman> CryptoServices { get; }

        public Dictionary<string, byte[]> TextureImages { get; }

        /// <summary>
        ///     Time in milliseconds between the server's actions
        /// </summary>
        public int TickRate { get; }

        public bool Status { get; private set; }

        public List<Player> Players { get; }
        public Dictionary<string, Map> TileMaps { get; }
        public Dictionary<string, TileSet> TileSets { get; }

        #endregion


        #region INITIALISE

        private void InitialiseTextures()
        {
            foreach (string filePath in Directory.EnumerateFiles($@"{MapLoader.AssetRoot}/Images", "*.png",
                SearchOption.AllDirectories))
            {
                TextureImages.Add(Path.GetFileNameWithoutExtension(filePath).ToLower(), File.ReadAllBytes(filePath));
            }
        }

        private void InitialiseNonMapTileSets()
        {
            foreach (string filePath in Directory.EnumerateFiles($@"{MapLoader.AssetRoot}/TileSets", "*.json",
                SearchOption.AllDirectories))
            {
                TileSets.Add(Path.GetFileNameWithoutExtension(filePath).ToLower(),
                    TileSetLoader.LoadTileSet(filePath, 0));
            }
        }

        private void InitialiseTileMaps()
        {
            short scale = 2;

            foreach (string filePath in Directory.EnumerateFiles($@"{MapLoader.AssetRoot}/Maps", "*.json",
                SearchOption.TopDirectoryOnly))
            {
                Map map = MapLoader.LoadMap(filePath, scale);

                TileMaps.Add(Path.GetFileNameWithoutExtension(filePath), map);
            }
        }

        #endregion


        #region IGameService

        public DiffieHellmanKeyPackage RegisterDiffieHellman(string guid, byte[] clientPublicKey)
        {
            using (Aes aes = new AesCryptoServiceProvider())
            {
                CryptoServices.Add(guid, new DiffieHellman(clientPublicKey)
                {
                    IV = aes.IV
                });
            }

            return new DiffieHellmanKeyPackage(CryptoServices[guid].PublicKey, CryptoServices[guid].IV);
        }

        public async Task<byte[]> GetImage(string guid, byte[] remotePublicKey, byte[] textureNameEncrypted)
        {
            string textureName = await CryptoServices[guid].DecryptAsync(remotePublicKey, textureNameEncrypted);
            return TextureImages[textureName];
        }

        public async Task<TileSetMetadata> GetTileSetMetadata(string guid, byte[] remotePublicKey, byte[] tileSetNameEncrypted)
        {
            string tileSetName = await CryptoServices[guid].DecryptAsync(remotePublicKey, tileSetNameEncrypted);
            return TileSets[tileSetName].GetMetadata();
        }

        public async Task<MapMetadata> GetMapMetadata(string guid, byte[] remotePublicKey, byte[] mapNameEncrypted)
        {
            string mapName = await CryptoServices[guid].DecryptAsync(remotePublicKey, mapNameEncrypted);
            return TileMaps[mapName].GetMetadata();
        }

        public async Task<List<Chunk>> GetChunk(string guid, byte[] remotePublicKey, byte[] mapNameEncrypted, byte[] coordsEncrypted)
        {
            string mapName = await CryptoServices[guid].DecryptAsync(remotePublicKey, mapNameEncrypted);
            string coordsJson = await CryptoServices[guid].DecryptAsync(remotePublicKey, coordsEncrypted);
            Vector2i coords = JsonConvert.DeserializeObject<Vector2i>(coordsJson);

            // todo upgrade to C# 8.0 for (yield return in async)
            List<Chunk> chunks = new List<Chunk>();

            foreach (MapLayer layer in TileMaps[mapName].Layers)
            {
                chunks.Add(layer.Map[coords.X][coords.Y]);
            }

            return chunks;
        }

        #endregion


        #region IHostedService

        public Task StartAsync(CancellationToken cancellationToken)
        {
            InitialiseTextures();
            InitialiseNonMapTileSets();
            InitialiseTileMaps();

            Status = true;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}