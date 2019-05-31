using System.Threading.Tasks;
using JourneyCore.Client.Display;

namespace JourneyCore.Client
{
    public class Program
    {
        private static ConsoleManager CManager { get; set; }

        public static GameLoop GLoop { get; private set; }


        private static async Task Main(string[] args)
        {
            CManager = new ConsoleManager();
            CManager.Hide(false);

            GLoop = new GameLoop();
            await GLoop.Initialise("http://localhost:5000", "GameService", (int)(1f / 30f * 1000f), 60);
        }
    }
}