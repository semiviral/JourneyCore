using JourneyCoreLib;
using JourneyCoreLib.Drawing;
using JourneyCoreLib.Rendering.Environment.Tiling;
using JourneyCoreLib.Graphics;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using JourneyCoreLib.Core.Context.Entities;
using JourneyCoreLib.Game.Context;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JourneyCoreGame
{
    public class Program
    {


        private static WindowManager WManager { get; set; }
        private static ConsoleManager CManager { get; set; }

        private static TileMap SpawnMap { get; set; }

        private static JourneyCoreLib.Game.Context.Context GameContext { get; set; }

        public static Entity player { get; set; }

        static void Main(string[] args)
        {
            WManager = new WindowManager("Journey to the Core", new VideoMode(1920, 1080, 8), 144, new Vector2f(2f, 2f), 15f);
            CManager = new ConsoleManager();
            CManager.Hide(false);


            InitialiseSprites();

            SpawnMap = TileMap.LoadTileMap("AdventurersGuild", new Vector2i(8, 8), 1);

            GameContext = new JourneyCoreLib.Game.Context.Context(null, "GameContext", "game");

            Sprite human = new Sprite(new Texture(@"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\JourneyCoreGame\Assets\Images\Sprites\Human.png", new IntRect(0, 0, 32, 32)));

            player = new Entity(GameContext, WManager, "player", "player", human);

            Runtime();
        }

        public static void Runtime()
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

        private static void InitialiseSprites()
        {
            TileLoader.LoadTiles(@"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\JourneyCoreGame\Assets\Images\Util\Tiled_TileSheets\JourneyCore-MapTileSet.xml");
        }
    }
}
