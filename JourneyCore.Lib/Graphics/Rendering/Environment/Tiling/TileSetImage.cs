using System.Xml.Serialization;

namespace JourneyCore.Lib.Graphics.Rendering.Environment.Tiling
{
    public struct TileSetImage
    {
        [XmlAttribute("source")] public string Source { get; set; }
        [XmlAttribute("width")] public short Width { get; set; }
        [XmlAttribute("height")] public short Height { get; set; }
    }
}