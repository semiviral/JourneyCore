using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JourneyCore.Lib.Game.Environment.Metadata;
using JourneyCore.Lib.Game.Environment.Tiling;
using JourneyCore.Lib.System.Components.Loaders;

namespace JourneyCore.Lib.Game.Environment.Mapping
{
    public class Map
    {
        public Map()
        {
            Rand = new Random();
            UsedTileSets = new List<TileSet>();
        }

        private Random Rand { get; }
        public List<MapLayer> Layers { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int PixelTileWidth { get; set; }
        public int PixelTileHeight { get; set; }
        public TileSetSource[] TileSets { get; set; }
        public string Name { get; set; }
        public List<TileSet> UsedTileSets { get; }


        public List<Tile> GetTiles()
        {
            return UsedTileSets.SelectMany(tileSet => tileSet.Tiles).ToList();
        }

        public Tile GetTile(int gid)
        {
            return GetTiles().SingleOrDefault(tile => tile.Gid == gid);
        }

        public MapMetadata GetMetadata()
        {
            return new MapMetadata(Name, Width, Height, Layers.Count,
                UsedTileSets.Select(tileSet => tileSet.GetMetadata()).ToList(), PixelTileWidth, PixelTileHeight);
        }


        #region INIT

        public List<TileSet> LoadTileSets()
        {
            foreach (TileSetSource source in TileSets)
            {
                TileSet tileSet = TileSetLoader.LoadTileSet(
                    $@"{MapLoader.AssetRoot}\Maps\TileSets\{Path.GetFileName(source.Source)}", source.FirstGid);

                PixelTileWidth = tileSet.TileWidth * MapLoader.Scale;
                PixelTileHeight = tileSet.TileHeight * MapLoader.Scale;

                UsedTileSets.Add(tileSet);
            }

            return UsedTileSets;
        }

        public List<MapLayer> BuildMap()
        {
            foreach (MapLayer layer in Layers)
            {
                layer.CreateMap((short)MapLoader.ChunkSize, (short)MapLoader.ChunkSize);
            }

            return Layers;
        }

        public List<MapLayer> ProcessTileEffects()
        {
            foreach (Chunk chunk in Layers.SelectMany(layer => layer.Map).SelectMany(map => map))
            {
                for (int x = 0; x < chunk.Length; x++)
                for (int y = 0; y < chunk[x].Length; y++)
                {
                    if (GetTile(chunk[x][y].Gid) == null)
                    {
                        continue;
                    }

                    if (GetTile(chunk[x][y].Gid).IsRandomizable)
                    {
                        chunk[x][y] = RandomizeTile(GetTile(chunk[x][y].Gid)).ToPrimitive();
                    }

                    if (GetTile(chunk[x][y].Gid).IsRandomlyRotatable)
                    {
                        chunk[x][y].Rotation = Rand.Next(0, 3);
                    }
                }
            }

            return Layers;
        }

        #endregion


        #region ACCENT

        private Tile RandomizeTile(Tile subjectTile)
        {
            if (subjectTile.Type == null)
            {
                return subjectTile;
            }

            return !subjectTile.IsRandomizable
                ? subjectTile
                : GetRandom(UsedTileSets.SelectMany(tileSet => tileSet.Tiles)
                    .Where(tile => tile.Type.Equals(subjectTile.Type)).ToArray());
        }


        private Tile GetRandom(IReadOnlyList<Tile> candidates)
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
            if (candidates.Select(tile => tile.Probability)
                .All(weight => Math.Abs(weight - candidates[0].Probability) < 0.01))
            {
                return candidates[Rand.Next(0, candidates.Count)];
            }

            // end optimizations

            int totalWeight = candidates.Select(sprite => (int)(sprite.Probability * 100)).Sum();

            Tile[] weightArray = new Tile[totalWeight];

            int iterations = 0;
            foreach (Tile tilePackage in candidates)
            {
                for (int j = 0; j < tilePackage.Probability * 100; j++)
                {
                    weightArray[iterations] = tilePackage;
                    iterations += 1;
                }
            }

            int randSelection = Rand.Next(0, weightArray.Length);
            return weightArray[randSelection];
        }
    }

    #endregion
}