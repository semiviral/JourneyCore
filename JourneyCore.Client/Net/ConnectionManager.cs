﻿using System;
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
        public ServerSynchronizer ServerStateSynchronizer { get; private set; }
        public bool IsServerReady { get; private set; }

        public event AsyncEventHandler<Exception> Closed;

        public async Task Initialise(string servicePath)
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
                        GameLoop.ExitWithFatality("Could not connect to server. Ending process, press any key.");
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
            Log.Information("Requesting server tick interval...");

            int tickRate = await GetServerTickInterval();

            ServerStateSynchronizer = new ServerSynchronizer(tickRate);
            ServerStateSynchronizer.SyncCallback += async (sender, args) =>
            {
                await Connection.InvokeAsync("ReceiveUpdatePackages", args);
            };
        }

        #region  RECEPTION METHODS

        private async Task<bool> GetServerStatus()
        {
            string retVal = await RESTClient.Request(RequestMethod.GET, $"{ServerUrl}/gameservice/status");

            return JsonConvert.DeserializeObject<bool>(retVal);
        }

        private async Task<int> GetServerTickInterval()
        {
            string retVal = await RESTClient.Request(RequestMethod.GET, $"{ServerUrl}/gameservice/tickrate");
            int tickRate = JsonConvert.DeserializeObject<int>(retVal);

            if (tickRate != 0)
            {
                return tickRate;
            }

            GameLoop.ExitWithFatality("Request for server tick interval returned an illegal value. Aborting game launch.");
            return -1;
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