using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace JourneyCore.Lib.Graphics.Rendering.Environment.Tiling
{
    public class Tile
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("type")]
        public string Group { get; set; }

        [XmlAttribute("probability")]
        public float Probability { get; set; }

        [XmlArray("properties")]
        [XmlArrayItem("property")]
        public List<CustomProperty> Properties { get; set; }

        [XmlIgnore]
        public Vector2i Size { get; set; }
        [XmlIgnore]
        public IntRect TextureRect { get; set; }
        [XmlIgnore]
        public QuadCoords TexCoords { get; set; }
        [XmlIgnore]
        public int LayerId { get; set; }
        [XmlIgnore]
        public List<Tile> Accents { get; set; }



        #region CUSTOM TILE PROPERTIES

        [XmlIgnore]
        public bool IsRandomizable { get; private set; }
        [XmlIgnore]
        public bool IsRandomlyRotatable { get; private set; }
        [XmlIgnore]
        public bool IsAccentable { get; private set; }
        [XmlIgnore]
        public string AccentGroup { get; private set; }
        [XmlIgnore]
        public float AccentProbability { get; private set; }

        #endregion



        public Tile()
        {
            Id = -1;
            Group = "None";
            Probability = 1.0f;
            TexCoords = new QuadCoords();
            Accents = new List<Tile>();
            IsRandomizable = false;
            IsRandomlyRotatable = false;
            IsAccentable = false;
            AccentGroup = string.Empty;
        }

        public void Initialise(int columns)
        {
            SetPosition(columns);

            foreach (CustomProperty property in Properties)
            {
                property.QualifyValue();
            }

            ApplyProperties();
        }

        private void SetPosition(int columns)
        {
            int y = (Id - 1) / columns;
            int x = (Id - 1) % columns;

            if (Size == null)
            {
                Size = new Vector2i(16, 16);
            }

            TextureRect = new IntRect(x, y, Size.X, Size.Y);
        }

        private void ApplyProperties()
        {
            CustomProperty isRandomizable = GetProperty("IsRandomizable");
            CustomProperty isRandomlyRotatable = GetProperty("IsRandomlyRotatable");
            CustomProperty isAccentable = GetProperty("IsAccentable");
            CustomProperty accentGroup = GetProperty("AccentGroup");
            CustomProperty accentProbability = GetProperty("AccentProbability");

            if (isRandomizable != null)
            {
                IsRandomizable = (bool)Convert.ChangeType(isRandomizable.QualifiedValue, isRandomizable.QualifiedType);
            }

            if (isRandomlyRotatable != null)
            {
                IsRandomlyRotatable = (bool)Convert.ChangeType(isRandomlyRotatable.QualifiedValue, isRandomlyRotatable.QualifiedType);
            }

            if (isAccentable != null)
            {
                IsAccentable = (bool)Convert.ChangeType(isAccentable.QualifiedValue, isAccentable.QualifiedType);
            }

            if (accentGroup != null)
            {
                AccentGroup = (string)Convert.ChangeType(accentGroup.QualifiedValue, accentGroup.QualifiedType);
            }

            if (accentProbability != null)
            {
                AccentProbability = (float)Convert.ChangeType(accentProbability.QualifiedValue, accentProbability.QualifiedType);
            }
        }

        public CustomProperty GetProperty(string propertyName)
        {
            return Properties.FirstOrDefault(property => property.Name.Equals(propertyName));
        }
    }
}
