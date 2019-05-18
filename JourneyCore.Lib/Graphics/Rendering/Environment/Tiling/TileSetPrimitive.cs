using System.Xml.Serialization;

namespace JourneyCore.Lib.Graphics.Rendering.Environment.Tiling
{
    public class TileSetPrimitive
    {
        [XmlAttribute("source")]
        public string Source { get; set; }
    }
}
