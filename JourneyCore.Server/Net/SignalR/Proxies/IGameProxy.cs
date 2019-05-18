using System.Collections.Generic;
using System.Threading.Tasks;
using JourneyCore.Lib.System;
using SFML.System;

namespace JourneyCore.Server.Net.SignalR.Proxies
{
    public interface IGameProxy
    {
        Task ReceiveUpdatePackages(List<UpdatePackage> updatePackages);
        Task RequestServerStatus(string connectionId);
        Task RequestTextureList(string connectionId);
        Task RequestTileMap(string connectionId, string tileMapName);
    }
}