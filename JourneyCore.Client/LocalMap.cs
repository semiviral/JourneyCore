using System.Linq;
using JourneyCore.Lib.Game.Environment.Mapping;
using JourneyCore.Lib.Game.Environment.Metadata;
using JourneyCore.Lib.Game.Environment.Tiling;
using JourneyCore.Lib.Graphics;
using JourneyCore.Lib.System;
using JourneyCore.Lib.System.Components.Loaders;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Client
{
    public class LocalMap
    {
        public byte[] Image { get; }
        public RenderStates RenderStates { get; }
        public MapMetadata Metadata { get; private set; }
        public VertexArray VArray { get; }
        public Minimap Minimap { get; }

        public LocalMap(byte[] mapImage)
        {
            Image = mapImage;
            RenderStates = new RenderStates(new Texture(Image));
            Metadata = new MapMetadata();
            VArray = new VertexArray(PrimitiveType.Quads);
            Minimap = new Minimap();
        }

        public void Update(MapMetadata mapMetadata)
        {
            VArray.Clear();
            VArray.Resize((uint)(mapMetadata.Width * mapMetadata.Height * 4 * mapMetadata.LayerCount));

            Minimap.VArray.Clear();
            Minimap.VArray.Resize((uint)(mapMetadata.Width * mapMetadata.Height * 4 * mapMetadata.LayerCount));

            Metadata = mapMetadata;
        }

        public void LoadChunk(Chunk chunk)
        {
            for (int x = 0; x < chunk.Length; x++)
            for (int y = 0; y < chunk[0].Length; y++)
            {
                AllocateTileToVArray(chunk[x][y],
                    new Vector2i(chunk.Left * MapLoader.ChunkSize + x, chunk.Top * MapLoader.ChunkSize + y),
                    chunk.Layer);
            }
        }


        #region MAP BUILDING

        private void AllocateTileToVArray(TilePrimitive tilePrimitive, Vector2i tileCoords, int layerId)
        {
            if (tilePrimitive.Gid == 0)
            {
                return;
            }

            TileMetadata tileMetadata = GetTileMetadata(tilePrimitive.Gid);

            int scaledSizeX = tileMetadata.TextureRect.Width * MapLoader.Scale;
            int scaledSizeY = tileMetadata.TextureRect.Height * MapLoader.Scale;

            Vector2f topLeft = GraphMath.CalculateVertexPosition(VertexCorner.TopLeft, tileCoords.X, tileCoords.Y,
                scaledSizeX, scaledSizeY);
            Vector2f topRight = GraphMath.CalculateVertexPosition(VertexCorner.TopRight, tileCoords.X, tileCoords.Y,
                scaledSizeX, scaledSizeY);
            Vector2f bottomRight = GraphMath.CalculateVertexPosition(VertexCorner.BottomRight, tileCoords.X,
                tileCoords.Y, scaledSizeX, scaledSizeY);
            Vector2f bottomLeft = GraphMath.CalculateVertexPosition(VertexCorner.BottomLeft, tileCoords.X, tileCoords.Y,
                scaledSizeX, scaledSizeY);

            QuadCoords textureCoords = GetTileTextureCoords(tilePrimitive);

            uint index = (uint)((tileCoords.Y * Metadata.Width + tileCoords.X) * 4 +
                                (layerId - 1) * (VArray.VertexCount / Metadata.LayerCount));

            VArray[index + 0] = new Vertex(topLeft, textureCoords.TopLeft);
            VArray[index + 1] = new Vertex(topRight, textureCoords.TopRight);
            VArray[index + 2] = new Vertex(bottomRight, textureCoords.BottomRight);
            VArray[index + 3] = new Vertex(bottomLeft, textureCoords.BottomLeft);

            Minimap.VArray[index + 0] = new Vertex(topLeft, tileMetadata.MiniMapColor);
            Minimap.VArray[index + 1] = new Vertex(topRight, tileMetadata.MiniMapColor);
            Minimap.VArray[index + 2] = new Vertex(bottomRight, tileMetadata.MiniMapColor);
            Minimap.VArray[index + 3] = new Vertex(bottomLeft, tileMetadata.MiniMapColor);
        }

        private TileMetadata GetTileMetadata(int gid)
        {
            return Metadata.TileSets.SelectMany(tileSet => tileSet.Tiles).Single(tile => tile.Gid == gid);
        }

        private QuadCoords GetTileTextureCoords(TilePrimitive tilePrimitive)
        {
            TileMetadata tileMetadata = GetTileMetadata(tilePrimitive.Gid);

            // width and height of all textures in a map will be the same
            int actualPixelLeft = tileMetadata.TextureRect.Left * tileMetadata.TextureRect.Width;
            int actualPixelTop = tileMetadata.TextureRect.Top * tileMetadata.TextureRect.Height;

            QuadCoords finalCoords = new QuadCoords();

            switch (tilePrimitive.Rotation)
            {
                case 0:
                    finalCoords.TopLeft = new Vector2f(actualPixelLeft, actualPixelTop);
                    finalCoords.TopRight =
                        new Vector2f(actualPixelLeft + tileMetadata.TextureRect.Width, actualPixelTop);
                    finalCoords.BottomRight = new Vector2f(actualPixelLeft + tileMetadata.TextureRect.Width,
                        actualPixelTop + tileMetadata.TextureRect.Height);
                    finalCoords.BottomLeft =
                        new Vector2f(actualPixelLeft, actualPixelTop + tileMetadata.TextureRect.Height);
                    break;
                case 1:
                    finalCoords.TopLeft =
                        new Vector2f(actualPixelLeft + tileMetadata.TextureRect.Width, actualPixelTop);
                    finalCoords.TopRight = new Vector2f(actualPixelLeft + tileMetadata.TextureRect.Width,
                        actualPixelTop + tileMetadata.TextureRect.Height);
                    finalCoords.BottomRight =
                        new Vector2f(actualPixelLeft, actualPixelTop + tileMetadata.TextureRect.Height);
                    finalCoords.BottomLeft = new Vector2f(actualPixelLeft, actualPixelTop);
                    break;
                case 2:
                    finalCoords.TopLeft = new Vector2f(actualPixelLeft + tileMetadata.TextureRect.Width,
                        actualPixelTop + tileMetadata.TextureRect.Height);
                    finalCoords.TopRight =
                        new Vector2f(actualPixelLeft, actualPixelTop + tileMetadata.TextureRect.Height);
                    finalCoords.BottomRight = new Vector2f(actualPixelLeft, actualPixelTop);
                    finalCoords.BottomLeft =
                        new Vector2f(actualPixelLeft + tileMetadata.TextureRect.Width, actualPixelTop);
                    break;
                case 3:
                    finalCoords.TopLeft =
                        new Vector2f(actualPixelLeft, actualPixelTop + tileMetadata.TextureRect.Height);
                    finalCoords.TopRight = new Vector2f(actualPixelLeft, actualPixelTop);
                    finalCoords.BottomRight =
                        new Vector2f(actualPixelLeft + tileMetadata.TextureRect.Width, actualPixelTop);
                    finalCoords.BottomLeft = new Vector2f(actualPixelLeft + tileMetadata.TextureRect.Width,
                        actualPixelTop + tileMetadata.TextureRect.Height);
                    break;
            }

            return finalCoords;
        }

        #endregion
    }
}