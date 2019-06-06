using System;
using SFML.Graphics;

namespace JourneyCore.Lib.Graphics.Drawing
{
    public class DrawItem
    {
        public string Guid { get; }
        public DateTime MaxLifetime { get; }
        public Action<float> PreDraw { get; }
        public DrawObject DrawSubject { get; }
        public RenderStates SubjectRenderStates { get; set; }

        public DrawItem(string guid, DateTime maxLifetime, Action<float> preDraw, DrawObject drawSubject,
            RenderStates subjectRenderStates)
        {
            Guid = guid;
            MaxLifetime = maxLifetime;
            PreDraw = preDraw;
            DrawSubject = drawSubject;
            SubjectRenderStates = subjectRenderStates;
        }
    }
}