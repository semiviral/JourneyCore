using SFML.Graphics;
using System;

namespace JourneyCoreDisplay.Drawing
{
    public class DrawQueueItem
    {
        public DrawPriority PriorityLevel { get; }
        public Action<float, RenderWindow> Draw { get; }

        public DrawQueueItem(DrawPriority priorityLevel, Action<float, RenderWindow> drawingFunction)
        {
            PriorityLevel = priorityLevel;
            Draw = drawingFunction;
        }
    }
}
