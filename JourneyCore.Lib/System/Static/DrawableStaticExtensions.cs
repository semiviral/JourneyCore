using JourneyCore.Lib.System.Math;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Lib.System.Static
{
    public static class SpriteStaticExtensions
    {
        public static Vertex[] GetVertices(this Sprite graphic)
        {
            Vertex[] vertices = new Vertex[4];
            float pixelRadiusX = graphic.TextureRect.Width / 2f * graphic.Scale.X;
            float pixelRadiusY = graphic.TextureRect.Height / 2f * graphic.Scale.Y;

            Vector2f topLeft = new Vector2f(graphic.Position.X - pixelRadiusX, graphic.Position.Y + pixelRadiusY);
            Vector2f topRight = new Vector2f(graphic.Position.X + pixelRadiusX, graphic.Position.Y + pixelRadiusY);
            Vector2f bottomRight = new Vector2f(graphic.Position.X + pixelRadiusX, graphic.Position.Y - pixelRadiusY);
            Vector2f bottomLeft = new Vector2f(graphic.Position.X - pixelRadiusX, graphic.Position.Y - pixelRadiusY);

            vertices[0] = new Vertex(GraphMath.RotatePoint(topLeft, graphic.Position, graphic.Rotation),
                new Vector2f(graphic.TextureRect.Left, graphic.TextureRect.Top));
            vertices[1] = new Vertex(GraphMath.RotatePoint(topRight, graphic.Position, graphic.Rotation),
                new Vector2f(graphic.TextureRect.Left + graphic.TextureRect.Width, graphic.TextureRect.Top));
            vertices[2] = new Vertex(GraphMath.RotatePoint(bottomRight, graphic.Position, graphic.Rotation),
                new Vector2f(graphic.TextureRect.Left + graphic.TextureRect.Width,
                    graphic.TextureRect.Top + graphic.TextureRect.Height));
            vertices[3] = new Vertex(GraphMath.RotatePoint(bottomLeft, graphic.Position, graphic.Rotation),
                new Vector2f(graphic.TextureRect.Left, graphic.TextureRect.Top + graphic.TextureRect.Height));

            return vertices;
        }

        public static Vertex[] GetVertices(this RectangleShape rectShape)
        {
            Vertex[] vertices = new Vertex[4];
            float pixelRadiusX = rectShape.Size.X / 2;
            float pixelRadiusY = rectShape.Size.Y / 2;

            Vector2f topLeft = new Vector2f(rectShape.Position.X - pixelRadiusX, rectShape.Position.Y + pixelRadiusY);
            Vector2f topRight = new Vector2f(rectShape.Position.X + pixelRadiusX, rectShape.Position.Y + pixelRadiusY);
            Vector2f bottomRight =
                new Vector2f(rectShape.Position.X + pixelRadiusX, rectShape.Position.Y - pixelRadiusY);
            Vector2f bottomLeft =
                new Vector2f(rectShape.Position.X - pixelRadiusX, rectShape.Position.Y - pixelRadiusY);

            vertices[0] = new Vertex(GraphMath.RotatePoint(topLeft, rectShape.Position, rectShape.Rotation),
                rectShape.FillColor);
            vertices[1] = new Vertex(GraphMath.RotatePoint(topRight, rectShape.Position, rectShape.Rotation),
                rectShape.FillColor);
            vertices[2] = new Vertex(GraphMath.RotatePoint(bottomRight, rectShape.Position, rectShape.Rotation),
                rectShape.FillColor);
            vertices[3] = new Vertex(GraphMath.RotatePoint(bottomLeft, rectShape.Position, rectShape.Rotation),
                rectShape.FillColor);

            return vertices;
        }
    }
}