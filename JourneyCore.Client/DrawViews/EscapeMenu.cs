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
            Button settingsButton = CreateSettingsButton();
            GameWindow.AddDrawItem(DrawViewLayer.EscapeMenu, 10, new DrawItem(new DrawObject(settingsButton)));

            Button exitButton = CreateExitButton();
            GameWindow.AddDrawItem(DrawViewLayer.EscapeMenu, 10, new DrawItem(new DrawObject(exitButton)));

            UIObjectContainer escapeMenuContainer = new UIObjectContainer
            {
                HorizontalPositioning = UIObjectHorizontalPositioning.Middle,
                VerticalPositioning = UIObjectVerticalPositioning.Bottom,
                HorizontalAutoStacking = true,
                Size = GameWindow.Size
            };
            escapeMenuContainer.UIObjects.Add(settingsButton);
            escapeMenuContainer.UIObjects.Add(exitButton);

            GameWindow.SubscribeUiObject(escapeMenuContainer, null);
        }

        #region OBJECT CREATION

        private Button CreateExitButton()
        {
            GameMenuButton exitButton = new GameMenuButton(GameLoop.DefaultFont, "Exit", true, true)
            {
                Activated = () => AppliedDrawView.Visible
            };
            exitButton.Released += (sender, args) => { GameLoop.CallFatality("Game exited."); };

            return exitButton;
        }

        private Button CreateSettingsButton()
        {
            GameMenuButton settingsButton = new GameMenuButton(GameLoop.DefaultFont, "Settings", true, true)
            {
                Activated = () => AppliedDrawView.Visible
            };
            settingsButton.Released += (sender, args) =>
            {
                AppliedDrawView.Visible = false;
                GameWindow.GetDrawView(DrawViewLayer.Settings).Visible = true;
            };

            return settingsButton;
        }

        #endregion
    }
}