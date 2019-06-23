using System;
using System.Threading.Tasks;
using JourneyCore.Lib.System.Event;
using JourneyCore.Lib.System.Net.Security;
using JourneyCore.Lib.System.Static;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Serilog;

namespace JourneyCore.Lib.System.Net
{
    public class GameServerConnection
    {
        public DiffieHellman CryptoService { get; }
        public string Guid { get; }
        public string ServerUrl { get; }
        public HubConnection Connection { get; private set; }
        public ServerStateUpdater StateUpdater { get; private set; }
        public bool IsServerReady { get; private set; }
        public bool IsHandshakeComplete { get; private set; }

        public GameServerConnection(string serverUrl)
        {
            CryptoService = new DiffieHellman();

            Guid = global::System.Guid.NewGuid().ToString();
            ServerUrl = serverUrl;
            IsServerReady = false;
            IsHandshakeComplete = false;

            Closed += OnClosed;
        }

        public async Task<string> GetResponseAsync(string urlSuffix)
        {
            return await RestClient.GetAsync($"{ServerUrl}/{urlSuffix}");
        }


        public async Task<string> GetHtmlSafeEncryptedBase64(string target)
        {
            return Convert.ToBase64String(await CryptoService.EncryptAsync(target)).HtmlEncodeBase64();
        }

        #region EVENTS

        public event AsyncEventHandler<string> FatalExit;
        public event AsyncEventHandler<Exception> Closed;

        private async Task OnFatalExit(object sender, string fatalityDescription)
        {
            if (FatalExit == null)
            {
                return;
            }

            await FatalExit.Invoke(sender, fatalityDescription);
        }

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
                        await OnFatalExit(this, "Could not connect to server. Aborting game launch.");
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

            string fullUrl =
                $"gameservice/security/handshake?guid={Guid}&clientPublicKeyBase64={CryptoService.PublicKeyString.HtmlEncodeBase64()}";

            // not using GetResponseAsync since we haven't
            // yet established the handshake
            string retVal = await GetResponseAsync(fullUrl);
            DiffieHellmanAuthPackage authPackage = JsonConvert.DeserializeObject<DiffieHellmanAuthPackage>(retVal);
            CryptoService.CalculateSharedKey(authPackage);

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
            string retVal = await GetResponseAsync("gameservice/status");

            return JsonConvert.DeserializeObject<bool>(retVal);
        }

        private async Task<int> GetServerTickInterval()
        {
            string retVal = await GetResponseAsync("gameservice/tickrate");
            int tickRate = JsonConvert.DeserializeObject<int>(retVal);

            if (tickRate > 0)
            {
                return tickRate;
            }

            await OnFatalExit(this,
                "Request for server tick interval returned an illegal value. Aborting game launch.");
            return -1;
        }

        #endregion
    }
}