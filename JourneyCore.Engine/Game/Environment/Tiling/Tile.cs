using System;
using System.Linq;
using JourneyCore.Lib.Game.Environment.Metadata;
using JourneyCore.Lib.Game.Object.Collision;
using JourneyCore.Lib.System.Loaders;
using SFML.Graphics;

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
            CustomProperty _isRandomizable = GetProperty("IsRandomizable");
            CustomProperty _isRandomlyRotatable = GetProperty("IsRandomlyRotatable");
            CustomProperty _miniMapColor = GetProperty("MiniMapColor");

            if (_isRandomizable != null)
            {
                IsRandomizable = (bool) Convert.ChangeType(_isRandomizable.Value, typeof(bool));
            }

            if (_isRandomlyRotatable != null)
            {
                IsRandomlyRotatable = (bool) Convert.ChangeType(_isRandomlyRotatable.Value, typeof(bool));
            }

            if (_miniMapColor != null)
            {
                string _hexString = _miniMapColor.Value.Substring(1);
                string _hexStringAlpha = _hexString.Substring(0, 2);
                string _hexStringBase = _hexString.Substring(2);

                MiniMapColor = new Color(Convert.ToUInt32($"0x{_hexStringBase}{_hexStringAlpha}", 16));
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
                new CollisionQuad(new FloatRect(tileObject.X, tileObject.Y, tileObject.Width, tileObject.Height),
                    tileObject.Rotation)).ToList());
        }

        public TilePrimitive ToPrimitive()
        {
            return new TilePrimitive(Gid, 0);
        }
    }
}