using System;
using System.Numerics;
using JourneyCore.Lib.Game.Object.Entity;

namespace JourneyCore.Lib.Game.Object
{
    public interface IAnchor
    {
        event EventHandler<EntityPositionChangedEventArgs> PositionChanged;
        event EventHandler<float> RotationChanged;

        void AnchorItem(IAnchorable anchorableItem);
        void AnchorItemPosition(IAnchorable anchorableItem);
        void AnchorItemRotation(IAnchorable anchorableItem);
    }

    public interface IAnchorable
    {
        void OnAnchorPositionChanged(object sender, EntityPositionChangedEventArgs args);
        void OnAnchorRotationChanged(object sender, float newRotation);
    }
}