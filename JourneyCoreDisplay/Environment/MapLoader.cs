using SFML.System;
using System.Collections.Generic;
using System.IO;

namespace JourneyCoreDisplay.Environment
{
    public static class MapLoader
    {
        public static Map LoadMap(string mapName, Vector2i tileSize, int scale)
        {
            string mapString = string.Empty;

            using (StreamReader reader = new StreamReader($@"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\JourneyCoreGame\Assets\Maps\{mapName}.map"))
            {
                mapString = reader.ReadToEnd();
            }

            string[] mapArray = mapString.Replace("\r\n", "\n").Split(':');

            List<int> processedMapCodes = new List<int>();

            int height = 0;
            int width = 0;
            for (int i = 0; i < mapArray.Length; i++)
            {
                if (mapArray[i].Equals("\n"))
                {
                    if (width == 0)
                    {
                        width = i - 1;
                    }

                    height += 1;

                    continue;
                }

                if (string.IsNullOrWhiteSpace(mapArray[i]))
                {
                    continue;
                }

                int.TryParse(mapArray[i], out int mapCode);
                processedMapCodes.Add(mapCode);
            }

            return new Map(new Vector2i(width, height), tileSize, processedMapCodes, scale);
        }
    }
}
