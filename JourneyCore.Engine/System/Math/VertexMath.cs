using System;
using SFML.System;

namespace JourneyCore.Lib.System.Math
{
    public static class VertexMath
    {
        public static Vector2f CalculateVertexPosition(VertexCorner corner, float x, float y, float sizeX, float sizeY)
        {
            Vector2f _vector = new Vector2f();

            switch (corner)
            {
                case VertexCorner.TopLeft:
                    _vector.X = x * sizeX;
                    _vector.Y = y * sizeY;

                    break;
                case VertexCorner.TopRight:
                    _vector.X = (x + 1) * sizeX;
                    _vector.Y = y * sizeY;

                    break;
                case VertexCorner.BottomRight:
                    _vector.X = (x + 1) * sizeX;
                    _vector.Y = (y + 1) * sizeY;

                    break;
                case VertexCorner.BottomLeft:
                    _vector.X = x * sizeX;
                    _vector.Y = (y + 1) * sizeY;

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(corner), corner, null);
            }

            return _vector;
        }
    }

    public enum VertexCorner
    {
        TopLeft,
        TopRight,
        BottomRight,
        BottomLeft
    }
}