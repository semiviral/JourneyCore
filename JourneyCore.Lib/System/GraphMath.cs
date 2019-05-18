using System;
using SFML.System;

namespace JourneyCore.Lib.System
{
    public static class GraphMath
    {
        public static double DistanceBetweenPoints(float x1, float y1, float x2, float y2)
        {
            float x0 = x1 - x2;
            float y0 = y1 - y2;

            return Math.Sqrt(x0 * x0 + y0 * y0);
        }

        public static float CosFromDegrees(float degrees)
        {
            return (float)Math.Cos(degrees * (Math.PI / 180));
        }

        public static float SinFromDegrees(float degrees)
        {
            return (float)Math.Sin(degrees * (Math.PI / 180));
        }

        public static Vector2f CalculateVertexPosition(VertexCorner corner, int x, int y, int sizeX, int sizeY)
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
            }

            return vector;
        }
    }


    public enum VertexCorner
    {
        TopLeft,
        TopRight,
        BottomRight,
        BottomLeft,
    }
}
