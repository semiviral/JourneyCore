﻿using System;
using SFML.System;

namespace JourneyCore.Lib.Game.Object
{
    public interface IAnchor
    {
        event EventHandler<Vector2f> PositionChanged;
        event EventHandler<float> RotationChanged;

        void AnchorItem(IAnchorable anchorableItem);
        void AnchorItemPosition(IAnchorable anchorableItem);
        void AnchorItemRotation(IAnchorable anchorableItem);
    }

    public interface IAnchorable
    {
        Vector2f Position { get; set; }
        float Rotation { get; set; }
    }
}