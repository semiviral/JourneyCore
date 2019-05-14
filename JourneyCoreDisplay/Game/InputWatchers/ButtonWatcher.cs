using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Window;

namespace JourneyCoreLib.Game.InputWatchers
{
    public class ButtonWatcher
    {
        private List<ButtonWatch> _watchedButtons;

        public ButtonWatcher()
        {
            _watchedButtons = new List<ButtonWatch>();
        }

        #region METHODS

        public void AddWatchedButtonAction(Mouse.Button button, Action<Mouse.Button> buttonAction)
        {
            if (!GetWatchedButtons().Contains(button))
            {
                _watchedButtons.Add(new ButtonWatch(button));
            }

            GetButtonWatch(button).AddButtonAction(buttonAction);
        }

        public void RemoveWatchedButtonAction(Mouse.Button button, Action<Mouse.Button> buttonAction)
        {
            if (!GetWatchedButtons().Contains(button))
            {
                throw new ArgumentException($"Keyboard.Key {button} does not exist in watched keys list.");
            }

            GetButtonWatch(button).RemoveButtonAction(buttonAction);
        }

        public List<Mouse.Button> GetWatchedButtons()
        {
            return _watchedButtons.Select(mouseButtonWatch => mouseButtonWatch.Button).ToList();
        }

        public ButtonWatch GetButtonWatch(Mouse.Button button)
        {
            return _watchedButtons.SingleOrDefault(buttonWatch => buttonWatch.Button.Equals(button));
        }

        public void CheckWatchedKeys()
        {
            foreach (Mouse.Button watchedButton in GetWatchedButtons())
            {
                if (Mouse.IsButtonPressed(watchedButton))
                {
                    GetButtonWatch(watchedButton).Invoke();
                }
            }
        }

        #endregion
    }
}
