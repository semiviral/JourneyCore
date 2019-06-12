using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Window;

namespace JourneyCore.Lib.Game.InputWatchers
{
    public class InputWatcher
    {
        private List<InputWatch<Keyboard.Key>> WatchedKeys { get; }
        private List<InputWatch<Mouse.Button>> WatchedButtons { get; }

        public Func<bool> EnableInputFunc { get; set; }

        public InputWatcher()
        {
            EnableInputFunc = () => true;
            WatchedKeys = new List<InputWatch<Keyboard.Key>>();
            WatchedButtons = new List<InputWatch<Mouse.Button>>();
        }

        #region METHODS

        public void AddWatchedInput(Mouse.Button button, Action<Mouse.Button> inputAction)
        {
            if (!GetWatchedButtons().Contains(button))
            {
                WatchedButtons.Add(new InputWatch<Mouse.Button>(button));
            }

            GetInputWatch(button).AddAction(inputAction);
        }

        public void AddWatchedInput(Keyboard.Key key, Action<Keyboard.Key> keyAction, bool waitReleased = false)
        {
            if (!GetWatchedKeys().Contains(key))
            {
                WatchedKeys.Add(new InputWatch<Keyboard.Key>(key));
            }

            GetInputWatch(key).AddAction(keyAction);
        }

        public void RemoveWatchedInputAction(Keyboard.Key key, Action<Keyboard.Key> inputAction, bool waitReleased = false)
        {
            if (!GetWatchedKeys().Contains(key))
            {
                throw new ArgumentException($"Keyboard.Key {key} does not exist in watched inputs list.");
            }

            GetInputWatch(key).RemoveAction(inputAction);
        }

        public void RemoveWatchedInputAction(Mouse.Button button, Action<Mouse.Button> inputAction)
        {
            if (!GetWatchedButtons().Contains(button))
            {
                throw new ArgumentException($"Mouse.Button `{button}` does not exist in watched inputs list.");
            }

            GetInputWatch(button).RemoveAction(inputAction);
        }

        public List<Keyboard.Key> GetWatchedKeys()
        {
            return WatchedKeys.Select(keyWatch => keyWatch.Input).ToList();
        }

        public List<Mouse.Button> GetWatchedButtons()
        {
            return WatchedButtons.Select(mouseButtonWatch => mouseButtonWatch.Input).ToList();
        }

        public InputWatch<Keyboard.Key> GetInputWatch(Keyboard.Key key)
        {
            return WatchedKeys.SingleOrDefault(keyWatch => keyWatch.Input.Equals(key));
        }

        public InputWatch<Mouse.Button> GetInputWatch(Mouse.Button button)
        {
            return WatchedButtons.SingleOrDefault(inputWatch => inputWatch.Input.Equals(button));
        }


        public void CheckWatchedInputs()
        {
            if (!EnableInputFunc())
            {
                //return;
            }

            GetWatchedKeys()?.ForEach(CheckAndExecuteWatchedInput);

            GetWatchedButtons()?.ForEach(CheckAndExecuteWatchedInput);
        }

        private void CheckAndExecuteWatchedInput(Keyboard.Key key)
        {
            if (!Keyboard.IsKeyPressed(key))
            {
                return;
            }

            GetInputWatch(key)?.Invoke();
        }

        private void CheckAndExecuteWatchedInput(Mouse.Button button)
        {
            if (!Mouse.IsButtonPressed(button))
            {
                return;
            }

            GetInputWatch(button)?.Invoke();
        }

        #endregion
    }
}