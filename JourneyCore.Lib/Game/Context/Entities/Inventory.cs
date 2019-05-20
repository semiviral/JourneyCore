using JourneyCore.Lib.Game.Context.Items;

namespace JourneyCore.Lib.Game.Context.Entities
{
    public class Inventory
    {
        public Inventory()
        {
            Items = new Item[11];
        }

        public Item[] Items { get; }

        public void AddInventoryItem(int slot, Item newItem)
        {
            Items[slot] = newItem;
        }

        public void DelInventoryItem(int slot)
        {
            Items[slot] = null;
        }

        public void MoveInventorySlot(int slot, int newSlot)
        {
            if (Items[slot] == null) return;

            bool emptyTempIndex = false;

            // checks to see if an item switch is necessary
            if (Items[newSlot] != null)
            {
                Items[10] = Items[newSlot];
                emptyTempIndex = true;
            }

            // Moves item from slot to new slot
            DelInventoryItem(newSlot);
            Items[newSlot] = Items[slot];
            DelInventoryItem(slot);

            // Empties temporary item slot
            if (!emptyTempIndex) return;

            Items[slot] = Items[10];
            DelInventoryItem(10);
        }

        public int FindFirstOpenItemSlot()
        {
            for (int i = 0; i < Items.Length; i++)
                if (Items[i] == null)
                    return i;

            return -1;
        }
    }
}