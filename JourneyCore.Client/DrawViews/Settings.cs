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

            Button _increaseBrightness = CreateBrightnessIncreaseButton();
            GameWindow.AddDrawItem(DrawViewLayer.Settings, 10,
                new DrawItem(new DrawObject(_increaseBrightness)));

            Button _decreaseBrightness = CreateBrightnessDecreaseButton();
            GameWindow.AddDrawItem(DrawViewLayer.Settings, 10,
                new DrawItem(new DrawObject(_decreaseBrightness)));

            Text _vSyncText = CreateVSyncText();
            GameWindow.AddDrawItem(DrawViewLayer.Settings, 10, new DrawItem(new DrawObject(_vSyncText)));

            Button _vSyncButton = CreateVSyncButton();
            GameWindow.AddDrawItem(DrawViewLayer.Settings, 10, new DrawItem(new DrawObject(_vSyncButton)));

            UiObjectContainer _vSyncContainer = new UiObjectContainer
            {
                HorizontalPositioning = UiObjectHorizontalPositioning.Middle,
                VerticalPositioning = UiObjectVerticalPositioning.Middle,
                HorizontalAutoStacking = true
            };
            _vSyncContainer.UiObjects.Add(_vSyncText);
            _vSyncContainer.UiObjects.Add(_vSyncButton);


            UiObjectContainer _objectContainer = new UiObjectContainer
            {
                VerticalAutoStacking = true,
                HorizontalPositioning = UiObjectHorizontalPositioning.Middle,
                VerticalPositioning = UiObjectVerticalPositioning.Middle
            };
            _objectContainer.UiObjects.Add(_increaseBrightness);
            _objectContainer.UiObjects.Add(_decreaseBrightness);
            _objectContainer.UiObjects.Add(_vSyncContainer);

            GameWindow.SubscribeUiObject(_objectContainer, null);

            _vSyncContainer.Size = _vSyncText.Size + _vSyncButton.Size + new Vector2u(30, 0);
            _objectContainer.Size = GameWindow.Size;
        }

        private Button CreateBrightnessIncreaseButton()
        {
            GameMenuButton _increaseBrightness = new GameMenuButton(GameLoop.DefaultFont, "Brightness +", true, true)
            {
                Activated = () => AppliedDrawView.Visible
            };
            _increaseBrightness.Released += (sender, args) =>
            {
                GameWindow.GetDrawView(DrawViewLayer.Minimap).ModifyOpacity(10);
            };

            return _increaseBrightness;
        }

        private Button CreateBrightnessDecreaseButton()
        {
            GameMenuButton _decreaseBrightness = new GameMenuButton(GameLoop.DefaultFont, "Brightness -", true, true)
            {
                Activated = () => AppliedDrawView.Visible
            };
            _decreaseBrightness.Released += (sender, args) =>
            {
                GameWindow.GetDrawView(DrawViewLayer.Minimap).ModifyOpacity(-10);
            };

            return _decreaseBrightness;
        }

        private Text CreateVSyncText()
        {
            Text _vSyncText = new Text("VSync:", GameLoop.DefaultFont)
            {
                OutlineColor = Color.Black,
                OutlineThickness = 2f
            };

            FloatRect _localBounds = _vSyncText.GetLocalBounds();
            _vSyncText.Origin = new Vector2f((_localBounds.Width + _localBounds.Left) / 2f,
                (_localBounds.Height + _localBounds.Top) / 2f);

            return _vSyncText;
        }

        private Button CreateVSyncButton()
        {
            bool _vSyncEnabled = false;

            GameMenuButton _vSyncButton = new GameMenuButton(GameLoop.DefaultFont, "Off", true, true)
            {
                Activated = () => AppliedDrawView.Visible
            };
            _vSyncButton.Released += (sender, args) =>
            {
                _vSyncButton.DisplayedText = _vSyncEnabled ? "Off" : "On";

                GameWindow.SetVSync(!_vSyncEnabled);
                _vSyncEnabled = !_vSyncEnabled;
            };

            return _vSyncButton;
        }
    }
}