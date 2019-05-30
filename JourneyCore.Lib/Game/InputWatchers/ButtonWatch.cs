using System;
using System.Collections.Generic;
using SFML.Window;

namespace JourneyCore.Lib.Game.InputWatchers
{
    public class ButtonWatch
    {
        private readonly List<Action<Mouse.Button>> _buttonActions;

        public ButtonWatch(Mouse.Button button, params Action<Mouse.Button>[] buttonActions)
        {
            Button = button;
            _buttonActions = new List<Action<Mouse.Button>>();

            foreach (Action<Mouse.Button> buttonAction in buttonActions)
            {
                _buttonActions.Add(buttonAction);
            }
        }

        public Mouse.Button Button { get; }

        public void Invoke()
        {
            foreach (Action<Mouse.Button> buttonAction in _buttonActions)
            {
                buttonAction(Button);
            }
        }

        public void AddButtonAction(Action<Mouse.Button> buttonAction)
        {
            _buttonActions.Add(buttonAction);
        }

        public void RemoveButtonAction(Action<Mouse.Button> buttonAction)
        {
            _buttonActions.Remove(buttonAction);
        }
    }
}