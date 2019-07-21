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
            int widthInChunks = Width / chunkSizeX;
            int heightInChunks = Height / chunkSizeY;

            Map = new Chunk[widthInChunks][];

            for (int x = 0; x < widthInChunks; x++)
            {
                Map[x] = new Chunk[heightInChunks];
            }

            for (int chunkX = 0; chunkX < widthInChunks; chunkX++)
            for (int chunkY = 0; chunkY < heightInChunks; chunkY++)
            {
                Chunk newChunk = new Chunk((short) MapLoader.ChunkSize, (short) MapLoader.ChunkSize,
                    chunkX, chunkY, Id);

                for (int x = 0; x < MapLoader.ChunkSize; x++)
                for (int y = 0; y < MapLoader.ChunkSize; y++)
                {
                    newChunk[x][y] = new TilePrimitive(
                        Data[(((chunkY * MapLoader.ChunkSize) + y) * Width) + (chunkX * MapLoader.ChunkSize) + x],
                        0);
                }

                Map[chunkX][chunkY] = newChunk;
            }

            return Map;
        }
    }
}