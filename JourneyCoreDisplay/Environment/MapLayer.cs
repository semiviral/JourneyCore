using SFML.Graphics;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace JourneyCoreDisplay.Environment
{
    public class MapLayer
    {
        internal Chunk[][] ChunkMap { get; set; }

        [XmlElement("properties")]
        public List<CustomProperty> CustomProperties { get; set; }

        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("width")]
        public int Width { get; set; }

        [XmlAttribute("height")]
        public int Height { get; set; }

        [XmlElement("data")]
        public string Data { get; set; }

        public MapLayer() { }
    }
}
