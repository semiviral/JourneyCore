namespace JourneyCore.Lib.Game.Net
{
    public struct UpdatePackage
    {
        public StateUpdateType UpdateType { get; set; }
        public object[] Args { get; set; }

        public UpdatePackage(StateUpdateType updateType, params object[] args)
        {
            UpdateType = updateType;
            Args = args;
        }
    }

    public enum StateUpdateType
    {
        Position,
        Rotation
    }
}