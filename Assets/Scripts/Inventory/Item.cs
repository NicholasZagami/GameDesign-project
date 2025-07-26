using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public Sprite icon;
    public GameObject itemPrefab;
    
    [Header("Category")]
    public ItemCategory category = ItemCategory.Generic;
    
    [Header("Item Properties")]
    public bool isDroppable = true;
    
    [Header("Description")]
    [TextArea(3, 6)]
    public string description;
    
    /// <summary>
    /// Ottiene il nome display della categoria di questo item
    /// </summary>
    public string GetCategoryDisplayName()
    {
        return CategoryHelper.GetCategoryDisplayName(category);
    }
    
    /// <summary>
    /// Ottiene il colore della categoria per questo item
    /// </summary>
    public Color GetCategoryColor()
    {
        return CategoryHelper.GetCategoryColor(category);
    }
    
    /// <summary>
    /// Verifica se questo item può essere posizionato in un determinato slot
    /// </summary>
    public bool CanBeInSlot(int slotIndex)
    {
        return CategoryHelper.CanItemBeInSlot(this, slotIndex);
    }
    
    /// <summary>
    /// Ottiene il range di slot dove questo item può essere posizionato
    /// </summary>
    public (int startIndex, int endIndex) GetValidSlotRange()
    {
        return CategoryHelper.GetSlotRangeForCategory(category);
    }
    
    /// <summary>
    /// Verifica se questo item può essere droppato
    /// </summary>
    public bool CanBeDropped()
    {
        return isDroppable;
    }
}