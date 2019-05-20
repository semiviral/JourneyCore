using System.Threading.Tasks;

namespace JourneyCore.Client
{
    public class Program
    {
        private static ConsoleManager CManager { get; set; }

        public static GameLoop gLoop { get; private set; }


        private static async Task Main(string[] args)
        {
            CManager = new ConsoleManager();
            CManager.Hide(false);

            gLoop = new GameLoop();
            await gLoop.Initialise("http://localhost:5000/GameClient", (int) (1f / 30f * 1000f));
        }
    }
}