using SFML.Window;

namespace JourneyCoreLib.System.Event
{
    public class GameKeyEventArgs : KeyEventArgs
    {
        public bool IsPressed { get; set; }

        public GameKeyEventArgs(KeyEvent keyEvent, bool isPressed) : base(keyEvent)
        {
            IsPressed = isPressed;
        }
    }
}
