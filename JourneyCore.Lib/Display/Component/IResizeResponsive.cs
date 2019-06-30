using System;
using SFML.Window;

namespace JourneyCore.Lib.Display.Component
{
    public interface IResizeResponsive
    {
        event EventHandler<SizeEventArgs> ParentResized;

        void OnParentResized(object sender, SizeEventArgs args);
    }
}