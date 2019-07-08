using System.IO;
using System.Text;
using JourneyCore.Lib.Game.Environment.Mapping;
using Newtonsoft.Json;

namespace JourneyCore.Lib.System.Loaders
{
    public static class MapLoader
    {
        public const string AssetRoot = @"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\Assets";

        public static int ChunkSize = 16;
        public static int Scale = 1;
        public static int TileSize = 16;

        public static int TilePixelSize { get; private set; }

        static MapLoader()
        {
            TilePixelSize = TileSize * Scale;
        }

        public static Map LoadMap(string mapPath, short tileScale)
        {
            Scale = tileScale;
            TilePixelSize = TileSize * Scale;

            using (StreamReader reader = new StreamReader(mapPath, Encoding.UTF8))
            {
                Map map = JsonConvert.DeserializeObject<Map>(reader.ReadToEnd());
                map.Name = Path.GetFileNameWithoutExtension(mapPath);

                map.LoadTileSets();
                map.BuildMap();
                map.ProcessTileEffects();
                map.ApplyProperties();

                return map;
            }
        }
    }
}