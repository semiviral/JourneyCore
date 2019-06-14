using System;
using System.Threading.Tasks;
using System.Web;
using JourneyCore.Lib.Game.Net.Security;
using JourneyCore.Lib.System.Event;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using RESTModule;
using Serilog;

namespace JourneyCore.Client.Net
{
    public class GameServerConnection
    {
        public string Guid { get; }
        public string ServerUrl { get; }
        public HubConnection Connection { get; private set; }
        public ServerStateUpdater StateUpdater { get; private set; }
        public bool IsServerReady { get; private set; }

        public GameServerConnection(string serverUrl)
        {
            Guid = System.Guid.NewGuid().ToString();
            ServerUrl = serverUrl;
            IsServerReady = false;

            Closed += OnClosed;
        }

        public event AsyncEventHandler<Exception> Closed;

        private async Task OnClosed(object sender, Exception ex)
        {
            if (Closed == null)
            {
                return;
            }

            await Closed.Invoke(sender, ex);
        }

        #region INIT

        public async Task InitialiseAsync(string servicePath)
        {
            await BuildConnection(servicePath);
            await WaitForServerReady();
            await BuildSynchroniser();
        }

        private async Task BuildConnection(string servicePath)
        {
            Log.Information("Initialising connection to game server...");

            Connection = new HubConnectionBuilder().WithUrl($"{ServerUrl}/{servicePath}").Build();

            Connection.Closed += async error =>
            {
                await Task.Delay(1000);
                await Connection.StartAsync();
            };

            DiffieHellman dHell = new DiffieHellman();

            string pubKey = Convert.ToBase64String(dHell.PublicKey);

            string fullUrl =
                $"{ServerUrl}/gameservice/security/handshake?guid={HttpUtility.HtmlEncode(Guid)}&clientPublicKey={HttpUtility.HtmlEncode(pubKey)}";

            string retVal = await RESTClient.RequestAsync(RequestMethod.GET, fullUrl);
            DiffieHellmanKeyPackage keyP = JsonConvert.DeserializeObject<DiffieHellmanKeyPackage>(retVal);

            

        }

        private async Task WaitForServerReady()
        {
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
                        GameLoop.CallFatality("Could not connect to server. Aborting game launch.");
                    }

                    await Connection.StartAsync();

                    IsServerReady = await GetServerReadyState();
                }
                catch (Exception ex)
                {
                    Log.Warning("Connection to server failed, retrying in 5 seconds...");
                    tries += 1;
                }
            }

            Log.Information("Connection to game server completed successfully.");
        }

        private async Task BuildSynchroniser()
        {
            Log.Information("Requesting server tick interval...");

            int tickRate = await GetServerTickInterval();

            StateUpdater = new ServerStateUpdater(tickRate);
            StateUpdater.SyncCallback += async (sender, args) =>
            {
                await Connection.InvokeAsync("ReceiveUpdatePackages", args);
            };
        }

        #endregion


        #region  RECEPTION METHODS

        private async Task<bool> GetServerReadyState()
        {
            string retVal = await RESTClient.RequestAsync(RequestMethod.GET, $"{ServerUrl}/gameservice/status");

            return JsonConvert.DeserializeObject<bool>(retVal);
        }

        private async Task<int> GetServerTickInterval()
        {
            string retVal = await RESTClient.RequestAsync(RequestMethod.GET, $"{ServerUrl}/gameservice/tickrate");
            int tickRate = JsonConvert.DeserializeObject<int>(retVal);

            if (tickRate > 0)
            {
                return tickRate;
            }

            GameLoop.CallFatality(
                "Request for server tick interval returned an illegal value. Aborting game launch.");
            return -1;
        }

        #endregion
    }
}