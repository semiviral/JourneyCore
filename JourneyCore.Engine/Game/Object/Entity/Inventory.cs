namespace JourneyCore.Lib.Game.Object.Entity
{
    public class Inventory
    {
        public Inventory()
        {
            Items = new Item.Item[11];
        }

        public Item.Item[] Items { get; }

        public void AddInventoryItem(int slot, Item.Item newItem)
        {
            Items[slot] = newItem;
        }

        public void DelInventoryItem(int slot)
        {
            Items[slot] = null;
        }

        public void MoveInventorySlot(int slot, int newSlot)
        {
            if (Items[slot] == null)
            {
                return;
            }

            bool _emptyTempIndex = false;

            // checks to see if an item switch is necessary
            if (Items[newSlot] != null)
            {
                Items[10] = Items[newSlot];
                _emptyTempIndex = true;
            }

            // Moves item from slot to new slot
            DelInventoryItem(newSlot);
            Items[newSlot] = Items[slot];
            DelInventoryItem(slot);

            // Empties temporary item slot
            if (!_emptyTempIndex)
            {
                return;
            }

            Items[slot] = Items[10];
            DelInventoryItem(10);
        }

        public int FindFirstOpenItemSlot()
        {
            for (int _i = 0; _i < Items.Length; _i++)
            {
                if (Items[_i] == null)
                {
                    return _i;
                }
            }

            return -1;
        }
    }
}