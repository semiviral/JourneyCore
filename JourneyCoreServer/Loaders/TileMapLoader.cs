using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using JourneyCoreLib.Rendering.Environment.Chunking;
using JourneyCoreLib.Rendering.Environment.Tiling;
using SFML.Graphics;
using SFML.System;

namespace JourneyCoreServer.Loaders
{
    public static class TileMapLoader
    {
        public static float TileScale { get; set; }

        public static Vector2i ChunkSize { get; set; }

        public static List<TileMap> LoadedMaps { get; }

        static TileMapLoader()
        {
            LoadedMaps = new List<TileMap>();
        }

        public static TileMap LoadMap(string mapName)
        {
            XmlSerializer mapSerializer = new XmlSerializer(typeof(TileMap));

            using (StreamReader reader = new StreamReader($@"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\JourneyCoreGame\Assets\Maps\{mapName}.xml", Encoding.UTF8))
            {
                TileMap map = (TileMap)mapSerializer.Deserialize(reader);
                map.ScaledTilePixelWidth = (int)(map.PixelTileWidth * TileScale);
                map.ScaledTilePixelHeight = (int)(map.PixelTileHeight * TileScale);
                map.PixelWidth = map.Width * map.ScaledTilePixelWidth;
                map.PixelHeight = map.Height * map.ScaledTilePixelHeight;
                map.VArray = new VertexArray(PrimitiveType.Quads);
                map.VArray.Resize((uint)((map.Width * map.Height * 4 + 1) * map.Layers.Count + 1));

                foreach (TileMapLayer layer in map.Layers)
                {
                    BuildChunkMap(map, layer);
                }

                return map;
            }
        }

        private static TileMap BuildChunkMap(TileMap map, TileMapLayer layer)
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
                    Chunk currentChunk = new Chunk(new int[ChunkSize.X][]);

                    for (int x = 0; x < ChunkSize.X; x++)
                    {
                        currentChunk.ChunkData[x] = new int[ChunkSize.Y];

                        for (int y = 0; y < ChunkSize.Y; y++)
                        {
                            currentChunk.ChunkData[x][y] = int.Parse(layerDataArray[layer.Width * (y + chunkY * map.ChunkHeight) + x + chunkX * map.ChunkWidth]);
                        }
                    }

                    layer.ChunkMap[chunkX][chunkY] = currentChunk;
                }
            }

            return map;
        }
    }
}
