using System;
using SFML.Graphics;

namespace JourneyCore.Lib.Display.Drawing
{
    public class DrawItem
    {
        public DrawObject DrawSubject { get; }
        public Action<float> PreDraw { get; }
        public RenderStates SubjectRenderStates { get; }
        public DateTime MaxLifetime { get; }

        public DrawItem(DrawObject drawSubject) : this(drawSubject, RenderStates.Default, null, DateTime.MinValue) { }

        public DrawItem(DrawObject drawSubject, RenderStates subjectRenderStates) : this(drawSubject, subjectRenderStates, null, DateTime.MinValue) { }

        public DrawItem(DrawObject drawSubject, RenderStates subjectRenderStates, Action<float> preDraw) : this(drawSubject, subjectRenderStates, preDraw, DateTime.MinValue) { }

        public DrawItem(DrawObject drawSubject, RenderStates subjectRenderStates, Action<float> preDraw,
            double lifetimeInMilliseconds) : this(drawSubject, subjectRenderStates, preDraw, DateTime.Now.AddMilliseconds(lifetimeInMilliseconds)) { }

        public DrawItem(DrawObject drawSubject, RenderStates subjectRenderStates, Action<float> preDraw,
            DateTime maxLifetime)
        {
            DrawSubject = drawSubject;
            PreDraw = preDraw;
            SubjectRenderStates = subjectRenderStates;
            MaxLifetime = maxLifetime;
        }
    }
}