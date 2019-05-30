using JourneyCore.Server.Net.SignalR.Services;
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

        [HttpGet("gameservice/textures/{textureName}")]
        public IActionResult GetTexture(string textureName)
        {
            return new JsonResult(GameService.GetTexture(textureName));
        }
    }
}
