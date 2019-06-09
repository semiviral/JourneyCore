using System.Collections.Generic;
using JourneyCore.Lib.Game.Environment.Tiling;
using JourneyCore.Lib.Game.Object;
using SFML.Graphics;

namespace JourneyCore.Lib.Game.Environment.Metadata
{
    public class TileMetadata
    {
        public int Gid { get; set; }
        public string Type { get; set; }
        public IntRect TextureRect { get; set; }
        public Color MiniMapColor { get; set; }
        public List<CollisionBox> Collidables { get; set; }

        public TileMetadata() { }

        public TileMetadata(int gid, string type, IntRect textureRect, Color miniMapColor, List<CollisionBox> collidables) :
            this(gid, type, textureRect, miniMapColor)
        {
            Collidables = collidables;
        }

        public TileMetadata(int gid, string type, IntRect textureRect, Color miniMapColor)
        {
            Gid = gid;
            Type = type;
            TextureRect = textureRect;
            MiniMapColor = miniMapColor;
        }
    }
}