using System;
using SFML.Graphics;

namespace JourneyCore.Lib.Display.Drawing
{
    public class DrawItem
    {
        public DrawItem(DrawObject subject) : this(subject, RenderStates.Default, null, DateTime.MinValue)
        {
        }

        public DrawItem(DrawObject subject, RenderStates subjectRenderStates) : this(subject,
            subjectRenderStates, null, DateTime.MinValue)
        {
        }

        public DrawItem(DrawObject subject, RenderStates subjectRenderStates, Action<float> preDraw) : this(
            subject, subjectRenderStates, preDraw, DateTime.MinValue)
        {
        }

        public DrawItem(DrawObject subject, RenderStates subjectRenderStates, Action<float> preDraw,
            double lifetimeInMilliseconds) : this(subject, subjectRenderStates, preDraw,
            DateTime.Now.AddMilliseconds(lifetimeInMilliseconds))
        {
        }

        public DrawItem(DrawObject subject, RenderStates subjectRenderStates, Action<float> preDraw,
            DateTime maxLifetime)
        {
            Subject = subject;
            PreDraw = preDraw;
            SubjectRenderStates = subjectRenderStates;
            MaxLifetime = maxLifetime;
        }

        public DrawObject Subject { get; }
        public Action<float> PreDraw { get; }
        public RenderStates SubjectRenderStates { get; }
        public DateTime MaxLifetime { get; }
    }
}