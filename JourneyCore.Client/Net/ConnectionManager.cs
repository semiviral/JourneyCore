using System;
using System.Threading.Tasks;
using JourneyCore.Lib.System.Event;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using RESTModule;
using Serilog;

namespace JourneyCore.Client.Net
{
    public class ConnectionManager
    {
        public ConnectionManager(string serverUrl)
        {
            ServerUrl = serverUrl;
            IsServerReady = false;

            Closed += OnClosed;
        }

        public string ServerUrl { get; }
        public HubConnection Connection { get; private set; }
        public ServerSynchroniser ServerStateSynchroniser { get; private set; }
        public bool IsServerReady { get; private set; }

        public event AsyncEventHandler<Exception> Closed;

        public async Task Initialise(string servicePath, int minimumServerUpdateFrameTime)
        {
            Log.Information("Initialising connection to game server...");

            Connection = new HubConnectionBuilder().WithUrl($"{ServerUrl}/{servicePath}").Build();

            Connection.Closed += async error =>
            {
                await Task.Delay(1000);
                await Connection.StartAsync();
            };

            int tries = 0;

            while (!IsServerReady)
            {
                try
                {
                    if (tries > 0)
                    {
                        await Task.Delay(5000);
                    }

                    if (tries > 4)
                    {
                        Log.Error("Could not connect to server. Ending process, press any key.");

                        Console.ReadLine();

                        Environment.Exit(1);
                    }

                    await Connection.StartAsync();

                    IsServerReady = await GetServerStatus();
                }
                catch (Exception ex)
                {
                    Log.Warning("Connection to server failed, retrying in 5 seconds...");
                    tries += 1;
                }
            }

            Log.Information("Connection to game server completed successfully.");

            ServerStateSynchroniser = new ServerSynchroniser(Connection, minimumServerUpdateFrameTime);
        }

        #region  RECEPTION METHODS

        private async Task<bool> GetServerStatus()
        {
            string retVal = await RESTClient.Request(RequestMethod.GET, $"{ServerUrl}/gameservice/status");

            return JsonConvert.DeserializeObject<bool>(retVal);
        }

        #endregion

        private async Task OnClosed(object sender, Exception ex)
        {
            if (Closed == null)
            {
                return;
            }

            await Closed.Invoke(sender, ex);
        }
    }
}