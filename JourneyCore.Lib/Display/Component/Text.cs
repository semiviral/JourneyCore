using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace JourneyCore.Lib.Display.Component
{
    public class Text : SFML.Graphics.Text, IUIObject
    {
        public Text()
        {
        }

        public Text(Text copy) : base(copy)
        {
            Size = copy.Size;
        }

        public Text(string str, Font font) : base(str, font)
        {
            FloatRect localBounds = GetLocalBounds();
            Size = new Vector2u((uint) (localBounds.Width + localBounds.Left),
                (uint) (localBounds.Height + localBounds.Top));
        }

        public Text(string str, Font font, uint characterSize) : base(str, font, characterSize)
        {
            FloatRect localBounds = GetLocalBounds();
            Size = new Vector2u((uint) (localBounds.Width + localBounds.Left),
                (uint) (localBounds.Height + localBounds.Top));
        }

        public Vector2u Size { get; set; }
        public event EventHandler<SizeEventArgs> Resized;

        public IEnumerable<IUIObject> SubscribableObjects()
        {
            return new IUIObject[] { };
        }
    }
}