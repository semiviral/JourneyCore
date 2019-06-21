using System;
using System.Collections.Generic;

namespace JourneyCore.Lib.System.Event
{
    public class InputActionList
    {
        private bool _SinglePress { get; }

        public List<Action> Actions { get; }
        public bool ReadyToPress { get; private set; }

        public InputActionList(bool singlePress)
        {
            _SinglePress = singlePress;

            ReadyToPress = true;
            Actions = new List<Action>();
        }

        public bool ActivatePress(bool pressed)
        {
            bool invokePress = pressed && ReadyToPress;

            if (_SinglePress)
            {
                ReadyToPress = !pressed;
            }

            return invokePress;
        }
    }
}