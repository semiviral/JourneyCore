using System;
using System.Collections.Generic;
using SFML.Window;

namespace JourneyCoreLib.Game.InputWatchers
{
    public class KeyWatch
    {
        public Keyboard.Key Key { get; }
        private List<Action<Keyboard.Key>> _keyActions;

        public KeyWatch(Keyboard.Key key, params Action<Keyboard.Key>[] keyActions)
        {
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
}
