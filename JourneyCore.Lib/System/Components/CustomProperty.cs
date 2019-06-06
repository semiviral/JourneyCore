using System.Xml.Serialization;

namespace JourneyCore.Lib.System.Components
{
    public class CustomProperty
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }

        public CustomProperty()
        {
            Type = "string";
        }
    }
}