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
            double difference = CurrentHp - newHp;

            CurrentHp = newHp;

            if (Math.Abs(difference) < 5) return;

            CalculateHearts();
        }

        public IntRect GetTextureRectByType(string name)
        {
            IntRect newRect = UiTileSet.Tiles.SingleOrDefault(tile => tile.Type.Equals(name)).TextureRect;

            newRect.Top *= newRect.Height;
            newRect.Left *= newRect.Width;

            return newRect;
        }

        private void CalculateHearts()
        {
            // each half heart is 1 hp
            // so therefore 2hp = 2 half hearts,
            // and 2 / 2 = 1, so 1 full heart
            // and 2 % 2 = 0, so 0 half hearts
            int fullHearts = (int) CurrentHp / 2;
            int halfHearts = (int) CurrentHp % 2;

            Sprite[] newHearts = new Sprite[fullHearts + halfHearts];

            for (int i = 0; i < fullHearts; i++)
                newHearts[i] = new Sprite(UiSpriteSheetTexture, GetTextureRectByType("HeartFull"));

            if (halfHearts > 0)
                newHearts[fullHearts] = new Sprite(UiSpriteSheetTexture, GetTextureRectByType("HeartHalf"));

            for (int i = 0; i < newHearts.Length; i++)
            {
                newHearts[i].Scale = new Vector2f(4f, 4f);

                float posX = i % HpRowWidth * newHearts[i].TextureRect.Width * newHearts[i].Scale.X;
                float posY = i / HpRowWidth * newHearts[i].TextureRect.Height * newHearts[i].Scale.Y;

                newHearts[i].Position = new Vector2f(posX, posY);
            }

            Hearts = newHearts;
        }
    }
}