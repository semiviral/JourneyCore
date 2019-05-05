using SFML.Graphics;
using System;
using System.Collections.Generic;

namespace JourneyCoreDisplay.Sprites
{
    public static class SpriteLoader
    {
        public static Dictionary<int, IntRect> LoadedSprites { get; set; }

        static SpriteLoader()
        {
            LoadedSprites = new Dictionary<int, IntRect>();
        }

        public static IntRect LoadSprite(int index, int width, int height, int x, int y)
        {
            LoadedSprites.Add(index, new IntRect(x * width, y * height, width, height));

            return LoadedSprites[index];
        }
    }
}
