using System.Collections.Generic;

namespace JourneyCore.Lib.Game.Environment.Metadata
{
    public class TileSetMetadata
    {
        public TileSetMetadata()
        {
        }

        public TileSetMetadata(string textureName, List<TileMetadata> tiles)
        {
            TextureName = textureName;
            Tiles = tiles;
        }

        public string TextureName { get; set; }
        public List<TileMetadata> Tiles { get; set; }
    }
}