using System.Collections.Generic;
using System.Linq;
using JourneyCore.Lib.Game.Environment.Metadata;

namespace JourneyCore.Lib.Game.Environment.Tiling
{
    public class TileSet
    {
        public string Name { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public int Columns { get; set; }
        public string Image { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public List<Tile> Tiles { get; set; }
        public string TextureName { get; set; }

        public TileSetMetadata GetMetadata()
        {
            return new TileSetMetadata(TextureName, Tiles.Select(tile => tile.GetMetadata()).ToList());
        }
    }
}