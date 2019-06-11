using SFML.System;

namespace JourneyCore.Lib.Game.Object
{
    public struct CollisionBox : ICollidable
    {
        public CollisionBoundingType BoundingType { get; set; }
        public Vector2f Position { get; set; }
        public Vector2f Size { get; set; }

        public CollisionBox(CollisionBoundingType boundingType, Vector2f position, Vector2f size)
        {
            BoundingType = CollisionBoundingType.Square;
            Position = position;
            Size = size;
        }

        public CollisionBox(CollisionBox collisionBox)
        {
            BoundingType = collisionBox.BoundingType;
            Position = collisionBox.Position;
            Size = collisionBox.Size;
        }

        public bool IsColliding(ICollidable collidable)
        {
            return false;
        }
    }
}
