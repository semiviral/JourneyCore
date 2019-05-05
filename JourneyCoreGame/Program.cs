using JourneyCoreDisplay;
using JourneyCoreDisplay.Drawing;
using JourneyCoreDisplay.Environment;
using JourneyCoreDisplay.Sprites;
using JourneyCoreDisplay.Time;
using SFML.Graphics;
using SFML.System;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JourneyCoreGame
{
    public class Program
    {
        public static int TargetFps {
            get => _targetFps;
            set {
                // fps changed stuff

                _targetFps = value;
                IndividualFrameTime = 1f / _targetFps;
            }
        }
        private static int _targetFps;
        public static float IndividualFrameTime { get; private set; }

        private static WindowManager WManager { get; set; }
        private static ConsoleManager CManager { get; set; }

        private static Delta DeltaClock { get; set; }
        private static float ElapsedTime => DeltaClock.GetDelta();

        private static Map SurfaceMap { get; set; }

        static void Main(string[] args)
        {
            TargetFps = 60;

            WManager = new WindowManager("Journey to the Core", new Vector2f(2f, 2f), 15f);
            CManager = new ConsoleManager();
            CManager.Hide(false);

            DeltaClock = new Delta();

            InitialiseSprites();

            SurfaceMap = MapLoader.LoadMap("Surface_01", new Vector2i(8, 8), 2);

            Runtime();
        }

        public static void Runtime()
        {
            AdjustFrameTime();

            while (WManager.IsActive)
            {
                if (ElapsedTime < IndividualFrameTime)
                {
                    Task.Delay((int)(IndividualFrameTime - ElapsedTime));
                }


                WManager.DrawQueue.Add(new DrawQueueItem(DrawPriority.Background, (fTime, window) =>
                {
                    window.Draw(SurfaceMap.VArray, new RenderStates(Map.MapTextures));
                }));

                WManager.UpdateWindow(ElapsedTime);
            }
        }

        private static void AdjustFrameTime()
        {
        }

        private static void InitialiseSprites()
        {
            //string filePath = @"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\JourneyCoreGame\Assets\Images\Sprites\MapSpriteSheet.png";

            SpriteLoader.LoadSprite(0, 16, 16, 0, 0);
            SpriteLoader.LoadSprite(1, 16, 16, 1, 0);
            SpriteLoader.LoadSprite(2, 16, 16, 2, 0);
            SpriteLoader.LoadSprite(3, 16, 16, 3, 0);
            SpriteLoader.LoadSprite(4, 16, 16, 4, 0);
            SpriteLoader.LoadSprite(5, 16, 16, 5, 0);
        }
    }
}
