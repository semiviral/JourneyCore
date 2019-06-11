using System;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace JourneyCore.Lib.Display {
    public interface IGameWindow {
        RenderWindow SetActive(bool activeState);
        Vector2i GetRelativeMousePosition();
        event EventHandler Closed;
        event EventHandler GainedFocus;
        event EventHandler LostFocus;
        event EventHandler<MouseWheelScrollEventArgs> MouseWheelScrolled;
        event EventHandler<MouseMoveEventArgs> MouseMoved;
        event EventHandler<MouseButtonEventArgs> MouseButtonPressed;
        event EventHandler<MouseButtonEventArgs> MouseButtonReleased;
    }
}