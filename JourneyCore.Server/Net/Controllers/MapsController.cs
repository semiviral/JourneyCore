using System;
using System.Threading.Tasks;
using JourneyCore.Lib.System.Static;
using JourneyCore.Server.Net.Services;
using Microsoft.AspNetCore.Mvc;

namespace JourneyCore.Server.Net.Controllers
{
    public class MapsController : Controller
    {
        private IGameService GameService { get; }

        public MapsController(IGameService gameService)
        {
            GameService = gameService;
        }

        [HttpGet("/maps/{mapNameBase64}")]
        public IActionResult GetChunkSpace(string guid, string remotePublicKeyBase64, string mapNameBase64, string coordsBase64)
        {
            byte[] remotePublicKey = Convert.FromBase64String(remotePublicKeyBase64.HtmlDecodeBase64());
            byte[] mapNameEncrypted = Convert.FromBase64String(mapNameBase64.HtmlDecodeBase64());
            byte[] coordsEncrypted = Convert.FromBase64String(coordsBase64.HtmlDecodeBase64());

            return new JsonResult(GameService.GetChunk(guid, remotePublicKey, mapNameEncrypted, coordsEncrypted));
        }

        [HttpGet("/maps/{mapNameBase64}/metadata")]
        public async Task<IActionResult> GetMapMetadata(string guid, string remotePublicKeyBase64, string mapNameBase64)
        {
            byte[] remotePublicKey = Convert.FromBase64String(remotePublicKeyBase64);
            byte[] mapNameEncrypted = Convert.FromBase64String(mapNameBase64);

            return new JsonResult(await GameService.GetMapMetadata(guid, remotePublicKey, mapNameEncrypted));
        }
    }
}