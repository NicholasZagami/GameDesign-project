using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] bagSlots;
    public InventorySlot[] equipmentSlots;
    public Item[] debugItems; // solo per test rapido

    private void Start()
    {
        // Test: aggiunge il primo oggetto nei primi slot
        if (debugItems.Length > 0)
        {
            bagSlots[0].AddItem(debugItems[0]);
            equipmentSlots[0].AddItem(debugItems[0]);
        }
    }

    public bool AddItemToBag(Item item)
    {
        foreach (var slot in bagSlots)
        {
            if (slot.GetItem() == null)
            {
                slot.AddItem(item);
                return true;
            }
        }
        Debug.Log("Inventario pieno.");
        return false;
    }
}