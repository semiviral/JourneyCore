using SFML.Graphics;

namespace JourneyCore.Lib.Game.Environment.Metadata
{
    public struct TileMetadata
    {
        public int Gid { get; }
        public string Type { get; }
        public IntRect TextureRect { get; }

        public TileMetadata(int gid, string type, IntRect textureRect)
        {
            Gid = gid;
            Type = type;
            TextureRect = textureRect;
        }
    }
}