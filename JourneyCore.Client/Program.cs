using System;
using System.Threading.Tasks;

namespace JourneyCore.Client
{
    public class Program
    {
        public static GameLoop GLoop { get; private set; }

        private static async Task Main(string[] args)
        {
            try
            {
                GLoop = new GameLoop();
                await GLoop.Initialise("http://localhost:5000", "GameService", 60);
                await GLoop.StartAsync();
            }
            catch (Exception ex) { }
        }
    }
}