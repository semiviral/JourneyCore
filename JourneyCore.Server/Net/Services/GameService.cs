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
using JourneyCore.Lib.System.Net.Security;
using JourneyCore.Server.Net.SignalR.Contexts;
using Newtonsoft.Json;
using SFML.System;

namespace JourneyCore.Server.Net.Services
{
    public class GameService : IGameService
    {
        private CollisionQuad _PlayerQuad;

        public GameService(IGameClientContext gameClientContext)
        {
            GameClientContext = gameClientContext;

            CryptoServices = new Dictionary<string, DiffieHellman>();
            TextureImages = new Dictionary<string, byte[]>();
            TileSets = new Dictionary<string, TileSet>();
            TileMaps = new Dictionary<string, Map>();
            Players = new List<Player>();

            TickRate = (int) ((1f / 30f) * 1000f);
        }

        public DiffieHellmanMessagePackage PackageMessage(string connectionId, byte[] secretMessage)
        {
            return new DiffieHellmanMessagePackage(CryptoServices[connectionId].PublicKey, secretMessage);
        }

        #region CLIENT-TO-SERVER REQUESTS

        public async Task RelayConnectionId(string connectionId)
        {
            await GameClientContext.SendConnectionId(connectionId);
        }

        private object PlayerQuadLock { get; set; }

        private CollisionQuad PlayerQuad
        {
            get
            {
                lock (PlayerQuadLock)
                {
                    return _PlayerQuad;
                }
            }
            set
            {
                lock (PlayerQuadLock)
                {
                    _PlayerQuad = value;
                }
            }
        }

        public async Task ReceivePlayerPositions(string connectionId, IEnumerable<Vector2f> positions)
        {
            bool _hasAdjusted = false;

            foreach (Vector2f _position in positions)
            {
                PlayerQuad.Position = _position;

                // todo select map colliders dynamically
//                foreach (Vector2f adjustment in GraphMath.GetOverlaps(playerQuad, TileMaps.First().Value.Colliders))
//                {
//                    hasAdjusted = true;
//
//                    playerQuad.Position += adjustment;
//                }

                if (!_hasAdjusted)
                {
                    continue;
                }

                await GameClientContext.PlayerPositionModification(connectionId, PlayerQuad.Position);
            }
        }

        public Task ReceivePlayerRotations(string connectionId, IEnumerable<float> rotations)
        {
            PlayerQuad.Rotation = rotations.Last();

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
            foreach (string _filePath in Directory.EnumerateFiles($@"{MapLoader.ASSET_ROOT}/Images", "*.png",
                SearchOption.AllDirectories))
            {
                TextureImages.Add(Path.GetFileNameWithoutExtension(_filePath).ToLower(), File.ReadAllBytes(_filePath));
            }
        }

        private void InitialiseNonMapTileSets()
        {
            foreach (string _filePath in Directory.EnumerateFiles($@"{MapLoader.ASSET_ROOT}/TileSets", "*.json",
                SearchOption.AllDirectories))
            {
                TileSets.Add(Path.GetFileNameWithoutExtension(_filePath).ToLower(),
                    TileSetLoader.LoadTileSet(_filePath, 0));
            }
        }

        private void InitialiseTileMaps()
        {
            short _scale = 2;

            foreach (string _filePath in Directory.EnumerateFiles($@"{MapLoader.ASSET_ROOT}/Maps", "*.json",
                SearchOption.TopDirectoryOnly))
            {
                Map _map = MapLoader.LoadMap(_filePath, _scale);

                TileMaps.Add(Path.GetFileNameWithoutExtension(_filePath), _map);
            }
        }

        #endregion


        #region IGAMESERVICE

        public EncryptionTicket RegisterEncryptedConnection(string id, EncryptionTicket ticket)
        {
            using (Aes _aes = new AesCryptoServiceProvider())
            {
                CryptoServices.Add(id, new DiffieHellman(ticket.PublicKey)
                {
                    Iv = _aes.IV
                });
            }

            return new EncryptionTicket(CryptoServices[id].PublicKey, CryptoServices[id].Iv);
        }

        public async Task<DiffieHellmanMessagePackage> GetImage(string id, byte[] remotePublicKey,
            byte[] textureNameEncrypted)
        {
            string _textureName = await CryptoServices[id].DecryptAsync(remotePublicKey, textureNameEncrypted);
            string _serializedImageBytes = JsonConvert.SerializeObject(TextureImages[_textureName]);

            return new DiffieHellmanMessagePackage(CryptoServices[id].PublicKey,
                await CryptoServices[id].EncryptAsync(_serializedImageBytes));
        }

        public async Task<DiffieHellmanMessagePackage> GetTileSetMetadata(string id, byte[] remotePublicKey,
            byte[] tileSetNameEncrypted)
        {
            string _tileSetName = await CryptoServices[id].DecryptAsync(remotePublicKey, tileSetNameEncrypted);
            string _serializedTileSetMetadata = JsonConvert.SerializeObject(TileSets[_tileSetName].GetMetadata());

            return new DiffieHellmanMessagePackage(CryptoServices[id].PublicKey,
                await CryptoServices[id].EncryptAsync(_serializedTileSetMetadata));
        }

        public async Task<DiffieHellmanMessagePackage> GetMapMetadata(string id, byte[] remotePublicKey,
            byte[] mapNameEncrypted)
        {
            string _mapName = await CryptoServices[id].DecryptAsync(remotePublicKey, mapNameEncrypted);

            MapMetadata _mapMetadata = TileMaps[_mapName].GetMetadata();
            string _serializedMapMetadata = JsonConvert.SerializeObject(_mapMetadata);

            return new DiffieHellmanMessagePackage(CryptoServices[id].PublicKey,
                await CryptoServices[id].EncryptAsync(_serializedMapMetadata));
        }

        public async Task<DiffieHellmanMessagePackage> GetChunk(string id, byte[] remotePublicKey,
            byte[] mapNameEncrypted,
            byte[] coordsEncrypted)
        {
            string _mapName = await CryptoServices[id].DecryptAsync(remotePublicKey, mapNameEncrypted);
            string _coordsJson = await CryptoServices[id].DecryptAsync(remotePublicKey, coordsEncrypted);
            Vector2i _coords = JsonConvert.DeserializeObject<Vector2i>(_coordsJson);

            // todo upgrade to C# 8.0 for (yield return in async)
            List<Chunk> _chunks = new List<Chunk>();

            if ((_coords.X < 0) || (_coords.Y < 0) || (_coords.X > (TileMaps[_mapName].Layers[0].Map.Length - 1)) ||
                (_coords.Y > (TileMaps[_mapName].Layers[0].Map[0].Length - 1)))
            {
                return new DiffieHellmanMessagePackage(CryptoServices[id].PublicKey,
                    await CryptoServices[id]
                        .EncryptAsync(JsonConvert.SerializeObject(
                            new IndexOutOfRangeException($"Specified index: {_coords} out of map range."))));
            }

            foreach (MapLayer _layer in TileMaps[_mapName].Layers)
            {
                _chunks.Add(_layer.Map[_coords.X][_coords.Y]);
            }

            string _serializedChunksList = JsonConvert.SerializeObject(_chunks);

            return new DiffieHellmanMessagePackage(CryptoServices[id].PublicKey,
                await CryptoServices[id].EncryptAsync(_serializedChunksList));
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

                TileSetMetadata _playerMetadata = TileSets["avatar"].GetMetadata();
                CollisionQuad _collider = _playerMetadata.Tiles
                    .First(tile => (tile.TextureRect.Left == 3) && (tile.TextureRect.Top == 1))?
                    .Colliders.First();

                PlayerQuadLock = new object();
                PlayerQuad = _playerMetadata.Tiles
                    .First(tile => (tile.TextureRect.Left == 3) && (tile.TextureRect.Top == 1))?
                    .Colliders.First();

                // todo no hard coding for player texture size
                Player _player = new Player(TextureImages["avatar"], TextureImages["projectiles"], 0)
                {
                    Collider = _collider
                };

                Players.Add(_player);
            }
            catch (Exception _ex)
            {
            }

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