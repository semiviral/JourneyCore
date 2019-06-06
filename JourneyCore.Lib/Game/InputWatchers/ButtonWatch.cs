using System;
using System.Collections.Generic;
using SFML.Window;

namespace JourneyCore.Lib.Game.InputWatchers
{
    public class ButtonWatch
    {
        public ButtonWatch(Mouse.Button button, params Action<Mouse.Button>[] buttonActions)
        {
            Button = button;
            ButtonActions = new List<Action<Mouse.Button>>();

            foreach (Action<Mouse.Button> buttonAction in buttonActions)
            {
                ButtonActions.Add(buttonAction);
            }
        }

        private List<Action<Mouse.Button>> ButtonActions { get; }

        public Mouse.Button Button { get; }

        public void Invoke()
        {
            foreach (Action<Mouse.Button> buttonAction in ButtonActions)
            {
                buttonAction.Invoke(Button);
            }
        }

        public void AddButtonAction(Action<Mouse.Button> buttonAction)
        {
            ButtonActions.Add(buttonAction);
        }

        public void RemoveButtonAction(Action<Mouse.Button> buttonAction)
        {
            ButtonActions.Remove(buttonAction);
        }
    }
}