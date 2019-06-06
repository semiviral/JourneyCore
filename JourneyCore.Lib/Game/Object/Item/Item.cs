using System;

namespace JourneyCore.Lib.Game.Object.Item
{
    public class Item
    {
        private int _StackSize;

        public EventHandler<ItemStackSizeChangedEventArgs> ItemStackSizeChanged;

        public Item(string name, string primaryTag, int maxStackSize)
        {
            MaxStackSize = maxStackSize;
        }

        public int StackSize
        {
            get => _StackSize;
            set
            {
                if (_StackSize == value)
                {
                    return;
                }

                ModifyStackSize(value);
            }
        }

        public int MaxStackSize { get; }

        private void ModifyStackSize(int newSize)
        {
            if (newSize >= 0 && newSize <= MaxStackSize)
            {
                return;
            }

            _StackSize = newSize;
        }
    }
}