using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JourneyCore.Lib.System;
using JourneyCore.Server.Net.SignalR.Proxies;
using Microsoft.AspNetCore.SignalR;
using SFML.System;
using SFML.Window;

namespace JourneyCore.Server.Net.SignalR.Hubs
{
    public class GameClientHub : Hub<IGameClientHub>
    {
        private IGameProxy GameProxy { get; }
        private CancellationToken IsCancelled { get; }

        public GameClientHub(IGameProxy gameProxy)
        {
            GameProxy = gameProxy;
            IsCancelled = new CancellationToken(false);
        }

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
            await GameProxy.RequestServerStatus(Context.ConnectionId);
        }

        public async Task RequestTextureList()
        {
            await GameProxy.RequestTextureList(Context.ConnectionId);
        }

        public async Task RequestTileMap(string tileMapName)
        {
            await GameProxy.RequestTileMap(Context.ConnectionId, tileMapName);
        }

        public async Task ReceiveUpdatePackages(List<UpdatePackage> updatePackages)
        {
            await GameProxy.ReceiveUpdatePackages(updatePackages);
        }

        #endregion
    }
}
