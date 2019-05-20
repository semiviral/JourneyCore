using System.Collections.Generic;
using System.Threading.Tasks;
using JourneyCore.Lib.System;
using SFML.System;

namespace JourneyCore.Server.Net.SignalR.Hubs
{
    public interface IGameClientHub
    {
        Task RequestServerStatus();
        Task RequestTextureList();
        Task RequestChunks(string tileMapName, Vector2i playerChunk);
        Task ReceiveUpdatePackages(List<UpdatePackage> updatePackages);
    }
}