using System.Collections.Generic;

namespace JourneyCore.Lib.Game.Environment.Metadata
{
    public struct MapMetadata
    {
        public string Name { get; }
        public int Width { get; }
        public int Height { get; }
        public int LayerCount { get; }
        public List<TileSetMetadata> TileSets { get; }
        public int TileWidth { get; }
        public int TileHeight { get; }

        public MapMetadata(string name, int width, int height, int layerCount, List<TileSetMetadata> tileSets,
            int tileWidth, int tileHeight)
        {
            Name = name;
            Width = width;
            Height = height;
            LayerCount = layerCount;
            TileSets = tileSets;
            TileWidth = tileWidth;
            TileHeight = tileHeight;
        }
    }
}