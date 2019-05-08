using JourneyCoreDisplay.Drawing;
using JourneyCoreDisplay.Sprites;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace JourneyCoreDisplay.Environment
{
    [XmlRoot("map")]
    public class Map
    {
        private Random _rand;

        // todo - make the file location dynamic
        public static Texture MapTextures { get; } = new Texture(@"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\JourneyCoreGame\Assets\Images\Sprites\JourneyCore-MapSprites.png");

        [XmlElement("layer")]
        public List<MapLayer> Layers { get; set; }

        [XmlAttribute("width")]
        public int Width { get; set; }

        [XmlAttribute("height")]
        public int Height { get; set; }

        [XmlAttribute("tilewidth")]
        public int PixelTileWidth { get; set; }

        [XmlAttribute("tileheight")]
        public int PixelTileHeight { get; set; }

        [XmlIgnore]
        public int ChunkWidth { get; set; }
        [XmlIgnore]
        public int ChunkHeight { get; set; }
        [XmlIgnore]
        public int PixelWidth { get; set; }
        [XmlIgnore]
        public int PixelHeight { get; set; }
        [XmlIgnore]
        public int ScaledTilePixelWidth { get; set; }
        [XmlIgnore]
        public int ScaledTilePixelHeight { get; set; }
        [XmlIgnore]
        public VertexArray VArray { get; internal set; }

        public Map()
        {
            ChunkWidth = 8;
            ChunkHeight = 8;

            _rand = new Random();
        }

        #region METHODS

        public int GetCoordinate(int layerId, int x, int y)
        {
            if (Layers[layerId].ChunkMap.Length <= x || Layers[layerId].ChunkMap.GetLength(1) <= y)
            {
                return -1;
            }

            // todo implement this
            return Layers[layerId].ChunkMap[x / ChunkWidth * ChunkWidth][y / ChunkHeight * ChunkHeight].ChunkData[x % ChunkWidth][y % ChunkHeight]; //_map[x, y];
        }

        #endregion



        #region CHUNK LOADING

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

            if (range.X > (Width / ChunkWidth) - 1)
            {
                range.X = (Width / ChunkWidth) - 1;
            }

            if (range.Y > (Height / ChunkHeight) - 1)
            {
                range.Y = (Height / ChunkHeight) - 1;
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


        public void LoadChunk(Vector2i chunkCoords)
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                MapLayer currentLayer = Layers[i];

                for (int x = 0; x < ChunkHeight; x++)
                {
                    if (currentLayer.ChunkMap[chunkCoords.X] == null)
                    {
                        continue;
                    }

                    for (int y = 0; y < ChunkWidth; y++)
                    {
                        if (currentLayer.ChunkMap[chunkCoords.X][chunkCoords.Y] == null) {
                            continue;
                        }

                        int tileSelection = currentLayer.ChunkMap[chunkCoords.X][chunkCoords.Y].ChunkData[x][y];
                        Tile currentTile = TileLoader.GetById(tileSelection);

                        if (currentTile == null)
                        {
                            continue;
                        }

                        List<Tile> drawList = AllocateTiles(currentTile);

                        for (int t = 0; t < drawList.Count; t++)
                        {
                            DrawTileToArray(drawList[t], chunkCoords, new Vector2i(x, y), currentLayer.Width);
                        }
                    }
                }
            }
        }

        public void LoadChunk(int x, int y)
        {
            LoadChunk(new Vector2i(x, y));
        }

        /// <summary>
        ///     Allocates a list of tiles that must be drawn in the current tilespace
        /// </summary>
        /// <param name="currentTile"></param>
        /// <returns></returns>
        private List<Tile> AllocateTiles(Tile currentTile)
        {
            List<Tile> allocatedTiles = new List<Tile>();

            if (currentTile.IsRandomizable)
            {
                currentTile = TileLoader.GetRandom(TileLoader.GetByGroup(currentTile.Group));
            }

            allocatedTiles.Add(currentTile);

            if (currentTile.IsAccentable)
            {
                int accentProbability = (int)(currentTile.AccentProbability * 100);

                if (!(_rand.Next(1,100) <= accentProbability)) {
                    return allocatedTiles;
                }

                List<Tile> accents = TileLoader.LoadedTiles.Where(tile => tile.AccentGroup.Equals(currentTile.Group)).ToList();

                allocatedTiles.AddRange(AllocateTiles(TileLoader.GetRandom(accents)));
            }

            return allocatedTiles;
        }

        private void DrawTileToArray(Tile tile, Vector2i chunkCoords, Vector2i currentChunk, int mapWidth)
        {
            // actual coordinate values
            // specific tiles in VArray
            int vArrayTileX = (chunkCoords.X * ChunkWidth) + currentChunk.X;
            int vArrayTileY = (chunkCoords.Y * ChunkHeight) + currentChunk.Y;

            int tileMapX = tile.Position.Left * PixelTileWidth;
            int tileMapY = tile.Position.Top * PixelTileHeight;

            Vector2f topLeft = MathOps.CalculateVertexPosition(VertexCorner.TopLeft, vArrayTileX, vArrayTileY, ScaledTilePixelWidth, ScaledTilePixelHeight);
            Vector2f topRight = MathOps.CalculateVertexPosition(VertexCorner.TopRight, vArrayTileX, vArrayTileY, ScaledTilePixelWidth, ScaledTilePixelHeight);
            Vector2f bottomRight = MathOps.CalculateVertexPosition(VertexCorner.BottomRight, vArrayTileX, vArrayTileY, ScaledTilePixelWidth, ScaledTilePixelHeight);
            Vector2f bottomLeft = MathOps.CalculateVertexPosition(VertexCorner.BottomLeft, vArrayTileX, vArrayTileY, ScaledTilePixelWidth, ScaledTilePixelHeight);

            uint index = (uint)((vArrayTileX + vArrayTileY * mapWidth) * 4);

            QuadCoords texCoords = RotateTile(tile, new Vector2i(tileMapX, tileMapY));

            // Top left
            VArray[index + 0] = new Vertex(topLeft, texCoords.TopLeft);
            // Top right
            VArray[index + 1] = new Vertex(topRight, texCoords.TopRight);
            // Bottom right
            VArray[index + 2] = new Vertex(bottomRight, texCoords.BottomRight);
            // Bottom left
            VArray[index + 3] = new Vertex(bottomLeft, texCoords.BottomLeft);

        }

        private QuadCoords RotateTile(Tile tile, Vector2i position, int rotation = 0)
        {
            QuadCoords qTexCoords = new QuadCoords();

            rotation = tile.IsRandomlyRotatable ? _rand.Next(0, 3) : rotation;

            switch (rotation)
            {
                case 0:
                    qTexCoords.TopLeft = new Vector2f(position.X, position.Y);
                    qTexCoords.TopRight = new Vector2f(position.X + tile.Position.Width, position.Y);
                    qTexCoords.BottomRight = new Vector2f(position.X + tile.Position.Width, position.Y + tile.Position.Height);
                    qTexCoords.BottomLeft = new Vector2f(position.X, position.Y + tile.Position.Height);
                    break;
                case 1:
                    qTexCoords.TopLeft = new Vector2f(position.X, position.Y + tile.Position.Height);
                    qTexCoords.TopRight = new Vector2f(position.X, position.Y);
                    qTexCoords.BottomRight = new Vector2f(position.X + tile.Position.Width, position.Y);
                    qTexCoords.BottomLeft = new Vector2f(position.X + tile.Position.Width, position.Y + tile.Position.Height);
                    break;
                case 2:
                    qTexCoords.TopLeft = new Vector2f(position.X + tile.Position.Width, position.Y + tile.Position.Height);
                    qTexCoords.TopRight = new Vector2f(position.X, position.Y + tile.Position.Height);
                    qTexCoords.BottomRight = new Vector2f(position.X, position.Y);
                    qTexCoords.BottomLeft = new Vector2f(position.X + tile.Position.Width, position.Y);
                    break;
                case 3:
                    qTexCoords.TopLeft = new Vector2f(position.X + tile.Position.Width, position.Y);
                    qTexCoords.TopRight = new Vector2f(position.X + tile.Position.Width, position.Y + tile.Position.Height);
                    qTexCoords.BottomRight = new Vector2f(position.X, position.Y + tile.Position.Height);
                    qTexCoords.BottomLeft = new Vector2f(position.X, position.Y);
                    break;
                default:
                    break;
            }

            return qTexCoords;
        }

        #endregion



        #region STATIC METHODS

        public static Map LoadMap(string mapName, Vector2i chunkSize, int scale)
        {
            XmlSerializer mapSerializer = new XmlSerializer(typeof(Map));

            using (StreamReader reader = new StreamReader($@"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\JourneyCoreGame\Assets\Maps\{mapName}.xml", Encoding.UTF8))
            {
                Map map = (Map)mapSerializer.Deserialize(reader);
                map.ScaledTilePixelWidth = map.PixelTileWidth * scale;
                map.ScaledTilePixelHeight = map.PixelTileHeight * scale;
                map.PixelWidth = map.Width * map.ScaledTilePixelWidth;
                map.PixelHeight = map.Height * map.ScaledTilePixelHeight;
                map.VArray = new VertexArray(PrimitiveType.Quads);
                map.VArray.Resize((uint)(((map.Width * map.Height * 4) + 1) * map.Layers.Count));

                foreach (MapLayer layer in map.Layers)
                {
                    BuildChunkMap(map, layer, chunkSize);
                }

                return map;
            }

        }

        private static void BuildChunkMap(Map map, MapLayer layer, Vector2i chunkSize)
        {
            string[] layerDataArray = layer.Data.Replace("\r\n", "\n").Replace("\n", ",").Split(',', StringSplitOptions.RemoveEmptyEntries);
            int layerChunkWidth = layer.Width / map.ChunkWidth;
            int layerChunkHeight = layer.Height / map.ChunkHeight;

            layer.ChunkMap = new Chunk[layerChunkWidth][];

            for (int chunkX = 0; chunkX < map.ChunkWidth; chunkX++)
            {
                layer.ChunkMap[chunkX] = new Chunk[layerChunkHeight];

                for (int chunkY = 0; chunkY < layerChunkHeight; chunkY++)
                {
                    Chunk currentChunk = new Chunk(new int[chunkSize.X][]);

                    for (int x = 0; x < chunkSize.X; x++)
                    {
                        currentChunk.ChunkData[x] = new int[chunkSize.Y];

                        for (int y = 0; y < chunkSize.Y; y++)
                        {
                            currentChunk.ChunkData[x][y] = int.Parse(layerDataArray[(layer.Width * (y + (chunkY * map.ChunkHeight)) + (x + (chunkX * map.ChunkWidth)))]);
                        }
                    }

                    layer.ChunkMap[chunkX][chunkY] = currentChunk;
                }
            }
        }

        #endregion
    }
}
