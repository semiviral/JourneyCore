using System;
using System.Linq;
using JourneyCore.Lib.Game.Environment.Metadata;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Lib.Display
{
    public class Ui
    {
        public Ui(TileSetMetadata uiTileSet, byte[] uiSpriteSheetImage)
        {
            UiSpriteSheetImage = uiSpriteSheetImage;
            Hearts = new Sprite[0];
            CurrentHp = 0f;

            UiTileSet = uiTileSet;
            UiSpriteSheetTexture = new Texture(UiSpriteSheetImage);
        }

        public static int HpRowWidth { get; } = 6;

        private TileSetMetadata UiTileSet { get; }
        private byte[] UiSpriteSheetImage { get; }
        private Texture UiSpriteSheetTexture { get; }
        public Sprite[] Hearts { get; private set; }
        private double CurrentHp { get; set; }

        public void UpdateHealth(double newHp)
        {
            double _difference = CurrentHp - newHp;

            CurrentHp = newHp;

            if (Math.Abs(_difference) < 5)
            {
                return;
            }

            CalculateHearts();
        }

        public IntRect GetTextureRectByType(string name)
        {
            IntRect _newRect = UiTileSet.Tiles.SingleOrDefault(tile => tile.Type.Equals(name)).TextureRect;

            _newRect.Top *= _newRect.Height;
            _newRect.Left *= _newRect.Width;

            return _newRect;
        }

        private void CalculateHearts()
        {
            // each half heart is 1 hp
            // so therefore 2hp = 2 half hearts,
            // and 2 / 2 = 1, so 1 full heart
            // and 2 % 2 = 0, so 0 half hearts
            int _fullHearts = (int) CurrentHp / 2;
            int _halfHearts = (int) CurrentHp % 2;

            Sprite[] _newHearts = new Sprite[_fullHearts + _halfHearts];

            for (int _i = 0; _i < _fullHearts; _i++)
            {
                _newHearts[_i] = new Sprite(UiSpriteSheetTexture, GetTextureRectByType("HeartFull"));
            }

            if (_halfHearts > 0)
            {
                _newHearts[_fullHearts] = new Sprite(UiSpriteSheetTexture, GetTextureRectByType("HeartHalf"));
            }

            for (int _i = 0; _i < _newHearts.Length; _i++)
            {
                _newHearts[_i].Scale = new Vector2f(4f, 4f);

                float _posX = (_i % HpRowWidth) * _newHearts[_i].TextureRect.Width * _newHearts[_i].Scale.X;
                float _posY = (_i / HpRowWidth) * _newHearts[_i].TextureRect.Height * _newHearts[_i].Scale.Y;

                _newHearts[_i].Position = new Vector2f(_posX, _posY);
            }

            Hearts = _newHearts;
        }
    }
}