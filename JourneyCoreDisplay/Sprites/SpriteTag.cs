using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JourneyCoreDisplay.Sprites
{
    public class SpriteTag
    {
        private Random _rand;

        public SpriteType Type { get; }
        /// <summary>
        ///     int is weight (for random eval)
        ///     IntRect is sprite location and size
        /// </summary>
        public List<WeightedSprite> Sprites { get; }

        public SpriteTag(SpriteType type, List<WeightedSprite> sprites)
        {
            _rand = new Random();

            Type = type;
            Sprites = new List<WeightedSprite>(sprites);
        }

        public SpriteTag(SpriteType type, params WeightedSprite[] args) : this(type, args.ToList()) { }

        public IntRect GetRandom()
        {
            // optimizations to avoid useless iterating

            // if only one sprite in list
            if (Sprites.Count == 1)
            {
                return Sprites[0].Sprite;
            }


            // All have equal weights
            if (Sprites.Select(sprite => sprite.Weight).All(weight => weight == Sprites[0].Weight))
            {
                return Sprites[_rand.Next(0, Sprites.Count)].Sprite;
            }

            // end optimizations

            int totalWeight = Sprites.Select(sprite => sprite.Weight).Sum();

            WeightedSprite[] weightArray = new WeightedSprite[totalWeight];

            int iterations = 0;
            for (int i = 0; i < Sprites.Count; i++)
            {
                for (int j = 0; j < Sprites[i].Weight; j++)
                {
                    weightArray[iterations] = Sprites[i];
                    iterations += 1;
                }
            }

            return weightArray[_rand.Next(0, weightArray.Length)].Sprite;
        }
    }

    public enum SpriteType
    {
        Nothing = 0,
        Grass = 1,
        Dirt = 2,
        Stone = 3,
        NexusPath = 4,
        SurfacePath = 5,
    }
}
