using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using JourneyCore.Lib.Graphics.Rendering.Environment.Tiling;
using SFML.Graphics;

namespace JourneyCore.Lib.System.Components.Loaders
{
    public class TileSetLoader
    {
        public static TileSet LoadTileSet(string filePath)
        {
            TileSet tileSet;

            XmlSerializer sheetSerializer = new XmlSerializer(typeof(TileSet));
            using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
            {
                tileSet = (TileSet) sheetSerializer.Deserialize(reader);

                for (int i = 0; i < tileSet.Tiles.Length; i++)
                {
                    bool isRandomizable = false;
                    bool isRandomlyRotatable = false;

                    if (tileSet.Tiles[i].Properties
                        != null)
                    {
                        isRandomizable = tileSet.Tiles[i].GetProperty("IsRandomizable")?.Value != null &&
                                              (bool) Convert.ChangeType(
                                                  tileSet.Tiles[i].GetProperty("IsRandomizable").Value,
                                                  typeof(bool));
                        isRandomlyRotatable = tileSet.Tiles[i].GetProperty("IsRandomlyRotatable")?.Value != null &&
                                              (bool) Convert.ChangeType(
                                                  tileSet.Tiles[i].GetProperty("IsRandomlyRotatable").Value,
                                                  typeof(bool));
                    }

                    Tile modifiedTile = new Tile
                    {
                        Id = (short) (tileSet.Tiles[i].Id + 1),
                        Group = tileSet.Tiles[i].Group,
                        Probability = tileSet.Tiles[i].Probability,
                        SizeX = tileSet.TileHeight,
                        SizeY = tileSet.TileWidth,
                        TextureRect = new IntRect(tileSet.Tiles[i].Id % tileSet.Columns,
                            tileSet.Tiles[i].Id / tileSet.Columns,
                            tileSet.TileWidth, tileSet.TileHeight),
                        TextureCoords =  tileSet.Tiles[i].TextureCoords,
                        IsRandomizable = isRandomizable,
                        IsRandomlyRotatable = isRandomlyRotatable,
                    };

                    tileSet.Tiles[i] = modifiedTile;
                }
            }

            return tileSet;
        }
    }
}