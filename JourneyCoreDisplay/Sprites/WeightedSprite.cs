using SFML.Graphics;

namespace JourneyCoreDisplay.Sprites
{
    public class WeightedSprite
    {
        public int Weight { get; }
        public IntRect Sprite { get; }

        public WeightedSprite(int weight, IntRect sprite)
        {
            Weight = weight;
            Sprite = sprite;
        }

        public WeightedSprite(int weight, int width, int height, int x, int y) : this(weight, new IntRect(x * width, y * height, width, height)) { }
    }
}
