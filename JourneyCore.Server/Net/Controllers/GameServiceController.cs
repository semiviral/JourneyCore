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

        [HttpGet("gameservice/tilesets/{tileSetName}")]
        public IActionResult GetTileSet(string tileSetName)
        {
            return new JsonResult(GameService.GetTileSetMetadata(tileSetName));
        }

        [HttpGet("gameservice/images/{imageName}")]
        public IActionResult GetImage(string imageName)
        {
            return new JsonResult(GameService.GetImage(imageName));
        }

        [HttpGet("gameservice/tickrate")]
        public IActionResult GetTickInterval()
        {
            return new JsonResult(GameService.TickRate);
        }
    }
}