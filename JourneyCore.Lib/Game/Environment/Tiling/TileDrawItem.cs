using System;
using JourneyCore.Lib.Game.Environment.Metadata;
using JourneyCore.Lib.Graphics;
using JourneyCore.Lib.System;
using JourneyCore.Lib.System.Components.Loaders;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Lib.Game.Environment.Tiling
{
    public class TileDrawItem
    {
        private TilePrimitive TilePrimitive { get; }
        private TileMetadata TileMetadata { get; }
        public Vector2i TileCoords { get; }

        public TileDrawItem(TilePrimitive tilePrimitive, TileMetadata tileMetadata, Vector2i tileCoords)
        {
            TilePrimitive = tilePrimitive;
            TileMetadata = tileMetadata;
            TileCoords = tileCoords;
        }

        public Vertex[] GetVertices()
        {
            int scaledSizeX = TileMetadata.TextureRect.Width * MapLoader.Scale;
            int scaledSizeY = TileMetadata.TextureRect.Height * MapLoader.Scale;

            Vector2f topLeft = VertexMath.CalculateVertexPosition(VertexCorner.TopLeft, TileCoords.X, TileCoords.Y,
                scaledSizeX, scaledSizeY);
            Vector2f topRight = VertexMath.CalculateVertexPosition(VertexCorner.TopRight, TileCoords.X, TileCoords.Y,
                scaledSizeX, scaledSizeY);
            Vector2f bottomRight = VertexMath.CalculateVertexPosition(VertexCorner.BottomRight, TileCoords.X,
                TileCoords.Y, scaledSizeX, scaledSizeY);
            Vector2f bottomLeft = VertexMath.CalculateVertexPosition(VertexCorner.BottomLeft, TileCoords.X, TileCoords.Y,
                scaledSizeX, scaledSizeY);

            QuadCoords textureCoords = GetTileTextureCoords();

            return new[]
            {
                new Vertex(topLeft, textureCoords.TopLeft),
                new Vertex(topRight, textureCoords.TopRight),
                new Vertex(bottomRight, textureCoords.BottomRight),
                new Vertex(bottomLeft, textureCoords.BottomLeft)
            };
        }

        private QuadCoords GetTileTextureCoords()
        {
            // width and height of all textures in a map will be the same
            int actualPixelLeft = TileMetadata.TextureRect.Left * TileMetadata.TextureRect.Width;
            int actualPixelTop = TileMetadata.TextureRect.Top * TileMetadata.TextureRect.Height;

            QuadCoords finalCoords = new QuadCoords();

            switch (TilePrimitive.Rotation)
            {
                case 0:
                    finalCoords.TopLeft = new Vector2f(actualPixelLeft, actualPixelTop);
                    finalCoords.TopRight =
                        new Vector2f(actualPixelLeft + TileMetadata.TextureRect.Width, actualPixelTop);
                    finalCoords.BottomRight = new Vector2f(actualPixelLeft + TileMetadata.TextureRect.Width,
                        actualPixelTop + TileMetadata.TextureRect.Height);
                    finalCoords.BottomLeft =
                        new Vector2f(actualPixelLeft, actualPixelTop + TileMetadata.TextureRect.Height);
                    break;
                case 1:
                    finalCoords.TopLeft =
                        new Vector2f(actualPixelLeft + TileMetadata.TextureRect.Width, actualPixelTop);
                    finalCoords.TopRight = new Vector2f(actualPixelLeft + TileMetadata.TextureRect.Width,
                        actualPixelTop + TileMetadata.TextureRect.Height);
                    finalCoords.BottomRight =
                        new Vector2f(actualPixelLeft, actualPixelTop + TileMetadata.TextureRect.Height);
                    finalCoords.BottomLeft = new Vector2f(actualPixelLeft, actualPixelTop);
                    break;
                case 2:
                    finalCoords.TopLeft = new Vector2f(actualPixelLeft + TileMetadata.TextureRect.Width,
                        actualPixelTop + TileMetadata.TextureRect.Height);
                    finalCoords.TopRight =
                        new Vector2f(actualPixelLeft, actualPixelTop + TileMetadata.TextureRect.Height);
                    finalCoords.BottomRight = new Vector2f(actualPixelLeft, actualPixelTop);
                    finalCoords.BottomLeft =
                        new Vector2f(actualPixelLeft + TileMetadata.TextureRect.Width, actualPixelTop);
                    break;
                case 3:
                    finalCoords.TopLeft =
                        new Vector2f(actualPixelLeft, actualPixelTop + TileMetadata.TextureRect.Height);
                    finalCoords.TopRight = new Vector2f(actualPixelLeft, actualPixelTop);
                    finalCoords.BottomRight =
                        new Vector2f(actualPixelLeft + TileMetadata.TextureRect.Width, actualPixelTop);
                    finalCoords.BottomLeft = new Vector2f(actualPixelLeft + TileMetadata.TextureRect.Width,
                        actualPixelTop + TileMetadata.TextureRect.Height);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return finalCoords;
        }
    }
}