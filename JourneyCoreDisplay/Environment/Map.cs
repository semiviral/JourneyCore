using JourneyCoreDisplay.Drawing;
using JourneyCoreDisplay.Sprites;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;

namespace JourneyCoreDisplay.Environment
{
    public class Map
    {
        // todo - make the file location dynamic
        public static Texture MapTextures { get; } = new Texture(@"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\JourneyCoreGame\Assets\Images\Sprites\MapSpriteSheet.png");

        private int[,] _map;

        /// <summary>
        ///     Size of chunk in tiles
        /// </summary>
        public Vector2i SizeInTiles { get; }

        /// <summary>
        ///     Map size in pixels
        /// </summary>
        public Vector2i SizeInPixels { get; }

        /// <summary>
        ///     Size of each tile in pixels
        /// </summary>
        public Vector2i TileSize { get; }



        public VertexArray VArray { get; private set; }

        public Map(Vector2i size, Vector2i tileSize, List<int> mapCodes, int scale)
        {
            SizeInTiles = size;
            TileSize = tileSize * scale;
            SizeInPixels = new Vector2i(SizeInTiles.X * TileSize.X, SizeInTiles.Y * TileSize.Y) * scale;


            VArray = new VertexArray(PrimitiveType.Quads);
            VArray.Resize((uint)((SizeInTiles.X * SizeInTiles.Y * 4) + 1));

            _map = new int[SizeInPixels.X, SizeInPixels.Y];
            ReadCodesIntoMap(mapCodes);
            ConstructMap();
        }

        private void ConstructMap()
        {
            for (int x = 0; x < SizeInTiles.X; x++)
            {
                for (int y = 0; y < SizeInTiles.Y; y++)
                {
                    IntRect currentTile = SpriteLoader.LoadedSprites[GetCoordinate(x, y)];

                    Vector2f topLeft = MathOps.CalculateVertexPosition(VertexCorner.TopLeft, x, y, TileSize.X, TileSize.Y);
                    Vector2f topRight = MathOps.CalculateVertexPosition(VertexCorner.TopRight, x, y, TileSize.X, TileSize.Y);
                    Vector2f bottomRight = MathOps.CalculateVertexPosition(VertexCorner.BottomRight, x, y, TileSize.X, TileSize.Y);
                    Vector2f bottomLeft = MathOps.CalculateVertexPosition(VertexCorner.BottomLeft, x, y, TileSize.X, TileSize.Y);

                    uint index = (uint)((x + y * SizeInTiles.X) * 4);

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

        private void ReadCodesIntoMap(List<int> mapCodes)
        {
            int i = 0;

            for (int y = 0; y < SizeInTiles.Y; y++)
            {
                for (int x = 0; x < SizeInTiles.X; x++)
                {
                    if (i >= mapCodes.Count)
                    {
                        break;
                    }

                    _map[x, y] = mapCodes[i];
                    i += 1;
                }
            }
        }

        public int GetCoordinate(int x, int y)
        {
            if (_map.GetLength(0) <= x || _map.GetLength(1) <= y)
            {
                return -1;
            }

            return _map[x, y];
        }
    }
}
