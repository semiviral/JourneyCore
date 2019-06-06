namespace JourneyCore.Lib.Game.Object.Item
{
    public class ItemStackSizeChangedEventArgs
    {
        public int OldStackSize { get; }
        public int NewStackSize { get; }

        public ItemStackSizeChangedEventArgs(int oldSize, int newSize)
        {
            OldStackSize = oldSize;
            NewStackSize = newSize;
        }
    }
}