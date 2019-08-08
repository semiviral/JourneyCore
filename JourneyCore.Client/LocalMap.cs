using System;
using System.Linq;
using System.Threading;
using JourneyCore.Lib.Display;
using JourneyCore.Lib.Display.Component;
using JourneyCore.Lib.Game.Environment.Mapping;
using JourneyCore.Lib.Game.Environment.Metadata;
using JourneyCore.Lib.Game.Environment.Tiling;
using JourneyCore.Lib.System.Loaders;
using JourneyCore.Lib.System.Math;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace JourneyCore.Client
{
    public class LocalMap
    {
        private readonly Minimap _Minimap;
        private readonly VertexArray _VArray;

        public LocalMap(byte[] mapImage)
        {
            VArrayLock = MinimapVArrayLock = new object();

            Image = mapImage;
            RenderStates = new RenderStates(new Texture(Image));
            Metadata = new MapMetadata();
            _VArray = new VertexArray(PrimitiveType.Quads);
            _Minimap = new Minimap();
        }

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

        public bool IsHovered { get; private set; }

        public void Update(MapMetadata mapMetadata)
        {
            Metadata = mapMetadata;

            VArray.Clear();
            VArray.Resize((uint) (Metadata.Width * Metadata.Height * 4 * Metadata.LayerCount));

            Minimap.VArray.Clear();
            Minimap.VArray.Resize((uint) (Metadata.Width * Metadata.Height * 4 * Metadata.LayerCount));
        }

        public void LoadChunk(Chunk chunk)
        {
            ThreadPool.QueueUserWorkItem(state => LoadChunkTask(chunk));
        }

        private void LoadChunkTask(Chunk chunk)
        {
            for (int _x = 0; _x < chunk.Length; _x++)
            for (int _y = 0; _y < chunk[0].Length; _y++)
            {
                Vector2f _tileCoords = new Vector2f((chunk.Left * MapLoader.ChunkSize) + _x,
                    (chunk.Top * MapLoader.ChunkSize) + _y);

                AllocateTileToVArray(chunk[_x][_y], _tileCoords, chunk.Layer);
            }
        }

        public void UnloadChunk(Vector2f coordinates)
        {
            ThreadPool.QueueUserWorkItem(state => UnloadChunkTask(coordinates));
        }

        private void UnloadChunkTask(Vector2f coordinates)
        {
            coordinates *= MapLoader.ChunkSize;

            for (int _layer = 0; _layer < Metadata.LayerCount; _layer++)
            for (int _x = 0; _x < MapLoader.ChunkSize; _x++)
            for (int _y = 0; _y < MapLoader.ChunkSize; _y++)
            {
                uint _index = (uint) ((((((int) coordinates.Y + _y) * Metadata.Width) + (int) coordinates.X + _x) * 4) +
                                     (_layer * (VArray.VertexCount / Metadata.LayerCount)));

                VArray[_index + 0] = new Vertex();
                VArray[_index + 1] = new Vertex();
                VArray[_index + 2] = new Vertex();
                VArray[_index + 3] = new Vertex();
            }
        }

        #region EVENTS

        public event EventHandler<MouseMoveEventArgs> Entered;
        public event EventHandler<MouseMoveEventArgs> Exited;
        public event EventHandler<MouseWheelScrollEventArgs> Scrolled;

        public void OnParentResized(object sender, SizeEventArgs args)
        {
        }

        public void OnMouseMoved(object sender, MouseMoveEventArgs args)
        {
        }

        public void OnMouseScrolled(object sender, MouseWheelScrollEventArgs args)
        {
        }

        #endregion


        #region MAP BUILDING

        private void AllocateTileToVArray(TilePrimitive tilePrimitive, Vector2f tileCoords, int drawLayer)
        {
            if (tilePrimitive.Gid == 0)
            {
                return;
            }

            TileMetadata _tileMetadata = GetTileMetadata(tilePrimitive.Gid);

            int _scaledSizeX = _tileMetadata.TextureRect.Width * MapLoader.Scale;
            int _scaledSizeY = _tileMetadata.TextureRect.Height * MapLoader.Scale;

            Vector2f _topLeft = VertexMath.CalculateVertexPosition(VertexCorner.TopLeft, tileCoords.X, tileCoords.Y,
                _scaledSizeX, _scaledSizeY);
            Vector2f _topRight = VertexMath.CalculateVertexPosition(VertexCorner.TopRight, tileCoords.X, tileCoords.Y,
                _scaledSizeX, _scaledSizeY);
            Vector2f _bottomRight = VertexMath.CalculateVertexPosition(VertexCorner.BottomRight, tileCoords.X,
                tileCoords.Y, _scaledSizeX, _scaledSizeY);
            Vector2f _bottomLeft = VertexMath.CalculateVertexPosition(VertexCorner.BottomLeft, tileCoords.X,
                tileCoords.Y,
                _scaledSizeX, _scaledSizeY);

            QuadCoords _textureCoords = GetTileTextureCoords(tilePrimitive);

            uint _index = (uint) ((((tileCoords.Y * Metadata.Width) + tileCoords.X) * 4) +
                                 ((drawLayer - 1) * (VArray.VertexCount / Metadata.LayerCount)));

            VArray[_index + 0] = new Vertex(_topLeft, _textureCoords.TopLeft);
            VArray[_index + 1] = new Vertex(_topRight, _textureCoords.TopRight);
            VArray[_index + 2] = new Vertex(_bottomRight, _textureCoords.BottomRight);
            VArray[_index + 3] = new Vertex(_bottomLeft, _textureCoords.BottomLeft);

            Minimap.VArray[_index + 0] = new Vertex(_topLeft, _tileMetadata.MiniMapColor);
            Minimap.VArray[_index + 1] = new Vertex(_topRight, _tileMetadata.MiniMapColor);
            Minimap.VArray[_index + 2] = new Vertex(_bottomRight, _tileMetadata.MiniMapColor);
            Minimap.VArray[_index + 3] = new Vertex(_bottomLeft, _tileMetadata.MiniMapColor);
        }

        private TileMetadata GetTileMetadata(int gid)
        {
            return Metadata.TileSets.SelectMany(tileSet => tileSet.Tiles).SingleOrDefault(tile => tile.Gid == gid);
        }

        private QuadCoords GetTileTextureCoords(TilePrimitive tilePrimitive)
        {
            TileMetadata _tileMetadata = GetTileMetadata(tilePrimitive.Gid);

            // width and height of all textures in a map will be the same
            int _actualPixelLeft = _tileMetadata.TextureRect.Left * _tileMetadata.TextureRect.Width;
            int _actualPixelTop = _tileMetadata.TextureRect.Top * _tileMetadata.TextureRect.Height;

            QuadCoords _finalCoords = new QuadCoords();

            switch (tilePrimitive.Rotation)
            {
                case 0:
                    _finalCoords.TopLeft = new Vector2f(_actualPixelLeft, _actualPixelTop);
                    _finalCoords.TopRight =
                        new Vector2f(_actualPixelLeft + _tileMetadata.TextureRect.Width, _actualPixelTop);
                    _finalCoords.BottomRight = new Vector2f(_actualPixelLeft + _tileMetadata.TextureRect.Width,
                        _actualPixelTop + _tileMetadata.TextureRect.Height);
                    _finalCoords.BottomLeft =
                        new Vector2f(_actualPixelLeft, _actualPixelTop + _tileMetadata.TextureRect.Height);
                    break;
                case 1:
                    _finalCoords.TopLeft =
                        new Vector2f(_actualPixelLeft + _tileMetadata.TextureRect.Width, _actualPixelTop);
                    _finalCoords.TopRight = new Vector2f(_actualPixelLeft + _tileMetadata.TextureRect.Width,
                        _actualPixelTop + _tileMetadata.TextureRect.Height);
                    _finalCoords.BottomRight =
                        new Vector2f(_actualPixelLeft, _actualPixelTop + _tileMetadata.TextureRect.Height);
                    _finalCoords.BottomLeft = new Vector2f(_actualPixelLeft, _actualPixelTop);
                    break;
                case 2:
                    _finalCoords.TopLeft = new Vector2f(_actualPixelLeft + _tileMetadata.TextureRect.Width,
                        _actualPixelTop + _tileMetadata.TextureRect.Height);
                    _finalCoords.TopRight =
                        new Vector2f(_actualPixelLeft, _actualPixelTop + _tileMetadata.TextureRect.Height);
                    _finalCoords.BottomRight = new Vector2f(_actualPixelLeft, _actualPixelTop);
                    _finalCoords.BottomLeft =
                        new Vector2f(_actualPixelLeft + _tileMetadata.TextureRect.Width, _actualPixelTop);
                    break;
                case 3:
                    _finalCoords.TopLeft =
                        new Vector2f(_actualPixelLeft, _actualPixelTop + _tileMetadata.TextureRect.Height);
                    _finalCoords.TopRight = new Vector2f(_actualPixelLeft, _actualPixelTop);
                    _finalCoords.BottomRight =
                        new Vector2f(_actualPixelLeft + _tileMetadata.TextureRect.Width, _actualPixelTop);
                    _finalCoords.BottomLeft = new Vector2f(_actualPixelLeft + _tileMetadata.TextureRect.Width,
                        _actualPixelTop + _tileMetadata.TextureRect.Height);
                    break;
            }

            return _finalCoords;
        }

        #endregion
    }
}