using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Lib.System.Static
{
    public static class TransformableStaticExtensions
    {
        private static Vector2f GetSpeedModifiedVector(Vector2f vector, int speed)
        {
            return vector * (speed / 10f);
        }

        public static Vector2f TryMovement(this Transformable transformable, Vector2f direction, int speed,
            int mapTileSize, float elapsedFrameTime)
        {
            Vector2f _position = transformable.Position;

            _position += GetSpeedModifiedVector(direction, speed) * mapTileSize * elapsedFrameTime;

            return _position;
        }

        public static Transformable Move(this Transformable transformable, Vector2f direction, int speed,
            int mapTileSize, float elapsedFrameTime)
        {
            transformable.Position += GetSpeedModifiedVector(direction, speed) * mapTileSize * elapsedFrameTime;

            return transformable;
        }

        public static float TryRotation(this Transformable transformable, float rotation, float elapsedFrameTime,
            bool isClockwise)
        {
            rotation *= elapsedFrameTime;

            if (!isClockwise)
            {
                rotation *= -1f;
            }

            return (transformable.Rotation + rotation) % 360;
        }

        public static Transformable Rotate(this Transformable transformable, float rotation, float elapsedFrameTime,
            bool isClockwise)
        {
            rotation *= elapsedFrameTime;

            if (!isClockwise)
            {
                rotation *= -1f;
            }

            transformable.Rotation += rotation % 360f;

            return transformable;
        }
    }
}