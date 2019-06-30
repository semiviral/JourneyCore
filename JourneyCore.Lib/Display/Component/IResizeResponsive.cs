using System;
using SFML.System;
using SFML.Window;

namespace JourneyCore.Lib.Display.Component
{
    public interface IResizeResponsive
    {
        Vector2u OriginalWindowSize { get; set; }

        event EventHandler<SizeEventArgs> ParentResized;

        void OnParentResized(object sender, SizeEventArgs args);
    }
}