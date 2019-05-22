using System.Xml.Serialization;

namespace JourneyCore.Lib.Graphics.Rendering.Environment.Tiling
{
    public class TileSetSource
    {
        [XmlAttribute("source")]
        public string Source { get; set; }

        [XmlAttribute("firstgid")]
        public short FirstGid { get; set; }
    }
}