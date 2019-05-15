using System.Collections.Generic;
using JourneyCoreLib.Rendering.Environment.Tiling;
using JourneyCoreServer.Loaders;
using RESTModule;
using RESTModule.ComponentModel;
using SFML.System;

namespace JourneyCoreServer
{
    public class Server
    {
        public Server()
        {
            Clients = new List<RESTClient>();

            InitialiseTileMapLoader();
            InitialiseTileSpriteLoader();
        }

        private void InitialiseTileMapLoader()
        {
            TileMapLoader.TileScale = 2f;
            TileMapLoader.ChunkSize = new Vector2i(8, 8);
            TileMapLoader.LoadMap("AdventurersGuild");
        }

        private void InitialiseTileSpriteLoader()
        {
            TileSpriteLoader.LoadTiles("JourneyCore-MapTiles");

        }

        public void UpdateClientStates()
        {
            string updatedState = string.Empty;

            foreach (RESTClient client in Clients)
            {
                client.Request(RequestMethod.POST, updatedState);
            }
        }
    }
}
