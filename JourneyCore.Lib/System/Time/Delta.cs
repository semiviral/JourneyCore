using SFML.System;

namespace JourneyCore.Lib.System.Time
{
    public class Delta
    {
        public Delta()
        {
            InternalClock = new Clock();
        }

        private Clock InternalClock { get; }

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