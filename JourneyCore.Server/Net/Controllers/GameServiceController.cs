using System;
using System.Threading.Tasks;
using JourneyCore.Lib.System.Static;
using JourneyCore.Server.Net.Services;
using Microsoft.AspNetCore.Mvc;

namespace JourneyCore.Server.Net.Controllers
{
    public class GameServiceController : Controller
    {
        private IGameService GameService { get; }

        public GameServiceController(IGameService gameService)
        {
            GameService = gameService;
        }

        [HttpGet("gameservice/status")]
        public IActionResult GetStatus()
        {
            return new JsonResult(GameService.Status);
        }

        [HttpGet("gameservice/security/handshake")]
        public IActionResult GetDiffieHellmanKeys(string guid, string clientPublicKeyBase64)
        {
            return new JsonResult(GameService.RegisterDiffieHellman(guid,
                Convert.FromBase64String(clientPublicKeyBase64.HtmlDecodeBase64())));
        }

        [HttpGet("gameservice/tilesets")]
        public async Task<IActionResult> GetTileSet(string guid, string remotePublicKeyBase64, string tileSetNameBase64)
        {
            byte[] remotePublicKey = Convert.FromBase64String(remotePublicKeyBase64.HtmlDecodeBase64());
            byte[] tileSetNameEncrypted = Convert.FromBase64String(tileSetNameBase64.HtmlDecodeBase64());

            return new JsonResult(await GameService.GetTileSetMetadata(guid, remotePublicKey, tileSetNameEncrypted));
        }

        [HttpGet("gameservice/images")]
        public async Task<IActionResult> GetImage(string guid, string remotePublicKeyBase64, string imageNameBase64)
        {
            byte[] remotePublicKey = Convert.FromBase64String(remotePublicKeyBase64.HtmlDecodeBase64());
            byte[] imageNameEncrypted = Convert.FromBase64String(imageNameBase64.HtmlDecodeBase64());

            return new JsonResult(await GameService.GetImage(guid, remotePublicKey, imageNameEncrypted));
        }

        [HttpGet("gameservice/tickrate")]
        public IActionResult GetTickInterval()
        {
            return new JsonResult(GameService.TickRate);
        }
    }
}