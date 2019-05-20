using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JourneyCore.Lib.Game.Context.Entities;
using JourneyCore.Lib.Graphics.Rendering.Environment.Chunking;
using JourneyCore.Lib.Graphics.Rendering.Environment.Tiling;
using JourneyCore.Lib.System;
using JourneyCore.Lib.System.Components.Loaders;
using JourneyCore.Server.Net.SignalR.Contexts;
using SFML.System;

namespace JourneyCore.Server.Net.SignalR.Services
{
    public class GameService : IGameService
    {
        public const ushort ChunkLoadRadius = 3;

        private const string AssetRoot = @"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\Assets";

        public GameService(IGameClientContext gameClientContext)
        {
            GameClientContext = gameClientContext;

            WorldInstances = new List<Instance>();
            TileMaps = new Dictionary<string, TileMap>();
            TileSets = new Dictionary<string, TileSet>();
            GameTextures = new Dictionary<string, byte[]>();
            Players = new List<Entity>();
        }

        private bool ServerStatus { get; set; }
        public Dictionary<string, TileSet> TileSets { get; }
        public Dictionary<string, byte[]> GameTextures { get; }
        public List<Entity> Players { get; }

        private IGameClientContext GameClientContext { get; }

        public List<Instance> WorldInstances { get; }
        public Dictionary<string, TileMap> TileMaps { get; }


        public Instance GetInstanceById(string id)
        {
            return WorldInstances.SingleOrDefault(instance => instance.Id.Equals(id));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            InitialiseGameTextures();
            InitialiseTileSets();
            InitialiseTileMaps();

            // start instances


            ServerStatus = true;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }


        #region INIT

        private void InitialiseGameTextures()
        {
            foreach (string filePath in Directory.EnumerateFiles($@"{AssetRoot}\Images", "*.png",
                SearchOption.AllDirectories))
                GameTextures.Add(Path.GetFileNameWithoutExtension(filePath), File.ReadAllBytes(filePath));
        }

        private void InitialiseTileMaps()
        {
            short scale = 2;

            foreach (string filePath in Directory.EnumerateFiles($@"{AssetRoot}\Maps", "*.xml",
                SearchOption.TopDirectoryOnly))
            {
                TileMap map = TileMapLoader.LoadMap(filePath, scale);
                TileMapLoader.BuildChunkMap(map);

                TileMaps.Add(Path.GetFileNameWithoutExtension(filePath), map);
            }
        }

        private void InitialiseTileSets()
        {
            foreach (string filePath in Directory.EnumerateFiles($@"{AssetRoot}\Maps\TileSets", "*.xml",
                SearchOption.TopDirectoryOnly))
                TileSets.Add(Path.GetFileNameWithoutExtension(filePath), TileSetLoader.LoadTileSet(filePath));
        }

        #endregion


        #region CLIENT-TO-SERVER REQUESTS

        public async Task SendServerStatus(string connectionId)
        {
            await GameClientContext.SendServerStatus(ServerStatus);
        }

        public async Task SendTextureList(string connectionId)
        {
            foreach ((string key, byte[] value) in GameTextures)
                await GameClientContext.SendTexture(connectionId, key, value);
        }

        public async Task SendChunks(string connectionId, string mapName, Vector2i playerChunk)
        {
            int minX = playerChunk.X - ChunkLoadRadius <= 0 ? 0 : playerChunk.X - ChunkLoadRadius;
            int minY = playerChunk.Y - ChunkLoadRadius <= 0 ? 0 : playerChunk.Y - ChunkLoadRadius;

            int width = minX + ChunkLoadRadius + 1;
            int height = minY + ChunkLoadRadius + 1;

            Chunk[][][] chunks = new Chunk[TileMaps[mapName].Layers.Length][][];

            for (int layer = 0; layer < TileMaps[mapName].Layers.Length; layer++)
            {
                chunks[layer] = new Chunk[width][];

                for (int x = minX; x < width; x++)
                {
                    chunks[layer][x] = new Chunk[height];
                    for (int y = minY; y < height; y++)
                        chunks[layer][x][y] = TileMaps[mapName].Layers[layer].ChunkMap[x][y];
                }
            }

            // TODO fix tilesetsource.First()
            // is temp solution
            List<short> usedTileIds = TileMaps[mapName].Layers.SelectMany(layer => layer.ChunkMap)
                .SelectMany(chunk => chunk).SelectMany(chunkData => chunkData.ChunkData).SelectMany(tileId => tileId)
                .Distinct().ToList();

            await GameClientContext.SendChunks(connectionId,
                Path.GetFileNameWithoutExtension(TileMaps[mapName].TileSetSources.First().Source), chunks,
                TileSets.SelectMany(tileSet => tileSet.Value.Tiles).Select(tile => tile)
                    .Where(tile => usedTileIds.Contains(tile.Id)).ToArray());
        }

        public Task ReceiveUpdatePackages(List<UpdatePackage> updatePackages)
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}