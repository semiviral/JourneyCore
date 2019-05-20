using SFML.System;

namespace JourneyCore.Lib.Graphics
{
    public struct QuadCoords
    {
        public Vector2f TopLeft { get; set; }
        public Vector2f TopRight { get; set; }
        public Vector2f BottomRight { get; set; }
        public Vector2f BottomLeft { get; set; }
    }
}