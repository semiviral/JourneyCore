using System;
using System.Collections.Generic;
using SFML.Window;

namespace JourneyCore.Lib.Game.InputWatchers
{
    public class KeyWatch
    {
        private List<Action<Keyboard.Key>> KeyActions { get; }

        public Keyboard.Key Key { get; }

        public KeyWatch(Keyboard.Key key, params Action<Keyboard.Key>[] keyActions)
        {
            Key = key;
            KeyActions = new List<Action<Keyboard.Key>>(keyActions);
        }

        public void Invoke()
        {
            KeyActions.ForEach(action => action(Key));
        }

        public void AddKeyAction(Action<Keyboard.Key> keyAction)
        {
            KeyActions.Add(keyAction);
        }

        public void RemoveKeyAction(Action<Keyboard.Key> keyAction)
        {
            KeyActions.Remove(keyAction);
        }
    }
}