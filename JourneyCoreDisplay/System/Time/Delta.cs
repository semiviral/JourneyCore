using SFML.System;

namespace JourneyCoreLib.Time
{
    public class Delta
    {
        private Clock InternalClock { get; }

        public Delta()
        {
            InternalClock = new Clock();
        }

        public int GetDelta()
        {
            return InternalClock.Restart().AsMilliseconds();
        }
    }
}
