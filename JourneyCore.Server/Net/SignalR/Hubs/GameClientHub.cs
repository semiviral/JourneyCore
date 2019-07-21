using System.Collections.Generic;
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

        public async Task ReceivePlayerPositions(IEnumerable<Vector2f> positions)
        {
            await GameService.ReceivePlayerPositions(Context.ConnectionId, positions);
        }

        public async Task ReceivePlayerRotations(IEnumerable<float> rotations)
        {
            await GameService.ReceivePlayerRotations(Context.ConnectionId, rotations);
        }

        #endregion
    }
}