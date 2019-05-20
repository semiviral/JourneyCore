using System.ComponentModel;
using System.Timers;
using SFML.Graphics;

namespace JourneyCore.Client
{
    public class FramesPerSecond
    {
        public FramesPerSecond()
        {
            TickTimer = new Timer
            {
                Interval = 500
            };

            TickTimer.SynchronizingObject = TimerLock;
        }

        private Text FramesText { get; set; }
        private Timer TickTimer { get; }
        private ISynchronizeInvoke TimerLock { get; set; }
    }
}