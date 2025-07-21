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
    
    public void SetCategoryVisual(ItemCategory category)
    {
        slotCategory = category;
        
        if (!showCategoryColors || backgroundImage == null) return;
        
        Color categoryColor = CategoryHelper.GetCategoryColor(category);
        categoryColor.a = categoryColorAlpha;
        backgroundImage.color = categoryColor;
        
        originalBackgroundColor = categoryColor;
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
        if (eventData.button == PointerEventData.InputButton.Right && currentItem != null)
        {
            TriggerDrop();
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
        string hoverStatus = isMouseOver ? " [HOVERED]" : "";
        return $"Slot {slotIndex} ({categoryName}): {itemName}{hoverStatus}";
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