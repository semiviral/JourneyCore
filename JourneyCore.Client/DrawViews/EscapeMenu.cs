using JourneyCore.Lib.Display;
using JourneyCore.Lib.Display.Component;
using JourneyCore.Lib.Display.Drawing;
using SFML.Graphics;

namespace JourneyCore.Client.DrawViews
{
    public class EscapeMenu
    {
        public EscapeMenu(GameWindow gameWindow)
        {
            GameWindow = gameWindow;
        }

        private GameWindow GameWindow { get; }
        private DrawView AppliedDrawView { get; set; }

        public void Initialise()
        {
            CreateEscapeMenuView();
            PopulateEscapeMenuView();
        }

        private void CreateEscapeMenuView()
        {
            GameWindow.CreateDrawView(new DrawView(DrawViewLayer.EscapeMenu,
                new View(new FloatRect(0f, 0f, GameWindow.Size.X, GameWindow.Size.Y))
                    {Viewport = new FloatRect(0f, 0f, 1f, 1f)}));
            AppliedDrawView = GameWindow.GetDrawView(DrawViewLayer.EscapeMenu);
        }

        private void PopulateEscapeMenuView()
        {
            Button _settingsButton = CreateSettingsButton();
            GameWindow.AddDrawItem(DrawViewLayer.EscapeMenu, 10, new DrawItem(new DrawObject(_settingsButton)));

            Button _exitButton = CreateExitButton();
            GameWindow.AddDrawItem(DrawViewLayer.EscapeMenu, 10, new DrawItem(new DrawObject(_exitButton)));

            UiObjectContainer _escapeMenuContainer = new UiObjectContainer
            {
                HorizontalPositioning = UiObjectHorizontalPositioning.Justify,
                VerticalPositioning = UiObjectVerticalPositioning.Bottom,
                HorizontalAutoStacking = true,
                Size = GameWindow.Size
            };
            _escapeMenuContainer.UiObjects.Add(_settingsButton);
            _escapeMenuContainer.UiObjects.Add(_exitButton);

            GameWindow.SubscribeUiObject(_escapeMenuContainer, null);
        }

        #region OBJECT CREATION

        private Button CreateExitButton()
        {
            GameMenuButton _exitButton = new GameMenuButton(GameLoop.DefaultFont, "Exit", true, true)
            {
                Margins = new Margin(0, 25),
                Activated = () => AppliedDrawView.Visible
            };
            _exitButton.Released += (sender, args) => { GameLoop.CallFatality("Game exited."); };

            return _exitButton;
        }

        private Button CreateSettingsButton()
        {
            GameMenuButton _settingsButton = new GameMenuButton(GameLoop.DefaultFont, "Settings", true, true)
            {
                Margins = new Margin(0, 25),
                Activated = () => AppliedDrawView.Visible
            };
            _settingsButton.Released += (sender, args) =>
            {
                AppliedDrawView.Visible = false;
                GameWindow.GetDrawView(DrawViewLayer.Settings).Visible = true;
            };

            return _settingsButton;
        }

        #endregion
    }
}