using JourneyCore.Lib.Game.Environment.Tiling;
using JourneyCore.Lib.System.Loaders;

namespace JourneyCore.Lib.Game.Environment.Mapping
{
    public class MapLayer
    {
        public short Id { get; set; }
        public string Name { get; set; }
        public short Width { get; set; }
        public short Height { get; set; }
        public int[] Data { get; set; }
        public Chunk[][] Map { get; set; }

        public Chunk[][] CreateMap(short chunkSizeX, short chunkSizeY)
        {
            int _widthInChunks = Width / chunkSizeX;
            int _heightInChunks = Height / chunkSizeY;

            Map = new Chunk[_widthInChunks][];

            for (int _x = 0; _x < _widthInChunks; _x++)
            {
                Map[_x] = new Chunk[_heightInChunks];
            }

            for (int _chunkX = 0; _chunkX < _widthInChunks; _chunkX++)
            for (int _chunkY = 0; _chunkY < _heightInChunks; _chunkY++)
            {
                Chunk _newChunk = new Chunk((short) MapLoader.ChunkSize, (short) MapLoader.ChunkSize,
                    _chunkX, _chunkY, Id);

                for (int _x = 0; _x < MapLoader.ChunkSize; _x++)
                for (int _y = 0; _y < MapLoader.ChunkSize; _y++)
                {
                    _newChunk[_x][_y] = new TilePrimitive(
                        Data[(((_chunkY * MapLoader.ChunkSize) + _y) * Width) + (_chunkX * MapLoader.ChunkSize) + _x],
                        0);
                }

                Map[_chunkX][_chunkY] = _newChunk;
            }

            return Map;
        }
    }
}