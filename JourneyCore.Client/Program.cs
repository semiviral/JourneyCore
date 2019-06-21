using System;
using Serilog;

namespace JourneyCore.Client
{
    public class Program
    {
        public static GameLoop GLoop { get; private set; }
        
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