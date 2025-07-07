using UnityEngine;

/// <summary>
/// Enum che definisce le categorie degli oggetti nell'inventario
/// </summary>
public enum ItemCategory
{
    Weapon = 0,     // Armi - Prima fila (slot 0-5)
    Potion = 1,     // Pozioni - Seconda fila (slot 6-11)
    Generic = 2     // Item generici - Terza fila (slot 12-17)
}

/// <summary>
/// Classe helper per gestire le categorie e i loro slot corrispondenti
/// </summary>
public static class CategoryHelper
{
    // Configurazione delle file (6 slot per fila)
    public const int SLOTS_PER_ROW = 6;
    public const int TOTAL_ROWS = 3;
    public const int TOTAL_SLOTS = SLOTS_PER_ROW * TOTAL_ROWS; // 18 slot totali
    
    /// <summary>
    /// Ottiene la categoria di un slot basato sul suo indice
    /// </summary>
    public static ItemCategory GetCategoryForSlotIndex(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < 6)
            return ItemCategory.Weapon;
        else if (slotIndex >= 6 && slotIndex < 12)
            return ItemCategory.Potion;
        else if (slotIndex >= 12 && slotIndex < 18)
            return ItemCategory.Generic;
        else
            return ItemCategory.Generic; // Default fallback
    }
    
    /// <summary>
    /// Ottiene il range di slot per una categoria specifica
    /// </summary>
    public static (int startIndex, int endIndex) GetSlotRangeForCategory(ItemCategory category)
    {
        switch (category)
        {
            case ItemCategory.Weapon:
                return (0, 5);
            case ItemCategory.Potion:
                return (6, 11);
            case ItemCategory.Generic:
                return (12, 17);
            default:
                return (12, 17); // Default to generic
        }
    }
    
    /// <summary>
    /// Verifica se un item può essere posizionato in un determinato slot
    /// </summary>
    public static bool CanItemBeInSlot(Item item, int slotIndex)
    {
        if (item == null) return false;
        
        ItemCategory slotCategory = GetCategoryForSlotIndex(slotIndex);
        return item.category == slotCategory;
    }
    
    /// <summary>
    /// Ottiene il nome display della categoria
    /// </summary>
    public static string GetCategoryDisplayName(ItemCategory category)
    {
        switch (category)
        {
            case ItemCategory.Weapon: return "Armi";
            case ItemCategory.Potion: return "Pozioni";
            case ItemCategory.Generic: return "Oggetti";
            default: return "Sconosciuto";
        }
    }
    
    /// <summary>
    /// Ottiene il colore per la categoria (per UI feedback)
    /// </summary>
    public static Color GetCategoryColor(ItemCategory category)
    {
        switch (category)
        {
            case ItemCategory.Weapon: return new Color(1f, 0.3f, 0.3f, 0.3f); // Rosso
            case ItemCategory.Potion: return new Color(0.3f, 1f, 0.3f, 0.3f); // Verde
            case ItemCategory.Generic: return new Color(0.3f, 0.3f, 1f, 0.3f); // Blu
            default: return Color.white;
        }
    }
}