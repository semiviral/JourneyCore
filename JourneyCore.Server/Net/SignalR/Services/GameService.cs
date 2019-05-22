using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JourneyCore.Lib.Game.Context.Entities;
using JourneyCore.Lib.Graphics.Rendering.Environment.Tiling;
using JourneyCore.Lib.System;
using JourneyCore.Lib.System.Components.Loaders;
using JourneyCore.Server.Net.SignalR.Contexts;

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
            TileMaps = new Dictionary<string, Map>();
            GameTextures = new Dictionary<string, byte[]>();
            Players = new List<Entity>();
        }

        private bool ServerStatus { get; set; }
        public Dictionary<string, byte[]> GameTextures { get; }
        public List<Entity> Players { get; }

        private IGameClientContext GameClientContext { get; }

        public List<Instance> WorldInstances { get; }
        public Dictionary<string, Map> TileMaps { get; }


        public Instance GetInstanceById(string id)
        {
            return WorldInstances.SingleOrDefault(instance => instance.Id.Equals(id));
        }


        #region INTERFACES IMPLEMENTATIONS

        public Task StartAsync(CancellationToken cancellationToken)
        {
            InitialiseGameTextures();
            InitialiseTileMaps();

            // start instances


            ServerStatus = true;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        #endregion


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
                Map map = MapLoader.LoadMap(filePath, scale);

                TileMaps.Add(Path.GetFileNameWithoutExtension(filePath), map);
            }
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

        public async Task SendMap(string connectionId, string mapName)
        {
            await GameClientContext.SendMap(connectionId, TileMaps[mapName].TileSetSources.First().Source,
                TileMaps[mapName]);
        }

        public Task ReceiveUpdatePackages(List<UpdatePackage> updatePackages)
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}