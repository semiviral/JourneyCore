using System;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;
using SFML.System;

namespace JourneyCoreLib.Graphics.Rendering.Sprites
{
    public class SpriteSheet
    {
        private Texture _sheet;

        public Vector2i Size { get; }

        public SpriteSheet(string file, Vector2i size)
        {
            _sheet = new Texture(file);
            Size = size;
        }

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
