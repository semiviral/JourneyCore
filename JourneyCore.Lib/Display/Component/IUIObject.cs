using System;
using System.Collections.Generic;
using SFML.System;
using SFML.Window;

namespace JourneyCore.Lib.Display.Component
{
    public interface IUIObject
    {
        Vector2u Size { get; set; }
        Vector2f Position { get; set; }
        Vector2f Origin { get; set; }

        event EventHandler<SizeEventArgs> Resized;
        
        IEnumerable<IUIObject> SubscribableObjects();
    }
}