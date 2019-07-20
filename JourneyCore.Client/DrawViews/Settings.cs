using JourneyCore.Lib.Display;
using JourneyCore.Lib.Display.Component;
using JourneyCore.Lib.Display.Drawing;
using SFML.Graphics;
using SFML.System;
using Text = JourneyCore.Lib.Display.Component.Text;

namespace JourneyCore.Client.DrawViews
{
    public class Settings
    {
        public Settings(GameWindow gameWindow)
        {
            GameWindow = gameWindow;
        }

        private GameWindow GameWindow { get; }
        private DrawView AppliedDrawView { get; set; }

        public void Initialise()
        {
            CreateSettingsView();
            PopulateSettingsView();
        }

        private void CreateSettingsView()
        {
            GameWindow.CreateDrawView(new DrawView(DrawViewLayer.Settings,
                new View(new FloatRect(0f, 0f, GameWindow.Size.X, GameWindow.Size.Y))
                    {Viewport = new FloatRect(0f, 0f, 1f, 1f)}));
            AppliedDrawView = GameWindow.GetDrawView(DrawViewLayer.Settings);
        }

        private void PopulateSettingsView()
        {
            GameWindow.AddDrawItem(DrawViewLayer.Settings, 0,
                new DrawItem(new DrawObject(new RectangleShape((Vector2f) GameWindow.Size)
                    {FillColor = new Color(0, 0, 0, 155)})));

            Button increaseBrightness = CreateBrightnessIncreaseButton();
            GameWindow.AddDrawItem(DrawViewLayer.Settings, 10,
                new DrawItem(new DrawObject(increaseBrightness)));

            Button decreaseBrightness = CreateBrightnessDecreaseButton();
            GameWindow.AddDrawItem(DrawViewLayer.Settings, 10,
                new DrawItem(new DrawObject(decreaseBrightness)));

            Text vSyncText = CreateVSyncText();
            GameWindow.AddDrawItem(DrawViewLayer.Settings, 10, new DrawItem(new DrawObject(vSyncText)));

            Button vSyncButton = CreateVSyncButton();
            GameWindow.AddDrawItem(DrawViewLayer.Settings, 10, new DrawItem(new DrawObject(vSyncButton)));

            UIObjectContainer vSyncContainer = new UIObjectContainer
            {
                HorizontalPositioning = UIObjectHorizontalPositioning.Middle,
                VerticalPositioning = UIObjectVerticalPositioning.Middle,
                HorizontalAutoStacking = true
            };
            vSyncContainer.UIObjects.Add(vSyncText);
            vSyncContainer.UIObjects.Add(vSyncButton);


            UIObjectContainer objectContainer = new UIObjectContainer
            {
                VerticalAutoStacking = true,
                HorizontalPositioning = UIObjectHorizontalPositioning.Middle,
                VerticalPositioning = UIObjectVerticalPositioning.Middle
            };
            objectContainer.UIObjects.Add(increaseBrightness);
            objectContainer.UIObjects.Add(decreaseBrightness);
            objectContainer.UIObjects.Add(vSyncContainer);

            GameWindow.SubscribeUiObject(objectContainer, null);

            vSyncContainer.Size = vSyncText.Size + vSyncButton.Size + new Vector2u(30, 0);
            objectContainer.Size = GameWindow.Size;
        }

        private Button CreateBrightnessIncreaseButton()
        {
            GameMenuButton increaseBrightness = new GameMenuButton(GameLoop.DefaultFont, "Brightness +", true, true)
            {
                Activated = () => AppliedDrawView.Visible
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
                Activated = () => AppliedDrawView.Visible
            };
            decreaseBrightness.Released += (sender, args) =>
            {
                GameWindow.GetDrawView(DrawViewLayer.Minimap).ModifyOpacity(-10);
            };

            return decreaseBrightness;
        }

        private Text CreateVSyncText()
        {
            Text vSyncText = new Text("VSync:", GameLoop.DefaultFont)
            {
                OutlineColor = Color.Black,
                OutlineThickness = 2f
            };

            FloatRect localBounds = vSyncText.GetLocalBounds();
            vSyncText.Origin = new Vector2f((localBounds.Width + localBounds.Left) / 2f,
                (localBounds.Height + localBounds.Top) / 2f);

            return vSyncText;
        }

        private Button CreateVSyncButton()
        {
            bool vSyncEnabled = false;

            GameMenuButton vSyncButton = new GameMenuButton(GameLoop.DefaultFont, "Off", true, true)
            {
                Activated = () => AppliedDrawView.Visible
            };
            vSyncButton.Released += (sender, args) =>
            {
                vSyncButton.DisplayedText = vSyncEnabled ? "Off" : "On";

                GameWindow.SetVSync(!vSyncEnabled);
                vSyncEnabled = !vSyncEnabled;
            };

            return vSyncButton;
        }
    }
}