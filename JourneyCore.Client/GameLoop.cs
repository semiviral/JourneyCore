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
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
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
        private WindowManager WinManager { get; set; }
        private UI UserInterface { get; set; }
        private LocalMap CurrentMap { get; set; }
        private Entity Player { get; set; }
        private InputWatcher InputWatcher { get; }
        private bool IsRunning { get; set; }


        public async Task StartAsync()
        {
            Shape uiSquare = new RectangleShape(new Vector2f(200f, 600f))
            {
                FillColor = Color.Cyan,
                Position = new Vector2f(0f, 0f),
                Origin = new Vector2f(0f, 0f)
            };

            WinManager.DrawItem("ui", 1, new DrawItem(Guid.NewGuid().ToString(), 0, (window, frameTime) =>
            {
                window.Draw(uiSquare);
            }));



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

            WinManager.DrawItem("game", 1,
                new DrawItem(Guid.NewGuid().ToString(), 0,
                    (window, frameTime) => { window.Draw(CurrentMap.VArray, CurrentMap.RenderStates); }));

            try
            {
                while (IsRunning)
                {
                    await InputWatcher.CheckWatchedInputs();

                    WinManager.UpdateWindow();
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

        public async Task Initialise(string serverUrl, string servicePath, int maximumFrameRate)
        {
            await InitialiseGameServerConnection(serverUrl, servicePath);
            InitialiseGameWindow(maximumFrameRate);
            await InitialiseLocalMap();
            await InitialisePlayer();
            SetupWatchedKeys();
            await SetupWatchedButtons();
            await InitialiseUserInterface();

            WinManager.GainedFocus += (sender, args) => { InputWatcher.WindowFocused = true; };
            WinManager.LostFocus += (sender, args) => { InputWatcher.WindowFocused = false; };
        }

        private async Task InitialiseGameServerConnection(string serverUrl, string servicePath)
        {
            NetManager = new GameServerConnection(serverUrl);
            await NetManager.InitialiseAsync(servicePath);
        }

        private void InitialiseGameWindow(int maximumFrameRate)
        {
            Log.Information("Initialising game window...");

            WinManager = new WindowManager("Journey to the Core", new VideoMode(1000, 600, 8), maximumFrameRate,
                new Vector2f(2f, 2f),
                15f);
            WinManager.Closed += (sender, args) => { IsRunning = false; ExitWithFatality("Game window closed."); };

            WinManager.CreateView("game", new View(new FloatRect(0f, 0f, 200f, 200f))
            {
                Viewport = new FloatRect(0f, 0f, 0.8f, 1f)
            });

            WinManager.CreateView("ui", new View(new FloatRect(0f, 0f, 200f, 600f))
            {
                Viewport = new FloatRect(0.8f, 0f, 0.2f, 1f)
            });

            Log.Information("Game window initialised.");
        }

        private async Task InitialiseLocalMap()
        {
            string retVal = await RESTClient.Request(RequestMethod.GET, $"{NetManager.ServerUrl}/gameservice/images/maps");
            CurrentMap = new LocalMap(JsonConvert.DeserializeObject<byte[]>(retVal));

            Log.Information("Requesting map: AdventurersGuild");

            CurrentMap.Update(await RequestMapMetadata("AdventurersGuild"));
        }

        private async Task InitialisePlayer()
        {
            Log.Information("Initialising player...");

            string retVal =
                await RESTClient.Request(RequestMethod.GET, $"{NetManager.ServerUrl}/gameservice/images/human");

            Player = new Entity("player", "player", 0,
                new Sprite(new Texture(JsonConvert.DeserializeObject<byte[]>(retVal))));
            Player.PropertyChanged += PlayerPropertyChanged;
            Player.PositionChanged += PlayerPositionChanged;
            Player.RotationChanged += PlayerRotationChanged;

            WinManager.MoveView("game", Player.Graphic.Position);

            WinManager.DrawItem("game", 2,
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

                Player.Move(movement, MapLoader.PixelTileWidth * MapLoader.Scale, WinManager.ElapsedTime);

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

                Player.Move(movement, MapLoader.PixelTileWidth * MapLoader.Scale, WinManager.ElapsedTime);

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

                Player.Move(movement, MapLoader.PixelTileWidth * MapLoader.Scale, WinManager.ElapsedTime);

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

                Player.Move(movement, MapLoader.PixelTileWidth * MapLoader.Scale, WinManager.ElapsedTime);

                return Task.CompletedTask;
            });

            InputWatcher.AddWatchedInput(Keyboard.Key.Q,
                async key => { await Player.RotateEntity(WinManager.ElapsedTime, 180f, false); });

            InputWatcher.AddWatchedInput(Keyboard.Key.E,
                async key => { await Player.RotateEntity(WinManager.ElapsedTime, 180f, true); });
        }

        private async Task SetupWatchedButtons()
        {
            string retVal = await RESTClient.Request(RequestMethod.GET,
                $"{NetManager.ServerUrl}/gameservice/images/projectiles");
            Texture texture = new Texture(JsonConvert.DeserializeObject<byte[]>(retVal));

            InputWatcher.AddWatchedInput(Mouse.Button.Left, button =>
            {
                if (WinManager.IsInMenu)
                {
                    return Task.CompletedTask;
                }

                Vector2i mousePosition = WinManager.GetRelativeMousePosition();

                double relativeMouseX = WinManager.GetView("game").Size.X *
                                        (mousePosition.X /
                                         (WinManager.Size.X * WinManager.GetView("game").Viewport.Width)) -
                                        100d;
                double relativeMouseY = WinManager.GetView("game").Size.Y *
                                        (mousePosition.Y /
                                         (WinManager.Size.Y * WinManager.GetView("game").Viewport.Height)) -
                                        100d;

                double angle = 180 / Math.PI * Math.Atan2(relativeMouseY, relativeMouseX) + Player.Graphic.Rotation +
                               90d;

                if (DateTime.Now < Player.ProjectileCooldown)
                {
                    return Task.CompletedTask;
                }

                Player.ProjectileCooldown = DateTime.Now.AddMilliseconds(10);

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
                    projectile.Move(movement, CurrentMap.Metadata.TileWidth, frameTime);

                    window.Draw(projectile.Graphic);
                });

                WinManager.DrawItem("game", 2, projectileDrawItem);

                return Task.CompletedTask;
            });
        }

        private async Task InitialiseUserInterface()
        {
            string retVal = await RESTClient.Request(RequestMethod.GET, $"{NetManager.ServerUrl}/gameservice/tilesets/ui");
            TileSetMetadata tileSetMetadata = JsonConvert.DeserializeObject<TileSetMetadata>(retVal);

            retVal = await RESTClient.Request(RequestMethod.GET, $"{NetManager.ServerUrl}/gameservice/images/ui");
            byte[] uiImage = JsonConvert.DeserializeObject<byte[]>(retVal);

            UserInterface = new UI(tileSetMetadata, uiImage);
            UserInterface.UpdateHealth(Player.CurrentHP);

            WinManager.DrawItem("ui", 2, new DrawItem(Guid.NewGuid().ToString(), 0, (window, frameTime) =>
            {
                foreach (Sprite sprite in UserInterface.Hearts)
                {
                    window.Draw(sprite);
                }
            }));
        }

        #endregion


        #region CLIENT-TO-SERVER

        private async Task<MapMetadata> RequestMapMetadata(string mapName)
        {
            string retVal =
                await RESTClient.Request(RequestMethod.GET, $"{NetManager.ServerUrl}/maps/metadata/{mapName}");

            return JsonConvert.DeserializeObject<MapMetadata>(retVal);
        }

        private async Task<Chunk[]> RequestChunk(Vector2i coords)
        {
            string retVal = await RESTClient.Request(RequestMethod.GET,
                $"{NetManager.ServerUrl}/maps/{CurrentMap.Metadata.Name}/{coords.X}/{coords.Y}");

            return JsonConvert.DeserializeObject<Chunk[]>(retVal);
        }

        #endregion


        #region EVENT

        private Task PlayerPositionChanged(object sender, Vector2f position)
        {
            WinManager.MoveView("game", position);

            NetManager.StateUpdater.AllocateStateUpdate(StateUpdateType.Position,
                new Vector2i((int)position.X, (int)position.Y));

            return Task.CompletedTask;
        }

        private Task PlayerRotationChanged(object sender, float rotation)
        {
            WinManager.RotateView("game", rotation);

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


        public static void ExitWithFatality(string error, int exitCode = -1)
        {
            // allow game window to close before fataling

            Log.Fatal(error);
            Log.Fatal("Press any key to continue.");

            Console.ReadLine();

            Environment.Exit(exitCode);
        }
    }
}