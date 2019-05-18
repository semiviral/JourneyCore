using System.Collections.Generic;
using System.Xml.Serialization;

namespace JourneyCore.Lib.Graphics.Rendering.Environment.Tiling
{
    [XmlRoot("tileset")]
    public class TileSet
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("tilewidth")]
        public int TileWidth { get; set; }

        [XmlAttribute("tileheight")]
        public int TileHeight { get; set; }

        [XmlAttribute("tilecount")]
        public int TileCount { get; set; }

        [XmlAttribute("columns")]
        public int Columns { get; set; }

        [XmlElement("image")]
        public TileSetImage SourceImage { get; set; }

        [XmlElement("tile")]
        public List<Tile> Tiles { get; set; }

        public TileSet() { }
    }
}
