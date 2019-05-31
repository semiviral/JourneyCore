using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFML.Window;

namespace JourneyCore.Lib.Game.InputWatchers
{
    public class InputWatcher
    {
        public InputWatcher()
        {
            WindowFocused = true;
            WatchedKeys = new List<KeyWatch>();
            WatchedButtons = new List<ButtonWatch>();
        }

        private List<KeyWatch> WatchedKeys { get; }
        private List<ButtonWatch> WatchedButtons { get; }

        public bool WindowFocused { get; set; }

        #region METHODS

        public void AddWatchedInput(Mouse.Button button, Func<Mouse.Button, Task> inputAction)
        {
            if (!GetWatchedButtons().Contains(button))
            {
                WatchedButtons.Add(new ButtonWatch(button));
            }

            GetInputWatch(button).AddButtonAction(inputAction);
        }

        public void AddWatchedInput(Keyboard.Key key, Func<Keyboard.Key, Task> keyAction)
        {
            if (!GetWatchedKeys().Contains(key))
            {
                WatchedKeys.Add(new KeyWatch(key));
            }

            GetInputWatch(key).AddKeyAction(keyAction);
        }

        public void RemoveWatchedInputAction(Keyboard.Key key, Func<Keyboard.Key, Task> inputAction)
        {
            if (!GetWatchedKeys().Contains(key))
            {
                throw new ArgumentException($"Keyboard.Key {key} does not exist in watched inputs list.");
            }

            GetInputWatch(key).RemoveKeyAction(inputAction);
        }

        public void RemoveWatchedInputAction(Mouse.Button button, Func<Mouse.Button, Task> inputAction)
        {
            if (!GetWatchedButtons().Contains(button))
            {
                throw new ArgumentException($"Mouse.Button `{button}` does not exist in watched inputs list.");
            }

            GetInputWatch(button).RemoveButtonAction(inputAction);
        }

        public List<Keyboard.Key> GetWatchedKeys()
        {
            return WatchedKeys.Select(keyWatch => keyWatch.Key).ToList();
        }

        public List<Mouse.Button> GetWatchedButtons()
        {
            return WatchedButtons.Select(mouseButtonWatch => mouseButtonWatch.Button).ToList();
        }

        public KeyWatch GetInputWatch(Keyboard.Key key)
        {
            return WatchedKeys.SingleOrDefault(keyWatch => keyWatch.Key.Equals(key));
        }

        public ButtonWatch GetInputWatch(Mouse.Button button)
        {
            return WatchedButtons.SingleOrDefault(buttonWatch => buttonWatch.Button.Equals(button));
        }


        public async Task CheckWatchedInputs()
        {
            if (!WindowFocused)
            {
                return;
            }

            foreach (Keyboard.Key watchedKey in GetWatchedKeys())
            {
                if (Keyboard.IsKeyPressed(watchedKey))
                {
                    await GetInputWatch(watchedKey).InvokeAsync();
                }
            }

            foreach (Mouse.Button watchedButton in GetWatchedButtons())
            {
                if (Mouse.IsButtonPressed(watchedButton))
                {
                    await GetInputWatch(watchedButton).InvokeAsync();
                }
            }
        }

        #endregion
    }
}