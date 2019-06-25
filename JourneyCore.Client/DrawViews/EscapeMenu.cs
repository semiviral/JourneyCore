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
        }

        private void PopulateEscapeMenuView()
        {
            // semi-transparent background rectangle
            GameWindow.AddDrawItem(DrawViewLayer.EscapeMenu, 0,
                new DrawItem(Guid.NewGuid().ToString(), DateTime.MinValue, null,
                    new DrawObject(typeof(RectangleShape),
                        new RectangleShape((Vector2f)GameWindow.Size) { FillColor = new Color(0, 0, 0, 155) }),
                    RenderStates.Default));

            Button exitButton = CreateExitButton();
            GameWindow.SubscribeUiObject(DrawViewLayer.EscapeMenu, 0, exitButton);
            GameWindow.AddDrawItem(DrawViewLayer.EscapeMenu, 10,
                new DrawItem(Guid.NewGuid().ToString(), DateTime.MinValue, null,
                    new DrawObject(typeof(Button), exitButton), RenderStates.Default));
        }

        #region OBJECT CREATION

        private Button CreateExitButton()
        {
            Font defaultFont =
                new Font(
                    @"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\Assets\Fonts\Avara.ttf");

            DrawView settingsDrawView = GameWindow.GetDrawView(DrawViewLayer.EscapeMenu);

            Button exitButton = new Button(defaultFont, "Exit")
            {
                Position = new Vector2f(settingsDrawView.View.Size.X / 2f, settingsDrawView.View.Size.Y / 2f),
                Size = new Vector2f(100f, 100f),
                BackgroundColor = Color.Cyan
            };
            exitButton.Origin = exitButton.Size / 2f;
            exitButton.BackgroundColor = Color.Transparent;
            exitButton.Entered += (sender, args) =>
            {
                exitButton.ForegroundColor = exitButton.IsPressed ? Color.Red : Color.Cyan;
            };
            exitButton.Exited += (sender, args) =>
            {
                if (!exitButton.IsPressed)
                {
                    exitButton.ForegroundColor = Color.White;
                }
            };
            exitButton.Pressed += (sender, args) => { exitButton.ForegroundColor = Color.Red; };
            exitButton.Released += (sender, args) =>
            {
                exitButton.ForegroundColor = exitButton.IsHovered ? Color.Cyan : Color.White;
            };
            exitButton.Released += (sender, args) => { GameLoop.CallFatality("Game exited."); };
            exitButton.Activated = () => settingsDrawView.Visible;

            return exitButton;
        }

        #endregion
    }
}