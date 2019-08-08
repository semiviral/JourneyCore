using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JourneyCore.Lib.Game.Environment.Metadata;
using JourneyCore.Lib.Game.Environment.Tiling;
using JourneyCore.Lib.Game.Object.Collision;
using JourneyCore.Lib.System.Loaders;
using SFML.System;

namespace JourneyCore.Lib.Game.Environment.Mapping
{
    public class Map
    {
        public Map()
        {
            Rand = new Random();
            UsedTileSets = new List<TileSet>();
            Colliders = new List<CollisionQuad>();
        }

        private Random Rand { get; }
        public List<MapLayer> Layers { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int PixelTileWidth { get; set; }
        public int PixelTileHeight { get; set; }
        public TileSetSource[] TileSets { get; set; }
        public string Name { get; set; }
        public List<CustomProperty> Properties { get; set; }
        public float SpawnPointX { get; private set; }
        public float SpawnPointY { get; private set; }
        public List<TileSet> UsedTileSets { get; }
        public List<CollisionQuad> Colliders { get; }


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
                UsedTileSets.Select(tileSet => tileSet.GetMetadata()).ToList(), PixelTileWidth, PixelTileHeight,
                SpawnPointX, SpawnPointY, Colliders);
        }


        #region INIT

        public List<TileSet> LoadTileSets()
        {
            foreach (TileSetSource _source in TileSets)
            {
                TileSet _tileSet = TileSetLoader.LoadTileSet(
                    $@"{MapLoader.ASSET_ROOT}\Maps\TileSets\{Path.GetFileName(_source.Source)}", _source.FirstGid);

                PixelTileWidth = _tileSet.TileWidth * MapLoader.Scale;
                PixelTileHeight = _tileSet.TileHeight * MapLoader.Scale;

                UsedTileSets.Add(_tileSet);
            }

            return UsedTileSets;
        }

        public List<MapLayer> BuildMap()
        {
            Layers.ForEach(layer => layer.CreateMap((short) MapLoader.ChunkSize, (short) MapLoader.ChunkSize));

            return Layers;
        }

        public List<MapLayer> ProcessTiles()
        {
            foreach (Chunk _chunk in Layers.SelectMany(layer => layer.Map).SelectMany(map => map))
            {
                for (int _x = 0; _x < _chunk.Length; _x++)
                for (int _y = 0; _y < _chunk[_x].Length; _y++)
                {
                    if (GetTile(_chunk[_x][_y].Gid) == null)
                    {
                        continue;
                    }

                    if (GetTile(_chunk[_x][_y].Gid).IsRandomizable)
                    {
                        _chunk[_x][_y] = RandomizeTile(GetTile(_chunk[_x][_y].Gid)).ToPrimitive();
                    }

                    if (GetTile(_chunk[_x][_y].Gid).IsRandomlyRotatable)
                    {
                        _chunk[_x][_y].Rotation = Rand.Next(0, 3);
                    }

                    List<TileMetadata> _tileMetadatas = UsedTileSets.SelectMany(tileSet => tileSet.Tiles)
                        .Select(tile => tile.GetMetadata()).ToList();

                    Vector2f _tileCoords = new Vector2f((_chunk.Left * MapLoader.ChunkSize) + _x,
                        (_chunk.Top * MapLoader.ChunkSize) + _y);

                    AllocateTileCollisions(
                        _tileMetadatas.FirstOrDefault(tileMetadata => tileMetadata.Gid == _chunk[_x][_y].Gid), _tileCoords);
                }
            }

            return Layers;
        }

        private void AllocateTileCollisions(TileMetadata tileMetadata, Vector2f tileCoords)
        {
            if (tileMetadata?.Colliders == null)
            {
                return;
            }

            foreach (CollisionQuad _collider in tileMetadata.Colliders)
            {
                CollisionQuad _copy = new CollisionQuad(_collider);

                _copy.Position = (tileCoords * MapLoader.TileSize) +
                                new Vector2f(_copy.Position.X, _copy.Position.Y);

                Colliders.Add(_copy);
            }
        }

        public void ApplyProperties()
        {
            {
                CustomProperty _spawnPointX = GetProperty("SpawnPointX");
                CustomProperty _spawnPointY = GetProperty("SpawnPointY");

                if (_spawnPointX != null)
                {
                    SpawnPointX = float.Parse(_spawnPointX.Value);
                }

                if (_spawnPointY != null)
                {
                    SpawnPointY = float.Parse(_spawnPointY.Value);
                }
            }
        }

        private CustomProperty GetProperty(string propertyName)
        {
            if (Properties == null)
            {
                return null;
            }

            return !Properties.Any(property => property.Name.Equals(propertyName))
                ? null
                : Properties.Single(property => property.Name.Equals(propertyName));
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

            int _totalWeight = candidates.Select(sprite => (int) (sprite.Probability * 100)).Sum();

            Tile[] _weightArray = new Tile[_totalWeight];

            int _iterations = 0;
            foreach (Tile _tilePackage in candidates)
            {
                for (int _j = 0; _j < (_tilePackage.Probability * 100); _j++)
                {
                    _weightArray[_iterations] = _tilePackage;
                    _iterations += 1;
                }
            }

            int _randSelection = Rand.Next(0, _weightArray.Length);
            return _weightArray[_randSelection];
        }
    }

    #endregion
}