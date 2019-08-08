using System.Collections.Generic;
using JourneyCore.Lib.Game.Object.Collision;
using JourneyCore.Lib.System.Loaders;

namespace JourneyCore.Lib.Game.Environment.Metadata
{
    public class MapMetadata
    {
        public MapMetadata()
        {
        }

        public MapMetadata(string name, int width, int height, int layerCount, List<TileSetMetadata> tileSets,
            int tileWidth, int tileHeight, float spawnPointX, float spawnPointY, List<CollisionQuad> colliders)
        {
            Name = name;
            Width = width;
            Height = height;
            LayerCount = layerCount;
            TileSets = tileSets;
            TileWidth = tileWidth;
            TileHeight = tileHeight;
            SpawnPointX = spawnPointX * MapLoader.TileSize;
            SpawnPointY = spawnPointY * MapLoader.TileSize;
            Colliders = colliders;
        }

        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int LayerCount { get; set; }
        public List<TileSetMetadata> TileSets { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public float SpawnPointX { get; set; }
        public float SpawnPointY { get; set; }
        public List<CollisionQuad> Colliders { get; set; }
    }
}