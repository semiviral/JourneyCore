using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Lib.System.Math
{
    public static class GraphMath
    {
        public static int SquareLength(double x0, double y0, double x1, double y1)
        {
            return SquareLength((int)x0, (int)y0, (int)x1, (int)y1);
        }

        public static int SquareLength(float x0, float y0, float x1, float y1)
        {
            return SquareLength((int)x0, (int)y0, (int)x1, (int)y1);
        }

        public static int SquareLength(int x0, int y0, int x1, int y1)
        {
            int x2 = x0 - x1;
            int y2 = y0 - y1;

            return x2 * x2 + y2 * y2;
        }

        public static double CosFromDegrees(double degrees)
        {
            return global::System.Math.Cos(ToRadians(degrees));
        }

        public static double SinFromDegrees(double degrees)
        {
            return global::System.Math.Sin(ToRadians(degrees));
        }

        public static double ToRadians(double degrees)
        {
            return global::System.Math.PI * degrees / 180d;
        }
        
        public static Vector2f RotatePoint(Vector2f outerCoords, Vector2f origin, float rotation)
        {
            double angleInRadians = rotation * (global::System.Math.PI / 180);
            float cosTheta = (float)global::System.Math.Cos(angleInRadians);
            float sinTheta = (float)global::System.Math.Sin(angleInRadians);

            return new Vector2f(
                cosTheta * (outerCoords.X - origin.X) - sinTheta * (outerCoords.Y - origin.Y) + origin.X,
                sinTheta * (outerCoords.X - origin.X) + cosTheta * (outerCoords.Y - origin.Y) + origin.Y);
        }

        public static bool CircleIntersect(Vector2f circleCenter, IntRect bounds)
        {


            return false;
        }
    }
}