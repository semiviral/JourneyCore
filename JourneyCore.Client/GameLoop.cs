using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using JourneyCore.Client.DrawViews;
using JourneyCore.Lib.Display;
using JourneyCore.Lib.Display.Drawing;
using JourneyCore.Lib.Game.Environment.Mapping;
using JourneyCore.Lib.Game.Environment.Metadata;
using JourneyCore.Lib.Game.Object.Collision;
using JourneyCore.Lib.Game.Object.Entity;
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
        private static Tuple<int, string> fatalExit;

        public GameLoop(uint maximumFrameRate)
        {
            DefaultFont =
                new Font(@"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\Assets\Fonts\Avara.ttf");

            IsFocused = true;
            ConManager = new ConsoleManager();

#if DEBUG
            ConManager.Hide(false);
#else
            ConManager.Hide(true);
#endif

            CreateGameWindow(maximumFrameRate);


            Log.Information("Game loop started.");
        }

        private bool IsFocused { get; set; }

        private GameServerConnection NetManager { get; set; }
        private ConsoleManager ConManager { get; }
        private GameWindow GameWindow { get; set; }
        private Ui UserInterface { get; set; }
        private LocalMap ActivateMap { get; set; }
        private Player Player { get; set; }
        private ServerStateUpdater ServerUpdater { get; set; }


        public static Font DefaultFont { get; set; }

        public void Start()
        {
            GameWindow.AddDrawItem(DrawViewLayer.Game, 0,
                new DrawItem(new DrawObject(ActivateMap.VArray), ActivateMap.RenderStates));
            GameWindow.AddDrawItem(DrawViewLayer.Minimap, 0, new DrawItem(new DrawObject(ActivateMap.Minimap.VArray)));

            try
            {
                while (GameWindow.IsActive)
                {
                    if (Thread.CurrentThread.ManagedThreadId != 1)
                    {
                        CallFatality("Window runtime attempting to execute outside of main thread. Exiting game.");
                    }

                    GameWindow.UpdateWindow();
                }
            }
            catch (Exception _ex)
            {
                CallFatality(_ex.Message);
                ExitWithFatality();
            }
        }

        public static void CallFatality(string error, int exitCode = -1)
        {
            fatalExit = new Tuple<int, string>(exitCode, error);
            ExitWithFatality();
        }

        private static void ExitWithFatality()
        {
            ThreadPool.QueueUserWorkItem(callback =>
            {
                Log.Fatal(fatalExit.Item2);

                Environment.Exit(fatalExit.Item1);
            });
        }

        private IEnumerable<Vector2f> GetAdjustmentVectors(CollisionQuad subjectQuad)
        {
            foreach (CollisionQuad _quad in ActivateMap.Metadata.Colliders)
            {
                foreach (Vector2f _adjustment in GraphMath.GetDiagnasticCollisionOffsets(subjectQuad, _quad))
                {
                    yield return _adjustment * -1f;
                }
                
                foreach (Vector2f _adjustment in GraphMath.GetDiagnasticCollisionOffsets(_quad, subjectQuad))
                {
                    yield return _adjustment * -1f;
                }
            }
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
                ConnectGameServer(serverUrl, servicePath).Wait();
                CreateDrawViews();
                await CreateLocalMap();
                await SetupPlayer();
                SetupWatchedKeys();
                SetupWatchedMouse();
                await CreateUserInterface();
                SetupMinimap();
                
                EscapeMenu _escapeMenu = new EscapeMenu(GameWindow);
                _escapeMenu.Initialise();

                Settings _settings = new Settings(GameWindow);
                _settings.Initialise();

                for (int _x = 0; _x < (ActivateMap.Metadata.Width / MapLoader.ChunkSize); _x++)
                for (int _y = 0; _y < (ActivateMap.Metadata.Height / MapLoader.ChunkSize); _y++)
                {
                    foreach (Chunk _chunk in await RequestChunk(new Vector2i(_x, _y)))
                    {
                        ActivateMap.LoadChunk(_chunk);
                    }
                }


                GameWindow.GainedFocus += (sender, args) => { IsFocused = true; };
                GameWindow.LostFocus += (sender, args) => { IsFocused = false; };
            }
            catch (Exception _ex)
            {
                Log.Error(_ex.Message);
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

            NetManager.On<Vector2f>("ReceivePlayerPositionModification",
                modification => { Player.Position = modification; });
            NetManager.On<float>("ReceivePlayerRotationModification", rotation => { });
            
            ServerUpdater = new ServerStateUpdater(NetManager, 33);
        }

        private void CreateGameWindow(uint maximumFrameRate)
        {
            Log.Information("Initializing game window...");

            GameWindow = new GameWindow("Journey to the Core", new VideoMode(1280, 720, 8), maximumFrameRate,
                new Vector2f(2f, 2f),
                15f);
            GameWindow.Closed += (sender, args) =>
            {
                GameWindow.SetActive(false);
                CallFatality("Game window closed.");
            };
            GameWindow.MouseWheelScrolled += (sender, args) =>
            {
                GameWindow.GetDrawView(DrawViewLayer.Minimap).ZoomFactor += args.Delta * -1f;
            };

            Log.Information("Game window initialized.");
        }

        private void CreateDrawViews()
        {
            const float view_size_y = 200f;
            const float minimap_size_x = 0.2f;

            GameWindow.CreateDrawView(new DrawView(DrawViewLayer.Game,
                new View(new FloatRect(0f, 0f, view_size_y * GameWindow.WIDESCREEN_RATIO, view_size_y))
                {
                    Viewport = new FloatRect(0f, 0f, 1f, 1f)
                }, true));

            GameWindow.CreateDrawView(new DrawView(DrawViewLayer.Ui,
                new View(new FloatRect(0f, 0f, 200f, 600f))
                {
                    Viewport = new FloatRect(0.8f, 0.3f, 0.2f, 0.7f)
                }, true));

            GameWindow.CreateDrawView(new DrawView(DrawViewLayer.Minimap,
                new View(new FloatRect(0f, 0f, view_size_y * GameWindow.WIDESCREEN_RATIO, view_size_y))
                {
                    Viewport = new FloatRect(0.8f, 0f, minimap_size_x, minimap_size_x * GameWindow.LETTERBOX_RATIO)
                }, true));
        }

        private async Task CreateLocalMap()
        {
            ActivateMap = new LocalMap(await RequestImage("maps"));

            Log.Information("Requesting map: AdventurersGuild");

            ActivateMap.Update(await RequestMapMetadata("AdventurersGuild"));

            foreach (CollisionQuad _quad in ActivateMap.Metadata.Colliders)
            {
                GameWindow.AddDrawItem(DrawViewLayer.Game, 30, new DrawItem(new DrawObject(_quad, _quad.GetVertices)));
            }
        }

        private async Task SetupPlayer()
        {
            Log.Information("Initializing player...");

            Player = await RequestPlayer(NetManager.ConnectionId);
            // todo get sprite size dynamically
            Player.ClientSideInitialise(new Vector2i(36, 36));
            Player.PropertyChanged += PlayerPropertyChanged;
            Player.PositionChanged += PlayerPositionChanged;
            Player.RotationChanged += PlayerRotationChanged;
            Player.GetCollisionAdjustments = GetAdjustmentVectors;
            Player.Position = new Vector2f(ActivateMap.Metadata.SpawnPointX, ActivateMap.Metadata.SpawnPointY);

            Player.AnchorItem(GameWindow.GetDrawView(DrawViewLayer.Game));
            Player.AnchorItem(GameWindow.GetDrawView(DrawViewLayer.Minimap));
            
            GameWindow.AddDrawItem(DrawViewLayer.Game, 10,
                new DrawItem(new DrawObject(Player.Graphic, Player.Graphic.GetVertices),
                    new RenderStates(Player.Graphic.Texture)));

            Log.Information("Player initialized.");
            
            Player.Collider.FillColor = Color.Magenta;
            GameWindow.AddDrawItem(DrawViewLayer.Game, 11,
                new DrawItem(new DrawObject(Player.Collider, Player.Collider.GetVertices), RenderStates.Default));
            
            // todo fix collision box and minimap object not centered on player
        }

        private void SetupWatchedKeys()
        {
            Log.Information("Creating input watch events...");

            Vector2f _movement = new Vector2f(0, 0);

            GameWindow.AddWatchedInput(Keyboard.Key.R, () => { Player.Rotation = 0f; }, () => IsFocused, true);
            
            GameWindow.AddWatchedInput(Keyboard.Key.W, () =>
            {
                _movement = new Vector2f(
                    (float) GraphMath.SinFromDegrees(Player.Graphic.Rotation +
                                                     (DrawView.DEFAULT_VIEW_ROTATION % 360)),
                    (float) GraphMath.CosFromDegrees(Player.Graphic.Rotation +
                                                     (DrawView.DEFAULT_VIEW_ROTATION % 360)) * -1f);

                if (Keyboard.IsKeyPressed(Keyboard.Key.A) || Keyboard.IsKeyPressed(Keyboard.Key.D))
                {
                    _movement *= 0.5f;
                }

                Player.MoveEntity(_movement, MapLoader.TilePixelSize, GameWindow.ElapsedTime);
            }, () => IsFocused);

            GameWindow.AddWatchedInput(Keyboard.Key.A, () =>
            {
                _movement = new Vector2f(
                    (float) GraphMath.CosFromDegrees(Player.Graphic.Rotation +
                                                     (DrawView.DEFAULT_VIEW_ROTATION % 360)) * -1f,
                    (float) GraphMath.SinFromDegrees(Player.Graphic.Rotation +
                                                     (DrawView.DEFAULT_VIEW_ROTATION % 360)) * -1f);

                if (Keyboard.IsKeyPressed(Keyboard.Key.W) || Keyboard.IsKeyPressed(Keyboard.Key.S))
                {
                    _movement *= 0.5f;
                }

                Player.MoveEntity(_movement, MapLoader.TilePixelSize, GameWindow.ElapsedTime);
            }, () => IsFocused);

            GameWindow.AddWatchedInput(Keyboard.Key.S, () =>
            {
                _movement = new Vector2f(
                    (float) GraphMath.SinFromDegrees(Player.Graphic.Rotation +
                                                     (DrawView.DEFAULT_VIEW_ROTATION % 360)) * -1f,
                    (float) GraphMath.CosFromDegrees(Player.Graphic.Rotation +
                                                     (DrawView.DEFAULT_VIEW_ROTATION % 360)));

                if (Keyboard.IsKeyPressed(Keyboard.Key.A) || Keyboard.IsKeyPressed(Keyboard.Key.D))
                {
                    _movement *= 0.5f;
                }

                Player.MoveEntity(_movement, MapLoader.TilePixelSize, GameWindow.ElapsedTime);
            }, () => IsFocused);

            GameWindow.AddWatchedInput(Keyboard.Key.D, () =>
            {
                _movement = new Vector2f(
                    (float) GraphMath.CosFromDegrees(Player.Graphic.Rotation +
                                                     (DrawView.DEFAULT_VIEW_ROTATION % 360)),
                    (float) GraphMath.SinFromDegrees(Player.Graphic.Rotation +
                                                     (DrawView.DEFAULT_VIEW_ROTATION % 360)));

                if (Keyboard.IsKeyPressed(Keyboard.Key.W) || Keyboard.IsKeyPressed(Keyboard.Key.S))
                {
                    _movement *= 0.5f;
                }

                Player.MoveEntity(_movement, MapLoader.TilePixelSize, GameWindow.ElapsedTime);
            }, () => IsFocused);

            GameWindow.AddWatchedInput(Keyboard.Key.Equal,
                () =>
                {
                    if ((Keyboard.IsKeyPressed(Keyboard.Key.LControl) ||
                         Keyboard.IsKeyPressed(Keyboard.Key.RControl)) &&
                        (Keyboard.IsKeyPressed(Keyboard.Key.LShift) ||
                         Keyboard.IsKeyPressed(Keyboard.Key.RShift)))
                    {
                        GameWindow.GetDrawView(DrawViewLayer.Minimap).ModifyOpacity(25);
                    }
                },
                () => IsFocused, true);

            GameWindow.AddWatchedInput(Keyboard.Key.Hyphen,
                () =>
                {
                    if ((Keyboard.IsKeyPressed(Keyboard.Key.LControl) ||
                         Keyboard.IsKeyPressed(Keyboard.Key.RControl)) &&
                        (Keyboard.IsKeyPressed(Keyboard.Key.LShift) ||
                         Keyboard.IsKeyPressed(Keyboard.Key.RShift)))
                    {
                        GameWindow.GetDrawView(DrawViewLayer.Minimap).ModifyOpacity(-25);
                    }
                },
                () => IsFocused, true);

            GameWindow.AddWatchedInput(Keyboard.Key.Equal, () =>
            {
                // stops key action from continuing when trying to
                // change opacity
                if (Keyboard.IsKeyPressed(Keyboard.Key.LShift) ||
                    Keyboard.IsKeyPressed(Keyboard.Key.RShift))
                {
                    return;
                }

                if (Keyboard.IsKeyPressed(Keyboard.Key.LControl) ||
                    Keyboard.IsKeyPressed(Keyboard.Key.RControl))
                {
                    GameWindow.GetDrawView(DrawViewLayer.Minimap).ZoomFactor += 2f * -1f;
                }
            }, () => IsFocused, true);

            GameWindow.AddWatchedInput(Keyboard.Key.Hyphen, () =>
            {
                // stops key action from continuing when trying to
                // change opacity
                if (Keyboard.IsKeyPressed(Keyboard.Key.LShift) ||
                    Keyboard.IsKeyPressed(Keyboard.Key.RShift))
                {
                    return;
                }

                if (Keyboard.IsKeyPressed(Keyboard.Key.LControl) ||
                    Keyboard.IsKeyPressed(Keyboard.Key.RControl))
                {
                    GameWindow.GetDrawView(DrawViewLayer.Minimap).ZoomFactor += -2f * -1f;
                }
            }, () => IsFocused, true);

            GameWindow.AddWatchedInput(Keyboard.Key.Q,
                () => { Player.RotateEntity(GameWindow.ElapsedTime, 180f, false); }, () => IsFocused);

            GameWindow.AddWatchedInput(Keyboard.Key.E,
                () => { Player.RotateEntity(GameWindow.ElapsedTime, 180f, true); }, () => IsFocused);

            GameWindow.AddWatchedInput(Keyboard.Key.Escape, () =>
            {
                GameWindow.GetDrawView(DrawViewLayer.Settings).Visible = false;

                DrawView _drawView = GameWindow.GetDrawView(DrawViewLayer.EscapeMenu);
                _drawView.Visible = !_drawView.Visible;
            }, null, true);
        }

        private void SetupWatchedMouse()
        {
            GameWindow.AddWatchedInput(Mouse.Button.Left, () =>
            {
                if (GameWindow.PressCaptured)
                {
                    return;
                }

                Vector2i _mousePosition = GameWindow.GetRelativeMousePosition();
                View _gameView = GameWindow.GetDrawView(DrawViewLayer.Game).View;

                double _relativeMouseX =
                    (_gameView.Size.X * (_mousePosition.X / (GameWindow.Size.X * _gameView.Viewport.Width))) -
                    (_gameView.Size.X / 2f);
                double _relativeMouseY =
                    (_gameView.Size.Y * (_mousePosition.Y / (GameWindow.Size.Y * _gameView.Viewport.Height))) -
                    (_gameView.Size.Y / 2f);

                DrawItem _projectileDrawItem =
                    Player.FireProjectile(_relativeMouseX, _relativeMouseY, ActivateMap.Metadata.TileWidth);

                if (_projectileDrawItem == null)
                {
                    return;
                }

                GameWindow.AddDrawItem(DrawViewLayer.Game, 20, _projectileDrawItem);
            }, true);
        }

        private async Task CreateUserInterface()
        {
            TileSetMetadata _uiTileSetMetadata = await RequestTileSetMetadata("ui");
            byte[] _uiImage = await RequestImage("ui");

            UserInterface = new Ui(_uiTileSetMetadata, _uiImage);
            UserInterface.UpdateHealth(Player.CurrentHp);
        }

        public void SetupMinimap()
        {
            CollisionQuad _playerTile = new CollisionQuad(Player.Collider)
            {
                Size = new Vector2f(ActivateMap.Metadata.TileWidth, ActivateMap.Metadata.TileHeight),
                FillColor = Color.Yellow,
                OutlineColor = new Color(200, 200, 200),
                OutlineThickness = 2f
            };
            _playerTile.Origin = _playerTile.Size.MultiplyBy(_playerTile.Scale) / 2f;

            DrawObject _playerTileDrawObject = new DrawObject(_playerTile, _playerTile.GetVertices);
            
            Player.AnchorItem(_playerTile);

            GameWindow.AddDrawItem(DrawViewLayer.Minimap, 20, new DrawItem(_playerTileDrawObject, RenderStates.Default));
        }
        
        #endregion


        #region CLIENT-TO-SERVER

        private async Task<byte[]> RequestImage(string imageName)
        {
            string _retVal = await NetManager.GetResponseAsync(
                $"gameservice/images?id={NetManager.ConnectionId}&remotePublicKeyBase64={NetManager.CryptoService.PublicKeyString.HtmlEncodeBase64()}&imageNameBase64={await NetManager.GetHtmlSafeEncryptedBase64(imageName)}");
            DiffieHellmanMessagePackage _messagePackage =
                JsonConvert.DeserializeObject<DiffieHellmanMessagePackage>(_retVal);

            string _serializedImageBytes =
                await NetManager.CryptoService.DecryptAsync(_messagePackage.RemotePublicKey,
                    _messagePackage.SecretMessage);
            byte[] _imageBytes = JsonConvert.DeserializeObject<byte[]>(_serializedImageBytes);

            return _imageBytes;
        }

        private async Task<MapMetadata> RequestMapMetadata(string mapName)
        {
            string _retVal = await NetManager.GetResponseAsync(
                $"maps/{await NetManager.GetHtmlSafeEncryptedBase64(mapName)}/metadata?id={NetManager.ConnectionId}&remotePublicKeyBase64={NetManager.CryptoService.PublicKeyString.HtmlEncodeBase64()}");
            DiffieHellmanMessagePackage _messagePackage =
                JsonConvert.DeserializeObject<DiffieHellmanMessagePackage>(_retVal);

            string _serializedMapMetadata =
                await NetManager.CryptoService.DecryptAsync(_messagePackage.RemotePublicKey,
                    _messagePackage.SecretMessage);
            MapMetadata _mapMetadata = JsonConvert.DeserializeObject<MapMetadata>(_serializedMapMetadata);

            return _mapMetadata;
        }

        private async Task<TileSetMetadata> RequestTileSetMetadata(string tileSetName)
        {
            string _retVal = await NetManager.GetResponseAsync(
                $"gameservice/tilesets?id={NetManager.ConnectionId}&remotePublicKeyBase64={NetManager.CryptoService.PublicKeyString.HtmlEncodeBase64()}&tileSetNameBase64={await NetManager.GetHtmlSafeEncryptedBase64(tileSetName)}");
            DiffieHellmanMessagePackage _messagePackage =
                JsonConvert.DeserializeObject<DiffieHellmanMessagePackage>(_retVal);

            string _serializedTileSetMetadata =
                await NetManager.CryptoService.DecryptAsync(_messagePackage.RemotePublicKey,
                    _messagePackage.SecretMessage);
            TileSetMetadata _tileSetMetadata = JsonConvert.DeserializeObject<TileSetMetadata>(_serializedTileSetMetadata);

            return _tileSetMetadata;
        }

        private async Task<List<Chunk>> RequestChunk(Vector2i coords)
        {
            string _retVal = await NetManager.GetResponseAsync(
                $"maps/{await NetManager.GetHtmlSafeEncryptedBase64(ActivateMap.Metadata.Name)}?id={NetManager.ConnectionId}&remotePublicKeyBase64={NetManager.CryptoService.PublicKeyString.HtmlEncodeBase64()}&coordsBase64={await NetManager.GetHtmlSafeEncryptedBase64(JsonConvert.SerializeObject(coords))}");
            DiffieHellmanMessagePackage _messagePackage =
                JsonConvert.DeserializeObject<DiffieHellmanMessagePackage>(_retVal);

            string _serializedString =
                await NetManager.CryptoService.DecryptAsync(_messagePackage.RemotePublicKey,
                    _messagePackage.SecretMessage);

            List<Chunk> _chunks = null;

            try
            {
                _chunks = JsonConvert.DeserializeObject<List<Chunk>>(_serializedString);
            }
            catch
            {
                Log.Error($"Failed to request chunk with coordinates {coords.X}:{coords.Y}");
            }

            return _chunks ?? new List<Chunk>();
        }

        private async Task<Player> RequestPlayer(string connectionId)
        {
            string _retVal = await NetManager.GetResponseAsync(
                $"gameservice/playerData?id={connectionId}&remotePublicKeyBase64={NetManager.CryptoService.PublicKeyString.HtmlEncodeBase64()}");
            DiffieHellmanMessagePackage _messagePackage =
                JsonConvert.DeserializeObject<DiffieHellmanMessagePackage>(_retVal);

            string _serializedString =
                await NetManager.CryptoService.DecryptAsync(_messagePackage.RemotePublicKey,
                    _messagePackage.SecretMessage);

            Player _player = null;

            try
            {
                _player = JsonConvert.DeserializeObject<Player>(_serializedString);
            }
            catch (Exception)
            {
                CallFatality("Failed to receive player data from server. Exiting game.");
            }

            return _player;
        }

        #endregion


        #region EVENT

        private void PlayerPositionChanged(object sender, EntityPositionChangedEventArgs args)
        {
            ServerUpdater.Positions.Enqueue(args.NewPosition);
        }

        private void PlayerRotationChanged(object sender, float rotation)
        {
            ServerUpdater.Rotations.Enqueue(rotation);
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