using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Window;

namespace JourneyCore.Lib.Game.InputWatchers
{
    public class InputWatcher
    {
        private Dictionary<Keyboard.Key, List<Action>> WatchedKeys { get; }
        private Dictionary<Mouse.Button, List<Action>> WatchedButtons { get; }

        public Func<bool> EnableInputFunc { get; set; }

        public InputWatcher()
        {
            EnableInputFunc = () => true;
            WatchedKeys = new Dictionary<Keyboard.Key, List<Action>>();
            WatchedButtons = new Dictionary<Mouse.Button, List<Action>>();
        }

        #region METHODS

        public void AddWatchedInput(Keyboard.Key key, Action inputAction, bool singlePress = false)
        {
            if (!GetWatchedKeys().Contains(key))
            {
                WatchedKeys.Add(key, new List<Action>());
            }

            WatchedKeys[key].Add(inputAction);
        }

        public void AddWatchedInput(Mouse.Button button, Action inputAction, bool singlePress = false)
        {
            if (!GetWatchedButtons().Contains(button))
            {
                WatchedButtons.Add(button, new List<Action>());
            }

            WatchedButtons[button].Add(inputAction);
        }

        public List<Keyboard.Key> GetWatchedKeys()
        {
            return WatchedKeys.Keys.ToList();
        }

        public List<Mouse.Button> GetWatchedButtons()
        {
            return WatchedButtons.Keys.ToList();
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

            InvokeInput(key);
        }

        private void CheckAndExecuteWatchedInput(Mouse.Button button)
        {
            if (!Mouse.IsButtonPressed(button))
            {
                return;
            }

            InvokeInput(button);
        }

        private void InvokeInput(Keyboard.Key key)
        {
            WatchedKeys[key].ForEach(action => action());
        }

        private void InvokeInput(Mouse.Button button)
        {
            WatchedButtons[button].ForEach(action => action());
        }

        #endregion
    }
}