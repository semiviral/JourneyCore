using System;
using System.ComponentModel;
using System.Threading.Tasks;
using JourneyCore.Client.Display;
using JourneyCore.Client.Display.UserInterface;
using JourneyCore.Client.Net;
using JourneyCore.Lib.Game.Context.Entities;
using JourneyCore.Lib.Game.Environment.Mapping;
using JourneyCore.Lib.Game.Environment.Metadata;
using JourneyCore.Lib.Game.InputWatchers;
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
        public GameLoop()
        {
            IsRunning = true;

            ConManager = new ConsoleManager();
            ConManager.Hide(false);

            InitialiseStaticLogger();

            Log.Information("Game loop started.");

            InputWatcher = new InputWatcher();
        }

        private GameServerConnection NetManager { get; set; }
        private ConsoleManager ConManager { get; }
        private GameWindow Win { get; set; }
        private UI UserInterface { get; set; }
        private LocalMap CurrentMap { get; set; }
        private Entity Player { get; set; }
        private InputWatcher InputWatcher { get; }
        private bool IsRunning { get; set; }


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

            Win.DrawItem("game", 1,
                new DrawItem(Guid.NewGuid().ToString(), 0,
                    (window, frameTime) => { window.Draw(CurrentMap.VArray, CurrentMap.RenderStates); }));
            Win.DrawItem("minimap", 0,
                new DrawItem(Guid.NewGuid().ToString(), 0,
                    (window, frameTime) => { window.Draw(CurrentMap.Minimap.VArray); }));

            try
            {
                while (IsRunning)
                {
                    await InputWatcher.CheckWatchedInputs();

                    Win.UpdateWindow();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public static void ExitWithFatality(string error, int exitCode = -1)
        {
            // allow game window to close before fataling

            Log.Fatal(error);
            Log.Fatal("Press any key to continue.");

            Console.ReadLine();

            Environment.Exit(exitCode);
        }


        #region INITIALISATION

        private void InitialiseStaticLogger()
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
        }

        public async Task Initialise(string serverUrl, string servicePath, int maximumFrameRate)
        {
            await InitialiseGameServerConnection(serverUrl, servicePath);
            InitialiseGameWindow(maximumFrameRate);
            await InitialiseLocalMap();
            await InitialisePlayer();
            SetupWatchedKeys();
            SetupWatchedButtons();
            await InitialiseUserInterface();
            InitialiseMiniMap();

            Win.GainedFocus += (sender, args) => { InputWatcher.WindowFocused = true; };
            Win.LostFocus += (sender, args) => { InputWatcher.WindowFocused = false; };
        }

        private async Task InitialiseGameServerConnection(string serverUrl, string servicePath)
        {
            NetManager = new GameServerConnection(serverUrl);
            await NetManager.InitialiseAsync(servicePath);
        }

        private void InitialiseGameWindow(int maximumFrameRate)
        {
            Log.Information("Initialising game window...");

            Win = new GameWindow("Journey to the Core", new VideoMode(1280, 720, 8), maximumFrameRate,
                new Vector2f(2f, 2f),
                15f);
            Win.Closed += (sender, args) =>
            {
                IsRunning = false;
                ExitWithFatality("Game window closed.");
            };

            const float viewSizeX = 300f;
            const float minimapSizeX = 0.2f;

            Win.CreateView("game", 0, new View(new FloatRect(0f, 0f, viewSizeX, viewSizeX * GameWindow.WidescreenRatio))
            {
                Viewport = new FloatRect(0f, 0f, 1f, 1f)
            });

            Win.CreateView("ui", 1, new View(new FloatRect(0f, 0f, 200f, 600f))
            {
                Viewport = new FloatRect(0.8f, 0.3f, 0.2f, 0.7f)
            });

            Win.CreateView("minimap", 2,
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
            Texture projectilesTexutre = new Texture(JsonConvert.DeserializeObject<byte[]>(retVal));


            Player = new Entity(projectilesTexutre, 0,
                new Sprite(humanTexture));
            Player.PropertyChanged += PlayerPropertyChanged;
            Player.PositionChanged += PlayerPositionChanged;
            Player.RotationChanged += PlayerRotationChanged;

            Win.MoveView("game", Player.Graphic.Position);

            // todo
            //  implement some sort of position/rotation
            //  anchoring class or subscription for items

            Win.DrawItem("game", 2,
                new DrawItem(Player.Guid, 0, (window, frameTime) => { window.Draw(Player.Graphic); }));

            Log.Information("Player intiailised.");
        }

        private void SetupWatchedKeys()
        {
            Log.Information("Creating key watch events...");

            Vector2f movement = new Vector2f(0, 0);

            InputWatcher.AddWatchedInput(Keyboard.Key.W, key =>
            {
                movement = new Vector2f((float)GraphMath.SinFromDegrees(Player.Graphic.Rotation),
                    (float)GraphMath.CosFromDegrees(Player.Graphic.Rotation) * -1f);

                if (Keyboard.IsKeyPressed(Keyboard.Key.A) || Keyboard.IsKeyPressed(Keyboard.Key.D))
                {
                    movement *= 0.5f;
                }

                Player.Move(movement, MapLoader.PixelTileWidth * MapLoader.Scale, Win.ElapsedTime);

                return Task.CompletedTask;
            });

            InputWatcher.AddWatchedInput(Keyboard.Key.A, key =>
            {
                movement = new Vector2f((float)GraphMath.CosFromDegrees(Player.Graphic.Rotation) * -1f,
                    (float)GraphMath.SinFromDegrees(Player.Graphic.Rotation) * -1f);

                if (Keyboard.IsKeyPressed(Keyboard.Key.W) || Keyboard.IsKeyPressed(Keyboard.Key.S))
                {
                    movement *= 0.5f;
                }

                Player.Move(movement, MapLoader.PixelTileWidth * MapLoader.Scale, Win.ElapsedTime);

                return Task.CompletedTask;
            });

            InputWatcher.AddWatchedInput(Keyboard.Key.S, key =>
            {
                movement = new Vector2f((float)GraphMath.SinFromDegrees(Player.Graphic.Rotation) * -1f,
                    (float)GraphMath.CosFromDegrees(Player.Graphic.Rotation));

                if (Keyboard.IsKeyPressed(Keyboard.Key.A) || Keyboard.IsKeyPressed(Keyboard.Key.D))
                {
                    movement *= 0.5f;
                }

                Player.Move(movement, MapLoader.PixelTileWidth * MapLoader.Scale, Win.ElapsedTime);

                return Task.CompletedTask;
            });

            InputWatcher.AddWatchedInput(Keyboard.Key.D, key =>
            {
                movement = new Vector2f((float)GraphMath.CosFromDegrees(Player.Graphic.Rotation),
                    (float)GraphMath.SinFromDegrees(Player.Graphic.Rotation));

                if (Keyboard.IsKeyPressed(Keyboard.Key.W) || Keyboard.IsKeyPressed(Keyboard.Key.S))
                {
                    movement *= 0.5f;
                }

                Player.Move(movement, MapLoader.PixelTileWidth * MapLoader.Scale, Win.ElapsedTime);

                return Task.CompletedTask;
            });

            InputWatcher.AddWatchedInput(Keyboard.Key.G, key =>
            {
                CurrentMap.Minimap.VArray.ModifyOpacity(-25, 10);

                return Task.CompletedTask;
            });

            InputWatcher.AddWatchedInput(Keyboard.Key.H, key =>
            {
                CurrentMap.Minimap.VArray.ModifyOpacity(25);

                return Task.CompletedTask;
            });

            InputWatcher.AddWatchedInput(Keyboard.Key.Q,
                async key => { await Player.RotateEntity(Win.ElapsedTime, 180f, false); });

            InputWatcher.AddWatchedInput(Keyboard.Key.E,
                async key => { await Player.RotateEntity(Win.ElapsedTime, 180f, true); });
        }

        private void SetupWatchedButtons()
        {
            InputWatcher.AddWatchedInput(Mouse.Button.Left, button =>
            {
                if (Win.IsInMenu)
                {
                    return Task.CompletedTask;
                }

                Vector2i mousePosition = Win.GetRelativeMousePosition();
                View gameView = Win.GetView("game");

                double relativeMouseX =
                    gameView.Size.X * (mousePosition.X / (Win.Size.X * gameView.Viewport.Width)) -
                    gameView.Size.X / 2f;
                double relativeMouseY =
                    gameView.Size.Y * (mousePosition.Y / (Win.Size.Y * gameView.Viewport.Height)) -
                    gameView.Size.Y / 2f;

                Win.DrawItem("game", 2,
                    Player.GetFiredProjectile(relativeMouseX, relativeMouseY, CurrentMap.Metadata.TileWidth));

                return Task.CompletedTask;
            });
        }

        private async Task InitialiseUserInterface()
        {
            string retVal =
                await RESTClient.RequestAsync(RequestMethod.GET, $"{NetManager.ServerUrl}/gameservice/tilesets/ui");
            TileSetMetadata tileSetMetadata = JsonConvert.DeserializeObject<TileSetMetadata>(retVal);

            retVal = await RESTClient.RequestAsync(RequestMethod.GET, $"{NetManager.ServerUrl}/gameservice/images/ui");
            byte[] uiImage = JsonConvert.DeserializeObject<byte[]>(retVal);

            UserInterface = new UI(tileSetMetadata, uiImage);
            UserInterface.UpdateHealth(Player.CurrentHP);

            Win.DrawItem("ui", 2, new DrawItem(Guid.NewGuid().ToString(), 0, (window, frameTime) =>
            {
                foreach (Sprite sprite in UserInterface.Hearts)
                {
                    window.Draw(sprite);
                }
            }));
        }

        private void InitialiseMiniMap()
        {
            Win.MoveView("minimap", Player.Graphic.Position);

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

            Player.MinimapObject = playerTile;

            // todo something with this
            //CurrentMap.Minimap.AnchorExpression = playerTile;

            Win.DrawItem("minimap", 1,
                new DrawItem(Player.Guid, 0, (window, frameTime) => { window.Draw(playerTile); }));
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

        private Task PlayerPositionChanged(object sender, Vector2f position)
        {
            Win.MoveView("game", position);
            Win.MoveView("minimap", position);

            NetManager.StateUpdater.AllocateStateUpdate(StateUpdateType.Position,
                new Vector2i((int)position.X, (int)position.Y));

            return Task.CompletedTask;
        }

        private Task PlayerRotationChanged(object sender, float rotation)
        {
            Win.RotateView("game", rotation);

            NetManager.StateUpdater.AllocateStateUpdate(StateUpdateType.Rotation, (int)rotation);

            return Task.CompletedTask;
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