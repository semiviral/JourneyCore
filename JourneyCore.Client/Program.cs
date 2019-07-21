using System;
using Serilog;

namespace JourneyCore.Client
{
    public static class Program
    {
        private static GameLoop GLoop { get; set; }

        private static void Main()
        {
            try
            {
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
    }
}