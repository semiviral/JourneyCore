using System;
using System.ComponentModel;
using System.Timers;
using SFML.Graphics;

namespace JourneyCore.Client
{
    public class FramesPerSecond
    {
        private Text FramesText { get; set; }
        private Timer TickTimer { get; set; }
        private ISynchronizeInvoke TimerLock { get; set; }

        public FramesPerSecond()
        {
            TickTimer = new Timer
            {
                Interval = 500
            };

            TickTimer.SynchronizingObject = TimerLock;
        }

    }
}
