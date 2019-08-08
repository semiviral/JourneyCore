using System.IO;
using System.Text;
using JourneyCore.Lib.Game.Environment.Tiling;
using Newtonsoft.Json;
using SFML.Graphics;

namespace JourneyCore.Lib.System.Loaders
{
    public class TileSetLoader
    {
        public static TileSet LoadTileSet(string tileSetPath, int firstGid)
        {
            using (StreamReader _reader = new StreamReader(tileSetPath, Encoding.UTF8))
            {
                TileSet _tileSet = JsonConvert.DeserializeObject<TileSet>(_reader.ReadToEnd());
                _tileSet.TextureName = Path.GetFileNameWithoutExtension(_tileSet.Image);

                if (_tileSet.Tiles == null)
                {
                    return _tileSet;
                }

                _tileSet.Tiles.ForEach(tile => BuildTile(_tileSet, tile, firstGid));

                return _tileSet;
            }
        }

        private static void BuildTile(TileSet tileSet, Tile tile, int firstGid)
        {
            tile.Gid = tile.Id + firstGid;
            tile.TextureRect = new IntRect(tile.Id % tileSet.Columns,
                tile.Id / tileSet.Columns,
                tileSet.TileWidth, tileSet.TileHeight);

            tile.ApplyProperties();
        }
    }
}