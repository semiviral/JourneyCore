using System;
using SFML.Window;

namespace JourneyCore.Lib.Display.Component
{
    public interface IScrollable
    {
        event EventHandler<MouseWheelScrollEventArgs> Scrolled;

        void OnMouseScrolled(object sender, MouseWheelScrollEventArgs args);
    }
}