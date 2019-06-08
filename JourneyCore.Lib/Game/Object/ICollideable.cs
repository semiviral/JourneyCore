using System;
using System.Collections.Generic;
using System.Text;

namespace JourneyCore.Lib.Game.Object
{
    public interface ICollidable
    {
        CollisionBoundingType BoundingType { get; }
        bool IsColliding(ICollidable collidable);
    }

    public enum CollisionBoundingType
    {
        Circle,
        Square,
        Triangle
    }
}
