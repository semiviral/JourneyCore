using System.IO;
using System.Text;
using System.Xml.Serialization;
using JourneyCore.Lib.Graphics.Rendering.Environment.Tiling;
using SFML.Graphics;

namespace JourneyCore.Lib.System.Components.Loaders
{
    public class TileSetLoader
    {
        public static TileSet LoadTileSet(string filePath, short firstGid)
        {
            TileSet tileSet;

            XmlSerializer sheetSerializer = new XmlSerializer(typeof(TileSet));
            using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
            {
                tileSet = (TileSet)sheetSerializer.Deserialize(reader);

                foreach (Tile tile in tileSet.Tiles)
                {
                    tile.Gid = (short)(tile.Id + firstGid);
                    tile.TextureRect = new IntRect(tile.Id % tileSet.Columns,
                        tile.Id / tileSet.Columns,
                        tileSet.TileWidth, tileSet.TileHeight);
                };
            }

            return tileSet;
        }
    }
}