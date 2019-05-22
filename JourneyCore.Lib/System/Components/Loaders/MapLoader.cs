using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using JourneyCore.Lib.Graphics.Rendering.Environment.Tiling;

namespace JourneyCore.Lib.System.Components.Loaders
{
    public static class MapLoader
    {
        private static Random Rand { get; }

        public static int ChunkSize = 8;
        public static int Scale = 1;
        public static int TileSize = 16;
        public static int PixelTileWidth => Scale * TileSize;

        static MapLoader()
        {
            Rand = new Random();
        }

        public static Map LoadMap(string mapPath, short tileScale)
        {
            Scale = tileScale;

            XmlSerializer mapSerializer = new XmlSerializer(typeof(Map));

            using (StreamReader reader = new StreamReader(mapPath, Encoding.UTF8))
            {
                Map map = (Map)mapSerializer.Deserialize(reader);

                map.LoadTileSets();
                map.BuildMap();
                map.ProcessTileEffects();

                return map;
            }
        }
    }
}