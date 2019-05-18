using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JourneyCore.Lib.Graphics.Rendering.Environment.Tiling;
using JourneyCore.Lib.System;
using JourneyCore.Lib.System.Components.Loaders;
using JourneyCore.Server.Net.SignalR.Contexts;
using JourneyCoreLib.Game.Context.Entities;
using SFML.System;
using SFML.Window;

namespace JourneyCore.Server.Net.SignalR.Services
{
    public class GameService : IGameService
    {
        private const string AssetRoot = @"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\Assets";
        private bool ServerStatus { get; set; }

        public List<Instance> WorldInstances { get; }
        public Dictionary<string, TileMap> TileMaps { get; }
        public Dictionary<string, byte[]> GameTextures { get; }
        public List<Entity> Players { get; private set; }

        private IGameClientContext GameClientContext { get; }

        public GameService(IGameClientContext gameClientContext)
        {
            GameClientContext = gameClientContext;

            WorldInstances = new List<Instance>();
            TileMaps = new Dictionary<string, TileMap>();
            GameTextures = new Dictionary<string, byte[]>();
            Players = new List<Entity>();
        }


        #region INIT

        private void InitialiseGameTextures()
        {
            foreach (string filePath in Directory.EnumerateFiles($@"{AssetRoot}\Images", "*.png", SearchOption.AllDirectories))
            {
                GameTextures.Add(Path.GetFileNameWithoutExtension(filePath), File.ReadAllBytes(filePath));
            }
        }

        private void InitialiseTileMaps()
        {
            int scale = 2;

            foreach (string filePath in Directory.EnumerateFiles($@"{AssetRoot}\Maps", "*.xml", SearchOption.TopDirectoryOnly))
            {
                TileMaps.Add(Path.GetFileNameWithoutExtension(filePath), TileMapLoader.LoadMap(filePath, scale));
            }
        }

        #endregion



        public Instance GetInstanceById(string id)
        {
            return WorldInstances.SingleOrDefault(instance => instance.Id.Equals(id));
        }

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
            throw new System.NotImplementedException();
        }



        #region CLIENT-TO-SERVER REQUESTS

        public async Task SendServerStatus(string connectionId)
        {
            await GameClientContext.SendServerStatus(ServerStatus);
        }

        public async Task SendTextureList(string connectionId)
        {
            foreach (KeyValuePair<string, byte[]> kvp in GameTextures)
            {
                await GameClientContext.SendTexture(connectionId, kvp.Key, kvp.Value);
            }
        }

        public async Task SendTileMap(string connectionId, string tileMapName)
        {
            await GameClientContext.SendTileMap(connectionId, TileMaps[tileMapName]);
        }

        public Task ReceiveUpdatePackages(List<UpdatePackage> updatePackages)
        {
            return Task.CompletedTask;
        }

        #endregion  
    }
}
