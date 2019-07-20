using System;
using JourneyCore.Lib.Game.Object.Entity;
using SFML.System;

namespace JourneyCore.Lib.Game.Object
{
    public interface IAnchor
    {
        event EventHandler<EntityPositionChangedEventArgs> PositionChanged;
        event EventHandler<float> RotationChanged;

        void AnchorItem(IAnchorable anchorableItem);
        void AnchorItemPosition(IAnchorable anchorableItem, Vector2f positionOffset);
        void AnchorItemRotation(IAnchorable anchorableItem);
    }

    public interface IAnchorable
    {
        Vector2f Position { get; set; }
        float Rotation { get; set; }
    }
}