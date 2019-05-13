﻿using System;
using JourneyCoreLib.Game.Context;

namespace JourneyCoreLib.Core.Context.Items
{
    public class Item : Game.Context.Context
    {
        public int StackSize {
            get => _stackSize; set {
                if (_stackSize == value) return;

                ModifyStackSize(value);
            }
        }
        private int _stackSize = 0;

        public int MaxStackSize { get; private set; }

        public EventHandler<ItemStackSizeChangedEventArgs> ItemStackSizeChanged;

        public Item(Game.Context.Context owner, string name, string primaryTag, int maxStackSize) : base(owner, name, primaryTag)
        {
            MaxStackSize = maxStackSize;
        }

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