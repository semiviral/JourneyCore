using System;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace JourneyCore.Lib.Display.Interactive
{
    public class Button : Drawable
    {
        private Text _TextObject;

        public Color FillColor { get; }
        public Sprite BackgroundSprite { get; }
        public Font DefaultFont { get; }
        public string Text
        {
            get => _TextObject.DisplayedString;
            set => _TextObject = new Text(value, DefaultFont);
        }

        public Button(Font defaultFont, string text)
        {
            FillColor = Color.Transparent;
            BackgroundSprite = new Sprite();
            DefaultFont = defaultFont;
            Text = text;
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            throw new NotImplementedException();
        }
    }
}
