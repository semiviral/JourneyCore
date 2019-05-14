using System;
using System.Collections.Generic;
using System.Text;
using JourneyCoreLib.System.Event;
using SFML.Window;

namespace JourneyCoreLib.Game.Keys
{
    public class KeyWatch
    {
        public bool IsPressed { get; set; }
        public Keyboard.Key Key { get; }
        private List<Action<Keyboard.Key>> _keyActions;

        public KeyWatch(Keyboard.Key key, params Action<Keyboard.Key>[] keyActions)
        {
            IsPressed = false;
            Key = key;
            _keyActions = new List<Action<Keyboard.Key>>();

            foreach (Action<Keyboard.Key> keyAction in keyActions)
            {
                _keyActions.Add(keyAction);
            }
        }

        public void Invoke()
        {
            foreach (Action<Keyboard.Key> keyAction in _keyActions)
            {
                keyAction(Key);
            }
        }

        public void AddKeyAction(Action<Keyboard.Key> keyAction)
        {
            _keyActions.Add(keyAction);
        }

        public void RemoveKeyAction(Action<Keyboard.Key> keyAction)
        {
            _keyActions.Remove(keyAction);
        }
    }

    public enum KeyActionType
    {
        Press,
        Release,
    }
}
