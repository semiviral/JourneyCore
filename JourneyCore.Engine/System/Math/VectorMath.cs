using SFML.System;

namespace JourneyCore.Lib.System.Math
{
    public static class VectorMath
    {
        public static Vector2f MultiplyBy(this Vector2f vect1, Vector2f vect2)
        {
            return new Vector2f(vect1.X * vect2.X, vect1.Y * vect2.Y);
        }
    }
}