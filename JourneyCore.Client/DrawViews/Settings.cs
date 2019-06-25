using System;
using System.Collections.Generic;
using System.Text;
using JourneyCore.Lib.Display;
using JourneyCore.Lib.Display.Component;
using JourneyCore.Lib.Display.Drawing;
using SFML.Graphics;

namespace JourneyCore.Client.DrawViews
{
    public class Settings
    {
        private GameWindow GameWindow { get; }

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
            GameWindow.CreateDrawView(new DrawView(DrawViewLayer.Settings, new View(new FloatRect(0f, 0f, GameWindow.Size.X, GameWindow.Size.Y)) { Viewport = new FloatRect(0f, 0f, 1f, 1f) }));
        }

        private void PopulateSettingsView()
        {
            Button MinimapBrightnessIncreaseButton = new Button(null, "+")
                { };
        }
    }
}
