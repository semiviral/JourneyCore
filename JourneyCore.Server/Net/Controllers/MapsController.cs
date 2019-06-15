using System.Linq;
using JourneyCore.Server.Net.Services;
using Microsoft.AspNetCore.Mvc;
using SFML.System;

namespace JourneyCore.Server.Net.Controllers
{
    public class MapsController : Controller
    {
        private IGameService GameService { get; }

        public MapsController(IGameService gameService)
        {
            GameService = gameService;
        }

        // todo implement handshakes of some sort to verify client authenticity
        // GET: GameService/GetChunks/MapName?coordX=000,coordY=000
        [HttpGet("/maps/{mapName}")]
        public IActionResult GetChunkSpace(string mapName, int x, int y)
        {
            return new JsonResult(GameService.GetChunk(mapName, new Vector2i(x, y)).ToArray());
        }

        [HttpGet("/maps/{mapName}/metadata")]
        public IActionResult GetMapMetadata(string mapName)
        {
            return new JsonResult(GameService.GetMapMetadata(mapName));
        }
    }
}