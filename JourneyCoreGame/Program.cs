using JourneyCoreDisplay;
using JourneyCoreDisplay.Drawing;
using JourneyCoreDisplay.Environment;
using JourneyCoreDisplay.Sprites;
using JourneyCoreDisplay.Time;
using SFML.Graphics;
using SFML.System;
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

            SurfaceMap = Map.LoadMap("Surface_01", new Vector2i(8, 8), new Vector2i(8, 8), 3);

            Runtime();
        }

        public static void Runtime()
        {
            SurfaceMap.LoadChunkRange(0, 0, SurfaceMap.SizeInTiles.X / SurfaceMap.ChunkSize.X, SurfaceMap.SizeInTiles.Y / SurfaceMap.ChunkSize.Y);

            while (WManager.IsActive)
            {
                WManager.DrawItem(new DrawQueueItem(DrawPriority.Background, (fTime, window) =>
                {
                    window.Draw(SurfaceMap.VArray, new RenderStates(Map.MapTextures));
                }));

                AdjustFrameTime();

                WManager.UpdateWindow(ElapsedTime);
            }
        }

        private static void AdjustFrameTime()
        {
            if (ElapsedTime >= IndividualFrameTime)
            {
                return;
            }

            Task.Delay((int)(IndividualFrameTime - ElapsedTime));
        }

        private static void InitialiseSprites()
        {
            SpriteLoader.LoadSprite(SpriteType.Nothing, new WeightedSprite(100, 16, 16, 0, 0));
            SpriteLoader.LoadSprite(SpriteType.Grass, new WeightedSprite(150, 16, 16, 1, 0));
            SpriteLoader.LoadSprite(SpriteType.Grass, new WeightedSprite(5, 16, 16, 1, 1));
            SpriteLoader.LoadSprite(SpriteType.Grass, new WeightedSprite(5, 16, 16, 1, 2));
            SpriteLoader.LoadSprite(SpriteType.Grass, new WeightedSprite(2, 16, 16, 1, 3));
            SpriteLoader.LoadSprite(SpriteType.Grass, new WeightedSprite(2, 16, 16, 1, 4));
            SpriteLoader.LoadSprite(SpriteType.Dirt, new WeightedSprite(100, 16, 16, 2, 0));
            SpriteLoader.LoadSprite(SpriteType.Dirt, new WeightedSprite(100, 16, 16, 2, 1));
            SpriteLoader.LoadSprite(SpriteType.Dirt, new WeightedSprite(100, 16, 16, 2, 2));
            SpriteLoader.LoadSprite(SpriteType.Dirt, new WeightedSprite(100, 16, 16, 2, 3));
            SpriteLoader.LoadSprite(SpriteType.Stone, new WeightedSprite(100, 16, 16, 3, 0));
            SpriteLoader.LoadSprite(SpriteType.NexusPath, new WeightedSprite(100, 16, 16, 4, 0));
            SpriteLoader.LoadSprite(SpriteType.SurfacePath, new WeightedSprite(100, 16, 16, 5, 0));
            SpriteLoader.LoadSprite(SpriteType.SurfacePath, new WeightedSprite(100, 16, 16, 5, 1));
            SpriteLoader.LoadSprite(SpriteType.SurfacePath, new WeightedSprite(100, 16, 16, 5, 2));
        }
    }
}
