using SFML.Graphics;

namespace JourneyCore.Lib.Game.Environment.Metadata
{
    public struct TileMetadata
    {
        public int Gid { get; }
        public string Type { get; }
        public IntRect TextureRect { get; }
        public Color MiniMapColor { get; }

        public TileMetadata(int gid, string type, IntRect textureRect, Color miniMapColor)
        {
            Gid = gid;
            Type = type;
            TextureRect = textureRect;
            MiniMapColor = miniMapColor;
        }
    }
}