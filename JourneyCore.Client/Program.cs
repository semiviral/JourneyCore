using System;
using JourneyCore.Lib.Game.Object.Collision;
using Serilog;
using SFML.Graphics;

namespace JourneyCore.Client
{
    public static class Program
    {
        private static GameLoop GLoop { get; set; }

        private static void Main()
        {
            try
            {
                CollisionTest();

                InitialiseStaticLogger();

                GLoop = new GameLoop(60);
                GLoop.Initialise("http://localhost:5000", "GameService");
                GLoop.Start();
            }
            catch (Exception ex)
            {
                GameLoop.CallFatality(ex.Message);
            }
        }

        private static void InitialiseStaticLogger()
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
        }

        private static void CollisionTest()
        {
            CollisionQuad quad1 = new CollisionQuad(new FloatRect(0f, 0f, 10f, 10f), 0f);
            CollisionQuad quad2 = new CollisionQuad(new FloatRect(5f, 5f, 10f, 10f), 45f);

            FloatRect overlap = new FloatRect();

            bool intersects = quad1.Intersects(quad2, out overlap);
        }
    }
}