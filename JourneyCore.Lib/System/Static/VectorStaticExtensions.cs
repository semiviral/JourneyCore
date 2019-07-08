using SFML.System;

namespace JourneyCore.Lib.System.Static
{
    public static class VectorStaticExtensions
    {
        public static Vector2f ZeroPointRound(this Vector2f inputVector)
        {
            inputVector.X = (int)inputVector.X;
            inputVector.Y = (int)inputVector.Y;

            return inputVector;
        }
    }
}