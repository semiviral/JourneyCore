using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JourneyCore.Lib.System;
using JourneyCore.Server.Net.SignalR.Services;
using Microsoft.AspNetCore.SignalR;

namespace JourneyCore.Server.Net.SignalR.Hubs
{
    public class GameClientHub : Hub<IGameClientHub>
    {
        public GameClientHub(IGameService gameService)
        {
            GameService = gameService;
            IsCancelled = new CancellationToken(false);
        }

        private IGameService GameService { get; }
        private CancellationToken IsCancelled { get; }

        /// <summary>
        ///     Method called when hub connection is created
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        #region CLIENT-TO-SERVER RELAY METHODS

        public async Task RequestServerStatus()
        {
            await GameService.SendServerStatus(Context.ConnectionId);
        }

        public async Task RequestTextureList()
        {
            await GameService.SendTextureList(Context.ConnectionId);
        }

        public async Task RequestMap(string tileMapName)
        {
            await GameService.SendMap(Context.ConnectionId, tileMapName);
        }

        public async Task ReceiveUpdatePackages(List<UpdatePackage> updatePackages)
        {
            await GameService.ReceiveUpdatePackages(updatePackages);
        }

        #endregion
    }
}