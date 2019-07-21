// resolves issue with using global::System.Math
// within namespace JourneyCore.Lib.System.Math

using System.Collections.Generic;
using JourneyCore.Lib.Game.Object.Collision;
using SFML.Graphics;
using SFML.System;
using static System.Math;

namespace JourneyCore.Lib.System.Math
{
    public static class GraphMath
    {
        public static int SquareLength(double x0, double y0, double x1, double y1)
        {
            return SquareLength((int) x0, (int) y0, (int) x1, (int) y1);
        }

        public static int SquareLength(float x0, float y0, float x1, float y1)
        {
            return SquareLength((int) x0, (int) y0, (int) x1, (int) y1);
        }

        public static int SquareLength(int x0, int y0, int x1, int y1)
        {
            int x2 = x0 - x1;
            int y2 = y0 - y1;

            return (x2 * x2) + (y2 * y2);
        }

        public static double CosFromDegrees(double degrees)
        {
            return Cos(ToRadians(degrees));
        }

        public static double SinFromDegrees(double degrees)
        {
            return Sin(ToRadians(degrees));
        }

        public static double ToRadians(double degrees)
        {
            return (PI * degrees) / 180d;
        }

        public static Vector2f RotatePoint(Vector2f outerPoint, Vector2f centerPoint, float rotation)
        {
            double angleInRadians = rotation * (PI / 180);
            float cosTheta = (float) Cos(angleInRadians);
            float sinTheta = (float) Sin(angleInRadians);

            return new Vector2f(
                ((cosTheta * (outerPoint.X - centerPoint.X)) - (sinTheta * (outerPoint.Y - centerPoint.Y))) +
                centerPoint.X,
                (sinTheta * (outerPoint.X - centerPoint.X)) + (cosTheta * (outerPoint.Y - centerPoint.Y)) +
                centerPoint.Y);
        }

        /// <summary>
        ///     Calculates whether two quads overlap
        /// </summary>
        /// <param name="quad1"></param>
        /// <param name="quad2"></param>
        /// <returns>The Vector2f value required to displace overlapping shapes</returns>
        public static IEnumerable<Vector2f> DiagnasticCollision(CollisionQuad quad1, CollisionQuad quad2)
        {
            foreach (Vector2f vertex in quad1.Vertices)
            {
                LineSegment line1 = new LineSegment(quad1.Origin, vertex);

                Vector2f displacement = new Vector2f(0f, 0f);

                for (int j = 0; j < quad2.Vertices.Length; j++)
                {
                    LineSegment line2 = new LineSegment(quad2.Vertices[j],
                        quad2.Vertices[(j + 1) % quad2.Vertices.Length]);

                    float h = ((line2.End.X - line2.Start.X) * (line1.Start.Y - line1.End.Y)) -
                              ((line1.Start.X - line1.End.X) * (line2.End.Y - line2.Start.Y));
                    float t1 = (((line2.Start.Y - line2.End.Y) * (line1.Start.X - line2.Start.X)) +
                                ((line2.End.X - line2.Start.X) * (line1.Start.Y - line2.Start.Y))) / h;
                    float t2 = (((line1.Start.Y - line1.End.Y) * (line1.Start.X - line2.Start.X)) +
                                ((line1.End.X - line1.Start.X) * (line1.Start.Y - line2.Start.Y))) / h;

                    if ((t1 < 0.0f) || (t1 >= 1.0f) || (t2 < 0.0f) || (t2 >= 1.0f))
                    {
                        continue;
                    }

                    displacement.X += (1.0f - t1) * (line1.End.X - line1.Start.X) * -1;
                    displacement.Y += (1.0f - t1) * (line1.End.Y - line1.Start.Y) * -1;

                    yield return displacement;
                }
            }
        }

        public static IEnumerable<Vector2f> CollisionCheck(CollisionQuad subjectQuad,
            IEnumerable<CollisionQuad> collisionQuads)
        {
            foreach (CollisionQuad quad in collisionQuads)
            {
                FloatRect overlap = new FloatRect();
                bool intersects = subjectQuad.Intersects(quad, out overlap);

                if (!intersects)
                {
                    continue;
                }

                yield return new Vector2f(overlap.Width, overlap.Height);
            }
        }
    }
}