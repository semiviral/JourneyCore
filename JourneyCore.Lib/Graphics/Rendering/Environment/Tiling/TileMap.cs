using JourneyCore.Lib.System;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace JourneyCore.Lib.Graphics.Rendering.Environment.Tiling
{
    [XmlRoot("map")]
    public class TileMap
    {
        private Random _rand;

        [XmlElement("layer")]
        public List<TileMapLayer> Layers { get; set; }

        [XmlAttribute("width")]
        public int Width { get; set; }

        [XmlAttribute("height")]
        public int Height { get; set; }

        [XmlAttribute("tilewidth")]
        public int PixelTileWidth { get; set; }

        [XmlAttribute("tileheight")]
        public int PixelTileHeight { get; set; }

        [XmlElement("tileset")]
        public TileSetPrimitive TileSetSource { get; set; }

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
        public VertexArray VArray { get; set; }
        [XmlIgnore]
        public TileSet TileSet { get; set; }

        public TileMap()
        {
            ChunkWidth = 8;
            ChunkHeight = 8;

            _rand = new Random();
        }



        #region METHODS

        private Tile GetTileById(int id)
        {
            return TileSet.Tiles.SingleOrDefault(tile => tile.Id == id);
        }

        public List<Tile> GetByGroup(string groupName)
        {
            return TileSet.Tiles.Where(tile => tile.Group.Equals(groupName)).ToList();
        }

        public List<Tile> GetByAccentGroup(string accentGroupName)
        {
            return TileSet.Tiles.Where(tile => !string.IsNullOrWhiteSpace(tile.AccentGroup) && tile.AccentGroup.Equals(accentGroupName)).ToList();
        }

        private Tile GetRandom(List<Tile> candidates)
        {
            // optimizations to avoid useless iterating

            if (candidates.Count < 1)
            {
                return default;
            }

            // if only one sprite in list
            if (candidates.Count == 1)
            {
                return candidates[0];
            }

            // in case all have equal weights
            if (candidates.Select(tile => tile.Probability).All(weight => weight == candidates[0].Probability))
            {
                return candidates[_rand.Next(0, candidates.Count)];
            }

            // end optimizations

            int totalWeight = candidates.Select(sprite => (int)(sprite.Probability * 100)).Sum();

            Tile[] weightArray = new Tile[totalWeight];

            int iterations = 0;
            for (int i = 0; i < candidates.Count; i++)
            {
                for (int j = 0; j < candidates[i].Probability * 100; j++)
                {
                    weightArray[iterations] = candidates[i];
                    iterations += 1;
                }
            }

            int randSelection = _rand.Next(0, weightArray.Length);
            return weightArray[randSelection];
        }

        public int GetCoordinate(int layerId, int x, int y)
        {
            if (Layers[layerId].ChunkMap.Length <= x || Layers[layerId].ChunkMap.GetLength(1) <= y)
            {
                return -1;
            }

            // todo implement this
            return Layers[layerId].ChunkMap[x / ChunkWidth * ChunkWidth][y / ChunkHeight * ChunkHeight].ChunkData[x % ChunkWidth][y % ChunkHeight];
        }

        public TileMapLayer GetTileLayer(string name)
        {
            return Layers.FirstOrDefault(layer => layer.Name.Equals(name));
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

            if (range.X > Width / ChunkWidth - 1)
            {
                range.X = Width / ChunkWidth - 1;
            }

            if (range.Y > Height / ChunkHeight - 1)
            {
                range.Y = Height / ChunkHeight - 1;
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
                TileMapLayer currentLayer = Layers[i];

                for (int x = 0; x < ChunkWidth; x++)
                {
                    if (currentLayer.ChunkMap[chunkCoords.X] == null)
                    {
                        continue;
                    }

                    for (int y = 0; y < ChunkHeight; y++)
                    {
                        if (currentLayer.ChunkMap[chunkCoords.X][chunkCoords.Y] == null)
                        {
                            continue;
                        }

                        int tileId = currentLayer.ChunkMap[chunkCoords.X][chunkCoords.Y].ChunkData[x][y];

                        // in this case, the selected tile
                        // is void
                        if (tileId == 0)
                        {
                            continue;
                        }

                        Tile currentTile = GetTileById(tileId);

                        if (currentTile == null)
                        {
                            continue;
                        }

                        currentTile.LayerId = currentLayer.Id;

                        Tile parsedTile = ParseTile(currentTile);

                        AllocateTileToVArray(parsedTile, chunkCoords, new Vector2i(x, y), currentLayer.Width);

                        foreach (Tile accentTile in parsedTile.Accents)
                        {
                            AllocateTileToVArray(ParseTile(accentTile), chunkCoords, new Vector2i(x, y), currentLayer.Width);
                        }
                    }
                }
            }
        }

        public void LoadChunk(int x, int y)
        {
            LoadChunk(new Vector2i(x, y));
        }

        private void AllocateTileToVArray(Tile tile, Vector2i chunkCoords, Vector2i currentChunk, int mapWidth)
        {
            // actual coordinate values
            // specific tiles in VArray
            int vArrayTileX = chunkCoords.X * ChunkWidth + currentChunk.X;
            int vArrayTileY = chunkCoords.Y * ChunkHeight + currentChunk.Y;

            Vector2f topLeft = GraphMath.CalculateVertexPosition(VertexCorner.TopLeft, vArrayTileX, vArrayTileY, ScaledTilePixelWidth, ScaledTilePixelHeight);
            Vector2f topRight = GraphMath.CalculateVertexPosition(VertexCorner.TopRight, vArrayTileX, vArrayTileY, ScaledTilePixelWidth, ScaledTilePixelHeight);
            Vector2f bottomRight = GraphMath.CalculateVertexPosition(VertexCorner.BottomRight, vArrayTileX, vArrayTileY, ScaledTilePixelWidth, ScaledTilePixelHeight);
            Vector2f bottomLeft = GraphMath.CalculateVertexPosition(VertexCorner.BottomLeft, vArrayTileX, vArrayTileY, ScaledTilePixelWidth, ScaledTilePixelHeight);

            uint index = (uint)((vArrayTileX + vArrayTileY * mapWidth) * 4 * tile.LayerId);

            VArray[index + 0] = new Vertex(topLeft, tile.TexCoords.TopLeft);
            VArray[index + 1] = new Vertex(topRight, tile.TexCoords.TopRight);
            VArray[index + 2] = new Vertex(bottomRight, tile.TexCoords.BottomRight);
            VArray[index + 3] = new Vertex(bottomLeft, tile.TexCoords.BottomLeft);
        }

        #endregion



        #region TILE EFFECT METHODS

        /// <summary>
        ///     Allocates a list of tiles that must be drawn in the current tilespace
        /// </summary>
        /// <param name="tile"></param>
        /// <returns></returns>
        private Tile ParseTile(Tile tile)
        {
            RandomizeTile(tile);
            RotateTile(tile);
            //AccentTile(tile);

            return tile;
        }

        private Tile RotateTile(Tile tile)
        {
            int randNum = _rand.Next(0, 3);

            if (!tile.IsRandomlyRotatable)
            {
                randNum = 0;
            }

            // width and height of all textures in a map will be the same
            int actualPixelLeft = tile.TextureRect.Left * tile.TextureRect.Width;
            int actualPixelTop = tile.TextureRect.Top * tile.TextureRect.Height;

            switch (randNum)
            {
                case 0:
                    tile.TexCoords.TopLeft = new Vector2f(actualPixelLeft, actualPixelTop);
                    tile.TexCoords.TopRight = new Vector2f(actualPixelLeft + tile.TextureRect.Width, actualPixelTop);
                    tile.TexCoords.BottomRight = new Vector2f(actualPixelLeft + tile.TextureRect.Width, actualPixelTop + tile.TextureRect.Height);
                    tile.TexCoords.BottomLeft = new Vector2f(actualPixelLeft, actualPixelTop + tile.TextureRect.Height);
                    break;
                case 1:
                    tile.TexCoords.TopLeft = new Vector2f(actualPixelLeft + tile.TextureRect.Width, actualPixelTop);
                    tile.TexCoords.TopRight = new Vector2f(actualPixelLeft + tile.TextureRect.Width, actualPixelTop + tile.TextureRect.Height);
                    tile.TexCoords.BottomRight = new Vector2f(actualPixelLeft, actualPixelTop + tile.TextureRect.Height);
                    tile.TexCoords.BottomLeft = new Vector2f(actualPixelLeft, actualPixelTop);
                    break;
                case 2:
                    tile.TexCoords.TopLeft = new Vector2f(actualPixelLeft + tile.TextureRect.Width, actualPixelTop + tile.TextureRect.Height);
                    tile.TexCoords.TopRight = new Vector2f(actualPixelLeft, actualPixelTop + tile.TextureRect.Height);
                    tile.TexCoords.BottomRight = new Vector2f(actualPixelLeft, actualPixelTop);
                    tile.TexCoords.BottomLeft = new Vector2f(actualPixelLeft + tile.TextureRect.Width, actualPixelTop);
                    break;
                case 3:
                    tile.TexCoords.TopLeft = new Vector2f(actualPixelLeft, actualPixelTop + tile.TextureRect.Height);
                    tile.TexCoords.TopRight = new Vector2f(actualPixelLeft, actualPixelTop);
                    tile.TexCoords.BottomRight = new Vector2f(actualPixelLeft + tile.TextureRect.Width, actualPixelTop);
                    tile.TexCoords.BottomLeft = new Vector2f(actualPixelLeft + tile.TextureRect.Width, actualPixelTop + tile.TextureRect.Height);
                    break;
                default:
                    break;
            }

            return tile;
        }

        private Tile RandomizeTile(Tile subjectTile)
        {
            if (!subjectTile.IsRandomizable)
            {
                return subjectTile;
            }

            return GetRandom(GetByGroup(subjectTile.Group));
        }

        private Tile AccentTile(Tile tile)
        {
            if (!tile.IsAccentable)
            {
                return tile;
            }

            double randNum = _rand.NextDouble();

            if (tile.AccentProbability >= randNum)
            {
                return tile;
            }

            Tile accent = GetRandom(GetByAccentGroup(tile.Group));

            if (accent == default(Tile))
            {
                return tile;
            }

            accent.LayerId = tile.LayerId + 1;
            tile.Accents.Add(accent);

            return tile;
        }

        #endregion
    }
}
