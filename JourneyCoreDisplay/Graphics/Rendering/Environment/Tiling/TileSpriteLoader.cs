using JourneyCoreLib.Environment;
using SFML.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace JourneyCoreLib.Rendering.Environment.Tiling
{
    public static class TileSpriteLoader
    {
        private static Random _rand;

        public static List<Tile> LoadedTiles { get; set; }

        static TileSpriteLoader()
        {
            _rand = new Random();
            LoadedTiles = new List<Tile>();
        }

        public static void LoadTiles(string filePath)
        {
            XmlSerializer sheetSerializer = new XmlSerializer(typeof(TileSet));
            using (FileStream reader = new FileStream(filePath, FileMode.Open))
            {
                TileSet tileSet = (TileSet)sheetSerializer.Deserialize(reader);

                for (int i = 0; i < tileSet.Tiles.Count; i++)
                {
                    tileSet.Tiles[i].Id += 1;
                    tileSet.Tiles[i].Size = new Vector2i(tileSet.TileHeight, tileSet.TileWidth);
                    tileSet.Tiles[i].Initialise(tileSet.Columns);

                    foreach (CustomProperty property in tileSet.Tiles[i].Properties)
                    {
                        property.QualifyValue();
                    }

                    LoadedTiles.Add(tileSet.Tiles[i]);
                }
            }
        }

        public static Tile GetById(int id)
        {
            return LoadedTiles.FirstOrDefault(sprite => sprite.Id == id);
        }

        public static List<Tile> GetByGroup(string groupName)
        {
            return LoadedTiles.Where(tile => tile.Group.Equals(groupName)).ToList();
        }

        public static List<Tile> GetByAccentGroup(string accentGroupName)
        {
            return LoadedTiles.Where(tile => !string.IsNullOrWhiteSpace(tile.AccentGroup) && tile.AccentGroup.Equals(accentGroupName)).ToList();
        }

        public static CustomProperty GetProperty(int id, string propertyName)
        {
            return GetById(id).Properties.FirstOrDefault(property => property.Name.Equals(propertyName));
        }

        public static Tile GetRandom(List<Tile> candidates)
        {
            // optimizations to avoid useless iterating

            if (candidates.Count < 1)
            {
                return default;
            }

            // if only one sprite in list
            if (candidates.Count == 1)
            {
                return candidates[0];
            }

            // in case all have equal weights
            if (candidates.Select(tile => tile.Probability).All(weight => weight == candidates[0].Probability))
            {
                return candidates[_rand.Next(0, candidates.Count)];
            }

            // end optimizations

            int totalWeight = candidates.Select(sprite => (int)(sprite.Probability * 100)).Sum();

            Tile[] weightArray = new Tile[totalWeight];

            int iterations = 0;
            for (int i = 0; i < candidates.Count; i++)
            {
                for (int j = 0; j < candidates[i].Probability * 100; j++)
                {
                    weightArray[iterations] = candidates[i];
                    iterations += 1;
                }
            }

            int randSelection = _rand.Next(0, weightArray.Length);
            return weightArray[randSelection];
        }

        public static Tile GetTileOrRandom(int id)
        {
            Tile currentTile = GetById(id);

            if (currentTile == null)
            {
                return default;
            }

            CustomProperty isRandableProp = GetProperty(id, "IsRandomizable");

            if (isRandableProp == null)
            {
                return GetRandom(GetByGroup(currentTile.Group));
            }

            bool isRandomizable = (bool)isRandableProp.QualifiedValue;

            if (isRandomizable)
            {
                return GetRandom(GetByGroup(currentTile.Group));
            }
            else
            {
                return GetById(id);
            }
        }
    }
}
