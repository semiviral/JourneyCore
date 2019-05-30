using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JourneyCore.Lib.Game.Context.Entities;
using JourneyCore.Lib.Game.Environment.Mapping;
using JourneyCore.Lib.Game.Environment.Metadata;
using JourneyCore.Lib.System;
using JourneyCore.Lib.System.Components.Loaders;
using JourneyCore.Server.Net.SignalR.Contexts;
using SFML.System;

namespace JourneyCore.Server.Net.SignalR.Services
{
    public class GameService : IGameService
    {
        public const ushort ChunkLoadRadius = 3;


        public GameService(IGameClientContext gameClientContext)
        {
            GameClientContext = gameClientContext;

            TextureImages = new Dictionary<string, byte[]>();
            TileMaps = new Dictionary<string, Map>();
            Players = new List<Entity>();
        }

        public bool Status { get; private set; }
        private IGameClientContext GameClientContext { get; }

        public List<Entity> Players { get; }
        public Dictionary<string, byte[]> TextureImages { get; }
        public Dictionary<string, Map> TileMaps { get; }


        #region INITIALISE

        private void InitialiseTextures()
        {
            foreach (string filePath in Directory.EnumerateFiles($@"{MapLoader.AssetRoot}\Images", "*.png", SearchOption.AllDirectories))
            {
                TextureImages.Add(Path.GetFileNameWithoutExtension(filePath).ToLower(), File.ReadAllBytes(filePath));
            }
        }

        private void InitialiseTileMaps()
        {
            short scale = 2;

            foreach (string filePath in Directory.EnumerateFiles($@"{MapLoader.AssetRoot}\Maps", "*.json",
                SearchOption.TopDirectoryOnly))
            {
                Map map = MapLoader.LoadMap(filePath, scale);

                TileMaps.Add(Path.GetFileNameWithoutExtension(filePath), map);
            }
        }

        #endregion


        #region INTERFACES IMPLEMENTATIONS

        public Task StartAsync(CancellationToken cancellationToken)
        {
            InitialiseTextures();
            InitialiseTileMaps();

            Status = true;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        #endregion


        #region CLIENT-TO-SERVER REQUESTS

        public Task ReceiveUpdatePackages(List<UpdatePackage> updatePackages)
        {
            return Task.CompletedTask;
        }

        #endregion

        public byte[] GetTexture(string textureName)
        {
            return TextureImages[textureName];
        }

        public MapMetadata GetMapMetadata(string mapName)
        {
            return TileMaps[mapName].GetMetadata();
        }

        public IEnumerable<Chunk> GetChunk(string mapName, Vector2i chunkCoords)
        {
            foreach (MapLayer layer in TileMaps[mapName].Layers)
            {
                yield return layer.Map[chunkCoords.X][chunkCoords.Y];
            }
        }
    }
}