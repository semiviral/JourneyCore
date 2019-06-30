using JourneyCore.Lib.Display.Component;
using SFML.Graphics;

namespace JourneyCore.Client
{
    public class GameMenuButton : Button
    {
        public GameMenuButton(Font defaultFont, string displayedText, bool autoSize) : base(defaultFont,
            displayedText, autoSize)
        {
            Origin = Size / 2f;
            BackgroundColor = Color.Transparent;
            Entered += (sender, args) => { ForegroundColor = IsPressed ? Color.Red : Color.Cyan; };
            Exited += (sender, args) =>
            {
                if (!IsPressed)
                {
                    ForegroundColor = Color.White;
                }
            };
            Pressed += (sender, args) => { ForegroundColor = Color.Red; };
            Released += (sender, args) => { ForegroundColor = IsHovered ? Color.Cyan : Color.White; };
        }
    }
}