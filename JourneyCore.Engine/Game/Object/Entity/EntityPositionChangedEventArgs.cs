using System;
using SFML.System;

namespace JourneyCore.Lib.Game.Object.Entity
{
    public class EntityPositionChangedEventArgs : EventArgs
    {
        public EntityPositionChangedEventArgs(Vector2f oldPosition, Vector2f newPosition)
        {
            OldPosition = oldPosition;
            NewPosition = newPosition;
        }

        public Vector2f OldPosition { get; }
        public Vector2f NewPosition { get; }
    }
}