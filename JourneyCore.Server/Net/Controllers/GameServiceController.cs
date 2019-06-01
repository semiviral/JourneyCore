using JourneyCore.Server.Net.SignalR.Services;
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

        [HttpGet("gameservice/textures/{textureName}")]
        public IActionResult GetTexture(string textureName)
        {
            return new JsonResult(GameService.GetTexture(textureName));
        }

        [HttpGet("gameservice/tickrate")]
        public IActionResult GetTickInterval()
        {
            return new JsonResult(GameService.TickRate);
        }
    }
}