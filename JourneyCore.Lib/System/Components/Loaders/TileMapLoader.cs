using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using JourneyCore.Lib.Graphics.Rendering.Environment.Chunking;
using JourneyCore.Lib.Graphics.Rendering.Environment.Tiling;

namespace JourneyCore.Lib.System.Components.Loaders
{
    public static class TileMapLoader
    {
        public const short ChunkSize = 8;
        public const short ChunkLoadRadius = 3;
        public static short Scale = 1;

        public static TileMap LoadMap(string mapPath, short tileScale)
        {
            Scale = tileScale;

            XmlSerializer mapSerializer = new XmlSerializer(typeof(TileMap));

            using (StreamReader reader = new StreamReader(mapPath, Encoding.UTF8))
            {
                return (TileMap) mapSerializer.Deserialize(reader);
                ;
            }
        }

        public static TileMap BuildChunkMap(TileMap map)
        {
            for (int layer = 0; layer < map.Layers.Length; layer++)
            {
                string[] layerDataArray = map.Layers[layer].Data.Replace("\r\n", "\n").Replace("\n", ",")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries);
                int layerChunkWidth = map.Layers[layer].Width / ChunkSize;
                int layerChunkHeight = map.Layers[layer].Height / ChunkSize;

                map.Layers[layer].ChunkMap = new Chunk[layerChunkWidth][];

                for (int chunkX = 0; chunkX < layerChunkWidth; chunkX++)
                {
                    map.Layers[layer].ChunkMap[chunkX] = new Chunk[layerChunkHeight];

                    for (int chunkY = 0; chunkY < layerChunkHeight; chunkY++)
                    {
                        Chunk currentChunk = new Chunk(new short[ChunkSize][]);

                        for (int x = 0; x < ChunkSize; x++)
                        {
                            currentChunk.ChunkData[x] = new short[ChunkSize];

                            for (int y = 0; y < ChunkSize; y++)
                                currentChunk.ChunkData[x][y] =
                                    short.Parse(
                                        layerDataArray[
                                            map.Layers[layer].Width * (y + chunkY * ChunkSize) + x + chunkX * ChunkSize]);
                        }

                        map.Layers[layer].ChunkMap[chunkX][chunkY] = currentChunk;
                    }
                }
            }

            return map;
        }
    }
}