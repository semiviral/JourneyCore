using SFML.System;

namespace JourneyCore.Lib.Game.Object
{
    public struct CollisionBox
    {
        public Vector2f Position { get; set; }
        public Vector2f Size { get; set; }

        public CollisionBox(Vector2f position, Vector2f size)
        {
            Position = position;
            Size = size;
        }

        public CollisionBox(CollisionBox collisionBox)
        {
            Position = collisionBox.Position;
            Size = collisionBox.Size;
        }
    }
}