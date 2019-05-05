using SFML.System;

namespace JourneyCoreDisplay.Drawing
{
    public static class MathOps
    {
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
