using System;

namespace JourneyCore.Lib.Game.Context.Items
{
    public class Item : Context
    {
        private int _stackSize;

        public EventHandler<ItemStackSizeChangedEventArgs> ItemStackSizeChanged;

        public Item(Context owner, string name, string primaryTag, int maxStackSize) : base(owner, name,
            primaryTag)
        {
            MaxStackSize = maxStackSize;
        }

        public int StackSize
        {
            get => _stackSize;
            set
            {
                if (_stackSize == value)
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

            _stackSize = newSize;
        }
    }
}