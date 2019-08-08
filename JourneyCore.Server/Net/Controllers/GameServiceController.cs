using System;
using System.Threading.Tasks;
using JourneyCore.Lib.System.Net.Security;
using JourneyCore.Lib.System.Static;
using JourneyCore.Server.Net.Services;
using Microsoft.AspNetCore.Mvc;

namespace JourneyCore.Server.Net.Controllers
{
    public class GameServiceController : Controller
    {
        public GameServiceController(IGameService gameService)
        {
            GameService = gameService;
        }

        private IGameService GameService { get; }

        [HttpGet("gameservice/status")]
        public IActionResult GetStatus()
        {
            return new JsonResult(GameService.Status);
        }

        [HttpGet("gameservice/security/handshake")]
        public IActionResult GetDiffieHellmanKeys(string id, string htmlSafeBase64Ticket)
        {
            return new JsonResult(GameService.RegisterEncryptedConnection(id,
                EncryptionTicket.ConvertFromHtmlSafeBase64(htmlSafeBase64Ticket)));
        }

        [HttpGet("gameservice/tilesets")]
        public async Task<IActionResult> GetTileSet(string id, string remotePublicKeyBase64, string tileSetNameBase64)
        {
            byte[] _remotePublicKey = Convert.FromBase64String(remotePublicKeyBase64.HtmlDecodeBase64());
            byte[] _tileSetNameEncrypted = Convert.FromBase64String(tileSetNameBase64.HtmlDecodeBase64());

            return new JsonResult(await GameService.GetTileSetMetadata(id, _remotePublicKey, _tileSetNameEncrypted));
        }

        [HttpGet("gameservice/images")]
        public async Task<IActionResult> GetImage(string id, string remotePublicKeyBase64, string imageNameBase64)
        {
            byte[] _remotePublicKey = Convert.FromBase64String(remotePublicKeyBase64.HtmlDecodeBase64());
            byte[] _imageNameEncrypted = Convert.FromBase64String(imageNameBase64.HtmlDecodeBase64());

            return new JsonResult(await GameService.GetImage(id, _remotePublicKey, _imageNameEncrypted));
        }

        [HttpGet("gameservice/tickrate")]
        public IActionResult GetTickInterval()
        {
            return new JsonResult(GameService.TickRate);
        }

        [HttpGet("gameservice/playerData")]
        public async Task<IActionResult> GetPlayer(string id, string remotePublicKeyBase64)
        {
            byte[] _remotePublicKey = Convert.FromBase64String(remotePublicKeyBase64.HtmlDecodeBase64());

            return new JsonResult(await GameService.GetPlayer(id, _remotePublicKey));
        }
    }
}