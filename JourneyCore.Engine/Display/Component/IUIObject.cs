using System;
using System.Collections.Generic;
using SFML.System;
using SFML.Window;

namespace JourneyCore.Lib.Display.Component
{
    public interface IUiObject
    {
        Vector2u Size { get; set; }
        Vector2f Position { get; set; }
        Vector2f Origin { get; set; }
        Margin Margins { get; set; }

        event EventHandler<SizeEventArgs> Resized;

        IEnumerable<IUiObject> SubscribableObjects();
    }
}