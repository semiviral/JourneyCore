using SFML.Graphics;

namespace JourneyCore.Lib.Game.Environment.Metadata
{
    public struct TileMetadata
    {
        public int Gid { get; }
        public IntRect TextureRect { get; }

        public TileMetadata(int gid, IntRect textureRect)
        {
            Gid = gid;
            TextureRect = textureRect;
        }
    }
}