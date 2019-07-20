using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Window;

namespace JourneyCore.Lib.System.Event.Input
{
    public class InputWatcher
    {
        public InputWatcher()
        {
            WatchedKeys = new Dictionary<Keyboard.Key, InputActionList>();
            WatchedButtons = new Dictionary<Mouse.Button, InputActionList>();
        }

        private Dictionary<Keyboard.Key, InputActionList> WatchedKeys { get; }
        private Dictionary<Mouse.Button, InputActionList> WatchedButtons { get; }

        #region METHODS

        public void AddWatchedInput(Keyboard.Key key, Action inputAction, Func<bool> enabledCheck = null,
            bool singlePress = false)
        {
            if (!GetWatchedKeys().Contains(key)) WatchedKeys.Add(key, new InputActionList(enabledCheck, singlePress));

            WatchedKeys[key].AddInputAction(inputAction);
        }

        public void AddWatchedInput(Mouse.Button button, Action inputAction, Func<bool> enabledCheck = null,
            bool singlePress = false)
        {
            if (!GetWatchedButtons().Contains(button))
                WatchedButtons.Add(button, new InputActionList(enabledCheck, singlePress));

            WatchedButtons[button].AddInputAction(inputAction);
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
            GetWatchedKeys()?.ForEach(CheckAndExecuteWatchedInput);
            GetWatchedButtons()?.ForEach(CheckAndExecuteWatchedInput);
        }

        private void CheckAndExecuteWatchedInput(Keyboard.Key key)
        {
            WatchedKeys[key].ActivatePress(Keyboard.IsKeyPressed(key));
        }

        private void CheckAndExecuteWatchedInput(Mouse.Button button)
        {
            WatchedButtons[button].ActivatePress(Mouse.IsButtonPressed(button));
        }

        #endregion
    }
}