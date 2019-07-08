using System;
using SFML.Graphics;
using SFML.Window;

namespace JourneyCore.Lib.Display.Component
{
    public interface IPressable
    {
        FloatRect Bounds { get; }
        bool RespectsCapture { get; }

        event EventHandler<MouseButtonEventArgs> Pressed;
        event EventHandler<MouseButtonEventArgs> Released;

        bool OnMousePressed(MouseButtonEventArgs args);
        bool OnMouseReleased(MouseButtonEventArgs args);
    }
}