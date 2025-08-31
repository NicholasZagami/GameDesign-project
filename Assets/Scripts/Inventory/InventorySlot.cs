using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    public Image iconImage;
    public Image backgroundImage;
    public Image borderImage;
    
    [Header("Category Visual Settings")]
    public bool showCategoryColors = true;
    public float categoryColorAlpha = 0.3f;
    
    [Header("Consumable Visual Settings")]
    public Color consumableHighlightColor = new Color(0.2f, 1f, 0.2f, 0.5f);
    
    private Item currentItem;
    private bool isMouseOver = false;
    private ItemCategory slotCategory;
    private int slotIndex = -1;
    
    private InventoryManager inventoryManager;
    private InventoryInputHandler inputHandler;
    
    private Color originalBackgroundColor;
    private Color incompatibleColor = new Color(1f, 0.2f, 0.2f, 0.5f);
    
    private void Start()
    {
        InitializeReferences();
        InitializeSlotInfo();
        InitializeVisuals();
    }
    
    private void InitializeReferences()
    {
        inventoryManager = InventoryManager.Instance;
        if (inventoryManager == null)
            inventoryManager = FindObjectOfType<InventoryManager>();
        
        inputHandler = FindObjectOfType<InventoryInputHandler>();
    }
    
    private void InitializeSlotInfo()
    {
        if (inventoryManager?.bagSlots != null)
        {
            slotIndex = inventoryManager.GetSlotIndex(this);
            if (slotIndex >= 0)
            {
                slotCategory = CategoryHelper.GetCategoryForSlotIndex(slotIndex);
            }
        }
    }
    
    private void InitializeVisuals()
    {
        if (backgroundImage != null)
            originalBackgroundColor = backgroundImage.color;
        
        if (showCategoryColors && slotIndex >= 0)
            SetCategoryVisual(slotCategory);
    }
    
    public bool AddItem(Item newItem)
    {
        if (newItem == null) return false;
        
        if (slotIndex >= 0 && !newItem.CanBeInSlot(slotIndex))
        {
            ShowIncompatibleFeedback();
            return false;
        }
        
        currentItem = newItem;
        iconImage.sprite = newItem.icon;
        iconImage.enabled = true;
        
        // Aggiorna l'aspetto visivo per gli item consumabili
        UpdateConsumableVisual();
        
        return true;
    }

    public void ClearSlot()
    {
        currentItem = null;
        iconImage.sprite = null;
        iconImage.enabled = false;
        
        if (showCategoryColors && slotIndex >= 0)
            SetCategoryVisual(slotCategory);
    }

    public Item GetItem()
    {
        return currentItem;
    }
    
    public void TriggerDrop()
    {
        if (currentItem == null || inventoryManager == null) return;
        
        inventoryManager.DropItemFromSlot(this);
    }
    
    /// <summary>
    /// Tenta di consumare l'item in questo slot
    /// </summary>
    public void TriggerConsume()
    {
        if (currentItem == null || !currentItem.CanBeConsumed())
        {
            Debug.Log("Nessun item consumabile in questo slot");
            return;
        }
        
        ItemConsumer consumer = ItemConsumer.Instance;
        if (consumer != null)
        {
            consumer.ConsumeItem(currentItem, this);
        }
        else
        {
            Debug.LogError("ItemConsumer non trovato! Assicurati che sia presente nella scena.");
        }
    }
    
    public void SetCategoryVisual(ItemCategory category)
    {
        slotCategory = category;
        
        if (!showCategoryColors || backgroundImage == null) return;
        
        Color categoryColor = CategoryHelper.GetCategoryColor(category);
        categoryColor.a = categoryColorAlpha;
        backgroundImage.color = categoryColor;
        
        originalBackgroundColor = categoryColor;
    }
    
    /// <summary>
    /// Aggiorna l'aspetto visivo per gli item consumabili
    /// </summary>
    private void UpdateConsumableVisual()
    {
        if (currentItem == null || backgroundImage == null) return;
        
        if (currentItem.CanBeConsumed())
        {
            // Verifica se l'item può essere consumato ora
            ItemConsumer consumer = ItemConsumer.Instance;
            if (consumer != null && consumer.CanConsumeItemNow(currentItem))
            {
                // Evidenzia con colore verde se può essere consumato
                Color highlightColor = consumableHighlightColor;
                backgroundImage.color = Color.Lerp(originalBackgroundColor, highlightColor, 0.6f);
            }
            else if (consumer != null)
            {
                // Colore più tenue se non può essere consumato ora (es. salute piena)
                Color dimmedColor = new Color(0.8f, 0.8f, 0.8f, categoryColorAlpha);
                backgroundImage.color = dimmedColor;
            }
        }
    }
    
    private void Update()
    {
        // Aggiorna costantemente l'aspetto visivo degli item consumabili
        if (currentItem != null && currentItem.CanBeConsumed())
        {
            UpdateConsumableVisual();
        }
    }
    
    private void ShowIncompatibleFeedback()
    {
        if (borderImage != null)
        {
            StartCoroutine(FlashIncompatibleBorder());
        }
    }
    
    private System.Collections.IEnumerator FlashIncompatibleBorder()
    {
        Color originalBorderColor = borderImage.color;
        
        borderImage.color = incompatibleColor;
        yield return new WaitForSeconds(0.2f);
        
        borderImage.color = originalBorderColor;
        yield return new WaitForSeconds(0.1f);
        
        borderImage.color = incompatibleColor;
        yield return new WaitForSeconds(0.2f);
        
        borderImage.color = originalBorderColor;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null) return;
        
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            TriggerDrop();
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Click sinistro per consumare gli item consumabili
            if (currentItem.CanBeConsumed())
            {
                TriggerConsume();
            }
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;
        
        if (inputHandler != null)
        {
            inputHandler.SetHoveredSlot(this);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;
        
        if (inputHandler != null)
        {
            inputHandler.ClearHoveredSlot(this);
        }
    }
    
    public string GetSlotInfo()
    {
        string categoryName = slotIndex >= 0 ? CategoryHelper.GetCategoryDisplayName(slotCategory) : "Unknown";
        string itemName = currentItem != null ? currentItem.itemName : "Empty";
        string consumableInfo = currentItem != null && currentItem.CanBeConsumed() ? " [CONSUMABLE]" : "";
        string hoverStatus = isMouseOver ? " [HOVERED]" : "";
        return $"Slot {slotIndex} ({categoryName}): {itemName}{consumableInfo}{hoverStatus}";
    }
    
    public void DisplayItem(Item item)
    {
        if (item == null)
        {
            ClearSlot();
            return;
        }
        
        iconImage.sprite = item.icon;
        iconImage.enabled = true;
    }
}