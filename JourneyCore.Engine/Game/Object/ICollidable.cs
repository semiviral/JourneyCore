using System;
using SFML.System;

namespace JourneyCore.Lib.Game.Object
{
    public interface ICollidable
    {
        Vector2f Size { get; set; }
        Vector2f Position { get; set; }
        Vector2f Origin { get; }
        bool Mobile { get; set; }
        
        event EventHandler<Vector2f> Colliding;
    }
}