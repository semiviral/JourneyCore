using System;
using System.Threading.Tasks;
using JourneyCore.Lib.System.Net.Security;
using JourneyCore.Lib.System.Static;
using JourneyCore.Server.Net.Services;
using Microsoft.AspNetCore.Mvc;

namespace JourneyCore.Server.Net.Controllers
{
    public class MapsController : Controller
    {
        public MapsController(IGameService gameService)
        {
            GameService = gameService;
        }

        private IGameService GameService { get; }

        [HttpGet("/maps/{mapNameBase64}")]
        public async Task<IActionResult> GetChunkSpace(string id, string remotePublicKeyBase64, string mapNameBase64,
            string coordsBase64)
        {
            byte[] _remotePublicKey = Convert.FromBase64String(remotePublicKeyBase64.HtmlDecodeBase64());
            byte[] _mapNameEncrypted = Convert.FromBase64String(mapNameBase64.HtmlDecodeBase64());
            byte[] _coordsEncrypted = Convert.FromBase64String(coordsBase64.HtmlDecodeBase64());

            DiffieHellmanMessagePackage _encryptedMessagePackage;

            try
            {
                _encryptedMessagePackage =
                    await GameService.GetChunk(id, _remotePublicKey, _mapNameEncrypted, _coordsEncrypted);
            }
            catch (Exception _ex)
            {
                return new JsonResult(_ex);
            }

            return new JsonResult(_encryptedMessagePackage);
        }

        [HttpGet("/maps/{mapNameBase64}/metadata")]
        public async Task<IActionResult> GetMapMetadata(string id, string remotePublicKeyBase64, string mapNameBase64)
        {
            byte[] _remotePublicKey = Convert.FromBase64String(remotePublicKeyBase64.HtmlDecodeBase64());
            byte[] _mapNameEncrypted = Convert.FromBase64String(mapNameBase64.HtmlDecodeBase64());

            return new JsonResult(await GameService.GetMapMetadata(id, _remotePublicKey, _mapNameEncrypted));
        }
    }
}