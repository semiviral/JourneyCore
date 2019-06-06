using SFML.System;

namespace JourneyCore.Lib.System.Time
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

        public int GetDeltaMilliseconds()
        {
            return InternalClock.Restart().AsMilliseconds();
        }
    }
}