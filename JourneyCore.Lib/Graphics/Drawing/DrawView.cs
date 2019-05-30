using System;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;
using SFML.Window;

namespace JourneyCore.Lib.Graphics.Drawing
{
    public class DrawView
    {
        public string Name { get; }
        public View View { get; }
        private SortedList<int, List<DrawItem>> DrawQueue { get; }
        
        public DrawView(string name, View view)
        {
            Name = name;
            View = view;

            DrawQueue = new SortedList<int, List<DrawItem>>();
        }

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

            List<Tuple<int, DrawItem>> toRemove = new List<Tuple<int, DrawItem>>();

            if (DrawQueue.Count > 0)
            {
                foreach ((int key, List<DrawItem> drawItems) in DrawQueue)
                {
                    foreach (DrawItem drawItem in drawItems)
                    {
                        if (drawItem.Lifetime.Ticks != DateTime.MinValue.Ticks &&
                            drawItem.Lifetime.Ticks < absoluteNow.Ticks)
                        {
                            toRemove.Add(new Tuple<int, DrawItem>(key, drawItem));
                            continue;
                        }

                        drawItem.Draw(window, frameTime);
                    }
                }
            }

            foreach ((int key, DrawItem drawItem) in toRemove)
            {
                DrawQueue[key].Remove(drawItem);
            }
        }
    }
}
