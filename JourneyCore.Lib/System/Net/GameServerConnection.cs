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
        public GameServerConnection(string serverUrl)
        {
            CryptoService = new DiffieHellman();

            ConnectionId = string.Empty;
            ServerUrl = serverUrl;
            IsServerReady = false;
            IsHandshakeComplete = false;

            Closed += OnClosed;
        }

        public DiffieHellman CryptoService { get; }
        public string ConnectionId { get; private set; }
        public string ServerUrl { get; }
        public HubConnection Connection { get; private set; }
        public bool IsServerReady { get; private set; }
        public bool IsHandshakeComplete { get; private set; }

        public async Task<string> GetResponseAsync(string urlSuffix)
        {
            return await RestClient.GetAsync($"{ServerUrl}/{urlSuffix}");
        }

        public async Task<string> GetHtmlSafeEncryptedBase64(string target)
        {
            return Convert.ToBase64String(await CryptoService.EncryptAsync(target)).HtmlEncodeBase64();
        }

        public bool On<T>(string methodName, Action<T> action)
        {
            Connection.On(methodName, action);

            return true;
        }

        public void InvokeHubAsync(string methodName, params object[] args)
        {
            Connection?.InvokeAsync(methodName, args);
        }


        #region INIT

        public async Task InitialiseAsync(string servicePath)
        {
            await BuildConnection(servicePath);
            await ReadyWait();
            await ServerHandshake();

            Log.Information("Connection to game server completed successfully.");
        }

        private async Task BuildConnection(string servicePath)
        {
            Log.Information("Initialising connection to game server...");

            bool connected = false;
            int tries = 0;

            while (!connected && tries < 5)
                try
                {
                    Connection = new HubConnectionBuilder().WithUrl($"{ServerUrl}/{servicePath}").Build();
                    Connection.Closed += async error =>
                    {
                        Log.Error(error.Message);

                        await Task.Delay(1000);
                        await Connection.StartAsync();
                    };

                    await Connection.StartAsync();

                    connected = true;
                }
                catch (Exception ex)
                {
                    if (tries == 4)
                    {
                        await OnFatalExit(this, ex.Message);
                    }
                    else
                    {
                        Log.Error($"{ex.Message}.. trying again.");

                        tries += 1;
                    }
                }

            On<string>("ReceiveConnectionId", connectionId => { ConnectionId = connectionId; });
            On<bool>("ReceiveServerStatus", status => { IsServerReady = status; });
        }

        public async Task ReadyWait()
        {
            Log.Information("Waiting for server ready flag...");

            while (!IsServerReady)
            {
                string retVal = await GetResponseAsync("gameservice/status");
                bool readyStatus = false;

                try
                {
                    readyStatus = JsonConvert.DeserializeObject<bool>(retVal);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);

                    await Task.Delay(1000);
                }

                IsServerReady = readyStatus;
            }

            Log.Information("Received server ready flag.");
            Log.Information("Requesting connection ID...");

            while (string.IsNullOrWhiteSpace(ConnectionId))
            {
                await Connection.InvokeAsync("RequestConnectionId");
                await Task.Delay(100);
            }

            Log.Information("Connection ID received.");
        }

        public async Task ServerHandshake()
        {
            Log.Information("Handshaking with server...");

            EncryptionTicket localTicket = new EncryptionTicket(CryptoService.PublicKey, CryptoService.IV);

            string retVal =
                await GetResponseAsync(
                    $"gameservice/security/handshake?id={ConnectionId}&htmlSafeBase64Ticket={localTicket.ConvertToHtmlSafeBase64()}");
            EncryptionTicket remoteTicket;

            try
            {
                remoteTicket = JsonConvert.DeserializeObject<EncryptionTicket>(retVal);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);

                return;
            }

            CryptoService.CalculateSharedKey(remoteTicket.PublicKey, remoteTicket.IV);

            IsHandshakeComplete = true;

            Log.Information("Encryption ticket received from server, handshake complete.");
        }

        #endregion


        #region EVENTS

        public event AsyncEventHandler<string> FatalExit;
        public event AsyncEventHandler<Exception> Closed;

        private async Task OnFatalExit(object sender, string fatalityDescription)
        {
            if (FatalExit == null) return;

            await FatalExit.Invoke(sender, fatalityDescription);
        }

        private async Task OnClosed(object sender, Exception ex)
        {
            if (Closed == null) return;

            await Closed.Invoke(sender, ex);
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

            if (tickRate > 0) return tickRate;

            await OnFatalExit(this,
                "Request for server tick interval returned an illegal value. Aborting game launch.");
            return -1;
        }

        #endregion
    }
}