using System;
using System.Collections.Generic;
using SFML.Graphics;

namespace JourneyCore.Lib.Graphics.Drawing
{
    public class DrawView
    {
        public DrawView(string name, int layer, View view)
        {
            Name = name;
            Layer = layer;
            View = view;

            DrawQueue = new SortedList<int, List<DrawItem>>();
        }

        public string Name { get; }
        public int Layer { get; }
        public View View { get; }
        private SortedList<int, List<DrawItem>> DrawQueue { get; }

        public void AddDrawItem(int layer, DrawItem drawItem)
        {
            if (!DrawQueue.Keys.Contains(layer))
            {
                DrawQueue.Add(layer, new List<DrawItem>());
            }

            DrawQueue[layer].Add(drawItem);
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
                try
                {
                    drawItems.RemoveAll(drawItem =>
                        drawItem == null ||
                        drawItem.Lifetime.Ticks != DateTime.MinValue.Ticks &&
                        drawItem.Lifetime.Ticks < absoluteNow.Ticks);
                }
                catch (Exception ex) { }

                foreach (DrawItem drawItem in drawItems)
                {
                    drawItem.Draw(window, frameTime);
                }
            }
        }
    }
}