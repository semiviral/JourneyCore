using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace JourneyCore.Lib.Display.Component
{
    public class Text : SFML.Graphics.Text, IUiObject
    {
        public Text()
        {
        }

        public Text(Text copy) : base(copy)
        {
            Margins = copy.Margins;
            AutoCalculateSize();
        }

        public Text(string str, Font font) : base(str, font)
        {
            Margins = new Margin();
            AutoCalculateSize();
        }

        public Text(string str, Font font, uint characterSize) : base(str, font, characterSize)
        {
            Margins = new Margin();
            AutoCalculateSize();
        }

        public Vector2u Size { get; set; }
        public Margin Margins { get; set; }
        public event EventHandler<SizeEventArgs> Resized;

        public IEnumerable<IUiObject> SubscribableObjects()
        {
            return new IUiObject[] { };
        }

        private void AutoCalculateSize()
        {
            FloatRect _localBounds = GetLocalBounds();
            Size = new Vector2u((uint) (_localBounds.Width + _localBounds.Left) + Margins.Left + Margins.Right,
                CharacterSize + Margins.Top + Margins.Bottom);
        }
    }
}