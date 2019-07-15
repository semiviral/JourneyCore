using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using JourneyCore.Lib.Game.Environment.Mapping;
using JourneyCore.Lib.Game.Environment.Metadata;
using JourneyCore.Lib.Game.Environment.Tiling;
using JourneyCore.Lib.Game.Object.Collision;
using JourneyCore.Lib.Game.Object.Entity;
using JourneyCore.Lib.System.Loaders;
using JourneyCore.Lib.System.Math;
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

        public void CollisionCheck()
        {
            foreach (Map map in TileMaps.Values)
            {
                foreach (CollisionQuad mobileCollider in map.Colliders.Where(collider => collider.Mobile))
                {
                    foreach (CollisionQuad quad in map.Colliders.Where(collider => !collider.Mobile))
                    {
                        foreach (Vector2f displacement in GraphMath.DiagnasticCollision(mobileCollider, quad))
                        {
                            mobileCollider.FlagCollision(this, displacement);
                        }
                    }
                }
            }
        }

        public DiffieHellmanMessagePackage PackageMessage(string connectionId, byte[] secretMessage)
        {
            return new DiffieHellmanMessagePackage(CryptoServices[connectionId].PublicKey, secretMessage);
        }

        #region CLIENT-TO-SERVER REQUESTS

        public async Task RelayReadyStatus(string connectionId)
        {
            await GameClientContext.SendServerStatus(connectionId, Status);
        }

        public async Task RelayConnectionId(string connectionId)
        {
            await GameClientContext.SendConnectionId(connectionId);
        }

        public Task ReceiveUpdatePackages(string connectionId, List<UpdatePackage> updatePackages)
        {
            foreach (UpdatePackage pacakage in updatePackages)
            {
                if (pacakage.UpdateType != StateUpdateType.Position) { }
            }

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


        #region IGAMESERVICE

        public EncryptionTicket RegisterEncryptedConnection(string id, EncryptionTicket ticket)
        {
            using (Aes aes = new AesCryptoServiceProvider())
            {
                CryptoServices.Add(id, new DiffieHellman(ticket.PublicKey)
                {
                    IV = aes.IV
                });
            }

            return new EncryptionTicket(CryptoServices[id].PublicKey, CryptoServices[id].IV);
        }

        public async Task<DiffieHellmanMessagePackage> GetImage(string id, byte[] remotePublicKey,
            byte[] textureNameEncrypted)
        {
            string textureName = await CryptoServices[id].DecryptAsync(remotePublicKey, textureNameEncrypted);
            string serializedImageBytes = JsonConvert.SerializeObject(TextureImages[textureName]);

            return new DiffieHellmanMessagePackage(CryptoServices[id].PublicKey,
                await CryptoServices[id].EncryptAsync(serializedImageBytes));
        }

        public async Task<DiffieHellmanMessagePackage> GetTileSetMetadata(string id, byte[] remotePublicKey,
            byte[] tileSetNameEncrypted)
        {
            string tileSetName = await CryptoServices[id].DecryptAsync(remotePublicKey, tileSetNameEncrypted);
            string serializedTileSetMetadata = JsonConvert.SerializeObject(TileSets[tileSetName].GetMetadata());

            return new DiffieHellmanMessagePackage(CryptoServices[id].PublicKey,
                await CryptoServices[id].EncryptAsync(serializedTileSetMetadata));
        }

        public async Task<DiffieHellmanMessagePackage> GetMapMetadata(string id, byte[] remotePublicKey,
            byte[] mapNameEncrypted)
        {
            string mapName = await CryptoServices[id].DecryptAsync(remotePublicKey, mapNameEncrypted);
            string serializedMapMetadata = JsonConvert.SerializeObject(TileMaps[mapName].GetMetadata());

            return new DiffieHellmanMessagePackage(CryptoServices[id].PublicKey,
                await CryptoServices[id].EncryptAsync(serializedMapMetadata));
        }

        public async Task<DiffieHellmanMessagePackage> GetChunk(string id, byte[] remotePublicKey,
            byte[] mapNameEncrypted,
            byte[] coordsEncrypted)
        {
            string mapName = await CryptoServices[id].DecryptAsync(remotePublicKey, mapNameEncrypted);
            string coordsJson = await CryptoServices[id].DecryptAsync(remotePublicKey, coordsEncrypted);
            Vector2i coords = JsonConvert.DeserializeObject<Vector2i>(coordsJson);

            // todo upgrade to C# 8.0 for (yield return in async)
            List<Chunk> chunks = new List<Chunk>();

            if (coords.X < 0 || coords.Y < 0 || coords.X > TileMaps[mapName].Layers[0].Map.Length - 1 ||
                coords.Y > TileMaps[mapName].Layers[0].Map[0].Length - 1)
            {
                return new DiffieHellmanMessagePackage(CryptoServices[id].PublicKey,
                    await CryptoServices[id]
                        .EncryptAsync(JsonConvert.SerializeObject(
                            new IndexOutOfRangeException($"Specified index: {coords} out of map range."))));
            }

            foreach (MapLayer layer in TileMaps[mapName].Layers)
            {
                chunks.Add(layer.Map[coords.X][coords.Y]);
            }

            string serializedChunksList = JsonConvert.SerializeObject(chunks);

            return new DiffieHellmanMessagePackage(CryptoServices[id].PublicKey,
                await CryptoServices[id].EncryptAsync(serializedChunksList));
        }

        public async Task<DiffieHellmanMessagePackage> GetPlayer(string id, byte[] remotePublicKey)
        {
            return new DiffieHellmanMessagePackage(CryptoServices[id].PublicKey,
                await CryptoServices[id].EncryptAsync(JsonConvert.SerializeObject(Players.First())));
        }

        #endregion


        #region IHOSTEDSERVICE

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                InitialiseTextures();
                InitialiseNonMapTileSets();
                InitialiseTileMaps();

                TileSetMetadata playerMetadata = TileSets["avatar"].GetMetadata();
                CollisionQuad collider = playerMetadata.Tiles
                    .First(tile => tile.TextureRect.Left == 3 && tile.TextureRect.Top == 1)?
                    .Colliders.First();

                // todo no hard coding for player texture size
                Player player = new Player(TextureImages["avatar"], TextureImages["projectiles"], 0)
                {
                    Collider = collider
                };

                Players.Add(player);
            }
            catch (Exception ex) { }

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