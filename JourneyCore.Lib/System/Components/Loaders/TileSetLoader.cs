using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using JourneyCore.Lib.Graphics.Rendering;
using JourneyCore.Lib.Graphics.Rendering.Environment.Tiling;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Lib.System.Components.Loaders
{
    public class TileSetLoader
    {
        private static Random _rand;

        static TileSetLoader()
        {
            _rand = new Random();
        }

        public static TileSet LoadTileSet(string filePath)
        {
            TileSet tileSet;

            XmlSerializer sheetSerializer = new XmlSerializer(typeof(TileSet));
            using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
            {
                tileSet = (TileSet)sheetSerializer.Deserialize(reader);

                for (int i = 0; i < tileSet.Tiles.Length; i++)
                {
                    Tile modifiedTile = new Tile
                    {
                        Id = (short)(tileSet.Tiles[i].Id + 1),
                        SizeX = tileSet.TileHeight,
                        SizeY = tileSet.TileWidth,
                        TextureRect = new IntRect((tileSet.Tiles[i].Id - 1) / tileSet.Columns, (tileSet.Tiles[i].Id - 1) % tileSet.Columns,
                            tileSet.Tiles[i].SizeX, tileSet.Tiles[i].SizeY),
                    };

                    modifiedTile.ApplyProperties();

                    tileSet.Tiles[i] = modifiedTile;
                }
            }

            return tileSet;
        }


    }
}