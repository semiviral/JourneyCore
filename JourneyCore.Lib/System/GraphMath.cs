using System;
using System.Runtime.CompilerServices;
using SFML.System;

namespace JourneyCore.Lib.System
{
    public enum Angle
    {
        Alpha,
        Beta,
        Gamma,
    }

    public static class GraphMath
    {
        public static int SquareLength(double x0, double y0, double x1, double y1)
        {
            return SquareLength((int)x0, (int)y0, (int)x1, (int)y1);
        }

        public static int SquareLength(float x0, float y0, float x1, float y1)
        {
            return SquareLength((int)x0, (int)y0, (int)x1, (int)y1);
        }

        public static int SquareLength(int x0, int y0, int x1, int y1)
        {
            int x2 = x0 - x1;
            int y2 = y0 - y1;

            return x2 * x2 + y2 * y2;
        }

        public static double CosFromDegrees(float degrees)
        {
            return Math.Cos(ToRadians(degrees));
        }

        public static double SinFromDegrees(float degrees)
        {
            return Math.Sin(ToRadians(degrees));
        }

        public static double ToRadians(double degrees)
        {
            return Math.PI * degrees / 180d;
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