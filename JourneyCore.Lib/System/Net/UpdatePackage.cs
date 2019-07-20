using System.Collections.Generic;

namespace JourneyCore.Lib.System.Net
{
    public struct UpdatePackage<T>
    {
        public StateUpdateType UpdateType { get; }
        public List<T> Values { get; }

        public UpdatePackage(StateUpdateType updateType, params T[] values)
        {
            UpdateType = updateType;
            Values = new List<T>(values);
        }
    }

    public enum StateUpdateType
    {
        None,
        Position,
        PositionModification,
        Rotation,
        RotationModification
    }
}