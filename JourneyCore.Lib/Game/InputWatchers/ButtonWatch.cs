using System;
using System.Collections.Generic;
using SFML.Window;

namespace JourneyCore.Lib.Game.InputWatchers
{
    public class ButtonWatch
    {
        private List<Action<Mouse.Button>> ButtonActions { get; }

        public Mouse.Button Button { get; }

        public ButtonWatch(Mouse.Button button, params Action<Mouse.Button>[] buttonActions)
        {
            Button = button;
            ButtonActions = new List<Action<Mouse.Button>>(buttonActions);
        }

        public void Invoke()
        {
            ButtonActions.ForEach(action => action(Button));
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