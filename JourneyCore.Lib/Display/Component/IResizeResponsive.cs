using SFML.System;
using SFML.Window;

namespace JourneyCore.Lib.Display.Component
{
    public interface IResizeResponsive
    {
        Vector2u OriginalWindowSize { get; set; }
        
        void OnParentResized(object sender, SizeEventArgs args);
    }
}