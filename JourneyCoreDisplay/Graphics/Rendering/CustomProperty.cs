using System;
using System.Reflection;
using System.Xml.Serialization;

namespace JourneyCoreLib.Environment
{
    public class CustomProperty
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }

        [XmlIgnore]
        public Type QualifiedType { get; private set; }
        [XmlIgnore]
        public object QualifiedValue { get; private set; }

        public CustomProperty() {
            Type = "string";
        }

        public void QualifyValue()
        {
            switch (Type)
            {
                case "bool":
                    QualifiedType = typeof(bool);
                    break;
                case "int":
                    QualifiedType = typeof(int);
                    break;
                case "float":
                    QualifiedType = typeof(float);
                    break;
                case "string":
                    QualifiedType = typeof(string);
                    break;
                default:
                    QualifiedType = null;
                    QualifiedValue = null;
                    break;
            }

            QualifiedValue = Convert.ChangeType(Value, QualifiedType);
        }
    }
}
