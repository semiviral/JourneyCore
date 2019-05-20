using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JourneyCore.Lib.Game.Context.Entities;
using JourneyCore.Lib.Game.InputWatchers;
using JourneyCore.Lib.Graphics;
using JourneyCore.Lib.Graphics.Drawing;
using JourneyCore.Lib.Graphics.Rendering.Environment.Chunking;
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

        private Random Rand { get; }
        private HubConnection Connection { get; set; }
        private WindowManager WManager { get; set; }
        private Entity Player { get; set; }
        private KeyWatcher KeyWatcher { get; }
        private ButtonWatcher ButtonWatcher { get; set; }
        private Dictionary<string, byte[]> Textures { get; }
        private bool IsServerReady { get; set; }
        private ServerSynchroniser ServerStateSynchroniser { get; set; }
        private Vector2i CurrentChunkLoadCenter { get; set; }
        private VertexArray CurrentVArray { get; set; }
        private RenderStates MapRenderState { get; set; }
        private string CurrentTextureName {
            get => currentTextureName;
            set {
                currentTextureName = value;

                if (!Textures.ContainsKey(CurrentTextureName)) return;

                MapRenderState = new RenderStates(new Texture(Textures[value]));
            }
        }

        public GameLoop()
        {
            Rand = new Random();

            Textures = new Dictionary<string, byte[]>();
            KeyWatcher = new KeyWatcher();

            IsServerReady = false;

            CurrentTextureName = "JourneyCore-MapSprites";
            CurrentVArray = new VertexArray(PrimitiveType.Quads);
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

            Connection.On<string, Chunk[][][], Tile[]>("ReceiveChunks", ReceiveChunks);

            await Connection.StartAsync();

            int tries = 0;

            while (!IsServerReady)
            {
                if (tries > 0) await Task.Delay(500);

                await Connection.InvokeAsync("RequestServerStatus");

                tries += 1;
            }

            await Connection.InvokeAsync("RequestTextureList");
            await Connection.InvokeAsync("RequestChunks", "AdventurersGuild", new Vector2i(0, 0));
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
            Player.ChunkChanged += async (sender, newChunk) =>
            {
                // TODO fix current map name
                await Connection.InvokeAsync("RequestChunks", "AdventurersGuild", newChunk);
            };

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

                Player.Move(movement, 16, WManager.ElapsedTime);

                return Task.CompletedTask;
            });

            KeyWatcher.AddWatchedKeyAction(Keyboard.Key.A, key =>
            {
                movement = new Vector2f(GraphMath.CosFromDegrees(Player.Graphic.Rotation) * -1f,
                    GraphMath.SinFromDegrees(Player.Graphic.Rotation) * -1f);

                if (Keyboard.IsKeyPressed(Keyboard.Key.D)) return Task.CompletedTask;

                if (Keyboard.IsKeyPressed(Keyboard.Key.W) || Keyboard.IsKeyPressed(Keyboard.Key.S)) movement *= 0.5f;

                Player.Move(movement, 16, WManager.ElapsedTime);

                return Task.CompletedTask;
            });

            KeyWatcher.AddWatchedKeyAction(Keyboard.Key.S, key =>
            {
                movement = new Vector2f(GraphMath.SinFromDegrees(Player.Graphic.Rotation) * -1f,
                    GraphMath.CosFromDegrees(Player.Graphic.Rotation));

                if (Keyboard.IsKeyPressed(Keyboard.Key.W)) return Task.CompletedTask;

                if (Keyboard.IsKeyPressed(Keyboard.Key.A) || Keyboard.IsKeyPressed(Keyboard.Key.D)) movement *= 0.5f;

                Player.Move(movement, 16, WManager.ElapsedTime);

                return Task.CompletedTask;
            });

            KeyWatcher.AddWatchedKeyAction(Keyboard.Key.D, key =>
            {
                movement = new Vector2f(GraphMath.CosFromDegrees(Player.Graphic.Rotation),
                    GraphMath.SinFromDegrees(Player.Graphic.Rotation));

                if (Keyboard.IsKeyPressed(Keyboard.Key.A)) return Task.CompletedTask;

                if (Keyboard.IsKeyPressed(Keyboard.Key.W) || Keyboard.IsKeyPressed(Keyboard.Key.S)) movement *= 0.5f;

                Player.Move(movement, 16, WManager.ElapsedTime);

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

        private void ReceiveServerStatus(bool serverReady)
        {
            IsServerReady = serverReady;
        }

        private void ReceiveTexture(string key, byte[] texture)
        {
            Textures.Add(key, texture);
        }

        private void ReceiveChunks(string textureName, Chunk[][][] chunks, Tile[] usedTiles)
        {
            CurrentTextureName = textureName;

            BuildGraphicMap(chunks, usedTiles);
        }

        #endregion



        #region MAP BUILDING

        public void BuildGraphicMap(Chunk[][][] chunks, Tile[] usedTiles)
        {
            CurrentVArray.Clear();
            CurrentVArray.Resize((uint)(chunks.GetLength(0) * chunks.GetLength(1) * TileMapLoader.ChunkSize * chunks.GetLength(2) * TileMapLoader.ChunkSize * 4 + 1));

            foreach (Chunk[][] chunkMap in chunks)
                for (int x = 0; x < chunks.Length; x++)
                    for (int y = 0; y < chunks[0].Length; y++)
                        LoadChunk(chunkMap[x][y], Player.CurrentChunk, usedTiles, chunkMap.GetLength(0));
        }

        private void LoadChunk(Chunk chunk, Vector2i chunkCoords, Tile[] usedTiles, int mapWidth)
        {
            for (int x = 0; x < TileMapLoader.ChunkSize; x++)
                for (int y = 0; y < TileMapLoader.ChunkSize; y++)
                {
                    int tileId = chunk.ChunkData[x][y] - 1;

                    // in this case, the selected tile
                    // is void
                    if (tileId == 0)
                    {
                        continue;
                    }

                    Tile currentTile = usedTiles.SingleOrDefault(tile => tile.Id == tileId);
                    Tile parsedTile = ParseTile(usedTiles, currentTile);

                    AllocateTileToVArray(parsedTile, chunkCoords, new Vector2i(x, y), mapWidth);
                }
        }

        private void AllocateTileToVArray(Tile tile, Vector2i chunkCoords, Vector2i innerChunkCoords,
            int mapWidth)
        {
            // todo set up scale for (* 2)
            int tilePixelSize = ((tile.SizeX + tile.SizeY) / 2) * TileMapLoader.Scale;

            // actual coordinate values
            // specific tiles in VArray
            int vArrayTileX = chunkCoords.X * TileMapLoader.ChunkSize + innerChunkCoords.X;
            int vArrayTileY = chunkCoords.Y * TileMapLoader.ChunkSize + innerChunkCoords.Y;

            Vector2f topLeft = GraphMath.CalculateVertexPosition(VertexCorner.TopLeft, vArrayTileX, vArrayTileY,
                tilePixelSize, tilePixelSize);
            Vector2f topRight = GraphMath.CalculateVertexPosition(VertexCorner.TopRight, vArrayTileX, vArrayTileY,
                tilePixelSize, tilePixelSize);
            Vector2f bottomRight = GraphMath.CalculateVertexPosition(VertexCorner.BottomRight, vArrayTileX, vArrayTileY,
                tilePixelSize, tilePixelSize);
            Vector2f bottomLeft = GraphMath.CalculateVertexPosition(VertexCorner.BottomLeft, vArrayTileX, vArrayTileY,
                tilePixelSize, tilePixelSize);

            uint index = (uint)((vArrayTileX + vArrayTileY * mapWidth) * 4 * tile.LayerId);

            CurrentVArray[index + 0] = new Vertex(topLeft, tile.TextureCoords.TopLeft);
            CurrentVArray[index + 1] = new Vertex(topRight, tile.TextureCoords.TopRight);
            CurrentVArray[index + 2] = new Vertex(bottomRight, tile.TextureCoords.BottomRight);
            CurrentVArray[index + 3] = new Vertex(bottomLeft, tile.TextureCoords.BottomLeft);
        }

        /// <summary>
        ///     Allocates a list of tiles that must be drawn in the current tilespace
        /// </summary>
        /// <param name="tile"></param>
        /// <returns></returns>
        private Tile ParseTile(IEnumerable<Tile> usedTiles, Tile tile)
        {
            RandomizeTile(usedTiles, tile);
            RotateTile(tile);

            return tile;
        }

        private Tile RotateTile(Tile tile)
        {
            int randNum = Rand.Next(0, 3);

            if (!tile.IsRandomlyRotatable) randNum = 0;

            // width and height of all textures in a map will be the same
            int actualPixelLeft = tile.TextureRect.Left * tile.TextureRect.Width;
            int actualPixelTop = tile.TextureRect.Top * tile.TextureRect.Height;

            QuadCoords finalCoords = new QuadCoords();

            switch (randNum)
            {
                case 0:
                    finalCoords.TopLeft = new Vector2f(actualPixelLeft, actualPixelTop);
                    finalCoords.TopRight = new Vector2f(actualPixelLeft + tile.TextureRect.Width, actualPixelTop);
                    finalCoords.BottomRight = new Vector2f(actualPixelLeft + tile.TextureRect.Width,
                        actualPixelTop + tile.TextureRect.Height);
                    finalCoords.BottomLeft = new Vector2f(actualPixelLeft, actualPixelTop + tile.TextureRect.Height);
                    break;
                case 1:
                    finalCoords.TopLeft = new Vector2f(actualPixelLeft + tile.TextureRect.Width, actualPixelTop);
                    finalCoords.TopRight = new Vector2f(actualPixelLeft + tile.TextureRect.Width,
                        actualPixelTop + tile.TextureRect.Height);
                    finalCoords.BottomRight = new Vector2f(actualPixelLeft, actualPixelTop + tile.TextureRect.Height);
                    finalCoords.BottomLeft = new Vector2f(actualPixelLeft, actualPixelTop);
                    break;
                case 2:
                    finalCoords.TopLeft = new Vector2f(actualPixelLeft + tile.TextureRect.Width,
                        actualPixelTop + tile.TextureRect.Height);
                    finalCoords.TopRight = new Vector2f(actualPixelLeft, actualPixelTop + tile.TextureRect.Height);
                    finalCoords.BottomRight = new Vector2f(actualPixelLeft, actualPixelTop);
                    finalCoords.BottomLeft = new Vector2f(actualPixelLeft + tile.TextureRect.Width, actualPixelTop);
                    break;
                case 3:
                    finalCoords.TopLeft = new Vector2f(actualPixelLeft, actualPixelTop + tile.TextureRect.Height);
                    finalCoords.TopRight = new Vector2f(actualPixelLeft, actualPixelTop);
                    finalCoords.BottomRight = new Vector2f(actualPixelLeft + tile.TextureRect.Width, actualPixelTop);
                    finalCoords.BottomLeft = new Vector2f(actualPixelLeft + tile.TextureRect.Width,
                        actualPixelTop + tile.TextureRect.Height);
                    break;
            }

            tile.TextureCoords = finalCoords;

            return tile;
        }

        private Tile RandomizeTile(IEnumerable<Tile> usedTiles, Tile subjectTile)
        {
            return !subjectTile.IsRandomizable ? subjectTile : GetRandom(usedTiles.Where(tile => tile.Group.Equals(subjectTile.Group)).ToArray());
        }


        private Tile GetRandom(IReadOnlyList<Tile> candidates)
        {
            // optimizations to avoid useless iterating

            if (candidates.Count < 1) return default;

            // if only one sprite in list
            if (candidates.Count == 1) return candidates[0];

            // in case all have equal weights
            if (candidates.Select(tile => tile.Probability).All(weight => Math.Abs(weight - candidates[0].Probability) < 0.01))
                return candidates[Rand.Next(0, candidates.Count)];

            // end optimizations

            int totalWeight = candidates.Select(sprite => (int)(sprite.Probability * 100)).Sum();

            Tile[] weightArray = new Tile[totalWeight];

            int iterations = 0;
            foreach (Tile tilePackage in candidates)
                for (int j = 0; j < tilePackage.Probability * 100; j++)
                {
                    weightArray[iterations] = tilePackage;
                    iterations += 1;
                }

            int randSelection = Rand.Next(0, weightArray.Length);
            return weightArray[randSelection];
        }

        #endregion
    }
}