using JourneyCore.Lib.Graphics.Rendering;
using JourneyCore.Lib.Graphics.Rendering.Environment.Tiling;
using SFML.System;
using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

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

                for (int i = 0; i < tileSet.Tiles.Count; i++)
                {
                    tileSet.Tiles[i].Id += 1;
                    tileSet.Tiles[i].Size = new Vector2i(tileSet.TileHeight, tileSet.TileWidth);
                    tileSet.Tiles[i].Initialise(tileSet.Columns);

                    foreach (CustomProperty property in tileSet.Tiles[i].Properties)
                    {
                        property.QualifyValue();
                    }
                }
            }

            return tileSet;
        }
    }
}
