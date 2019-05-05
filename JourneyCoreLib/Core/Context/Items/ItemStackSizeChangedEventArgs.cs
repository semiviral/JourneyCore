namespace JourneyCoreLib.Core.Context.Items
{
    public class ItemStackSizeChangedEventArgs
    {
        public int OldStackSize { get; private set; }
        public int NewStackSize { get; private set; }

        public ItemStackSizeChangedEventArgs(int oldSize, int newSize)
        {
            OldStackSize = oldSize;
            NewStackSize = newSize;
        }
    }
}
