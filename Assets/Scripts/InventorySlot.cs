using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    public Image iconImage;
    public Image backgroundImage; // Per il colore della categoria
    public Image borderImage; // Per highlight quando non compatibile
    
    [Header("Drop Settings")]
    public KeyCode dropKey = KeyCode.Q;
    
    [Header("Category Visual Settings")]
    public bool showCategoryColors = true;
    public float categoryColorAlpha = 0.3f;
    
    [Header("Mouse Hover Visual")]
    public bool showHoverEffect = true;
    public Color hoverTintColor = new Color(1f, 1f, 1f, 0.2f);
    
    [Header("Debug Settings")]
    public bool enableMouseDebug = true; // NUOVO: Debug dettagliato mouse events
    
    private Item currentItem;
    private bool isMouseOver = false;
    private ItemCategory slotCategory;
    private int slotIndex = -1;
    
    // References
    private InventoryManager inventoryManager;
    private InventoryInputHandler inputHandler;
    
    // Colors
    private Color originalBackgroundColor;
    private Color incompatibleColor = new Color(1f, 0.2f, 0.2f, 0.5f);
    
    private void Start()
    {
        InitializeReferences();
        InitializeSlotInfo();
        InitializeVisuals();
        VerifyUISetup(); // NUOVO: Verifica setup UI
    }
    
    private void InitializeReferences()
    {
        inventoryManager = InventoryManager.Instance;
        if (inventoryManager == null)
            inventoryManager = FindObjectOfType<InventoryManager>();
        
        inputHandler = FindObjectOfType<InventoryInputHandler>();
        
        if (enableMouseDebug)
        {
            Debug.Log($"🔧 Slot {gameObject.name} - InputHandler found: {inputHandler != null}");
        }
    }
    
    private void InitializeSlotInfo()
    {
        // Trova l'indice di questo slot nell'array del manager
        if (inventoryManager?.bagSlots != null)
        {
            slotIndex = inventoryManager.GetSlotIndex(this);
            if (slotIndex >= 0)
            {
                slotCategory = CategoryHelper.GetCategoryForSlotIndex(slotIndex);
                Debug.Log($"Slot {slotIndex} inizializzato per categoria: {CategoryHelper.GetCategoryDisplayName(slotCategory)}");
            }
        }
    }
    
    private void InitializeVisuals()
    {
        // Salva colore originale del background
        if (backgroundImage != null)
            originalBackgroundColor = backgroundImage.color;
        
        // Applica colore categoria se abilitato
        if (showCategoryColors && slotIndex >= 0)
            SetCategoryVisual(slotCategory);
    }
    
    /// <summary>
    /// NUOVO: Verifica che l'UI sia configurato correttamente per il mouse
    /// </summary>
    private void VerifyUISetup()
    {
        // Verifica che abbia un component che può ricevere raycast
        bool canReceiveRaycast = false;
        
        // Controlla Image components
        Image[] images = GetComponentsInChildren<Image>();
        foreach (Image img in images)
        {
            if (img.raycastTarget)
            {
                canReceiveRaycast = true;
                if (enableMouseDebug)
                    Debug.Log($"✅ Slot {slotIndex} - Found raycast target: {img.name}");
                break;
            }
        }
        
        // Controlla Button components
        Button button = GetComponent<Button>();
        if (button != null)
        {
            canReceiveRaycast = true;
            if (enableMouseDebug)
                Debug.Log($"✅ Slot {slotIndex} - Has Button component");
        }
        
        if (!canReceiveRaycast)
        {
            Debug.LogWarning($"⚠ Slot {slotIndex} ({gameObject.name}) - No raycast targets found! Mouse hover will not work.");
            Debug.LogWarning("💡 Add an Image with 'raycastTarget = true' or ensure UI setup is correct.");
        }
        
        // Verifica parent Canvas
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogError($"❌ Slot {slotIndex} - No parent Canvas found!");
        }
        else
        {
            GraphicRaycaster raycaster = parentCanvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                Debug.LogWarning($"⚠ Slot {slotIndex} - Parent Canvas has no GraphicRaycaster!");
            }
            else if (enableMouseDebug)
            {
                Debug.Log($"✅ Slot {slotIndex} - GraphicRaycaster found on {parentCanvas.name}");
            }
        }
    }

    /// <summary>
    /// Aggiunge un item allo slot con controllo categoria
    /// </summary>
    public bool AddItem(Item newItem)
    {
        if (newItem == null) return false;
        
        // Controlla compatibilità categoria
        if (slotIndex >= 0 && !newItem.CanBeInSlot(slotIndex))
        {
            Debug.LogWarning($"Item {newItem.itemName} ({newItem.GetCategoryDisplayName()}) non può essere aggiunto allo slot {slotIndex} ({CategoryHelper.GetCategoryDisplayName(slotCategory)})");
            ShowIncompatibleFeedback();
            return false;
        }
        
        currentItem = newItem;
        iconImage.sprite = newItem.icon;
        iconImage.enabled = true;
        
        Debug.Log($"✓ Item {newItem.itemName} aggiunto allo slot {slotIndex}");
        return true;
    }

    public void ClearSlot()
    {
        currentItem = null;
        iconImage.sprite = null;
        iconImage.enabled = false;
        
        // Ripristina colore categoria
        if (showCategoryColors && slotIndex >= 0)
            SetCategoryVisual(slotCategory);
    }

    public Item GetItem()
    {
        return currentItem;
    }
    
    /// <summary>
    /// Metodo pubblico per triggare il drop (chiamato dall'InputHandler)
    /// </summary>
    public void TriggerDrop()
    {
        if (currentItem == null || inventoryManager == null) 
        {
            Debug.Log($"Cannot drop from slot {slotIndex} - no item or no manager");
            return;
        }
        
        Debug.Log($"🎯 Triggering drop: {currentItem.itemName} ({currentItem.GetCategoryDisplayName()}) from slot {slotIndex}");
        
        // USA IL METODO CENTRALIZZATO dell'InventoryManager
        GameObject droppedItem = inventoryManager.DropItemFromSlot(this);
        
        if (droppedItem != null)
        {
            Debug.Log($"✅ Successfully dropped {currentItem.itemName} from slot {slotIndex}");
        }
        else
        {
            Debug.LogError($"❌ Failed to drop {currentItem.itemName} from slot {slotIndex}");
        }
    }
    
    /// <summary>
    /// Imposta il visual della categoria per questo slot
    /// </summary>
    public void SetCategoryVisual(ItemCategory category)
    {
        slotCategory = category;
        
        if (!showCategoryColors || backgroundImage == null) return;
        
        Color categoryColor = CategoryHelper.GetCategoryColor(category);
        categoryColor.a = categoryColorAlpha;
        backgroundImage.color = categoryColor;
        
        // Salva il nuovo colore come originale
        originalBackgroundColor = categoryColor;
    }
    
    /// <summary>
    /// Mostra feedback visivo per item incompatibile
    /// </summary>
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
        
        // Flash rosso
        borderImage.color = incompatibleColor;
        yield return new WaitForSeconds(0.2f);
        
        borderImage.color = originalBorderColor;
        yield return new WaitForSeconds(0.1f);
        
        borderImage.color = incompatibleColor;
        yield return new WaitForSeconds(0.2f);
        
        borderImage.color = originalBorderColor;
    }
    
    /// <summary>
    /// Applica effetto hover visivo
    /// </summary>
    private void ApplyHoverEffect()
    {
        if (!showHoverEffect || backgroundImage == null) return;
        
        Color hoverColor = originalBackgroundColor + hoverTintColor;
        backgroundImage.color = hoverColor;
    }
    
    /// <summary>
    /// Rimuove effetto hover visivo
    /// </summary>
    private void RemoveHoverEffect()
    {
        if (!showHoverEffect || backgroundImage == null) return;
        
        backgroundImage.color = originalBackgroundColor;
    }
    
    // Click destro alternativo per droppare
    public void OnPointerClick(PointerEventData eventData)
    {
        if (enableMouseDebug)
        {
            Debug.Log($"🖱️ Click on slot {slotIndex} - Button: {eventData.button}");
        }
        
        if (eventData.button == PointerEventData.InputButton.Right && currentItem != null)
        {
            Debug.Log($"Right-click drop on slot {slotIndex}");
            TriggerDrop();
        }
    }
    
    // Mouse hover detection
    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;
        
        if (enableMouseDebug)
        {
            Debug.Log($"🖱️ ENTER slot {slotIndex} - Item: {(currentItem != null ? currentItem.itemName : "Empty")}");
        }
        
        // Registra questo slot come quello sotto il mouse nell'InputHandler
        if (inputHandler != null)
        {
            inputHandler.SetHoveredSlot(this);
        }
        else
        {
            Debug.LogWarning($"⚠ Slot {slotIndex} - InputHandler is NULL! Cannot register hover.");
        }
        
        // Applica effetto hover
        ApplyHoverEffect();
        
        // Log informativo
        if (currentItem != null)
        {
            Debug.Log($"🖱️ Mouse over slot {slotIndex}: {currentItem.itemName} ({currentItem.GetCategoryDisplayName()}) - Press {dropKey} to drop or Right-click");
        }
        else if (slotIndex >= 0)
        {
            Debug.Log($"🖱️ Mouse over empty slot {slotIndex} - Category: {CategoryHelper.GetCategoryDisplayName(slotCategory)}");
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;
        
        if (enableMouseDebug)
        {
            Debug.Log($"🖱️ EXIT slot {slotIndex}");
        }
        
        // Rimuovi questo slot come quello sotto il mouse nell'InputHandler
        if (inputHandler != null)
        {
            inputHandler.ClearHoveredSlot(this);
        }
        
        // Rimuovi effetto hover
        RemoveHoverEffect();
    }
    
    /// <summary>
    /// Ottiene informazioni sullo slot per debug
    /// </summary>
    public string GetSlotInfo()
    {
        string categoryName = slotIndex >= 0 ? CategoryHelper.GetCategoryDisplayName(slotCategory) : "Unknown";
        string itemName = currentItem != null ? currentItem.itemName : "Empty";
        string hoverStatus = isMouseOver ? " [HOVERED]" : "";
        return $"Slot {slotIndex} ({categoryName}): {itemName}{hoverStatus}";
    }
    
    /// <summary>
    /// Verifica se questo slot è attualmente sotto il mouse
    /// </summary>
    public bool IsHovered()
    {
        return isMouseOver;
    }
    
    /// <summary>
    /// NUOVO: Test manuale dell'hover per debug
    /// </summary>
    [ContextMenu("Test Manual Hover")]
    public void TestManualHover()
    {
        Debug.Log($"=== MANUAL HOVER TEST for Slot {slotIndex} ===");
        Debug.Log($"Item: {(currentItem != null ? currentItem.itemName : "Empty")}");
        Debug.Log($"Mouse Over: {isMouseOver}");
        Debug.Log($"InputHandler: {(inputHandler != null ? "Found" : "NULL")}");
        
        if (inputHandler != null)
        {
            inputHandler.SetHoveredSlot(this);
            Debug.Log("Manually set as hovered slot");
        }
    }
    
    /// <summary>
    /// NUOVO: Test manuale del drop per debug
    /// </summary>
    [ContextMenu("Test Manual Drop")]
    public void TestManualDrop()
    {
        Debug.Log($"=== MANUAL DROP TEST for Slot {slotIndex} ===");
        if (currentItem != null)
        {
            TriggerDrop();
        }
        else
        {
            Debug.Log("No item to drop");
        }
    }
}