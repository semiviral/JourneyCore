using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFML.Window;

namespace JourneyCore.Lib.Game.InputWatchers
{
    public class ButtonWatch
    {
        private List<Func<Mouse.Button, Task>>  ButtonActions { get; }

        public ButtonWatch(Mouse.Button button, params Func<Mouse.Button, Task>[] buttonActions)
        {
            Button = button;
            ButtonActions = new List<Func<Mouse.Button, Task>>();

            foreach (Func<Mouse.Button, Task> buttonAction in buttonActions)
            {   
                ButtonActions.Add(buttonAction);
            }
        }

        public Mouse.Button Button { get; }

        public async Task InvokeAsync()
        {
            foreach (Func<Mouse.Button, Task> buttonAction in ButtonActions)
            {
                await buttonAction.Invoke(Button);
            }
        }

        public void AddButtonAction(Func<Mouse.Button, Task> buttonAction)
        {
            ButtonActions.Add(buttonAction);
        }

        public void RemoveButtonAction(Func<Mouse.Button, Task> buttonAction)
        {
            ButtonActions.Remove(buttonAction);
        }
    }
}