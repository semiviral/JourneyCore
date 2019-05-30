using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFML.Window;

namespace JourneyCore.Lib.Game.InputWatchers
{
    public class KeyWatch
    {
        private List<Func<Keyboard.Key, Task>> KeyActions { get; }

        public KeyWatch(Keyboard.Key key, params Func<Keyboard.Key, Task>[] keyActions)
        {
            Key = key;
            KeyActions = new List<Func<Keyboard.Key, Task>>();

            foreach (Func<Keyboard.Key, Task> keyAction in keyActions)
            {
                KeyActions.Add(keyAction);
            }
        }

        public Keyboard.Key Key { get; }

        public async Task InvokeAsync()
        {
            foreach (Func<Keyboard.Key, Task> keyAction in KeyActions)
            {
                await keyAction(Key);
            }
        }

        public void AddKeyAction(Func<Keyboard.Key, Task> keyAction)
        {
            KeyActions.Add(keyAction);
        }

        public void RemoveKeyAction(Func<Keyboard.Key, Task> keyAction)
        {
            KeyActions.Remove(keyAction);
        }
    }
}