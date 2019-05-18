using SFML.Graphics;
using System;

namespace JourneyCore.Lib.Graphics.Drawing
{
    public class DrawQueueItem
    {
        public DrawPriority PriorityLevel { get; }
        public Action<float, RenderWindow> Draw { get; }
        public DateTime Lifetime { get; }

        public DrawQueueItem(DrawPriority priorityLevel, Action<float, RenderWindow> drawingFunction, DateTime lifetime = default)
        {
            PriorityLevel = priorityLevel;
            Draw = drawingFunction;
            Lifetime = lifetime;
        }
    }
}
