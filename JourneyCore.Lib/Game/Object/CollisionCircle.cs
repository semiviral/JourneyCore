using SFML.System;

namespace JourneyCore.Lib.Game.Object
{
    public struct CollisionCircle : ICollidable
    {
        public Vector2f Position { get; set; }
        public uint Radius { get; set; }

        public CollisionCircle(Vector2f position, uint radius)
        {
            Position = position;
            Radius = radius;
        }

        public CollisionCircle(CollisionCircle collisionCircle)
        {
            Position = collisionCircle.Position;
            Radius = collisionCircle.Radius;
        }

        public bool Intersects(ICollidable collidable)
        {
            switch (collidable)
            {
                case CollisionBox _:
                    break;
                case CollisionCircle _:
                    break;
            }

            return false;
        }
    }
}