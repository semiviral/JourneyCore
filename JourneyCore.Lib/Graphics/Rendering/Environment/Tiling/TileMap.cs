using System.Xml.Serialization;

namespace JourneyCore.Lib.Graphics.Rendering.Environment.Tiling
{
    [XmlRoot("map")]
    public struct TileMap
    {
        [XmlElement("layer")] public TileMapLayer[] Layers { get; set; }
        [XmlAttribute("width")] public short Width { get; set; }
        [XmlAttribute("height")] public short Height { get; set; }
        [XmlAttribute("tilewidth")] public short PixelTileWidth { get; set; }
        [XmlAttribute("tileheight")] public int PixelTileHeight { get; set; }
        [XmlElement("tileset")] public TileSetPrimitive[] TileSetSources { get; set; }
    }
}