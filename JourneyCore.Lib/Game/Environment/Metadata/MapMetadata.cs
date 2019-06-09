using System.Collections.Generic;

namespace JourneyCore.Lib.Game.Environment.Metadata
{
    public class MapMetadata
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int LayerCount { get; set; }
        public List<TileSetMetadata> TileSets { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }

        public MapMetadata() { }

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