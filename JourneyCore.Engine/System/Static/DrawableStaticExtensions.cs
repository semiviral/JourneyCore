using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using JourneyCore.Lib.System.Math;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Lib.System.Static
{
    public static class DrawnTypesStaticExtensions
    {
        public static Vertex[] GetVertices(this Sprite graphic, Vector2f offset)
        {
            Vertex[] _vertices = new Vertex[4];
            float _pixelRadiusX = (graphic.TextureRect.Width / 2f) * graphic.Scale.X;
            float _pixelRadiusY = (graphic.TextureRect.Height / 2f) * graphic.Scale.Y;

            Vector2f _topLeft = new Vector2f(offset.X - _pixelRadiusX, offset.Y + _pixelRadiusY);
            Vector2f _topRight = new Vector2f(offset.X + _pixelRadiusX, offset.Y + _pixelRadiusY);
            Vector2f _bottomRight = new Vector2f(offset.X + _pixelRadiusX, offset.Y - _pixelRadiusY);
            Vector2f _bottomLeft = new Vector2f(offset.X - _pixelRadiusX, offset.Y - _pixelRadiusY);

            _vertices[0] = new Vertex(GraphMath.RotatePoint(_topLeft, graphic.Position, graphic.Rotation),
                new Vector2f(graphic.TextureRect.Left, graphic.TextureRect.Top));
            _vertices[1] = new Vertex(GraphMath.RotatePoint(_topRight, graphic.Position, graphic.Rotation),
                new Vector2f(graphic.TextureRect.Left + graphic.TextureRect.Width, graphic.TextureRect.Top));
            _vertices[2] = new Vertex(GraphMath.RotatePoint(_bottomRight, graphic.Position, graphic.Rotation),
                new Vector2f(graphic.TextureRect.Left + graphic.TextureRect.Width,
                    graphic.TextureRect.Top + graphic.TextureRect.Height));
            _vertices[3] = new Vertex(GraphMath.RotatePoint(_bottomLeft, graphic.Position, graphic.Rotation),
                new Vector2f(graphic.TextureRect.Left, graphic.TextureRect.Top + graphic.TextureRect.Height));

            return _vertices;
        }

        public static IEnumerable<Vertex> GetVertices(this Shape shape, Vector2f offset)
        {
            Vector2f _originScaled = shape.Origin.MultiplyBy(shape.Scale);

            for (uint _i = 0; _i < shape.GetPointCount(); _i++)
            {
                yield return new Vertex(offset + GraphMath.RotatePoint(shape.GetPoint(_i).MultiplyBy(shape.Scale), _originScaled, shape.Rotation),
                    shape.FillColor);
            }
        }
    }
}