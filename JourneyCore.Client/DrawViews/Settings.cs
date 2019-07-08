using System;
using JourneyCore.Lib.Display;
using JourneyCore.Lib.Display.Component;
using JourneyCore.Lib.Display.Drawing;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Client.DrawViews
{
    public class Settings
    {
        private GameWindow GameWindow { get; }
        private DrawView AppliedDrawView { get; set; }

        public Settings(GameWindow gameWindow)
        {
            GameWindow = gameWindow;
        }

        public void Initialise()
        {
            CreateSettingsView();
            PopulateSettingsView();
        }

        private void CreateSettingsView()
        {
            GameWindow.CreateDrawView(new DrawView(DrawViewLayer.Settings,
                new View(new FloatRect(0f, 0f, GameWindow.Size.X, GameWindow.Size.Y))
                    { Viewport = new FloatRect(0f, 0f, 1f, 1f) }));
            AppliedDrawView = GameWindow.GetDrawView(DrawViewLayer.Settings);
        }

        private void PopulateSettingsView()
        {
            GameWindow.AddDrawItem(DrawViewLayer.Settings, 0,
                new DrawItem(DateTime.MinValue, null,
                    new DrawObject(
                        new RectangleShape((Vector2f)GameWindow.Size) { FillColor = new Color(0, 0, 0, 155) }),
                    RenderStates.Default));

            Button increaseBrightness = CreateBrightnessIncreaseButton();
            GameWindow.SubscribeUiObject(DrawViewLayer.Settings, 0, increaseBrightness);
            GameWindow.AddDrawItem(DrawViewLayer.Settings, 10,
                new DrawItem(DateTime.MinValue, null,
                    new DrawObject(increaseBrightness), RenderStates.Default));

            Button decreaseBrightness = CreateBrightnessDecreaseButton();
            GameWindow.SubscribeUiObject(DrawViewLayer.Settings, 0, decreaseBrightness);
            GameWindow.AddDrawItem(DrawViewLayer.Settings, 10,
                new DrawItem(DateTime.MinValue, null,
                    new DrawObject(decreaseBrightness), RenderStates.Default));
        }

        private Button CreateBrightnessIncreaseButton()
        {
            GameMenuButton increaseBrightness = new GameMenuButton(GameLoop.DefaultFont, "Brightness +", true, true)
            {
                Position = new Vector2f(AppliedDrawView.View.Size.X / 2f, AppliedDrawView.View.Size.Y / 2f)
            };
            increaseBrightness.Released += (sender, args) =>
            {
                GameWindow.GetDrawView(DrawViewLayer.Minimap).ModifyOpacity(10);
            };

            return increaseBrightness;
        }

        private Button CreateBrightnessDecreaseButton()
        {
            GameMenuButton decreaseBrightness = new GameMenuButton(GameLoop.DefaultFont, "Brightness -", true, true)
            {
                Position = new Vector2f(AppliedDrawView.View.Size.X / 2f, AppliedDrawView.View.Size.Y / 2f + 50f)
            };
            decreaseBrightness.Released += (sender, args) =>
            {
                GameWindow.GetDrawView(DrawViewLayer.Minimap).ModifyOpacity(-25);
            };

            return decreaseBrightness;
        }
    }
}