using System;
using JourneyCore.Lib.Display;
using JourneyCore.Lib.Game.Environment.Metadata;
using JourneyCore.Lib.System.Loaders;
using JourneyCore.Lib.System.Math;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Lib.Game.Environment.Tiling
{
    public class TileDrawItem
    {
        public TileDrawItem(TilePrimitive tilePrimitive, TileMetadata tileMetadata, Vector2i tileCoords)
        {
            TilePrimitive = tilePrimitive;
            TileMetadata = tileMetadata;
            TileCoords = tileCoords;
        }

        private TilePrimitive TilePrimitive { get; }
        private TileMetadata TileMetadata { get; }
        public Vector2i TileCoords { get; }

        public Vertex[] GetVertices()
        {
            int _scaledSizeX = TileMetadata.TextureRect.Width * MapLoader.Scale;
            int _scaledSizeY = TileMetadata.TextureRect.Height * MapLoader.Scale;

            Vector2f _topLeft = VertexMath.CalculateVertexPosition(VertexCorner.TopLeft, TileCoords.X, TileCoords.Y,
                _scaledSizeX, _scaledSizeY);
            Vector2f _topRight = VertexMath.CalculateVertexPosition(VertexCorner.TopRight, TileCoords.X, TileCoords.Y,
                _scaledSizeX, _scaledSizeY);
            Vector2f _bottomRight = VertexMath.CalculateVertexPosition(VertexCorner.BottomRight, TileCoords.X,
                TileCoords.Y, _scaledSizeX, _scaledSizeY);
            Vector2f _bottomLeft = VertexMath.CalculateVertexPosition(VertexCorner.BottomLeft, TileCoords.X,
                TileCoords.Y,
                _scaledSizeX, _scaledSizeY);

            QuadCoords _textureCoords = GetTileTextureCoords();

            return new[]
            {
                new Vertex(_topLeft, _textureCoords.TopLeft),
                new Vertex(_topRight, _textureCoords.TopRight),
                new Vertex(_bottomRight, _textureCoords.BottomRight),
                new Vertex(_bottomLeft, _textureCoords.BottomLeft)
            };
        }

        private QuadCoords GetTileTextureCoords()
        {
            // width and height of all textures in a map will be the same
            int _actualPixelLeft = TileMetadata.TextureRect.Left * TileMetadata.TextureRect.Width;
            int _actualPixelTop = TileMetadata.TextureRect.Top * TileMetadata.TextureRect.Height;

            QuadCoords _finalCoords = new QuadCoords();

            switch (TilePrimitive.Rotation)
            {
                case 0:
                    _finalCoords.TopLeft = new Vector2f(_actualPixelLeft, _actualPixelTop);
                    _finalCoords.TopRight =
                        new Vector2f(_actualPixelLeft + TileMetadata.TextureRect.Width, _actualPixelTop);
                    _finalCoords.BottomRight = new Vector2f(_actualPixelLeft + TileMetadata.TextureRect.Width,
                        _actualPixelTop + TileMetadata.TextureRect.Height);
                    _finalCoords.BottomLeft =
                        new Vector2f(_actualPixelLeft, _actualPixelTop + TileMetadata.TextureRect.Height);
                    break;
                case 1:
                    _finalCoords.TopLeft =
                        new Vector2f(_actualPixelLeft + TileMetadata.TextureRect.Width, _actualPixelTop);
                    _finalCoords.TopRight = new Vector2f(_actualPixelLeft + TileMetadata.TextureRect.Width,
                        _actualPixelTop + TileMetadata.TextureRect.Height);
                    _finalCoords.BottomRight =
                        new Vector2f(_actualPixelLeft, _actualPixelTop + TileMetadata.TextureRect.Height);
                    _finalCoords.BottomLeft = new Vector2f(_actualPixelLeft, _actualPixelTop);
                    break;
                case 2:
                    _finalCoords.TopLeft = new Vector2f(_actualPixelLeft + TileMetadata.TextureRect.Width,
                        _actualPixelTop + TileMetadata.TextureRect.Height);
                    _finalCoords.TopRight =
                        new Vector2f(_actualPixelLeft, _actualPixelTop + TileMetadata.TextureRect.Height);
                    _finalCoords.BottomRight = new Vector2f(_actualPixelLeft, _actualPixelTop);
                    _finalCoords.BottomLeft =
                        new Vector2f(_actualPixelLeft + TileMetadata.TextureRect.Width, _actualPixelTop);
                    break;
                case 3:
                    _finalCoords.TopLeft =
                        new Vector2f(_actualPixelLeft, _actualPixelTop + TileMetadata.TextureRect.Height);
                    _finalCoords.TopRight = new Vector2f(_actualPixelLeft, _actualPixelTop);
                    _finalCoords.BottomRight =
                        new Vector2f(_actualPixelLeft + TileMetadata.TextureRect.Width, _actualPixelTop);
                    _finalCoords.BottomLeft = new Vector2f(_actualPixelLeft + TileMetadata.TextureRect.Width,
                        _actualPixelTop + TileMetadata.TextureRect.Height);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return _finalCoords;
        }
    }
}