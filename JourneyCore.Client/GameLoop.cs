using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using JourneyCore.Lib.Graphics.Drawing;
using JourneyCore.Lib.Graphics.Rendering.Environment.Tiling;
using JourneyCore.Lib.System;
using JourneyCore.Lib.System.Components.Loaders;
using JourneyCoreLib.Game.Context.Entities;
using JourneyCoreLib.Game.InputWatchers;
using Microsoft.AspNetCore.SignalR.Client;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace JourneyCore.Client
{
    public class GameLoop
    {
        private HubConnection Connection { get; set; }
        private WindowManager WManager { get; set; }
        private TileMap CurrentMap { get; set; }
        private Entity Player { get; set; }
        private KeyWatcher KeyWatcher { get; set; }
        private ButtonWatcher ButtonWatcher { get; set; }
        private Dictionary<string, byte[]> Textures { get; set; }
        private bool IsServerReady { get; set; }
        private ServerSynchroniser ServerStateSynchroniser { get; set; }

        public GameLoop()
        {
            Textures = new Dictionary<string, byte[]>();
            KeyWatcher = new KeyWatcher();

            IsServerReady = false;
        }



        #region INITIALISATION

        /// <summary>
        ///     
        /// </summary>
        /// <param name="minimumServerUpdateFrameTime">Minimum amount of time between synchronizing game state in milliseconds</param>
        public async Task Initialise(string serverUrl, int minimumServerUpdateFrameTime)
        {
            await InitialiseConnection(serverUrl);

            ServerStateSynchroniser = new ServerSynchroniser(Connection, minimumServerUpdateFrameTime);

            InitialiseWindowManager();
            InitialiseKeyWatcher();
            InitialiseButtonWatcher();
            InitialisePlayer();
            InitialiseView();

            Runtime();
        }

        private async Task InitialiseConnection(string serverUrl)
        {
            Connection = new HubConnectionBuilder().WithUrl(serverUrl).Build();

            Connection.Closed += async (error) =>
            {
                await Task.Delay(1000);
                await Connection.StartAsync();
            };

            Connection.On<bool>("ReceiveServerStatus", (serverStatus) =>
            {
                ReceiveServerStatus(serverStatus);
            });

            Connection.On<string, byte[]>("ReceiveTexture", (key, texture) =>
            {
                ReceiveTexture(key, texture);
            });

            Connection.On<TileMap>("ReceiveTileMap", (tileMap) =>
            {
                ReceieveTileMap(tileMap);
            });

            await Connection.StartAsync();

            int tries = 0;

            while (!IsServerReady)
            {
                if (tries > 0)
                {
                    await Task.Delay(500);
                }

                await Connection.InvokeAsync("RequestServerStatus");

                tries += 1;
            }

            await Connection.InvokeAsync("RequestTextureList");
            await Connection.InvokeAsync("RequestTileMap", "AdventurersGuild");
        }

        private void InitialiseWindowManager()
        {
            WManager = new WindowManager("Journey to the Core", new VideoMode(1000, 600, 8), 60, new Vector2f(2f, 2f), 15f);
        }

        private void InitialisePlayer()
        {
            Player = new Entity(null, "player", "player", DateTime.MinValue, new Sprite(new Texture(Textures["JourneyCore-Human"])));
            Player.PositionChanged += PlayerPositionChanged;
            Player.RotationChanged += PlayerRotationChanged;

            WManager.DrawItem(new DrawQueueItem(DrawPriority.Foreground, (fTime, window) =>
            {
                KeyWatcher.CheckWatchedKeys();
                ButtonWatcher.CheckWatchedButtons();
                window.Draw(Player.Graphic);
            }));
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
            Vector2f movement = new Vector2f(0, 0);

            KeyWatcher.AddWatchedKeyAction(Keyboard.Key.W, (key) =>
            {
                movement = new Vector2f(GraphMath.SinFromDegrees(Player.Graphic.Rotation), GraphMath.CosFromDegrees(Player.Graphic.Rotation) * -1f);

                if (Keyboard.IsKeyPressed(Keyboard.Key.S))
                {
                    return Task.CompletedTask;
                }

                if (Keyboard.IsKeyPressed(Keyboard.Key.A) || Keyboard.IsKeyPressed(Keyboard.Key.D))
                {
                    movement *= 0.5f;
                }

                Player.Move(movement, CurrentMap.PixelTileHeight, WManager.ElapsedTime);

                return Task.CompletedTask;
            });

            KeyWatcher.AddWatchedKeyAction(Keyboard.Key.A, (key) =>
            {
                movement = new Vector2f(GraphMath.CosFromDegrees(Player.Graphic.Rotation) * -1f, GraphMath.SinFromDegrees(Player.Graphic.Rotation) * -1f);

                if (Keyboard.IsKeyPressed(Keyboard.Key.D))
                {
                    return Task.CompletedTask;
                }

                if (Keyboard.IsKeyPressed(Keyboard.Key.W) || Keyboard.IsKeyPressed(Keyboard.Key.S))
                {
                    movement *= 0.5f;
                }

                Player.Move(movement, CurrentMap.PixelTileHeight, WManager.ElapsedTime);

                return Task.CompletedTask;
            });

            KeyWatcher.AddWatchedKeyAction(Keyboard.Key.S, (key) =>
            {
                movement = new Vector2f(GraphMath.SinFromDegrees(Player.Graphic.Rotation) * -1f, GraphMath.CosFromDegrees(Player.Graphic.Rotation));

                if (Keyboard.IsKeyPressed(Keyboard.Key.W))
                {
                    return Task.CompletedTask;
                }

                if (Keyboard.IsKeyPressed(Keyboard.Key.A) || Keyboard.IsKeyPressed(Keyboard.Key.D))
                {
                    movement *= 0.5f;
                }

                Player.Move(movement, CurrentMap.PixelTileHeight, WManager.ElapsedTime);

                return Task.CompletedTask;
            });

            KeyWatcher.AddWatchedKeyAction(Keyboard.Key.D, (key) =>
            {
                movement = new Vector2f(GraphMath.CosFromDegrees(Player.Graphic.Rotation), GraphMath.SinFromDegrees(Player.Graphic.Rotation));

                if (Keyboard.IsKeyPressed(Keyboard.Key.A))
                {
                    return Task.CompletedTask;
                }

                if (Keyboard.IsKeyPressed(Keyboard.Key.W) || Keyboard.IsKeyPressed(Keyboard.Key.S))
                {
                    movement *= 0.5f;
                }

                Player.Move(movement, CurrentMap.PixelTileHeight, WManager.ElapsedTime);

                return Task.CompletedTask;
            });

            KeyWatcher.AddWatchedKeyAction(Keyboard.Key.Q, async (key) =>
            {
                Player.RotateEntity(WManager.ElapsedTime, 180f, false);
            });

            KeyWatcher.AddWatchedKeyAction(Keyboard.Key.E, async (key) =>
            {
                Player.RotateEntity(WManager.ElapsedTime, 180f, true);
            });
        }

        private void InitialiseButtonWatcher()
        {
            ButtonWatcher = new ButtonWatcher();

            //_buttonWatcher.AddWatchedButtonAction(Mouse.Button.Left, (button) =>
            //{
            //    if (_wManager.IsInMenu)
            //    {
            //        return;
            //    }


            //    Vector2i mousePosition = _wManager.GetRelativeMousePosition();

            //    float xAxis = (_wManager.Size.X * _player.EntityView.Width) / 2f;
            //    float yAxis = (_wManager.Size.Y * _player.EntityView.Height) / 2f;

            //    double distance = GraphMath.DistanceBetweenPoints(xAxis, yAxis, mousePosition.X, mousePosition.Y);


            //    double zeroPointX = xAxis;
            //    double zeroPointY = yAxis + distance;

            //    //Entity projectile = _player.GetProjectile();

            //    // projectile cooldown
            //    if (projectile == null)
            //    {
            //        return;
            //    }

            //    _wManager.DrawItem(new DrawQueueItem(DrawPriority.Foreground, (fTime, window) =>
            //    {
            //        Vector2f movement = new Vector2f(GraphMath.SinFromDegrees(projectile.Graphic.Rotation), GraphMath.CosFromDegrees(projectile.Graphic.Rotation) * -1f);

            //        projectile.Move(movement);

            //        window.Draw(projectile.Graphic);
            //    }, projectile.Lifetime));
            //});
        }

        #endregion

        public void Runtime()
        {
            CurrentMap.LoadChunkRange(0, 0, 16, 16);

            //Shader transparency = new Shader(null, null, @"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\JourneyCoreGame\Assets\Shaders\transparency.frag");
            //transparency.SetUniform("opacity", 0.5f);
            //transparency.SetUniform("texture", Map.MapTextures);

            RenderStates overlayStates = new RenderStates(new Texture(Textures[Path.GetFileNameWithoutExtension(CurrentMap.TileSetSource.Source)]));

            WManager.DrawItem(new DrawQueueItem(DrawPriority.Background, (fTime, window) =>
            {
                window.Draw(CurrentMap.VArray, overlayStates);
            }));

            Font courierNew = new Font(@"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\Assets\Fonts\Courier New.ttf");

            while (WManager.IsActive)
            {
                WManager.UpdateWindow();
            }
        }


        #region EVENT

        private Task PlayerPositionChanged(object sender, Vector2f position)
        {
            WManager.MoveView(position);

            ServerStateSynchroniser.AllocateStateUpdate(StateUpdateType.Position, new Vector2i((int)position.X, (int)position.Y));

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

        private void ReceiveServerStatus(bool serverReady)
        {
            IsServerReady = serverReady;
        }

        private void ReceiveTexture(string key, byte[] texture)
        {
            Textures.Add(key, texture);
        }

        private void ReceieveTileMap(TileMap tileMap)
        {
            TileMapLoader.BuildChunkMap(tileMap, new Vector2i(8, 8));

            CurrentMap = tileMap;
        }

        #endregion


    }
}
