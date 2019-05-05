using JourneyCoreDisplay.Drawing;
using JourneyCoreDisplay.Sprites;
using SFML.Graphics;
using SFML.System;
using System;

namespace JourneyCoreDisplay.Environment
{
    public class Chunk
    {
        // vArr Vertexes order:
        // TL, TR, BR, BL

        /// <summary>
        ///     Size of chunk in pixels
        /// </summary>
        public Vector2i Size { get; }

        /// <summary>
        ///     Size of each tile in pixels
        /// </summary>
        public Vector2i TileSize { get; }

        /// <summary>
        ///     Size of chunk in tiles
        /// </summary>
        public Vector2i TiledSize { get; }

        public VertexArray VArray { get; private set; }

        public Chunk(Vector2i size, Vector2i tileSize, Vector2i topLeftPoint, Map map, int scale = 1)
        {
            Size = size * scale;
            TileSize = tileSize * scale;
            TiledSize = new Vector2i(Size.X / TileSize.X, Size.Y / TileSize.Y);

            VArray = new VertexArray(PrimitiveType.Quads);
            VArray.Resize((uint)((TiledSize.X * TiledSize.Y * 4) + 1));

            PopulateChunk(map, topLeftPoint);
        }

        public Chunk(int width, int height, int tileWidth, int tileHeight, Map map, int texStartX, int texStartY) :
            this(new Vector2i(width, height), new Vector2i(tileWidth, tileHeight), new Vector2i(texStartX, texStartY), map)
        { }

        private void PopulateChunk(Map map, Vector2i topLeftPoint)
        {
            for (int x = 0; x < TiledSize.X; x++)
            {
                for (int y = 0; y < TiledSize.Y; y++)
                {
                    IntRect currentTile = SpriteLoader.LoadedSprites[map.GetCoordinate(x + topLeftPoint.X, y + topLeftPoint.Y)];

                    Vector2f topLeft = MathOps.CalculateVertexPosition(VertexCorner.TopLeft, x, y, TileSize.X, TileSize.Y);
                    Vector2f topRight = MathOps.CalculateVertexPosition(VertexCorner.TopRight, x, y, TileSize.X, TileSize.Y);
                    Vector2f bottomRight = MathOps.CalculateVertexPosition(VertexCorner.BottomRight, x, y, TileSize.X, TileSize.Y);
                    Vector2f bottomLeft = MathOps.CalculateVertexPosition(VertexCorner.BottomLeft, x, y, TileSize.X, TileSize.Y);

                    uint index = (uint)((x + y * TiledSize.X) * 4);

                    // Top left
                    VArray[index + 0] = new Vertex(topLeft, new Vector2f(currentTile.Left, currentTile.Top));
                    // Top right
                    VArray[index + 1] = new Vertex(topRight, new Vector2f(currentTile.Left + currentTile.Width, currentTile.Top));
                    // Bottom right
                    VArray[index + 2] = new Vertex(bottomRight, new Vector2f(currentTile.Left + currentTile.Width, currentTile.Top + currentTile.Height));
                    // Bottom left
                    VArray[index + 3] = new Vertex(bottomLeft, new Vector2f(currentTile.Left, currentTile.Top + currentTile.Height));
                }
            }
        }
    }
}
