using System;
using System.ComponentModel;
using System.Threading.Tasks;
using JourneyCore.Client.Display;
using JourneyCore.Client.Display.UserInterface;
using JourneyCore.Client.Net;
using JourneyCore.Lib.Game.Environment.Mapping;
using JourneyCore.Lib.Game.Environment.Metadata;
using JourneyCore.Lib.Game.InputWatchers;
using JourneyCore.Lib.Game.Object.Entity;
using JourneyCore.Lib.Graphics.Drawing;
using JourneyCore.Lib.System;
using JourneyCore.Lib.System.Components.Loaders;
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
        private static Tuple<int, string> _FatalExit;

        private GameServerConnection NetManager { get; set; }
        private ConsoleManager ConManager { get; }
        private GameWindow Window { get; set; }
        private Ui UserInterface { get; set; }
        private LocalMap CurrentMap { get; set; }
        private Player Player { get; set; }
        private InputWatcher InputWatcher { get; }

        public GameLoop()
        {
            ConManager = new ConsoleManager();
            ConManager.Hide(false);

            InitialiseStaticLogger();

            Log.Information("Game loop started.");

            InputWatcher = new InputWatcher();
        }

        public async Task StartAsync()
        {
            // todo this doesn't belong, loading whole map for dev purposes
            for (int x = 0; x < CurrentMap.Metadata.Width / MapLoader.ChunkSize; x++)
            {
                for (int y = 0; y < CurrentMap.Metadata.Height / MapLoader.ChunkSize; y++)
                {
                    foreach (Chunk chunk in await RequestChunk(new Vector2i(x, y)))
                    {
                        CurrentMap.LoadChunk(chunk);
                    }
                }
            }

            Window.DrawItem("game", 0,
                new DrawItem(Guid.NewGuid().ToString(), DateTime.MinValue, null,
                    new DrawObject(typeof(VertexArray), CurrentMap.VArray), CurrentMap.RenderStates));
            Window.DrawItem("minimap", 0,
                new DrawItem(Guid.NewGuid().ToString(), DateTime.MinValue, null,
                    new DrawObject(typeof(VertexArray), CurrentMap.Minimap.VArray), RenderStates.Default));

            Window.SetActive(true);

            try
            {
                while (Window.IsActive)
                {
                    //InputWatcher.CheckWatchedInputs();

                    Window.UpdateWindow();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        // todo fix exiting to not block threads

        public static void CallFatality(string error, int exitCode = -1)
        {
            _FatalExit = new Tuple<int, string>(exitCode, error);
        }

        private void ExitWithFatality()
        {
            Log.Fatal(_FatalExit.Item2);
            Log.Fatal("Press any key to continue.");

            Console.ReadLine();

            Environment.Exit(_FatalExit.Item1);
        }


        #region INITIALISATION

        private void InitialiseStaticLogger()
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
        }

        public async Task Initialise(string serverUrl, string servicePath, int maximumFrameRate)
        {
            try
            {
                await InitialiseGameServerConnection(serverUrl, servicePath);
                InitialiseGameWindow(maximumFrameRate);
                await InitialiseLocalMap();
                await InitialisePlayer();
                SetupWatchedKeys();
                SetupWatchedMouse();
                await InitialiseUserInterface();
                InitialiseMiniMap();

                Window.GainedFocus += (sender, args) => { InputWatcher.WindowFocused = true; };
                Window.LostFocus += (sender, args) => { InputWatcher.WindowFocused = false; };
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        private async Task InitialiseGameServerConnection(string serverUrl, string servicePath)
        {
            NetManager = new GameServerConnection(serverUrl);
            await NetManager.InitialiseAsync(servicePath);
        }

        private void InitialiseGameWindow(int maximumFrameRate)
        {
            Log.Information("Initialising game window...");

            Window = new GameWindow("Journey to the Core", new VideoMode(1280, 720, 8), maximumFrameRate,
                new Vector2f(2f, 2f),
                15f);
            Window.Closed += (sender, args) =>
            {
                Window.SetActive(false);
                CallFatality("Game window closed.");
            };
            Window.MouseWheelScrolled += (sender, args) =>
            {
                Window.GetDrawView("minimap").ZoomFactor += args.Delta * -1f;
            };

            const float viewSizeX = 300f;
            const float minimapSizeX = 0.2f;

            Window.CreateDrawView("game", 0,
                new View(new FloatRect(0f, 0f, viewSizeX, viewSizeX * GameWindow.WidescreenRatio))
                {
                    Viewport = new FloatRect(0f, 0f, 1f, 1f)
                });

            Window.CreateDrawView("ui", 1, new View(new FloatRect(0f, 0f, 200f, 600f))
            {
                Viewport = new FloatRect(0.8f, 0.3f, 0.2f, 0.7f)
            });

            Window.CreateDrawView("minimap", 2,
                new View(new FloatRect(0f, 0f, viewSizeX, viewSizeX * GameWindow.WidescreenRatio))
                {
                    Viewport = new FloatRect(1f - (minimapSizeX * GameWindow.LetterboxRatio - 0.005f), 0.005f,
                        minimapSizeX * GameWindow.LetterboxRatio, minimapSizeX)
                });

            Log.Information("Game window initialised.");
        }

        private async Task InitialiseLocalMap()
        {
            string retVal =
                await RESTClient.RequestAsync(RequestMethod.GET, $"{NetManager.ServerUrl}/gameservice/images/maps");
            CurrentMap = new LocalMap(JsonConvert.DeserializeObject<byte[]>(retVal));

            Log.Information("Requesting map: AdventurersGuild");

            CurrentMap.Update(await RequestMapMetadata("AdventurersGuild"));
        }

        private async Task InitialisePlayer()
        {
            Log.Information("Initialising player...");

            string retVal =
                await RESTClient.RequestAsync(RequestMethod.GET, $"{NetManager.ServerUrl}/gameservice/images/human");
            Texture humanTexture = new Texture(JsonConvert.DeserializeObject<byte[]>(retVal));


            retVal = RESTClient.Request(RequestMethod.GET,
                $"{NetManager.ServerUrl}/gameservice/images/projectiles");
            Texture projectilesTexture = new Texture(JsonConvert.DeserializeObject<byte[]>(retVal));


            Player = new Player(new Sprite(humanTexture), projectilesTexture, 0);
            Player.PropertyChanged += PlayerPropertyChanged;
            Player.PositionChanged += PlayerPositionChanged;
            Player.RotationChanged += PlayerRotationChanged;

            Player.AnchorItem(Window.GetDrawView("game"));
            Player.AnchorItem(Window.GetDrawView("minimap"));

            Window.DrawItem("game", 10,
                new DrawItem(Player.Guid, DateTime.MinValue, null,
                    new DrawObject(Player.Graphic.GetType(), Player.Graphic, Player.Graphic.GetVertices),
                    new RenderStates(Player.Graphic.Texture)));

            Log.Information("Player intiailised.");
        }

        private void SetupWatchedKeys()
        {
            Log.Information("Creating input watch events...");

            Vector2f movement = new Vector2f(0, 0);

            InputWatcher.AddWatchedInput(Keyboard.Key.W, key =>
            {
                movement = new Vector2f(
                    (float)GraphMath.SinFromDegrees(Player.Graphic.Rotation + DrawView.DefaultPlayerViewRotation % 360),
                    (float)GraphMath.CosFromDegrees(Player.Graphic.Rotation +
                                                    DrawView.DefaultPlayerViewRotation % 360) * -1f);

                if (Keyboard.IsKeyPressed(Keyboard.Key.A) || Keyboard.IsKeyPressed(Keyboard.Key.D))
                {
                    movement *= 0.5f;
                }

                Player.MoveEntity(movement, MapLoader.TilePixelSize * MapLoader.Scale, Window.ElapsedTime);
            });

            InputWatcher.AddWatchedInput(Keyboard.Key.A, key =>
            {
                movement = new Vector2f(
                    (float)GraphMath.CosFromDegrees(Player.Graphic.Rotation +
                                                    DrawView.DefaultPlayerViewRotation % 360) * -1f,
                    (float)GraphMath.SinFromDegrees(Player.Graphic.Rotation +
                                                    DrawView.DefaultPlayerViewRotation % 360) * -1f);

                if (Keyboard.IsKeyPressed(Keyboard.Key.W) || Keyboard.IsKeyPressed(Keyboard.Key.S))
                {
                    movement *= 0.5f;
                }

                Player.MoveEntity(movement, MapLoader.TilePixelSize * MapLoader.Scale, Window.ElapsedTime);
            });

            InputWatcher.AddWatchedInput(Keyboard.Key.S, key =>
            {
                movement = new Vector2f(
                    (float)GraphMath.SinFromDegrees(Player.Graphic.Rotation +
                                                    DrawView.DefaultPlayerViewRotation % 360) * -1f,
                    (float)GraphMath.CosFromDegrees(Player.Graphic.Rotation +
                                                    DrawView.DefaultPlayerViewRotation % 360));

                if (Keyboard.IsKeyPressed(Keyboard.Key.A) || Keyboard.IsKeyPressed(Keyboard.Key.D))
                {
                    movement *= 0.5f;
                }

                Player.MoveEntity(movement, MapLoader.TilePixelSize * MapLoader.Scale, Window.ElapsedTime);
            });

            InputWatcher.AddWatchedInput(Keyboard.Key.D, key =>
            {
                movement = new Vector2f(
                    (float)GraphMath.CosFromDegrees(Player.Graphic.Rotation + DrawView.DefaultPlayerViewRotation % 360),
                    (float)GraphMath.SinFromDegrees(Player.Graphic.Rotation +
                                                    DrawView.DefaultPlayerViewRotation % 360));

                if (Keyboard.IsKeyPressed(Keyboard.Key.W) || Keyboard.IsKeyPressed(Keyboard.Key.S))
                {
                    movement *= 0.5f;
                }

                Player.MoveEntity(movement, MapLoader.TilePixelSize * MapLoader.Scale, Window.ElapsedTime);
            });

            InputWatcher.AddWatchedInput(Keyboard.Key.G, key => { CurrentMap.Minimap.VArray.ModifyOpacity(-25, 10); });

            InputWatcher.AddWatchedInput(Keyboard.Key.H, key => { CurrentMap.Minimap.VArray.ModifyOpacity(25); });

            InputWatcher.AddWatchedInput(Keyboard.Key.Q,
                key => { Player.RotateEntity(Window.ElapsedTime, 180f, false); });

            InputWatcher.AddWatchedInput(Keyboard.Key.E,
                key => { Player.RotateEntity(Window.ElapsedTime, 180f, true); });
        }

        private void SetupWatchedMouse()
        {
            InputWatcher.AddWatchedInput(Mouse.Button.Left, button =>
            {
                if (Window.IsInMenu)
                {
                    return;
                }

                Vector2i mousePosition = Window.GetRelativeMousePosition();
                View gameView = Window.GetDrawView("game").View;

                double relativeMouseX =
                    gameView.Size.X * (mousePosition.X / (Window.Size.X * gameView.Viewport.Width)) -
                    gameView.Size.X / 2f;
                double relativeMouseY =
                    gameView.Size.Y * (mousePosition.Y / (Window.Size.Y * gameView.Viewport.Height)) -
                    gameView.Size.Y / 2f;

                DrawItem projectileDrawItem =
                    Player.FireProjectile(relativeMouseX, relativeMouseY, CurrentMap.Metadata.TileWidth);

                if (projectileDrawItem == null)
                {
                    return;
                }

                Window.DrawItem("game", 20, projectileDrawItem);
            });
        }

        private async Task InitialiseUserInterface()
        {
            string retVal =
                await RESTClient.RequestAsync(RequestMethod.GET, $"{NetManager.ServerUrl}/gameservice/tilesets/ui");
            TileSetMetadata tileSetMetadata = JsonConvert.DeserializeObject<TileSetMetadata>(retVal);

            retVal = await RESTClient.RequestAsync(RequestMethod.GET, $"{NetManager.ServerUrl}/gameservice/images/ui");
            byte[] uiImage = JsonConvert.DeserializeObject<byte[]>(retVal);

            UserInterface = new Ui(tileSetMetadata, uiImage);
            UserInterface.UpdateHealth(Player.CurrentHp);
        }

        private void InitialiseMiniMap()
        {
            RectangleShape playerTile =
                new RectangleShape(
                    new Vector2f(CurrentMap.Metadata.TileWidth / 2f, CurrentMap.Metadata.TileHeight / 2f))
                {
                    FillColor = Color.Yellow,
                    OutlineColor = new Color(200, 200, 200),
                    OutlineThickness = 1f,
                    Position = Player.Graphic.Position
                };
            playerTile.Origin = playerTile.Size / 2f;

            DrawObject playerTileObj = new DrawObject(playerTile.GetType(), playerTile, playerTile.GetVertices);
            Player.AnchorItem(playerTileObj);

            Window.DrawItem("minimap", 10,
                new DrawItem(Guid.NewGuid().ToString(), DateTime.MinValue, null, playerTileObj, RenderStates.Default));
        }

        #endregion


        #region CLIENT-TO-SERVER

        private async Task<MapMetadata> RequestMapMetadata(string mapName)
        {
            string retVal =
                await RESTClient.RequestAsync(RequestMethod.GET, $"{NetManager.ServerUrl}/maps/metadata/{mapName}");

            return JsonConvert.DeserializeObject<MapMetadata>(retVal);
        }

        private async Task<Chunk[]> RequestChunk(Vector2i coords)
        {
            string retVal = await RESTClient.RequestAsync(RequestMethod.GET,
                $"{NetManager.ServerUrl}/maps/{CurrentMap.Metadata.Name}/{coords.X}/{coords.Y}");

            return JsonConvert.DeserializeObject<Chunk[]>(retVal);
        }

        #endregion


        #region EVENT

        private void PlayerPositionChanged(object sender, Vector2f position)
        {
            NetManager.StateUpdater.AllocateStateUpdate(StateUpdateType.Position, position);
        }

        private void PlayerRotationChanged(object sender, float rotation)
        {
            NetManager.StateUpdater.AllocateStateUpdate(StateUpdateType.Rotation, rotation);
        }

        private void PlayerPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case "CurrentHP":

                    break;
            }
        }

        #endregion
    }
}