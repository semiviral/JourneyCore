using System;
using SFML.Window;

namespace JourneyCore.Lib.Display.Component
{
    public interface IPressable
    {
        event EventHandler<MouseButtonEventArgs> Pressed;
        event EventHandler<MouseButtonEventArgs> Released;

        void OnMousePressed(object sender, MouseButtonEventArgs args);
        void OnMouseReleased(object sender, MouseButtonEventArgs args);
    }
}