using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using JourneyCore.Lib.Display;
using JourneyCore.Lib.Display.Component;
using JourneyCore.Lib.Display.Drawing;
using JourneyCore.Lib.Game.Environment.Mapping;
using JourneyCore.Lib.Game.Environment.Metadata;
using JourneyCore.Lib.Game.Object.Entity;
using JourneyCore.Lib.System.Event;
using JourneyCore.Lib.System.Loaders;
using JourneyCore.Lib.System.Math;
using JourneyCore.Lib.System.Net;
using JourneyCore.Lib.System.Net.Security;
using JourneyCore.Lib.System.Static;
using Newtonsoft.Json;
using Serilog;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace JourneyCore.Client
{
    public class GameLoop : Context
    {
        private static Tuple<int, string> _fatalExit;

        private bool IsFocused { get; set; }

        private GameServerConnection NetManager { get; set; }
        private ConsoleManager ConManager { get; }
        private GameWindow Window { get; set; }
        private Ui UserInterface { get; set; }
        private LocalMap CurrentMap { get; set; }
        private Player Player { get; set; }
        private InputWatcher InputWatcher { get; }

        public GameLoop(uint maximumFrameRate)
        {
            IsFocused = true;
            ConManager = new ConsoleManager();
            ConManager.Hide(false);

            CreateGameWindow(maximumFrameRate);

            Log.Information("Game loop started.");

            InputWatcher = new InputWatcher();
        }

        public void Start()
        {
            Window.AddDrawItem(DrawViewLayer.Game, 0,
                new DrawItem(Guid.NewGuid().ToString(), DateTime.MinValue, null,
                    new DrawObject(typeof(VertexArray), CurrentMap.VArray), CurrentMap.RenderStates));
            Window.AddDrawItem(DrawViewLayer.Minimap, 0,
                new DrawItem(Guid.NewGuid().ToString(), DateTime.MinValue, null,
                    new DrawObject(typeof(VertexArray), CurrentMap.Minimap.VArray), RenderStates.Default));

            try
            {
                while (Window.IsActive)
                {
                    // ensures window runtime methods are only executed in main thread
                    if (Thread.CurrentThread.ManagedThreadId != 1)
                    {
                        CallFatality("Window runtime attempting to execute outside of main thread. Exiting game.");
                    }

                    InputWatcher.CheckWatchedInputs();

                    Window.UpdateWindow();
                }
            }
            catch (Exception ex)
            {
                CallFatality(ex.Message);
                ExitWithFatality();
            }
        }

        public static void CallFatality(string error, int exitCode = -1)
        {
            _fatalExit = new Tuple<int, string>(exitCode, error);
            ExitWithFatality();
        }

        private static void ExitWithFatality()
        {
            ThreadPool.QueueUserWorkItem(callback =>
            {
                Log.Fatal(_fatalExit.Item2);

                Environment.Exit(_fatalExit.Item1);
            });
        }


        #region INITIALISATION

        public void Initialise(string serverUrl, string servicePath)
        {
            DoInitialise(serverUrl, servicePath).Wait();
        }

        private async Task DoInitialise(string serverUrl, string servicePath)
        {
            try
            {
                await ConnectGameServer(serverUrl, servicePath);
                CreateDrawViews();
                await CreateLocalMap();
                await SetupPlayer();
                SetupWatchedKeys();
                SetupWatchedMouse();
                await CreateUserInterface();
                SetupMinimap();

                Settings settings = new Settings(Window);
                settings.Initialise();

#if DEBUG
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
#endif

                Window.GainedFocus += (sender, args) => { IsFocused = true; };
                Window.LostFocus += (sender, args) => { IsFocused = false; };
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        private async Task ConnectGameServer(string serverUrl, string servicePath)
        {
            NetManager = new GameServerConnection(serverUrl);
            NetManager.FatalExit += (sender, args) =>
            {
                CallFatality(args);
                return Task.CompletedTask;
            };
            await NetManager.InitialiseAsync(servicePath);
        }

        private void CreateGameWindow(uint maximumFrameRate)
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
                Window.GetDrawView(DrawViewLayer.Minimap).ZoomFactor += args.Delta * -1f;
            };

            Log.Information("Game window initialised.");
        }

        private void CreateDrawViews()
        {
            const float viewSizeY = 200f;
            const float minimapSizeX = 0.2f;

            Window.CreateDrawView(DrawViewLayer.Game,
                new View(new FloatRect(0f, 0f, viewSizeY * GameWindow.WidescreenRatio, viewSizeY))
                {
                    Viewport = new FloatRect(0f, 0f, 1f, 1f)
                }, true);

            Window.CreateDrawView(DrawViewLayer.UI,
                new View(new FloatRect(0f, 0f, 200f, 600f))
                {
                    Viewport = new FloatRect(0.8f, 0.3f, 0.2f, 0.7f)
                }, true);

            Window.CreateDrawView(DrawViewLayer.Minimap,
                new View(new FloatRect(0f, 0f, viewSizeY * GameWindow.WidescreenRatio, viewSizeY))
                {
                    Viewport = new FloatRect(0.8f, 0f, minimapSizeX, minimapSizeX * GameWindow.LetterboxRatio)
                }, true);
        }

        private async Task CreateLocalMap()
        {
            CurrentMap = new LocalMap(await RequestImage("maps"));

            Log.Information("Requesting map: AdventurersGuild");

            CurrentMap.Update(await RequestMapMetadata("AdventurersGuild"));
        }

        private async Task SetupPlayer()
        {
            Log.Information("Initialising player...");

            Texture humanTexture = new Texture(await RequestImage("avatar"));
            Texture projectilesTexture = new Texture(await RequestImage("projectiles"));

            // todo no hard coding for player texture size
            Player = new Player(new Sprite(humanTexture, new IntRect(3 * 32, 1 * 32, 32, 32)), projectilesTexture, 0);
            Player.PropertyChanged += PlayerPropertyChanged;
            Player.PositionChanged += PlayerPositionChanged;
            Player.RotationChanged += PlayerRotationChanged;

            Player.AnchorItem(Window.GetDrawView(DrawViewLayer.Game));
            Player.AnchorItem(Window.GetDrawView(DrawViewLayer.Minimap));

            Window.AddDrawItem(DrawViewLayer.Game, 10,
                new DrawItem(Player.Guid, DateTime.MinValue, null,
                    new DrawObject(Player.Graphic.GetType(), Player.Graphic, Player.Graphic.GetVertices),
                    new RenderStates(Player.Graphic.Texture)));

            Log.Information("Player intiailised.");
        }

        private void SetupWatchedKeys()
        {
            Log.Information("Creating input watch events...");

            Vector2f movement = new Vector2f(0, 0);

            InputWatcher.AddWatchedInput(Keyboard.Key.W, () =>
            {
                movement = new Vector2f(
                    (float)GraphMath.SinFromDegrees(Player.Graphic.Rotation + DrawView.DefaultPlayerViewRotation % 360),
                    (float)GraphMath.CosFromDegrees(Player.Graphic.Rotation +
                                                    DrawView.DefaultPlayerViewRotation % 360) * -1f);

                if (Keyboard.IsKeyPressed(Keyboard.Key.A) || Keyboard.IsKeyPressed(Keyboard.Key.D))
                {
                    movement *= 0.5f;
                }

                Player.MoveEntity(movement, MapLoader.TilePixelSize, Window.ElapsedTime);
            }, () => IsFocused);

            InputWatcher.AddWatchedInput(Keyboard.Key.A, () =>
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

                Player.MoveEntity(movement, MapLoader.TilePixelSize, Window.ElapsedTime);
            }, () => IsFocused);

            InputWatcher.AddWatchedInput(Keyboard.Key.S, () =>
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

                Player.MoveEntity(movement, MapLoader.TilePixelSize, Window.ElapsedTime);
            }, () => IsFocused);

            InputWatcher.AddWatchedInput(Keyboard.Key.D, () =>
            {
                movement = new Vector2f(
                    (float)GraphMath.CosFromDegrees(Player.Graphic.Rotation + DrawView.DefaultPlayerViewRotation % 360),
                    (float)GraphMath.SinFromDegrees(Player.Graphic.Rotation +
                                                    DrawView.DefaultPlayerViewRotation % 360));

                if (Keyboard.IsKeyPressed(Keyboard.Key.W) || Keyboard.IsKeyPressed(Keyboard.Key.S))
                {
                    movement *= 0.5f;
                }

                Player.MoveEntity(movement, MapLoader.TilePixelSize, Window.ElapsedTime);
            }, () => IsFocused);

            InputWatcher.AddWatchedInput(Keyboard.Key.G, () => { CurrentMap.Minimap.VArray.ModifyOpacity(-25, 10); },
                () => IsFocused);

            InputWatcher.AddWatchedInput(Keyboard.Key.H, () => { CurrentMap.Minimap.VArray.ModifyOpacity(25); },
                () => IsFocused);

            InputWatcher.AddWatchedInput(Keyboard.Key.Q,
                () => { Player.RotateEntity(Window.ElapsedTime, 180f, false); }, () => IsFocused);

            InputWatcher.AddWatchedInput(Keyboard.Key.E,
                () => { Player.RotateEntity(Window.ElapsedTime, 180f, true); }, () => IsFocused);

            InputWatcher.AddWatchedInput(Keyboard.Key.Escape, () =>
            {
                DrawView drawView = Window.GetDrawView(DrawViewLayer.Settings);
                drawView.Visible = !drawView.Visible;
            }, () => true, true);
        }

        private void SetupWatchedMouse()
        {
            InputWatcher.AddWatchedInput(Mouse.Button.Left, () =>
            {
                Vector2i mousePosition = Window.GetRelativeMousePosition();
                View gameView = Window.GetDrawView(DrawViewLayer.Game).View;

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

                Window.AddDrawItem(DrawViewLayer.Game, 20, projectileDrawItem);
            });
        }

        private async Task CreateUserInterface()
        {
            TileSetMetadata uiTileSetMetadata = await RequestTileSetMetadata("ui");
            byte[] uiImage = await RequestImage("ui");

            UserInterface = new Ui(uiTileSetMetadata, uiImage);
            UserInterface.UpdateHealth(Player.CurrentHp);
        }

        private void SetupMinimap()
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

            Window.AddDrawItem(DrawViewLayer.Minimap, 10,
                new DrawItem(Guid.NewGuid().ToString(), DateTime.MinValue, null, playerTileObj, RenderStates.Default));
        }

        #endregion


        #region CLIENT-TO-SERVER

        private async Task<byte[]> RequestImage(string imageName)
        {
            string retVal = await NetManager.GetResponseAsync(
                $"gameservice/images?guid={NetManager.Guid}&remotePublicKeyBase64={NetManager.CryptoService.PublicKeyString.HtmlEncodeBase64()}&imageNameBase64={await NetManager.GetHtmlSafeEncryptedBase64(imageName)}");
            DiffieHellmanMessagePackage messagePackage =
                JsonConvert.DeserializeObject<DiffieHellmanMessagePackage>(retVal);

            string serializedImageBytes =
                await NetManager.CryptoService.DecryptAsync(messagePackage.RemotePublicKey,
                    messagePackage.SecretMessage);
            byte[] imageBytes = JsonConvert.DeserializeObject<byte[]>(serializedImageBytes);

            return imageBytes;
        }

        private async Task<MapMetadata> RequestMapMetadata(string mapName)
        {
            string retVal = await NetManager.GetResponseAsync(
                $"maps/{await NetManager.GetHtmlSafeEncryptedBase64(mapName)}/metadata?guid={NetManager.Guid}&remotePublicKeyBase64={NetManager.CryptoService.PublicKeyString.HtmlEncodeBase64()}");
            DiffieHellmanMessagePackage messagePackage =
                JsonConvert.DeserializeObject<DiffieHellmanMessagePackage>(retVal);

            string serializedMapMetadata =
                await NetManager.CryptoService.DecryptAsync(messagePackage.RemotePublicKey,
                    messagePackage.SecretMessage);
            MapMetadata mapMetadata = JsonConvert.DeserializeObject<MapMetadata>(serializedMapMetadata);

            return mapMetadata;
        }

        private async Task<TileSetMetadata> RequestTileSetMetadata(string tileSetName)
        {
            string retVal = await NetManager.GetResponseAsync(
                $"gameservice/tilesets?guid={NetManager.Guid}&remotePublicKeyBase64={NetManager.CryptoService.PublicKeyString.HtmlEncodeBase64()}&tileSetNameBase64={await NetManager.GetHtmlSafeEncryptedBase64(tileSetName)}");
            DiffieHellmanMessagePackage messagePackage =
                JsonConvert.DeserializeObject<DiffieHellmanMessagePackage>(retVal);

            string serializedTileSetMetadata =
                await NetManager.CryptoService.DecryptAsync(messagePackage.RemotePublicKey,
                    messagePackage.SecretMessage);
            TileSetMetadata tileSetMetadata = JsonConvert.DeserializeObject<TileSetMetadata>(serializedTileSetMetadata);

            return tileSetMetadata;
        }

        private async Task<Chunk[]> RequestChunk(Vector2i coords)
        {
            string retVal = await NetManager.GetResponseAsync(
                $"maps/{await NetManager.GetHtmlSafeEncryptedBase64(CurrentMap.Metadata.Name)}?guid={NetManager.Guid}&remotePublicKeyBase64={NetManager.CryptoService.PublicKeyString.HtmlEncodeBase64()}&coordsBase64={await NetManager.GetHtmlSafeEncryptedBase64(JsonConvert.SerializeObject(coords))}");
            DiffieHellmanMessagePackage messagePackage =
                JsonConvert.DeserializeObject<DiffieHellmanMessagePackage>(retVal);

            string serializedChunks =
                await NetManager.CryptoService.DecryptAsync(messagePackage.RemotePublicKey,
                    messagePackage.SecretMessage);
            Chunk[] chunks = JsonConvert.DeserializeObject<Chunk[]>(serializedChunks);

            return chunks;
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