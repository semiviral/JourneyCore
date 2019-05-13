using JourneyCoreLib.Core.Context.Entities;
using JourneyCoreLib.Drawing;
using JourneyCoreLib.Rendering.Environment.Tiling;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace JourneyCoreLib.Game
{
    public class GameLoop
    {
        public static Vector2i MapTileSize { get; private set; }

        private WindowManager WManager { get; set; }

        private TileMap SpawnMap { get; set; }

        private Context.Context GameContext { get; set; }

        public Entity player { get; set; }


        public GameLoop()
        {
            GameContext = new Context.Context(null, "GameContext", "game");
            
            WManager = new WindowManager("Journey to the Core", new VideoMode(1920, 1080, 8), 300, new Vector2f(2f, 2f), 15f);


            InitialiseTiles();

            SpawnMap = TileMap.LoadTileMap("AdventurersGuild", new Vector2i(8, 8), 1);
            MapTileSize = new Vector2i(SpawnMap.PixelTileWidth, SpawnMap.PixelTileHeight);


            Sprite human = new Sprite(new Texture(@"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\JourneyCoreGame\Assets\Images\Sprites\Human.png", new IntRect(0, 0, 32, 32)));

            player = new Entity(GameContext, WManager, "player", "player", human);
        }

        private void InitialiseTiles()
        {
            TileLoader.LoadTiles(@"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\JourneyCoreGame\Assets\Images\Util\Tiled_TileSheets\JourneyCore-MapTileSet.xml");

        }

        public void Runtime()
        {
            SpawnMap.LoadChunkRange(0, 0, 16, 16);

            //Shader transparency = new Shader(null, null, @"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\JourneyCoreGame\Assets\Shaders\transparency.frag");
            //transparency.SetUniform("opacity", 0.5f);
            //transparency.SetUniform("texture", Map.MapTextures);

            RenderStates overlayStates = new RenderStates(TileMap.MapTextures);

            WManager.DrawPersistent(new DrawQueueItem(DrawPriority.Background, (fTime, window) =>
            {
                window.Draw(SpawnMap.VArray, overlayStates);
            }));

            while (WManager.IsActive)
            {
                WManager.UpdateWindow();
            }
        }
    }
}
