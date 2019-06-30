using System;
using SFML.Graphics;

namespace JourneyCore.Lib.Display.Drawing
{
    public class DrawItem
    {
        public DateTime MaxLifetime { get; }
        public Action<float> PreDraw { get; }
        public DrawObject DrawSubject { get; }
        public RenderStates SubjectRenderStates { get; set; }

        public DrawItem(DateTime maxLifetime, Action<float> preDraw, DrawObject drawSubject,
            RenderStates subjectRenderStates)
        {
            MaxLifetime = maxLifetime;
            PreDraw = preDraw;
            DrawSubject = drawSubject;
            SubjectRenderStates = subjectRenderStates;
        }
    }
}