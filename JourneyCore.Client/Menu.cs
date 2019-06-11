using System;
using JourneyCore.Lib.Display;
using JourneyCore.Lib.Display.Interactive;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using SFML.Graphics;

namespace JourneyCore.Client
{
    public class Menu : Drawable
    {
        public Button testButton { get; }

        public Menu()
        {
            Font defaultFont = new Font(@"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\Assets\Fonts\Courier New.ttf");
            testButton = new Button(defaultFont, "Test");
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            testButton.Draw(target, states);
        }
    }
}
