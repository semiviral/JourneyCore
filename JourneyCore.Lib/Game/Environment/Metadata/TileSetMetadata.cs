using System.Collections.Generic;

namespace JourneyCore.Lib.Game.Environment.Metadata
{
    public struct TileSetMetadata
    {
        public string TextureName { get; }
        public List<TileMetadata> Tiles { get; }

        public TileSetMetadata(string textureName, List<TileMetadata> tiles)
        {
            TextureName = textureName;
            Tiles = tiles;
        }
    }
}