using System;
using SFML.System;

namespace JourneyCore.Lib.System.Math
{
    public static class VertexMath
    {
        public static Vector2f CalculateVertexPosition(VertexCorner corner, float x, float y, float sizeX, float sizeY)
        {
            Vector2f vector = new Vector2f();

            switch (corner)
            {
                case VertexCorner.TopLeft:
                    vector.X = x * sizeX;
                    vector.Y = y * sizeY;

                    break;
                case VertexCorner.TopRight:
                    vector.X = (x + 1) * sizeX;
                    vector.Y = y * sizeY;

                    break;
                case VertexCorner.BottomRight:
                    vector.X = (x + 1) * sizeX;
                    vector.Y = (y + 1) * sizeY;

                    break;
                case VertexCorner.BottomLeft:
                    vector.X = x * sizeX;
                    vector.Y = (y + 1) * sizeY;

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(corner), corner, null);
            }

            return vector;
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