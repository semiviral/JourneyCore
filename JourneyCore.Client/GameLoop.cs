using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JourneyCore.Client.Display;
using JourneyCore.Client.Net;
using JourneyCore.Lib.Game.Context.Entities;
using JourneyCore.Lib.Game.InputWatchers;
using JourneyCore.Lib.Graphics.Drawing;
using JourneyCore.Lib.Graphics.Rendering.Environment.Tiling;
using JourneyCore.Lib.System;
using JourneyCore.Lib.System.Components.Loaders;
using Microsoft.AspNetCore.SignalR.Client;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace JourneyCore.Client
{
    public class GameLoop
    {
        private string currentTextureName;

        public GameLoop()
        {
            Rand = new Random();

            Textures = new Dictionary<string, byte[]>();
            KeyWatcher = new KeyWatcher();

            IsServerReady = false;

            CurrentTextureName = "JourneyCore-MapSprites";
            CurrentVArray = new VertexArray(PrimitiveType.Quads);
        }

        private Random Rand { get; }
        private HubConnection Connection { get; set; }
        private WindowManager WManager { get; set; }
        private Entity Player { get; set; }
        private KeyWatcher KeyWatcher { get; }
        private ButtonWatcher ButtonWatcher { get; set; }
        private Dictionary<string, byte[]> Textures { get; }
        private bool IsServerReady { get; set; }
        private ServerSynchroniser ServerStateSynchroniser { get; set; }
        private VertexArray CurrentVArray { get; }
        private RenderStates MapRenderState { get; set; }

        private string CurrentTextureName
        {
            get => currentTextureName;
            set
            {
                currentTextureName = value;

                if (!Textures.ContainsKey(CurrentTextureName)) return;

                MapRenderState = new RenderStates(new Texture(Textures[value]));
            }
        }

        public void Runtime()
        {
            //Shader transparency = new Shader(null, null, @"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\JourneyCoreGame\Assets\Shaders\transparency.frag");
            //transparency.SetUniform("opacity", 0.5f);
            //transparency.SetUniform("texture", Map.MapTextures);

            WManager.DrawItem(new DrawQueueItem(DrawPriority.Background,
                (fTime, window) =>
                {
                    if (!Textures.ContainsKey(CurrentTextureName)) return;

                    window.Draw(CurrentVArray, MapRenderState);
                }));

            while (WManager.IsActive) WManager.UpdateWindow();
        }


        #region INITIALISATION

        /// <summary>
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

            Connection.Closed += async error =>
            {
                await Task.Delay(1000);
                await Connection.StartAsync();
            };

            Connection.On<bool>("ReceiveServerStatus", ReceiveServerStatus);

            Connection.On<string, byte[]>("ReceiveTexture", ReceiveTexture);

            Connection.On<string, Tile[][][]>("ReceiveMap", ReceiveMap);

            await Connection.StartAsync();

            int tries = 0;

            while (!IsServerReady)
            {
                if (tries > 0) await Task.Delay(500);

                await Connection.InvokeAsync("RequestServerStatus");

                tries += 1;
            }

            await Connection.InvokeAsync("RequestTextureList");
            await Connection.InvokeAsync("RequestMap", "AdventurersGuild");
        }

        private void InitialiseWindowManager()
        {
            WManager = new WindowManager("Journey to the Core", new VideoMode(1000, 600, 8), 60, new Vector2f(2f, 2f),
                15f);
        }

        private void InitialisePlayer()
        {
            Player = new Entity(null, "player", "player", DateTime.MinValue,
                new Sprite(new Texture(Textures["JourneyCore-Human"])));
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

            KeyWatcher.AddWatchedKeyAction(Keyboard.Key.W, key =>
            {
                movement = new Vector2f(GraphMath.SinFromDegrees(Player.Graphic.Rotation),
                    GraphMath.CosFromDegrees(Player.Graphic.Rotation) * -1f);

                if (Keyboard.IsKeyPressed(Keyboard.Key.S)) return Task.CompletedTask;

                if (Keyboard.IsKeyPressed(Keyboard.Key.A) || Keyboard.IsKeyPressed(Keyboard.Key.D)) movement *= 0.5f;

                Player.Move(movement, MapLoader.PixelTileWidth * MapLoader.Scale, WManager.ElapsedTime);

                return Task.CompletedTask;
            });

            KeyWatcher.AddWatchedKeyAction(Keyboard.Key.A, key =>
            {
                movement = new Vector2f(GraphMath.CosFromDegrees(Player.Graphic.Rotation) * -1f,
                    GraphMath.SinFromDegrees(Player.Graphic.Rotation) * -1f);

                if (Keyboard.IsKeyPressed(Keyboard.Key.D)) return Task.CompletedTask;

                if (Keyboard.IsKeyPressed(Keyboard.Key.W) || Keyboard.IsKeyPressed(Keyboard.Key.S)) movement *= 0.5f;

                Player.Move(movement, MapLoader.PixelTileWidth * MapLoader.Scale, WManager.ElapsedTime);

                return Task.CompletedTask;
            });

            KeyWatcher.AddWatchedKeyAction(Keyboard.Key.S, key =>
            {
                movement = new Vector2f(GraphMath.SinFromDegrees(Player.Graphic.Rotation) * -1f,
                    GraphMath.CosFromDegrees(Player.Graphic.Rotation));

                if (Keyboard.IsKeyPressed(Keyboard.Key.W)) return Task.CompletedTask;

                if (Keyboard.IsKeyPressed(Keyboard.Key.A) || Keyboard.IsKeyPressed(Keyboard.Key.D)) movement *= 0.5f;

                Player.Move(movement, MapLoader.PixelTileWidth * MapLoader.Scale, WManager.ElapsedTime);

                return Task.CompletedTask;
            });

            KeyWatcher.AddWatchedKeyAction(Keyboard.Key.D, key =>
            {
                movement = new Vector2f(GraphMath.CosFromDegrees(Player.Graphic.Rotation),
                    GraphMath.SinFromDegrees(Player.Graphic.Rotation));

                if (Keyboard.IsKeyPressed(Keyboard.Key.A)) return Task.CompletedTask;

                if (Keyboard.IsKeyPressed(Keyboard.Key.W) || Keyboard.IsKeyPressed(Keyboard.Key.S)) movement *= 0.5f;

                Player.Move(movement, MapLoader.PixelTileWidth * MapLoader.Scale, WManager.ElapsedTime);

                return Task.CompletedTask;
            });

            KeyWatcher.AddWatchedKeyAction(Keyboard.Key.Q,
                async key => { await Player.RotateEntity(WManager.ElapsedTime, 180f, false); });

            KeyWatcher.AddWatchedKeyAction(Keyboard.Key.E,
                async key => { await Player.RotateEntity(WManager.ElapsedTime, 180f, true); });
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


        #region EVENT

        private Task PlayerPositionChanged(object sender, Vector2f position)
        {
            WManager.MoveView(position);

            ServerStateSynchroniser.AllocateStateUpdate(StateUpdateType.Position,
                new Vector2i((int) position.X, (int) position.Y));

            return Task.CompletedTask;
        }

        private Task PlayerRotationChanged(object sender, float rotation)
        {
            WManager.RotateView(rotation);

            ServerStateSynchroniser.AllocateStateUpdate(StateUpdateType.Rotation, (int) rotation);

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

        private void ReceiveMap(string textureName, Tile[][][] map)
        {
            CurrentTextureName = Path.GetFileNameWithoutExtension(textureName);
            BuildGraphicMap(map);
        }

        #endregion


        #region MAP BUILDING

        public void BuildGraphicMap(Tile[][][] layerMap)
        {
            CurrentVArray.Clear();
            CurrentVArray.Resize(
                (uint) (layerMap.Length * layerMap[0].Length * 4 + 1));

            for (int layer = 0; layer < layerMap.Length; layer++)
                for (int x = 0; x < layerMap[0].Length; x++)
                for (int y = 0; y < layerMap[0][0].Length; y++)
                    LoadChunk(layerMap[layer], layer);
        }

        private void LoadChunk(Tile[][] map, int layerId)
        {
            for (int x = 0; x < map.Length; x++)
            for (int y = 0; y < map[0].Length; y++)
            {
                AllocateTileToVArray(map[x][y], new Vector2i(x, y), map.Length, layerId);
            }
        }

        private void AllocateTileToVArray(Tile tile, Vector2i tileCoords, int mapWidth, int layerId) {
            Vector2f topLeft = GraphMath.CalculateVertexPosition(VertexCorner.TopLeft, tileCoords.X, tileCoords.Y,
                tile.SizeX * MapLoader.Scale, tile.SizeY * MapLoader.Scale);
            Vector2f topRight = GraphMath.CalculateVertexPosition(VertexCorner.TopRight, tileCoords.X, tileCoords.Y,
                tile.SizeX * MapLoader.Scale, tile.SizeY * MapLoader.Scale);
            Vector2f bottomRight = GraphMath.CalculateVertexPosition(VertexCorner.BottomRight, tileCoords.X, tileCoords.Y,
                tile.SizeX * MapLoader.Scale, tile.SizeY * MapLoader.Scale);
            Vector2f bottomLeft = GraphMath.CalculateVertexPosition(VertexCorner.BottomLeft, tileCoords.X, tileCoords.Y,
                tile.SizeX * MapLoader.Scale, tile.SizeY * MapLoader.Scale);

            if (layerId == 2) { 
}

            uint index = (uint) ((tileCoords.X + tileCoords.Y * mapWidth) * 4 * layerId);

            CurrentVArray[index + 0] = new Vertex(topLeft, tile.TextureCoords.TopLeft);
            CurrentVArray[index + 1] = new Vertex(topRight, tile.TextureCoords.TopRight);
            CurrentVArray[index + 2] = new Vertex(bottomRight, tile.TextureCoords.BottomRight);
            CurrentVArray[index + 3] = new Vertex(bottomLeft, tile.TextureCoords.BottomLeft);
        }

        #endregion
    }
}