using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    [Header("IDs")]
    public string itemId;

    [Header("Basic Info")]
    public string itemName;
    public Sprite icon;
    public GameObject itemPrefab;
    
    [Header("Category")]
    public ItemCategory category = ItemCategory.Generic;
    
    [Header("Item Properties")]
    public bool isDroppable = true;
    public bool isConsumable = false;
    
    [Header("Consumable Properties")]
    [SerializeField] private float healAmount = 10f;
    [SerializeField] private AudioClip consumeSound;
    [SerializeField] private GameObject consumeEffect;
    
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
    
    /// <summary>
    /// Verifica se questo item può essere consumato
    /// </summary>
    public bool CanBeConsumed()
    {
        return isConsumable && (category == ItemCategory.Potion || healAmount > 0);
    }
    
    /// <summary>
    /// Ottiene la quantità di cura di questo item
    /// </summary>
    public float GetHealAmount()
    {
        return healAmount;
    }
    
    /// <summary>
    /// Ottiene il suono di consumo
    /// </summary>
    public AudioClip GetConsumeSound()
    {
        return consumeSound;
    }
    
    /// <summary>
    /// Ottiene l'effetto di consumo
    /// </summary>
    public GameObject GetConsumeEffect()
    {
        return consumeEffect;
    }
}