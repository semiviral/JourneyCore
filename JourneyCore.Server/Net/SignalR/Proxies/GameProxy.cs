using System.Collections.Generic;
using System.Threading.Tasks;
using JourneyCore.Lib.System;
using JourneyCore.Server.Net.SignalR.Contexts;
using JourneyCore.Server.Net.SignalR.Services;
using SFML.System;

namespace JourneyCore.Server.Net.SignalR.Proxies
{
    public class GameProxy : IGameProxy
    {
        #region MEMBERS

        private IGameService GameService { get; }
        private IGameClientContext GameClientContext { get; }

        #endregion

        public GameProxy(IGameService gameService, IGameClientContext gameClientContext)
        {
            GameService = gameService;
            GameClientContext = gameClientContext;
        }



        #region CLIENT TO SERVER

        public async Task RequestServerStatus(string connectionId)
        {
            await GameService.SendServerStatus(connectionId);
        }

        public async Task RequestTextureList(string connectionId)
        {
            await GameService.SendTextureList(connectionId);
        }

        public async Task RequestTileMap(string connectionId, string tileMapName)
        {
            await GameService.SendTileMap(connectionId, tileMapName);
        }

        public async Task ReceiveUpdatePackages(List<UpdatePackage> updatePackages)
        {
            await GameService.ReceiveUpdatePackages(updatePackages);
        }

        #endregion
    }
}
