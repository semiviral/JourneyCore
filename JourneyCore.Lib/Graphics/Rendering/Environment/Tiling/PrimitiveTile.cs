using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using SFML.Graphics;

namespace JourneyCore.Lib.Graphics.Rendering.Environment.Tiling
{
    public class PrimitiveTile
    {
        [XmlAttribute("gid")]
        public short Gid { get; set; }
        public short Rotation { get; set; }
        public IntRect TextureRect { get; set; }
        
        public PrimitiveTile()
        {
            Gid = 0;
            Rotation = 0;
            TextureRect = new IntRect(0, 0, 0, 0);
        }

        public PrimitiveTile(short gid, short rotation, IntRect textureRect)
        {
            Gid = gid;
            Rotation = rotation;
            TextureRect = textureRect;
        }
    }
}
