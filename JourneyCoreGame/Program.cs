using JourneyCoreDisplay;
using JourneyCoreDisplay.Drawing;
using JourneyCoreDisplay.Environment;
using JourneyCoreDisplay.Sprites;
using JourneyCoreDisplay.Time;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JourneyCoreGame
{
    public class Program
    {


        private static WindowManager WManager { get; set; }
        private static ConsoleManager CManager { get; set; }

        private static Map SpawnMap { get; set; }

        static void Main(string[] args)
        {
            WManager = new WindowManager("Journey to the Core", new VideoMode(800, 800, 8), 60, new Vector2f(2f, 2f), 15f);
            CManager = new ConsoleManager();
            CManager.Hide(false);


            InitialiseSprites();

            SpawnMap = Map.LoadMap("AdventurersGuild", new Vector2i(8, 8), 1);

            Runtime();
        }

        public static void Runtime()
        {
            SpawnMap.LoadChunkRange(0, 0, 96, 96);

            //Shader transparency = new Shader(null, null, @"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\JourneyCoreGame\Assets\Shaders\transparency.frag");
            //transparency.SetUniform("opacity", 0.5f);
            //transparency.SetUniform("texture", Map.MapTextures);

            //RenderStates overlayStates = new RenderStates
            //{
            //    BlendMode = BlendMode.Alpha,
            //    Shader = transparency,
            //    Texture = Map.MapTextures
            //};

            WManager.DrawItem(new DrawQueueItem(DrawPriority.Background, (fTime, window) =>
            {
                window.Draw(SpawnMap.VArray, new RenderStates(Map.MapTextures));
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
