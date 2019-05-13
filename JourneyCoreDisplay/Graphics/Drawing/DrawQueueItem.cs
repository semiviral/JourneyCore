using SFML.Graphics;
using System;

namespace JourneyCoreLib.Drawing
{
    public class DrawQueueItem
    {
        public DrawPriority PriorityLevel { get; }
        public Action<int, RenderWindow> Draw { get; }

        public DrawQueueItem(DrawPriority priorityLevel, Action<int, RenderWindow> drawingFunction)
        {
            PriorityLevel = priorityLevel;
            Draw = drawingFunction;
        }
    }
}
