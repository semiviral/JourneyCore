using System.Threading.Tasks;
using JourneyCore.Server.Net.Services;
using Microsoft.AspNetCore.SignalR;
using SFML.System;

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

        public async Task RequestConnectionId()
        {
            await GameService.RelayConnectionId(Context.ConnectionId);
        }

        public async Task ReceivePlayerMovement(Vector2f movement)
        {
            await GameService.ReceivePlayerMovement(Context.ConnectionId, movement);
        }

        public async Task ReceivePlayerRotation(float rotation)
        {
            await GameService.ReceivePlayerRotation(Context.ConnectionId, rotation);
        }

        #endregion
    }
}