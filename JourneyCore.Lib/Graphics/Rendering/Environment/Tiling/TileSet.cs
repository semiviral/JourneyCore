using System.Xml.Serialization;

namespace JourneyCore.Lib.Graphics.Rendering.Environment.Tiling
{
    [XmlRoot("tileset")]
    public struct TileSet
    {
        [XmlAttribute("name")] public string Name { get; set; }
        [XmlAttribute("tilewidth")] public short TileWidth { get; set; }
        [XmlAttribute("tileheight")] public short TileHeight { get; set; }
        [XmlAttribute("tilecount")] public short TileCount { get; set; }
        [XmlAttribute("columns")] public short Columns { get; set; }
        [XmlElement("image")] public TileSetImage SourceImage { get; set; }
        [XmlElement("tile")] public Tile[] Tiles { get; set; }
    }
}