using System.IO;
using System.Text;
using JourneyCore.Lib.Game.Environment.Mapping;
using Newtonsoft.Json;

namespace JourneyCore.Lib.System.Loaders
{
    public static class MapLoader
    {
        public const string ASSET_ROOT = @"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\Assets";

        public static int ChunkSize = 16;
        public static int Scale = 1;
        public static int TileSize = 16;

        static MapLoader()
        {
            TilePixelSize = TileSize * Scale;
        }

        public static int TilePixelSize { get; private set; }

        public static Map LoadMap(string mapPath, short tileScale)
        {
            Scale = tileScale;
            TilePixelSize = TileSize * Scale;

            using (StreamReader _reader = new StreamReader(mapPath, Encoding.UTF8))
            {
                Map _map = JsonConvert.DeserializeObject<Map>(_reader.ReadToEnd());
                _map.Name = Path.GetFileNameWithoutExtension(mapPath);

                _map.LoadTileSets();
                _map.BuildMap();
                _map.ProcessTiles();
                _map.ApplyProperties();

                return _map;
            }
        }
    }
}