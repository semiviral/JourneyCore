using System;
using System.Linq;
using JourneyCore.Lib.Game.Environment.Metadata;
using JourneyCore.Lib.Game.Object;
using JourneyCore.Lib.System.Loaders;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Lib.Game.Environment.Tiling
{
    public class Tile
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public double Probability { get; set; }
        public TileObjectGroup ObjectGroup { get; set; }
        public CustomProperty[] Properties { get; set; }
        public int Gid { get; set; }
        public IntRect TextureRect { get; set; }
        public bool IsRandomizable { get; private set; }
        public bool IsRandomlyRotatable { get; private set; }
        public Color MiniMapColor { get; private set; }

        public void ApplyProperties()
        {
            CustomProperty isRandomizable = GetProperty("IsRandomizable");
            CustomProperty isRandomlyRotatable = GetProperty("IsRandomlyRotatable");
            CustomProperty miniMapColor = GetProperty("MiniMapColor");

            if (isRandomizable != null)
            {
                IsRandomizable = (bool)Convert.ChangeType(isRandomizable.Value, typeof(bool));
            }

            if (isRandomlyRotatable != null)
            {
                IsRandomlyRotatable = (bool)Convert.ChangeType(isRandomlyRotatable.Value, typeof(bool));
            }

            if (miniMapColor != null)
            {
                string hexString = miniMapColor.Value.Substring(1);
                string hexStringAlpha = hexString.Substring(0, 2);
                string hexStringBase = hexString.Substring(2);

                MiniMapColor = new Color(Convert.ToUInt32($"0x{hexStringBase}{hexStringAlpha}", 16));
            }
        }

        public CustomProperty GetProperty(string propertyName)
        {
            if (Properties == null)
            {
                return null;
            }

            return !Properties.Any(property => property.Name.Equals(propertyName))
                ? null
                : Properties.Single(property => property.Name.Equals(propertyName));
        }

        public TileMetadata GetMetadata()
        {
            if (ObjectGroup == null)
            {
                return new TileMetadata(Gid, Type, TextureRect, MiniMapColor);
            }

            return new TileMetadata(Gid, Type, TextureRect, MiniMapColor, ObjectGroup.Objects.Select(tileObject =>
                new CollisionBox(new Vector2f(tileObject.X, tileObject.Y),
                    new Vector2f(tileObject.Width, tileObject.Height))).ToList());
        }

        public TilePrimitive ToPrimitive()
        {
            return new TilePrimitive(Gid, 0);
        }
    }
}