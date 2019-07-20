using SFML.System;

namespace JourneyCore.Lib.System.Math
{
    public struct LineSegment
    {
        public Vector2f Start { get; }
        public Vector2f End { get; }

        public LineSegment(Vector2f start, Vector2f end)
        {
            Start = start;
            End = end;
        }
    }
}