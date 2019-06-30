using System;
using JourneyCore.Lib.Display;
using JourneyCore.Lib.Display.Component;
using JourneyCore.Lib.Display.Drawing;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Client.DrawViews
{
    public class EscapeMenu
    {
        private GameWindow GameWindow { get; }
        private DrawView AppliedDrawView { get; set; }

        public EscapeMenu(GameWindow gameWindow)
        {
            GameWindow = gameWindow;
        }

        public void Initialise()
        {
            CreateEscapeMenuView();
            PopulateEscapeMenuView();
        }

        private void CreateEscapeMenuView()
        {
            GameWindow.CreateDrawView(new DrawView(DrawViewLayer.EscapeMenu,
                new View(new FloatRect(0f, 0f, GameWindow.Size.X, GameWindow.Size.Y))
                    { Viewport = new FloatRect(0f, 0f, 1f, 1f) }));
            AppliedDrawView = GameWindow.GetDrawView(DrawViewLayer.EscapeMenu);
        }

        private void PopulateEscapeMenuView()
        {
            // semi-transparent background rectangle
            GameWindow.AddDrawItem(DrawViewLayer.EscapeMenu, 0,
                new DrawItem(DateTime.MinValue, null,
                    new DrawObject(
                        new RectangleShape((Vector2f)GameWindow.Size) { FillColor = new Color(0, 0, 0, 155) }),
                    RenderStates.Default));

            Button exitButton = CreateExitButton();
            GameWindow.SubscribeUiObject(DrawViewLayer.EscapeMenu, 0, exitButton);
            GameWindow.AddDrawItem(DrawViewLayer.EscapeMenu, 10,
                new DrawItem(DateTime.MinValue, null,
                    new DrawObject(exitButton), RenderStates.Default));

            Button settingsButton = CreateSettingsButton();
            GameWindow.SubscribeUiObject(DrawViewLayer.EscapeMenu, 0, settingsButton);
            GameWindow.AddDrawItem(DrawViewLayer.EscapeMenu, 10,
                new DrawItem(DateTime.MinValue, null, new DrawObject(settingsButton),
                    RenderStates.Default));
        }

        #region OBJECT CREATION

        private Button CreateExitButton()
        {
            GameMenuButton exitButton = new GameMenuButton(GameLoop.DefaultFont, "Exit", true)
            {
                Position = new Vector2f(AppliedDrawView.View.Size.X / 2f, AppliedDrawView.View.Size.Y / 2f + 50f)
            };
            exitButton.Released += (sender, args) => { GameLoop.CallFatality("Game exited."); };
            exitButton.Activated = () => AppliedDrawView.Visible;

            return exitButton;
        }

        private Button CreateSettingsButton()
        {
            GameMenuButton settingsButton = new GameMenuButton(GameLoop.DefaultFont, "Settings", true)
            {
                Position = new Vector2f(AppliedDrawView.View.Size.X / 2f, AppliedDrawView.View.Size.Y / 2f)
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