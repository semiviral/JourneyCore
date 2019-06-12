using System;
using System.Collections.Generic;

namespace JourneyCore.Lib.Game.InputWatchers
{
    public class InputWatch<T> where T : Enum
    {
        private List<Action<T>> Actions { get; }

        public T Input { get; }

        public InputWatch(T input, params Action<T>[] actions)
        {
            Input = input;
            Actions = new List<Action<T>>(actions);
        }

        public void Invoke()
        {
            Actions.ForEach(action => action(Input));
        }

        public void AddAction(Action<T> action)
        {
            Actions.Add(action);
        }

        public void RemoveAction(Action<T> action)
        {
            Actions.Remove(action);
        }
    }
}