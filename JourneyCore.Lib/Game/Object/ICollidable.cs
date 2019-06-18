using SFML.System;

namespace JourneyCore.Lib.Game.Object
{
    public interface ICollidable
    {
        Vector2f Position { get; set; }

        bool Intersects(ICollidable collidable);
    }
}