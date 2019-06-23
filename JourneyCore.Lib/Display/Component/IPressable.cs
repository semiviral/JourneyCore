using System;
using SFML.Window;

namespace JourneyCore.Lib.Display.Component
{
    public interface IPressable
    {
        event EventHandler<MouseButtonEventArgs> Pressed;
        event EventHandler<MouseButtonEventArgs> Released;
    }
}