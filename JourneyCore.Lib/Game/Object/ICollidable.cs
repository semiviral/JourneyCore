using System;
using SFML.System;

namespace JourneyCore.Lib.Game.Object
{
    public interface ICollidable
    {
        Vector2f Position { get; set; }
        Vector2f CenterPoint { get; }

        event EventHandler<Vector2f> Colliding;
    }
}