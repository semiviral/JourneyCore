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

        public void On<T>(string methodName, Action<T> action)
        {
            Connection.On(methodName, action);
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

            bool _connected = false;
            int _tries = 0;

            while (!_connected && (_tries < 5))
            {
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

                    _connected = true;
                }
                catch (Exception _ex)
                {
                    if (_tries == 4)
                    {
                        await OnFatalExit(this, $"{_ex.Message}.. exiting game.");
                    }
                    else
                    {
                        Log.Error($"{_ex.Message}.. trying again.");

                        _tries += 1;
                    }
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
                string _retVal = await GetResponseAsync("gameservice/status");
                bool _readyStatus = false;

                try
                {
                    _readyStatus = JsonConvert.DeserializeObject<bool>(_retVal);
                }
                catch (Exception _ex)
                {
                    Log.Error(_ex.Message);

                    await Task.Delay(2000);
                }

                IsServerReady = _readyStatus;
            }

            Log.Information("Received server ready flag.");
            Log.Information("Requesting connection ID...");

            while (string.IsNullOrWhiteSpace(ConnectionId))
            {
                await Connection.InvokeAsync("RequestConnectionId");
                await Task.Delay(500);
            }

            Log.Information("Connection ID received.");
        }

        public async Task ServerHandshake()
        {
            Log.Information("Handshaking with server...");

            EncryptionTicket _localTicket = new EncryptionTicket(CryptoService.PublicKey, CryptoService.Iv);

            string _retVal =
                await GetResponseAsync(
                    $"gameservice/security/handshake?id={ConnectionId}&htmlSafeBase64Ticket={_localTicket.ConvertToHtmlSafeBase64()}");
            EncryptionTicket _remoteTicket;

            try
            {
                _remoteTicket = JsonConvert.DeserializeObject<EncryptionTicket>(_retVal);
            }
            catch (Exception _ex)
            {
                Log.Error(_ex.Message);

                return;
            }

            CryptoService.CalculateSharedKey(_remoteTicket.PublicKey, _remoteTicket.Iv);

            IsHandshakeComplete = true;

            Log.Information("Encryption ticket received from server, handshake complete.");
        }

        #endregion


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


        #region  RECEPTION METHODS

        private async Task<bool> GetServerReadyState()
        {
            string _retVal = await GetResponseAsync("gameservice/status");

            return JsonConvert.DeserializeObject<bool>(_retVal);
        }

        private async Task<int> GetServerTickInterval()
        {
            string _retVal = await GetResponseAsync("gameservice/tickrate");
            int _tickRate = JsonConvert.DeserializeObject<int>(_retVal);

            if (_tickRate > 0)
            {
                return _tickRate;
            }

            await OnFatalExit(this,
                "Request for server tick interval returned an illegal value. Aborting game launch.");
            return -1;
        }

        #endregion
    }
}