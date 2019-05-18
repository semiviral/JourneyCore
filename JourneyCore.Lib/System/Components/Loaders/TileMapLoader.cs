using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using JourneyCore.Lib.Graphics.Rendering.Environment.Chunking;
using JourneyCore.Lib.Graphics.Rendering.Environment.Tiling;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Lib.System.Components.Loaders
{
    public static class TileMapLoader
    {
        static TileMapLoader() { }

        public static TileMap LoadMap(string mapPath, int tileScale)
        {
            XmlSerializer mapSerializer = new XmlSerializer(typeof(TileMap));

            using (StreamReader reader = new StreamReader(mapPath, Encoding.UTF8))
            {
                TileMap map = (TileMap)mapSerializer.Deserialize(reader);
                map.ScaledTilePixelWidth = map.PixelTileWidth * tileScale;
                map.ScaledTilePixelHeight = map.PixelTileHeight * tileScale;
                map.PixelWidth = map.Width * map.ScaledTilePixelWidth;
                map.PixelHeight = map.Height * map.ScaledTilePixelHeight;
                map.TileSet = TileSetLoader.LoadTileSet($@"{Path.GetDirectoryName(mapPath)}\TileSets\{Path.GetFileName(map.TileSetSource.Source)}");

                return map;
            }
        }

        public static TileMap BuildChunkMap(TileMap map, Vector2i chunkSize)
        {
            map.VArray = new VertexArray(PrimitiveType.Quads);
            map.VArray.Resize((uint)((map.Width * map.Height * 4 + 1) * map.Layers.Count + 1));

            foreach (TileMapLayer layer in map.Layers)
            {
                string[] layerDataArray = layer.Data.Replace("\r\n", "\n").Replace("\n", ",").Split(',', StringSplitOptions.RemoveEmptyEntries);
                int layerChunkWidth = layer.Width / map.ChunkWidth;
                int layerChunkHeight = layer.Height / map.ChunkHeight;

                layer.ChunkMap = new Chunk[layerChunkWidth][];

                for (int chunkX = 0; chunkX < map.ChunkWidth; chunkX++)
                {
                    layer.ChunkMap[chunkX] = new Chunk[layerChunkHeight];

                    for (int chunkY = 0; chunkY < layerChunkHeight; chunkY++)
                    {
                        Chunk currentChunk = new Chunk(new int[chunkSize.X][]);

                        for (int x = 0; x < chunkSize.X; x++)
                        {
                            currentChunk.ChunkData[x] = new int[chunkSize.Y];

                            for (int y = 0; y < chunkSize.Y; y++)
                            {
                                currentChunk.ChunkData[x][y] = int.Parse(layerDataArray[layer.Width * (y + chunkY * map.ChunkHeight) + x + chunkX * map.ChunkWidth]);
                            }
                        }

                        layer.ChunkMap[chunkX][chunkY] = currentChunk;
                    }
                }
            }

            return map;
        }
    }
}
