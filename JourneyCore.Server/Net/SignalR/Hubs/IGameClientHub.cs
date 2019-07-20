using System.Threading.Tasks;
using SFML.System;

namespace JourneyCore.Server.Net.SignalR.Hubs
{
    public interface IGameClientHub
    {
        Task RequestReadyStatus();
        Task RequestConnectionId();
        Task ReceivePlayerMovement(Vector2f movement);
        Task ReceivePlayerRotation(float rotation);
    }
}