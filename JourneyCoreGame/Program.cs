using JourneyCoreDisplay;
using JourneyCoreDisplay.Drawing;
using JourneyCoreDisplay.Environment;
using JourneyCoreDisplay.Sprites;
using JourneyCoreDisplay.Time;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace JourneyCoreGame
{
    public class Program
    {


        private static WindowManager WManager { get; set; }
        private static ConsoleManager CManager { get; set; }

        private static TileMap SpawnMap { get; set; }

        static void Main(string[] args)
        {
            WManager = new WindowManager("Journey to the Core", new VideoMode(1920, 1080, 8), 60, new Vector2f(2f, 2f), 15f);
            CManager = new ConsoleManager();
            CManager.Hide(false);


            InitialiseSprites();

            SpawnMap = TileMap.LoadTileMap("AdventurersGuild", new Vector2i(8, 8), 1);

            Runtime();
        }

        public static void Runtime()
        {
            SpawnMap.LoadChunkRange(0, 0, 2, 2);

            //Shader transparency = new Shader(null, null, @"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\JourneyCoreGame\Assets\Shaders\transparency.frag");
            //transparency.SetUniform("opacity", 0.5f);
            //transparency.SetUniform("texture", Map.MapTextures);

            RenderStates overlayStates = new RenderStates(TileMap.MapTextures);


            WManager.DrawItem(new DrawQueueItem(DrawPriority.Background, (fTime, window) =>
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
