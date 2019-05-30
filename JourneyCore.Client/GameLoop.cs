using System;
using System.Linq;
using System.Threading.Tasks;
using JourneyCore.Client.Display;
using JourneyCore.Client.Net;
using JourneyCore.Lib.Game.Context.Entities;
using JourneyCore.Lib.Game.Environment.Mapping;
using JourneyCore.Lib.Game.Environment.Metadata;
using JourneyCore.Lib.Game.Environment.Tiling;
using JourneyCore.Lib.Game.InputWatchers;
using JourneyCore.Lib.Graphics;
using JourneyCore.Lib.Graphics.Drawing;
using JourneyCore.Lib.System;
using JourneyCore.Lib.System.Components.Loaders;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using RESTModule;
using Serilog;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace JourneyCore.Client
{
    public class GameLoop : Context
    {
        private MapMetadata _currentMap;

        public GameLoop()
        {
            CManager = new ConsoleManager();
            CManager.Hide(false);

            InitialiseStaticLogger();

            Log.Information("Game loop started.");

            KeyWatcher = new KeyWatcher();
            IsServerReady = false;
            CurrentVArray = new VertexArray(PrimitiveType.Quads);
        }

        private string ServerUrl { get; set; }
        private HubConnection Connection { get; set; }
        private ConsoleManager CManager { get; }
        private WindowManager WManager { get; set; }
        private Entity Player { get; set; }
        private KeyWatcher KeyWatcher { get; }
        private ButtonWatcher ButtonWatcher { get; set; }
        private bool IsServerReady { get; set; }
        private ServerSynchroniser ServerStateSynchroniser { get; set; }

        private byte[] CurrentMapImage { get; set; }
        private RenderStates MapRenderStates { get; set; }

        private MapMetadata CurrentMap
        {
            get => _currentMap;
            set
            {
                Task.Run(() => UpdateCurrentMap(value));

                _currentMap = value;
            }
        }

        private VertexArray CurrentVArray { get; }


        public async Task Runtime()
        {
            for (int x = 0; x < CurrentMap.Width / MapLoader.ChunkSize; x++)
            {
                for (int y = 0; y < CurrentMap.Height / MapLoader.ChunkSize; y++)
                {
                    foreach (Chunk chunk in await RequestChunk(new Vector2i(x, y)))
                    {
                        LoadChunk(chunk);
                    }
                }
            }

            WManager.DrawItem(1,
                new DrawItem(Guid.NewGuid().ToString(), 0,
                    (window, frameTime) => { window.Draw(CurrentVArray, MapRenderStates); }));

            try
            {
                while (WManager.IsActive)
                {
                    WManager.SetActive(true);

                    KeyWatcher.CheckWatchedKeys();
                    ButtonWatcher.CheckWatchedButtons();

                    WManager.UpdateWindow();

                    WManager.SetActive(false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #region INITIALISATION

        private void InitialiseStaticLogger()
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
        }

        public async Task Initialise(string serverUrl, string servicePath, int minimumServerUpdateFrameTime)
        {
            await InitialiseConnection(serverUrl, servicePath);

            ServerStateSynchroniser = new ServerSynchroniser(Connection, minimumServerUpdateFrameTime);

            InitialiseWindowManager();
            await InitialisePlayer();
            InitialiseKeyWatcher();
            InitialiseButtonWatcher();
            InitialiseView();

            WManager.GainedFocus += (sender, args) => { KeyWatcher.WindowFocused = true; };
            WManager.LostFocus += (sender, args) => { KeyWatcher.WindowFocused = false; };
            WManager.GainedFocus += (sender, args) => { ButtonWatcher.WindowFocused = true; };
            WManager.LostFocus += (sender, args) => { ButtonWatcher.WindowFocused = false; };

            await Runtime();
        }

        private async Task InitialiseConnection(string serverUrl, string servicePath)
        {
            ServerUrl = serverUrl;

            Log.Information("Initialising connection to game server...");

            Connection = new HubConnectionBuilder().WithUrl($"{serverUrl}/{servicePath}").Build();

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
            Log.Information("Requesting map: AdventurersGuild");

            CurrentMap = await RequestMapMetadata("AdventurersGuild");
        }

        private void InitialiseWindowManager()
        {
            Log.Information("Initialising game window...");

            WManager = new WindowManager("Journey to the Core", new VideoMode(1000, 600, 8), 60, new Vector2f(2f, 2f),
                15f);

            Log.Information("Game window initialised.");
        }

        private async Task InitialisePlayer()
        {
            Log.Information("Initialising player...");

            string retVal = await RESTClient.Request(RequestMethod.GET, $"{ServerUrl}/gameservice/textures/human");

            Player = new Entity("player", "player", 0,
                new Sprite(new Texture(JsonConvert.DeserializeObject<byte[]>(retVal))));
            Player.PositionChanged += PlayerPositionChanged;
            Player.RotationChanged += PlayerRotationChanged;

            WManager.DrawItem(2, new DrawItem(Player.Guid, 0, (window, frameTime) => { window.Draw(Player.Graphic); }));

            Log.Information("Player intiailised.");
        }

        private void InitialiseView()
        {
            WManager.SetView(new View(Player.Graphic.Position, new Vector2f(200f, 200f))
            {
                Viewport = new FloatRect(0f, 0f, 0.8f, 1f)
            });
        }

        private void InitialiseKeyWatcher()
        {
            Log.Information("Creating key watch events...");

            Vector2f movement = new Vector2f(0, 0);

            KeyWatcher.AddWatchedKeyAction(Keyboard.Key.W, key =>
            {
                movement = new Vector2f((float)GraphMath.SinFromDegrees(Player.Graphic.Rotation),
                    (float)GraphMath.CosFromDegrees(Player.Graphic.Rotation) * -1f);

                if (Keyboard.IsKeyPressed(Keyboard.Key.A) || Keyboard.IsKeyPressed(Keyboard.Key.D))
                {
                    movement *= 0.5f;
                }

                Player.Move(movement, MapLoader.PixelTileWidth * MapLoader.Scale, WManager.ElapsedTime);

                return Task.CompletedTask;
            });

            KeyWatcher.AddWatchedKeyAction(Keyboard.Key.A, key =>
            {
                movement = new Vector2f((float)GraphMath.CosFromDegrees(Player.Graphic.Rotation) * -1f,
                    (float)GraphMath.SinFromDegrees(Player.Graphic.Rotation) * -1f);

                if (Keyboard.IsKeyPressed(Keyboard.Key.W) || Keyboard.IsKeyPressed(Keyboard.Key.S))
                {
                    movement *= 0.5f;
                }

                Player.Move(movement, MapLoader.PixelTileWidth * MapLoader.Scale, WManager.ElapsedTime);

                return Task.CompletedTask;
            });

            KeyWatcher.AddWatchedKeyAction(Keyboard.Key.S, key =>
            {
                movement = new Vector2f((float)GraphMath.SinFromDegrees(Player.Graphic.Rotation) * -1f,
                    (float)GraphMath.CosFromDegrees(Player.Graphic.Rotation));

                if (Keyboard.IsKeyPressed(Keyboard.Key.A) || Keyboard.IsKeyPressed(Keyboard.Key.D))
                {
                    movement *= 0.5f;
                }

                Player.Move(movement, MapLoader.PixelTileWidth * MapLoader.Scale, WManager.ElapsedTime);

                return Task.CompletedTask;
            });

            KeyWatcher.AddWatchedKeyAction(Keyboard.Key.D, key =>
            {
                movement = new Vector2f((float)GraphMath.CosFromDegrees(Player.Graphic.Rotation),
                    (float)GraphMath.SinFromDegrees(Player.Graphic.Rotation));

                if (Keyboard.IsKeyPressed(Keyboard.Key.W) || Keyboard.IsKeyPressed(Keyboard.Key.S))
                {
                    movement *= 0.5f;
                }

                Player.Move(movement, MapLoader.PixelTileWidth * MapLoader.Scale, WManager.ElapsedTime);

                return Task.CompletedTask;
            });

            KeyWatcher.AddWatchedKeyAction(Keyboard.Key.Q,
                async key => { await Player.RotateEntity(WManager.ElapsedTime, 180f, false); });

            KeyWatcher.AddWatchedKeyAction(Keyboard.Key.E,
                async key => { await Player.RotateEntity(WManager.ElapsedTime, 180f, true); });
        }

        private async Task InitialiseButtonWatcher()
        {
            ButtonWatcher = new ButtonWatcher();

            string retVal = await RESTClient.Request(RequestMethod.GET, $"{ServerUrl}/gameservice/textures/projectiles");
            Texture texture = new Texture(JsonConvert.DeserializeObject<byte[]>(retVal));

            ButtonWatcher.AddWatchedButtonAction(Mouse.Button.Left, async button =>
            {
                if (WManager.IsInMenu)
                {
                    return;
                }

                Vector2i mousePosition = WManager.GetRelativeMousePosition();

                double relativeMouseX = WManager.GetView().Size.X *
                                        (mousePosition.X / (WManager.Size.X * WManager.GetView().Viewport.Width)) -
                                        100d;
                double relativeMouseY = WManager.GetView().Size.Y *
                                        (mousePosition.Y / (WManager.Size.Y * WManager.GetView().Viewport.Height)) -
                                        100d;

                double angle = 180 / Math.PI * Math.Atan2(relativeMouseY, relativeMouseX) + Player.Graphic.Rotation +
                               90d;

                if (DateTime.Now < Player.ProjectileCooldown)
                {
                    return;
                }

                Player.ProjectileCooldown = DateTime.Now.AddMilliseconds(100);

                Entity projectile = new Entity("playerProjectile", "projectile", 2000,
                    new Sprite(texture, new IntRect(0, 0, 8, 8)))
                {
                    Graphic = { Rotation = (float)angle, Position = Player.Graphic.Position },
                    Speed = 75
                };

                DrawItem projectileDrawItem = new DrawItem(projectile.Guid, projectile.Lifetime, (window, frameTime) =>
                {
                    Vector2f movement = new Vector2f((float)GraphMath.SinFromDegrees(projectile.Graphic.Rotation),
                        (float)GraphMath.CosFromDegrees(projectile.Graphic.Rotation) * -1f);
                    projectile.Move(movement, CurrentMap.TileWidth, frameTime);

                    window.Draw(projectile.Graphic);
                });

                WManager.DrawItem(2, projectileDrawItem);
            });
        }

        #endregion


        #region CLIENT-TO-SERVER

        private async Task<MapMetadata> RequestMapMetadata(string mapName)
        {
            string retVal = await RESTClient.Request(RequestMethod.GET, $"{ServerUrl}/maps/metadata/{mapName}");

            return JsonConvert.DeserializeObject<MapMetadata>(retVal);
        }

        private async Task<Chunk[]> RequestChunk(Vector2i coords)
        {
            string retVal = await RESTClient.Request(RequestMethod.GET,
                $"{ServerUrl}/maps/{CurrentMap.Name}/{coords.X}/{coords.Y}");

            return JsonConvert.DeserializeObject<Chunk[]>(retVal);
        }

        #endregion


        #region EVENT

        private Task PlayerPositionChanged(object sender, Vector2f position)
        {
            WManager.MoveView(position);

            ServerStateSynchroniser.AllocateStateUpdate(StateUpdateType.Position,
                new Vector2i((int)position.X, (int)position.Y));

            return Task.CompletedTask;
        }

        private Task PlayerRotationChanged(object sender, float rotation)
        {
            WManager.RotateView(rotation);

            ServerStateSynchroniser.AllocateStateUpdate(StateUpdateType.Rotation, (int)rotation);

            return Task.CompletedTask;
        }

        #endregion


        #region SERVER-TO-CLIENT RECEPTION METHODS

        private async Task<bool> GetServerStatus()
        {
            string retVal = await RESTClient.Request(RequestMethod.GET, $"{ServerUrl}/gameservice/status");

            return JsonConvert.DeserializeObject<bool>(retVal);
        }

        private async Task UpdateCurrentMap(MapMetadata mapMetadata)
        {
            CurrentVArray.Clear();
            CurrentVArray.Resize((uint)(mapMetadata.Width * mapMetadata.Height * 4 * mapMetadata.LayerCount + 1));

            string retVal = await RESTClient.Request(RequestMethod.GET, $"{ServerUrl}/gameservice/textures/maps");
            CurrentMapImage = JsonConvert.DeserializeObject<byte[]>(retVal);

            MapRenderStates = new RenderStates(new Texture(CurrentMapImage));
        }

        #endregion


        #region MAP BUILDING

        private void LoadChunk(Chunk chunk)
        {
            for (int x = 0; x < chunk.Length; x++)
            for (int y = 0; y < chunk[0].Length; y++)
            {
                AllocateTileToVArray(chunk[x][y],
                    new Vector2i(chunk.Left * MapLoader.ChunkSize + x, chunk.Top * MapLoader.ChunkSize + y),
                    chunk.Layer);
            }
        }

        private void AllocateTileToVArray(TilePrimitive tilePrimitive, Vector2i tileCoords, int layerId)
        {
            if (tilePrimitive.Gid == 0)
            {
                return;
            }

            TileMetadata tileMetadata = GetTileMetadata(tilePrimitive.Gid);

            int scaledSizeX = tileMetadata.TextureRect.Width * MapLoader.Scale;
            int scaledSizeY = tileMetadata.TextureRect.Height * MapLoader.Scale;

            Vector2f topLeft = GraphMath.CalculateVertexPosition(VertexCorner.TopLeft, tileCoords.X, tileCoords.Y,
                scaledSizeX, scaledSizeY);
            Vector2f topRight = GraphMath.CalculateVertexPosition(VertexCorner.TopRight, tileCoords.X, tileCoords.Y,
                scaledSizeX, scaledSizeY);
            Vector2f bottomRight = GraphMath.CalculateVertexPosition(VertexCorner.BottomRight, tileCoords.X,
                tileCoords.Y, scaledSizeX, scaledSizeY);
            Vector2f bottomLeft = GraphMath.CalculateVertexPosition(VertexCorner.BottomLeft, tileCoords.X, tileCoords.Y,
                scaledSizeX, scaledSizeY);

            QuadCoords textureCoords = GetTileTextureCoords(tilePrimitive);

            uint index = (uint)((tileCoords.Y * CurrentMap.Width + tileCoords.X) * 4 +
                                (layerId - 1) * (CurrentVArray.VertexCount / CurrentMap.LayerCount));

            CurrentVArray[index + 0] = new Vertex(topLeft, textureCoords.TopLeft);
            CurrentVArray[index + 1] = new Vertex(topRight, textureCoords.TopRight);
            CurrentVArray[index + 2] = new Vertex(bottomRight, textureCoords.BottomRight);
            CurrentVArray[index + 3] = new Vertex(bottomLeft, textureCoords.BottomLeft);
        }

        private TileMetadata GetTileMetadata(int gid)
        {
            return CurrentMap.TileSets.SelectMany(tileSet => tileSet.Tiles).Single(tile => tile.Gid == gid);
        }

        private QuadCoords GetTileTextureCoords(TilePrimitive tilePrimitive)
        {
            TileMetadata tileMetadata = GetTileMetadata(tilePrimitive.Gid);

            // width and height of all textures in a map will be the same
            int actualPixelLeft = tileMetadata.TextureRect.Left * tileMetadata.TextureRect.Width;
            int actualPixelTop = tileMetadata.TextureRect.Top * tileMetadata.TextureRect.Height;

            QuadCoords finalCoords = new QuadCoords();

            switch (tilePrimitive.Rotation)
            {
                case 0:
                    finalCoords.TopLeft = new Vector2f(actualPixelLeft, actualPixelTop);
                    finalCoords.TopRight =
                        new Vector2f(actualPixelLeft + tileMetadata.TextureRect.Width, actualPixelTop);
                    finalCoords.BottomRight = new Vector2f(actualPixelLeft + tileMetadata.TextureRect.Width,
                        actualPixelTop + tileMetadata.TextureRect.Height);
                    finalCoords.BottomLeft =
                        new Vector2f(actualPixelLeft, actualPixelTop + tileMetadata.TextureRect.Height);
                    break;
                case 1:
                    finalCoords.TopLeft =
                        new Vector2f(actualPixelLeft + tileMetadata.TextureRect.Width, actualPixelTop);
                    finalCoords.TopRight = new Vector2f(actualPixelLeft + tileMetadata.TextureRect.Width,
                        actualPixelTop + tileMetadata.TextureRect.Height);
                    finalCoords.BottomRight =
                        new Vector2f(actualPixelLeft, actualPixelTop + tileMetadata.TextureRect.Height);
                    finalCoords.BottomLeft = new Vector2f(actualPixelLeft, actualPixelTop);
                    break;
                case 2:
                    finalCoords.TopLeft = new Vector2f(actualPixelLeft + tileMetadata.TextureRect.Width,
                        actualPixelTop + tileMetadata.TextureRect.Height);
                    finalCoords.TopRight =
                        new Vector2f(actualPixelLeft, actualPixelTop + tileMetadata.TextureRect.Height);
                    finalCoords.BottomRight = new Vector2f(actualPixelLeft, actualPixelTop);
                    finalCoords.BottomLeft =
                        new Vector2f(actualPixelLeft + tileMetadata.TextureRect.Width, actualPixelTop);
                    break;
                case 3:
                    finalCoords.TopLeft =
                        new Vector2f(actualPixelLeft, actualPixelTop + tileMetadata.TextureRect.Height);
                    finalCoords.TopRight = new Vector2f(actualPixelLeft, actualPixelTop);
                    finalCoords.BottomRight =
                        new Vector2f(actualPixelLeft + tileMetadata.TextureRect.Width, actualPixelTop);
                    finalCoords.BottomLeft = new Vector2f(actualPixelLeft + tileMetadata.TextureRect.Width,
                        actualPixelTop + tileMetadata.TextureRect.Height);
                    break;
            }

            return finalCoords;
        }

        #endregion
    }
}