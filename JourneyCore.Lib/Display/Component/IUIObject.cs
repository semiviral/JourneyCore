using System;
using System.Collections.Generic;
using System.Text;
using SFML.Window;

namespace JourneyCore.Lib.Display.Component
{
    public interface IUIObject
    {
        void OnWindowResized(object sender, SizeEventArgs args);
    }
}
