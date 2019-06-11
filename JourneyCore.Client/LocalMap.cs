using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JourneyCore.Lib.Display;
using JourneyCore.Lib.Game.Environment.Mapping;
using JourneyCore.Lib.Game.Environment.Metadata;
using JourneyCore.Lib.Game.Environment.Tiling;
using JourneyCore.Lib.Game.Object;
using JourneyCore.Lib.System;
using JourneyCore.Lib.System.Components.Loaders;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Client
{
    public class LocalMap
    {
        private readonly Minimap _Minimap;
        private readonly VertexArray _VArray;
        private object VArrayLock { get; }
        private object MinimapVArrayLock { get; }

        public byte[] Image { get; }
        public RenderStates RenderStates { get; }
        public MapMetadata Metadata { get; private set; }

        public VertexArray VArray
        {
            get
            {
                lock (VArrayLock)
                {
                    return _VArray;
                }
            }
        }

        public Minimap Minimap
        {
            get
            {
                lock (MinimapVArrayLock)
                {
                    return _Minimap;
                }
            }
        }

        public List<ICollidable> CollisionObjects { get; }

        public LocalMap(byte[] mapImage)
        {
            VArrayLock = MinimapVArrayLock = new object();

            Image = mapImage;
            RenderStates = new RenderStates(new Texture(Image));
            Metadata = new MapMetadata();
            _VArray = new VertexArray(PrimitiveType.Quads);
            _Minimap = new Minimap();
            CollisionObjects = new List<ICollidable>();
        }

        public void Update(MapMetadata mapMetadata)
        {
            Metadata = mapMetadata;

            VArray.Clear();
            VArray.Resize((uint)(Metadata.Width * Metadata.Height * 4 * Metadata.LayerCount));

            Minimap.VArray.Clear();
            Minimap.VArray.Resize((uint)(Metadata.Width * Metadata.Height * 4 * Metadata.LayerCount));
        }

        public void LoadChunk(Chunk chunk)
        {
            Thread chunkLoadingThread = new Thread(() => LoadChunkThreaded(chunk));
            chunkLoadingThread.Start();
        }

        private void LoadChunkThreaded(Chunk chunk)
        {
            for (int x = 0; x < chunk.Length; x++)
            for (int y = 0; y < chunk[0].Length; y++)
            {
                Vector2f tileCoords = new Vector2f(chunk.Left * MapLoader.ChunkSize + x,
                    chunk.Top * MapLoader.ChunkSize + y);

                ProcessCollisions(chunk[x][y], tileCoords);

                AllocateTileToVArray(chunk[x][y], tileCoords, chunk.Layer);
            }
        }


        #region MAP BUILDING

        private void ProcessCollisions(TilePrimitive tilePrimitive, Vector2f tileCoords)
        {
            TileMetadata tileMetadata = GetTileMetadata(tilePrimitive.Gid);

            if (tileMetadata?.Collidables == null)
            {
                return;
            }

            foreach (CollisionBox collidable in tileMetadata.Collidables)
            {
                CollisionObjects.Add(new CollisionBox(collidable)
                {
                    Position = new Vector2f(tileCoords.X * MapLoader.TilePixelSize + collidable.Position.X,
                        tileCoords.Y * MapLoader.TilePixelSize + collidable.Position.Y)
                });
            }
        }

        private void AllocateTileToVArray(TilePrimitive tilePrimitive, Vector2f tileCoords, int drawLayer)
        {
            if (tilePrimitive.Gid == 0)
            {
                return;
            }

            TileMetadata tileMetadata = GetTileMetadata(tilePrimitive.Gid);

            int scaledSizeX = tileMetadata.TextureRect.Width * MapLoader.Scale;
            int scaledSizeY = tileMetadata.TextureRect.Height * MapLoader.Scale;

            Vector2f topLeft = VertexMath.CalculateVertexPosition(VertexCorner.TopLeft, tileCoords.X, tileCoords.Y,
                scaledSizeX, scaledSizeY);
            Vector2f topRight = VertexMath.CalculateVertexPosition(VertexCorner.TopRight, tileCoords.X, tileCoords.Y,
                scaledSizeX, scaledSizeY);
            Vector2f bottomRight = VertexMath.CalculateVertexPosition(VertexCorner.BottomRight, tileCoords.X,
                tileCoords.Y, scaledSizeX, scaledSizeY);
            Vector2f bottomLeft = VertexMath.CalculateVertexPosition(VertexCorner.BottomLeft, tileCoords.X,
                tileCoords.Y,
                scaledSizeX, scaledSizeY);

            QuadCoords textureCoords = GetTileTextureCoords(tilePrimitive);

            uint index = (uint)((tileCoords.Y * Metadata.Width + tileCoords.X) * 4 +
                                (drawLayer - 1) * (VArray.VertexCount / Metadata.LayerCount));

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
            return Metadata.TileSets.SelectMany(tileSet => tileSet.Tiles).SingleOrDefault(tile => tile.Gid == gid);
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