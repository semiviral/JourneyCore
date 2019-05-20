using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Lib.Graphics.Rendering.Sprites
{
    public class SpriteSheet
    {
        private readonly Texture _sheet;

        public SpriteSheet(string file, Vector2i size)
        {
            _sheet = new Texture(file);
            Size = size;
        }

        public Vector2i Size { get; }

        public Sprite GetSprite(int left, int top)
        {
            return GetSprite(new Vector2i(left, top));
        }

        public Sprite GetSprite(Vector2i coords)
        {
            return new Sprite(_sheet, new IntRect(coords.X * Size.X, coords.Y * Size.Y, Size.X, Size.Y));
        }
    }
}