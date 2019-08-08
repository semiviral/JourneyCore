using System;
using System.Collections.Generic;

namespace JourneyCore.Lib.System.Event.Input
{
    public class InputActionList
    {
        public InputActionList(Func<bool> enabledCheck, bool singlePress)
        {
            SinglePress = singlePress;
            EnabledCheck = enabledCheck ?? (() => true);
            Actions = new List<Action>();
            HasReleased = true;
        }

        private bool SinglePress { get; }
        private List<Action> Actions { get; }
        private Func<bool> EnabledCheck { get; }
        private bool HasReleased { get; set; }

        public void AddInputAction(Action inputAction)
        {
            Actions.Add(inputAction);
        }

        public bool ActivatePress(bool pressed)
        {
            if (!CheckActivationRequirements(pressed))
            {
                return false;
            }

            IterateActions();

            return true;
        }

        private bool CheckActivationRequirements(bool pressed)
        {
            if ((EnabledCheck == null) || !EnabledCheck())
            {
                return false;
            }

            bool _invokePress = pressed && HasReleased;

            if (SinglePress)
            {
                HasReleased = !pressed;
            }

            return _invokePress;
        }

        private void IterateActions()
        {
            Actions.ForEach(action => action());
        }
    }
}