using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFML.Window;

namespace JourneyCore.Lib.Game.InputWatchers
{
    public class KeyWatch
    {
        private readonly List<Func<Keyboard.Key, Task>> _keyActions;

        public KeyWatch(Keyboard.Key key, params Func<Keyboard.Key, Task>[] keyActions)
        {
            Key = key;
            _keyActions = new List<Func<Keyboard.Key, Task>>();

            foreach (Func<Keyboard.Key, Task> keyAction in keyActions) _keyActions.Add(keyAction);
        }

        public Keyboard.Key Key { get; }

        public void Invoke()
        {
            foreach (Func<Keyboard.Key, Task> keyAction in _keyActions) keyAction(Key);
        }

        public void AddKeyAction(Func<Keyboard.Key, Task> keyAction)
        {
            _keyActions.Add(keyAction);
        }

        public void RemoveKeyAction(Func<Keyboard.Key, Task> keyAction)
        {
            _keyActions.Remove(keyAction);
        }
    }
}