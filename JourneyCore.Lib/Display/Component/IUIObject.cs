using SFML.Window;

namespace JourneyCore.Lib.Display.Component
{
    public interface IUIObject
    {
        void OnParentResized(object sender, SizeEventArgs args);
    }
}