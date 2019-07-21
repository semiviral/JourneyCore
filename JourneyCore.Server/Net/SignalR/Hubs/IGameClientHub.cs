using System.Collections.Generic;
using System.Threading.Tasks;
using SFML.System;

namespace JourneyCore.Server.Net.SignalR.Hubs
{
    public interface IGameClientHub
    {
        Task RequestConnectionId();
        Task ReceivePlayerPositions(IEnumerable<Vector2f> positions);
        Task ReceivePlayerRotations(IEnumerable<float> rotations);
    }
}