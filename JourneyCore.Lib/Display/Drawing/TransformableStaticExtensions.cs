using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Lib.Display.Drawing
{
    public static class TransformableStaticExtensions
    {
        private static Vector2f GetSpeedModifiedVector(Vector2f vector, int speed)
        {
            return vector * (speed / 10f);
        }

        public static Transformable Move(this Transformable transformable, Vector2f direction, int speed,
            int mapTileSize, float elapsedFrameTime)
        {
            transformable.Position += GetSpeedModifiedVector(direction, speed) * mapTileSize * elapsedFrameTime;

            return transformable;
        }

        // move for having max tile distance
        public static Transformable Move(this Transformable transformable)
        {
            return transformable;
        }

        public static Transformable Rotate(this Transformable transformable, float elapsedTime, float rotation,
            bool isClockwise)
        {
            rotation *= elapsedTime;

            if (!isClockwise)
            {
                rotation *= -1;
            }

            transformable.Rotation += rotation % 360f;

            return transformable;
        }
    }
}