using JourneyCoreDisplay.Drawing;
using JourneyCoreDisplay.Sprites;
using SFML.Graphics;
using SFML.System;
using System;
using System.IO;
using System.Linq;

namespace JourneyCoreDisplay.Environment
{
    public class Map
    {
        // todo - make the file location dynamic
        public static Texture MapTextures { get; } = new Texture(@"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\JourneyCoreGame\Assets\Images\Sprites\JourneyCore-MapSprites.png");

        private Chunk[][] _map;

        /// <summary>
        ///     Size of chunk in tiles
        /// </summary>
        public Vector2i SizeInTiles { get; }

        /// <summary>
        ///     Map size in pixels
        /// </summary>
        public Vector2i SizeInPixels { get; }

        /// <summary>
        ///     Size of each tile in pixels
        /// </summary>
        public Vector2i TileSize { get; }

        /// <summary>
        ///     Tiled size of each chunk
        /// </summary>
        public Vector2i ChunkSize { get; }



        public VertexArray VArray { get; private set; }

        public Map(Vector2i size, Vector2i tileSize, Vector2i chunkSize, int[][] map, int scale)
        {
            SizeInTiles = size;
            TileSize = tileSize * scale;
            SizeInPixels = new Vector2i(SizeInTiles.X * TileSize.X, SizeInTiles.Y * TileSize.Y) * scale;
            ChunkSize = chunkSize;


            VArray = new VertexArray(PrimitiveType.Quads);
            VArray.Resize((uint)((SizeInTiles.X * SizeInTiles.Y * 4) + 1));

            _map = ParseMapToChunks(map, chunkSize);
        }

        public void LoadChunkRange(Vector2i start, Vector2i range)
        {
            if (start.X < 0)
            {
                start.X = 0;
            }

            if (start.Y < 0)
            {
                start.Y = 0;
            }

            if (range.X > _map.Length)
            {
                range.X = _map.Length;
            }

            if (range.Y > _map[0].Length)
            {
                range.Y = _map[0].Length;
            }

            for (int x = start.X; x < range.X; x++)
            {
                for (int y = start.Y; y < range.Y; y++)
                {
                    LoadChunk(x, y);
                }
            }
        }

        public void LoadChunkRange(int startX, int startY, int maxX, int maxY)
        {
            LoadChunkRange(new Vector2i(startX, startY), new Vector2i(maxX, maxY));
        }


        public void LoadChunk(Vector2i coordinates)
        {
            for (int x = 0; x < ChunkSize.X; x++)
            {
                for (int y = 0; y < ChunkSize.Y; y++)
                {
                    int tileSelection = _map[coordinates.X][coordinates.Y].ChunkData[x][y];
                    IntRect currentTile = SpriteLoader.LoadedSprites.First(sprite => (int)sprite.Type == tileSelection).GetRandom();

                    if (currentTile.Left > 32)
                    {
                        //break here
                    }

                    // actual coordinate values
                    // specific tiles in VArray
                    int realTileX = (coordinates.X * ChunkSize.X) + x;
                    int realTileY = (coordinates.Y * ChunkSize.Y) + y;

                    Vector2f topLeft = MathOps.CalculateVertexPosition(VertexCorner.TopLeft, realTileX, realTileY, TileSize.X, TileSize.Y);
                    Vector2f topRight = MathOps.CalculateVertexPosition(VertexCorner.TopRight, realTileX, realTileY, TileSize.X, TileSize.Y);
                    Vector2f bottomRight = MathOps.CalculateVertexPosition(VertexCorner.BottomRight, realTileX, realTileY, TileSize.X, TileSize.Y);
                    Vector2f bottomLeft = MathOps.CalculateVertexPosition(VertexCorner.BottomLeft, realTileX, realTileY, TileSize.X, TileSize.Y);

                    uint index = (uint)((realTileX + realTileY * SizeInTiles.X) * 4);

                    // Top left
                    VArray[index + 0] = new Vertex(topLeft, new Vector2f(currentTile.Left, currentTile.Top));
                    // Top right
                    VArray[index + 1] = new Vertex(topRight, new Vector2f(currentTile.Left + currentTile.Width, currentTile.Top));
                    // Bottom right
                    VArray[index + 2] = new Vertex(bottomRight, new Vector2f(currentTile.Left + currentTile.Width, currentTile.Top + currentTile.Height));
                    // Bottom left
                    VArray[index + 3] = new Vertex(bottomLeft, new Vector2f(currentTile.Left, currentTile.Top + currentTile.Height));
                }
            }
        }

        public void LoadChunk(int x, int y)
        {
            LoadChunk(new Vector2i(x, y));
        }

        public int GetCoordinate(int x, int y)
        {
            if (_map.Length <= x || _map.GetLength(1) <= y)
            {
                return -1;
            }

            return 0; //_map[x, y];
        }

        #region STATIC METHODS

        public static Chunk[][] ParseMapToChunks(int[][] intMap, Vector2i chunkSize)
        {
            Vector2i sizeInTiles = new Vector2i(intMap.Length, intMap[0].Length);
            Vector2i mapChunkSize = new Vector2i(sizeInTiles.X / chunkSize.X, sizeInTiles.Y / chunkSize.Y);

            Chunk[][] chunkData = new Chunk[sizeInTiles.X / chunkSize.X][];

            // iterates by chunks, then interates each chunk internally
            for (int chunkX = 0; chunkX < mapChunkSize.X; chunkX++)
            {
                chunkData[chunkX] = new Chunk[mapChunkSize.Y];

                for (int chunkY = 0; chunkY < mapChunkSize.Y; chunkY++)
                {
                    Chunk currentChunk = new Chunk(new int[chunkSize.X][]);

                    for (int x = 0; x < chunkSize.X; x++)
                    {
                        currentChunk.ChunkData[x] = new int[chunkSize.Y];

                        for (int y = 0; y < chunkSize.Y; y++)
                        {
                            currentChunk.ChunkData[x][y] = intMap[(chunkX * chunkSize.X) + x][(chunkY * chunkSize.Y) + y];
                        }
                    }

                    chunkData[chunkX][chunkY] = currentChunk;
                }
            }

            return chunkData;
        }

        public static Map LoadMap(string mapName, Vector2i tileSize, Vector2i chunkSize, int scale)
        {
            string mapString = string.Empty;

            using (StreamReader reader = new StreamReader($@"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\JourneyCoreGame\Assets\Maps\{mapName}.map"))
            {
                mapString = reader.ReadToEnd();
            }

            string[] stringArray = mapString.Replace("\r\n", "\n").Split(':', StringSplitOptions.RemoveEmptyEntries);
            int[] mapArray = stringArray.Where(str => char.IsDigit(str[0])).Select(int.Parse).ToArray();

            int width = Array.IndexOf(stringArray, "\n");
            int height = mapArray.Length / width;
            int[][] intMap = new int[width][];

            for (int x = 0; x < width; x++)
            {
                intMap[x] = new int[height];

                for (int y = 0; y < height; y++)
                {
                    if (y == 0)
                    {
                        intMap[x][y] = mapArray[x + y];

                        continue;
                    }

                    intMap[x][y] = mapArray[(width * y) + x];
                }
            }

            return new Map(new Vector2i(width, height), tileSize, chunkSize, intMap, scale);
        }

        #endregion
    }
}
