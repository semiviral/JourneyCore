using System.IO;
using System.Text;
using JourneyCore.Lib.Game.Environment.Tiling;
using Newtonsoft.Json;
using SFML.Graphics;

namespace JourneyCore.Lib.System.Components.Loaders
{
    public class TileSetLoader
    {
        public static TileSet LoadTileSet(string tileSetPath, int firstGid)
        {
            using (StreamReader reader = new StreamReader(tileSetPath, Encoding.UTF8))
            {
                TileSet tileSet = JsonConvert.DeserializeObject<TileSet>(reader.ReadToEnd());
                tileSet.TextureName = Path.GetFileNameWithoutExtension(tileSet.Image);

                if (tileSet.Tiles == null)
                {
                    return tileSet;
                }

                tileSet.Tiles.ForEach(tile => BuildTile(tileSet, tile, firstGid));

                return tileSet;
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