using System.Collections.Generic;
using JourneyCore.Lib.Game.Object.Collision;
using SFML.Graphics;

namespace JourneyCore.Lib.Game.Environment.Metadata
{
    public class TileMetadata
    {
        public TileMetadata()
        {
        }

        public TileMetadata(int gid, string type, IntRect textureRect, Color miniMapColor,
            List<CollisionQuad> colliders) :
            this(gid, type, textureRect, miniMapColor)
        {
            Colliders = colliders;
        }

        public TileMetadata(int gid, string type, IntRect textureRect, Color miniMapColor)
        {
            Gid = gid;
            Type = type;
            TextureRect = textureRect;
            MiniMapColor = miniMapColor;
        }

        public int Gid { get; set; }
        public string Type { get; set; }
        public IntRect TextureRect { get; set; }
        public Color MiniMapColor { get; set; }
        public List<CollisionQuad> Colliders { get; set; }
    }
}