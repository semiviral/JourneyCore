using System;
using System.Threading.Tasks;
using JourneyCore.Lib.Game.Net.Security;
using JourneyCore.Lib.System.Event;
using JourneyCore.Lib.System.Static;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using RESTModule;
using Serilog;

namespace JourneyCore.Client.Net
{
    public class GameServerConnection
    {
        private DiffieHellman EncryptionService { get; }

        public string Guid { get; }
        public string ServerUrl { get; }
        public HubConnection Connection { get; private set; }
        public ServerStateUpdater StateUpdater { get; private set; }
        public bool IsServerReady { get; private set; }
        public bool IsHandshakeComplete { get; private set; }

        public GameServerConnection(string serverUrl)
        {
            EncryptionService = new DiffieHellman();

            Guid = System.Guid.NewGuid().ToString();
            ServerUrl = serverUrl;
            IsServerReady = false;
            IsHandshakeComplete = false;

            Closed += OnClosed;
        }

        public async Task<string> GetResponseAsync(RequestMethod requestMethod, string urlSuffix)
        {
            return await RESTClient.RequestAsync(requestMethod, $"{ServerUrl}/{urlSuffix}");
        }

        #region EVENTS

        public event AsyncEventHandler<Exception> Closed;

        private async Task OnClosed(object sender, Exception ex)
        {
            if (Closed == null)
            {
                return;
            }

            await Closed.Invoke(sender, ex);
        }

        #endregion


        #region INIT

        public async Task InitialiseAsync(string servicePath)
        {
            BuildConnection(servicePath);
            await WaitForServerReady();
            await ServerHandshake();
            await BuildSynchroniser();

            Log.Information("Connection to game server completed successfully.");
        }

        private void BuildConnection(string servicePath)
        {
            Log.Information("Initialising connection to game server...");

            Connection = new HubConnectionBuilder().WithUrl($"{ServerUrl}/{servicePath}").Build();

            Connection.Closed += async error =>
            {
                await Task.Delay(1000);
                await Connection.StartAsync();
            };
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
                catch (Exception)
                {
                    Log.Warning("Connection to server failed, retrying in 5 seconds...");
                    tries += 1;
                }
            }
        }

        public async Task ServerHandshake()
        {
            Log.Information("Handshaking with server...");

            try
            {
                string pubKey = Convert.ToBase64String(EncryptionService.PublicKey);

                string fullUrl =
                    $"{ServerUrl}/gameservice/security/handshake?guid={Guid}&clientPublicKey={pubKey.HtmlEncodeBase64()}";

                string retVal = await RESTClient.RequestAsync(RequestMethod.GET, fullUrl);
                DiffieHellmanKeyPackage keyPackage = JsonConvert.DeserializeObject<DiffieHellmanKeyPackage>(retVal);

                EncryptionService.CalculateSharedKey(keyPackage);
            }
            catch (Exception ex)
            {
                Log.Error($"Error occured while attempting handshake: {ex.Message}");
            }

            IsHandshakeComplete = true;

            Log.Information("Successfully completed handshake with server.");
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