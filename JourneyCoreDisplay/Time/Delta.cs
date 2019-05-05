using SFML.System;

namespace JourneyCoreDisplay.Time
{
    public class Delta
    {
        private Clock InternalClock { get; }

        public Delta()
        {
            InternalClock = new Clock();
        }

        public float GetDelta()
        {
            return InternalClock.Restart().AsSeconds();
        }
    }
}
