using System.Collections.Generic;
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
        }

        private IGameService GameService { get; }

        /// <summary>
        ///     Method called when hub connection is created
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        #region CLIENT-TO-SERVER RELAY METHODS

        public async Task ReceiveUpdatePackages(List<UpdatePackage> updatePackages)
        {
            await GameService.ReceiveUpdatePackages(updatePackages);
        }

        #endregion
    }
}