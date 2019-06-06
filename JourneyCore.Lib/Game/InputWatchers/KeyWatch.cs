using System;
using System.Collections.Generic;
using SFML.Window;

namespace JourneyCore.Lib.Game.InputWatchers
{
    public class KeyWatch
    {
        public KeyWatch(Keyboard.Key key, params Action<Keyboard.Key>[] keyActions)
        {
            Key = key;
            KeyActions = new List<Action<Keyboard.Key>>();

            foreach (Action<Keyboard.Key> keyAction in keyActions)
            {
                KeyActions.Add(keyAction);
            }
        }

        private List<Action<Keyboard.Key>> KeyActions { get; }

        public Keyboard.Key Key { get; }

        public void Invoke()
        {
            foreach (Action<Keyboard.Key> keyAction in KeyActions)
            {
                keyAction(Key);
            }
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