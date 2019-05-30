using System.Xml.Serialization;

namespace JourneyCore.Lib.System.Components
{
    public class CustomProperty
    {
        public CustomProperty()
        {
            Type = "string";
        }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }
    }
}