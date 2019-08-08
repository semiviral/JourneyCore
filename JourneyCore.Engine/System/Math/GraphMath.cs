// resolves issue with using global::System.Math
// within namespace JourneyCore.Engine.System.Math

using System.Collections.Generic;
using System.Linq;
using JourneyCore.Lib.Game.Object.Collision;
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
            int _x2 = x0 - x1;
            int _y2 = y0 - y1;

            return (_x2 * _x2) + (_y2 * _y2);
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
            double _angleInRadians = rotation * (PI / 180);
            float _cosTheta = (float) Cos(_angleInRadians);
            float _sinTheta = (float) Sin(_angleInRadians);

            return new Vector2f(
                ((_cosTheta * (outerPoint.X - centerPoint.X)) - (_sinTheta * (outerPoint.Y - centerPoint.Y))) +
                centerPoint.X,
                (_sinTheta * (outerPoint.X - centerPoint.X)) + (_cosTheta * (outerPoint.Y - centerPoint.Y)) +
                centerPoint.Y);
        }

        /// <summary>
        ///     Calculates whether two quads overlap
        /// </summary>
        /// <param name="quad1"></param>
        /// <param name="quad2"></param>
        /// <returns>The Vector2f value required to displace overlapping shapes</returns>
        public static IEnumerable<Vector2f> GetDiagnasticCollisionOffsets(CollisionQuad quad1, CollisionQuad quad2)
        {
            Vector2f[] _quad1Points = quad1.GetAllPointsScaled().ToArray();
            Vector2f[] _quad2Points = quad2.GetAllPointsScaled().ToArray();

            for (uint _i = 0; _i < _quad1Points.Length; _i++)
            {
                // outer point math works with assumption that the quad's origin is the same as its position
                LineSegment _line1 = new LineSegment(quad1.Position,
                    quad1.Position + (_quad1Points[_i] - quad1.Origin.MultiplyBy(quad1.Scale)));

                for (uint _j = 0; _j < _quad2Points.Length; _j++)
                {
                    LineSegment _line2 = new LineSegment(quad2.Position + _quad2Points[_j],
                        quad2.Position + _quad2Points[(_j + 1) % _quad2Points.Length]);

                    float _h = ((_line2.End.X - _line2.Start.X) * (_line1.Start.Y - _line1.End.Y)) -
                              ((_line1.Start.X - _line1.End.X) * (_line2.End.Y - _line2.Start.Y));
                    float _t1 = (((_line2.Start.Y - _line2.End.Y) * (_line1.Start.X - _line2.Start.X)) +
                                ((_line2.End.X - _line2.Start.X) * (_line1.Start.Y - _line2.Start.Y))) / _h;
                    float _t2 = (((_line1.Start.Y - _line1.End.Y) * (_line1.Start.X - _line2.Start.X)) +
                                ((_line1.End.X - _line1.Start.X) * (_line1.Start.Y - _line2.Start.Y))) / _h;

                    if (_t1 < 0.0f || _t1 >= 1.0f || _t2 < 0.0f || _t2 >= 1.0f)
                    {
                        continue;
                    }

                    yield return (1.0f - _t1) * new Vector2f(_line1.End.X - _line1.Start.X, _line1.End.Y - _line1.Start.Y);
                }
            }
        }
    }
}