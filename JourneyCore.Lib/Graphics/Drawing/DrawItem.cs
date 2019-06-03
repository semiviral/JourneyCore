using System;
using SFML.Graphics;

namespace JourneyCore.Lib.Graphics.Drawing
{
    public class DrawItem
    {
        public DrawItem(string guid, int lifetimeInMilliseconds, Action<RenderWindow, float> draw)
        {
            Guid = guid;
            Lifetime = lifetimeInMilliseconds == 0
                ? DateTime.MinValue
                : DateTime.Now.AddMilliseconds(lifetimeInMilliseconds);
            Draw = (window, frameTime) =>
            {
                window.SetActive(true);

                draw(window, frameTime);

                window.SetActive(false);
            };
        }

        public string Guid { get; }
        public Action<RenderWindow, float> Draw { get; }
        public DateTime Lifetime { get; }
    }
}