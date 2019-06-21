using System;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace JourneyCore.Client
{
    public class Program
    {
        private static SynchronizationContext UIContext { get; set; }

        public static GameLoop GLoop { get; private set; }
        
        private static async Task Main()
        {
            UIContext = SynchronizationContext.Current;

            try
            {
                InitialiseStaticLogger();

                GLoop = new GameLoop();
                await GLoop.Initialise("http://localhost:5000", "GameService", 60).ConfigureAwait(true);
                GLoop.Start();
            }
            catch (Exception) { }
        }

        private static void InitialiseStaticLogger()
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
        }
    }
}