using System.Xml.Serialization;
using JourneyCore.Lib.Graphics.Rendering.Environment.Chunking;

namespace JourneyCore.Lib.Graphics.Rendering.Environment.Tiling
{
    public struct TileMapLayer
    {
        [XmlAttribute("id")] public short Id { get; set; }
        [XmlAttribute("name")] public string Name { get; set; }
        [XmlAttribute("width")] public short Width { get; set; }
        [XmlAttribute("height")] public short Height { get; set; }
        [XmlElement("data")] public string Data { get; set; }
        [XmlIgnore] public Chunk[][] ChunkMap { get; set; }
    }
}