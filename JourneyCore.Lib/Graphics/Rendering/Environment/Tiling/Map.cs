using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using JourneyCore.Lib.System.Components.Loaders;
using SFML.System;

namespace JourneyCore.Lib.Graphics.Rendering.Environment.Tiling
{
    [XmlRoot("map")]
    public class Map
    {
        private Random Rand { get; }

        [XmlElement("layer")]
        public List<MapLayer> Layers { get; set; }

        [XmlAttribute("width")]
        public short Width { get; set; }

        [XmlAttribute("height")]
        public short Height { get; set; }

        [XmlAttribute("tilewidth")]
        public short PixelTileWidth { get; set; }

        [XmlAttribute("tileheight")]
        public int PixelTileHeight { get; set; }

        [XmlElement("tileset")]
        public TileSetSource[] TileSetSources { get; set; }

        [XmlIgnore]
        public List<TileSet> TileSets { get; }

        [XmlIgnore]
        public short ChunkSizeX { get; set; }

        [XmlIgnore]
        public short ChunkSizeY { get; set; }

        public Map()
        {
            Rand = new Random();
            TileSets = new List<TileSet>();
        }

        public List<TileSet> LoadTileSets()
        {
            foreach (TileSetSource source in TileSetSources)
            {
                TileSets.Add(TileSetLoader.LoadTileSet(Path.GetFullPath(source.Source), source.FirstGid));
            }

            return TileSets;
        }

        public List<MapLayer> BuildMap()
        {
            foreach (MapLayer layer in Layers)
            {
                layer.CreateMap(ChunkSizeX, ChunkSizeY);
            }

            return Layers;
        }

        public List<MapLayer> ProcessTileEffects()
        {
            foreach (Chunk chunk in Layers.SelectMany(layer => layer.Map).SelectMany(map => map))
            {
                for (int x = 0; x < chunk.Length; x++)
                {
                    for (int y = 0; y < chunk[x].Length; y++)
                    {
                        if (GetTiles()[chunk[x][y].Gid].IsRandomizable)
                            chunk[x][y] = RandomizeTile(GetTiles()[chunk[x][y].Gid]).ToPrimitive();

                        if (GetTiles()[chunk[x][y].Gid].IsRandomlyRotatable)
                            chunk[x][y].Rotation = (short)Rand.Next(0, 3);
                    }
                }
            }

            return Layers;
        }

        public List<Tile> GetTiles()
        {
            return TileSets.SelectMany(tileSet => tileSet.Tiles).ToList();
        }

        #region ACCENT

        private PrimitiveTile RotateTile(Tile tile)
        {
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

        private Tile RandomizeTile(Tile subjectTile)
        {
            return !subjectTile.IsRandomizable
                ? subjectTile
                : GetRandom(TileSets.SelectMany(tileSet => tileSet.Tiles).Where(tile => tile.Group.Equals(subjectTile.Group)).ToArray());
        }


        private Tile GetRandom(IReadOnlyList<Tile> candidates)
        {
            // optimizations to avoid useless iterating

            if (candidates.Count < 1) return default;

            // if only one sprite in list
            if (candidates.Count == 1) return candidates[0];

            // in case all have equal weights
            if (candidates.Select(tile => tile.Probability)
                .All(weight => Math.Abs(weight - candidates[0].Probability) < 0.01))
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
    }

    #endregion

}