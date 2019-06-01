using System;
using System.Collections.Generic;
using SFML.Graphics;

namespace JourneyCore.Lib.Graphics.Drawing
{
    public class DrawView
    {
        public DrawView(string name, View view)
        {
            Name = name;
            View = view;

            DrawQueue = new SortedList<int, List<DrawItem>>();
        }

        public string Name { get; }
        public View View { get; }
        private SortedList<int, List<DrawItem>> DrawQueue { get; }

        public void AddDrawItem(int priority, DrawItem drawItem)
        {
            if (!DrawQueue.Keys.Contains(priority))
            {
                DrawQueue.Add(priority, new List<DrawItem>());
            }

            DrawQueue[priority].Add(drawItem);
        }

        public void Draw(RenderWindow window, float frameTime)
        {
            DateTime absoluteNow = DateTime.Now;

            if (DrawQueue.Count <= 0)
            {
                return;
            }

            foreach ((int key, List<DrawItem> drawItems) in DrawQueue)
            {
                drawItems.RemoveAll(drawItem =>
                    drawItem.Lifetime.Ticks != DateTime.MinValue.Ticks &&
                    drawItem.Lifetime.Ticks < absoluteNow.Ticks);

                foreach (DrawItem drawItem in drawItems)
                {
                    drawItem.Draw(window, frameTime);
                }
            }
        }
    }
}