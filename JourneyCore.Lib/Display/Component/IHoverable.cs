using System;
using SFML.Window;

namespace JourneyCore.Lib.Display.Component
{
    public interface IHoverable
    {
        bool IsHovered { get; }

        event EventHandler<MouseMoveEventArgs> Entered;
        event EventHandler<MouseMoveEventArgs> Exited;

        void OnMouseMoved(object sender, MouseMoveEventArgs args);
    }
}