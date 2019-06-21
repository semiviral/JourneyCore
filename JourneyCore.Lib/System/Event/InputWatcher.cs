using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Window;

namespace JourneyCore.Lib.System.Event
{
    public class InputWatcher
    {
        private Dictionary<Keyboard.Key, InputActionList> WatchedKeys { get; }
        private Dictionary<Mouse.Button, InputActionList> WatchedButtons { get; }

        public Func<bool> EnableInputFunc { get; set; }

        public InputWatcher()
        {
            EnableInputFunc = () => true;
            WatchedKeys = new Dictionary<Keyboard.Key, InputActionList>();
            WatchedButtons = new Dictionary<Mouse.Button, InputActionList>();
        }

        #region METHODS

        public void AddWatchedInput(Keyboard.Key key, Action inputAction, bool singlePress = false)
        {
            if (!GetWatchedKeys().Contains(key))
            {
                WatchedKeys.Add(key, new InputActionList(singlePress));
            }

            WatchedKeys[key].Actions.Add(inputAction);
        }

        public void AddWatchedInput(Mouse.Button button, Action inputAction, bool singlePress = false)
        {
            if (!GetWatchedButtons().Contains(button))
            {
                WatchedButtons.Add(button, new InputActionList(singlePress));
            }

            WatchedButtons[button].Actions.Add(inputAction);
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
            if (!WatchedKeys[key].ActivatePress(Keyboard.IsKeyPressed(key)))
            {
                return;
            }

            InvokeInput(key);
        }

        private void CheckAndExecuteWatchedInput(Mouse.Button button)
        {
            if (!WatchedButtons[button].ActivatePress(Mouse.IsButtonPressed(button)))
            {
                return;
            }

            InvokeInput(button);
        }

        private void InvokeInput(Keyboard.Key key)
        {
            WatchedKeys[key].Actions.ForEach(action => action());
        }

        private void InvokeInput(Mouse.Button button)
        {
            WatchedButtons[button].Actions.ForEach(action => action());
        }

        #endregion
    }
}