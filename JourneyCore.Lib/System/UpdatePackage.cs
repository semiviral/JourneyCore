namespace JourneyCore.Lib.System
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
}
