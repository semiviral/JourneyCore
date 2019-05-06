using SFML.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace JourneyCoreDisplay.Sprites
{
    public static class SpriteLoader
    {
        public static List<SpriteTag> LoadedSprites { get; set; }

        static SpriteLoader()
        {
            LoadedSprites = new List<SpriteTag>();
        }

        public static void LoadSprite(SpriteType type, WeightedSprite weightedSprite)
        {
            if (!LoadedSprites.Any(sprite => sprite.Type.Equals(type)))
            {
                LoadedSprites.Add(new SpriteTag(type, weightedSprite));
            }
            else
            {
                LoadedSprites.First(sprite => sprite.Type.Equals(type)).Sprites.Add(weightedSprite);
            }
        }
    }
}
