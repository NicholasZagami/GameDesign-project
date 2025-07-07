using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryInputHandler : MonoBehaviour
{
    [Header("References")]
    public InventoryManager inventoryManager;
    
    [Header("Input Settings")]
    public InputActionAsset inputActions;
    
    [Header("UI Feedback")]
    public GameObject slotHighlight;
    
    [Header("Mouse Priority Settings")]
    public bool enableKeyboardSelection = false;
    public bool enableMouseHoverDebug = true;
    
    private InputAction inventoryInputAction;
    private int currentSelectedSlot = 0;
    private bool inputProcessedThisFrame = false;
    
    // 🚀 ULTRA SIMPLE SYSTEM
    [Header("🚀 ULTRA SIMPLE SYSTEM")]
    public bool useUltraSimpleSystem = true;
    public Canvas inventoryCanvas; // Assegna la Canvas dell'inventario manualmente
    public Camera uiCamera; // Camera che renderizza l'UI (null per Overlay)
    
    private void Awake()
    {
        if (inventoryManager == null)
            inventoryManager = FindObjectOfType<InventoryManager>();
        
        if (inputActions != null)
            inventoryInputAction = inputActions.FindAction("InventoryInputActions");
    }
    
    private void Start()
    {
        // Auto-trova Canvas se non assegnata
        if (inventoryCanvas == null)
        {
            inventoryCanvas = FindCanvasWithSlots();
        }
        
        if (useUltraSimpleSystem)
        {
            Debug.Log("🚀 USING ULTRA SIMPLE SYSTEM");
            Debug.Log($"Canvas: {(inventoryCanvas != null ? inventoryCanvas.name : "NULL")}");
            Debug.Log($"Canvas Mode: {(inventoryCanvas != null ? inventoryCanvas.renderMode.ToString() : "NULL")}");
            Debug.Log($"UI Camera: {(uiCamera != null ? uiCamera.name : "NULL")}");
        }
    }
    
    private Canvas FindCanvasWithSlots()
    {
        if (inventoryManager?.bagSlots != null && inventoryManager.bagSlots.Length > 0)
        {
            InventorySlot firstSlot = inventoryManager.bagSlots[0];
            if (firstSlot != null)
            {
                return firstSlot.GetComponentInParent<Canvas>();
            }
        }
        return null;
    }
    
    private void OnEnable()
    {
        if (inventoryInputAction != null)
        {
            inventoryInputAction.Enable();
            inventoryInputAction.performed += OnInventoryInput;
        }
    }
    
    private void OnDisable()
    {
        if (inventoryInputAction != null)
        {
            inventoryInputAction.performed -= OnInventoryInput;
            inventoryInputAction.Disable();
        }
    }
    
    private void Update()
    {
        inputProcessedThisFrame = false;
        
        // Debug ultra semplice
        if (enableMouseHoverDebug && Time.frameCount % 60 == 0 && useUltraSimpleSystem)
        {
            InventorySlot detectedSlot = GetSlotUnderMouseUltraSimple();
            Vector2 mousePos = Input.mousePosition;
            Debug.Log($"🚀 Ultra Simple - Mouse: {mousePos} | Canvas: {(inventoryCanvas != null ? "Found" : "NULL")} | Detected: {(detectedSlot != null ? detectedSlot.GetSlotInfo() : "NULL")}");
            
            // Debug posizioni dei primi 3 slot
            if (inventoryManager?.bagSlots != null)
            {
                for (int i = 0; i < Mathf.Min(3, inventoryManager.bagSlots.Length); i++)
                {
                    if (inventoryManager.bagSlots[i] != null)
                    {
                        Rect slotRect = GetSlotScreenRect(inventoryManager.bagSlots[i]);
                        Debug.Log($"   Slot {i} rect: {slotRect}");
                    }
                }
            }
        }
        
        // Gestione Q
        if (Input.GetKeyDown(KeyCode.Q))
        {
            HandleDropUltraSimple();
        }
        
        // Keyboard navigation
        if (enableKeyboardSelection)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                NavigateSlot(-1);
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                NavigateSlot(1);
        }
    }
    
    /// <summary>
    /// 🚀 ULTRA SIMPLE - Usa direttamente i bounds dei RectTransform
    /// </summary>
    private InventorySlot GetSlotUnderMouseUltraSimple()
    {
        if (inventoryManager?.bagSlots == null || inventoryCanvas == null) return null;
        
        Vector2 mousePosition = Input.mousePosition;
        
        foreach (InventorySlot slot in inventoryManager.bagSlots)
        {
            if (slot == null) continue;
            
            Rect slotRect = GetSlotScreenRect(slot);
            
            if (slotRect.Contains(mousePosition))
            {
                return slot;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// 🚀 Ottiene il rettangolo dello slot in coordinate schermo
    /// </summary>
    private Rect GetSlotScreenRect(InventorySlot slot)
    {
        RectTransform rectTransform = slot.GetComponent<RectTransform>();
        if (rectTransform == null) return new Rect(0, 0, 0, 0);
        
        // Calcola i corners del RectTransform in world space
        Vector3[] worldCorners = new Vector3[4];
        rectTransform.GetWorldCorners(worldCorners);
        
        // Converti in screen space
        Vector2 min = Vector2.positiveInfinity;
        Vector2 max = Vector2.negativeInfinity;
        
        Camera cam = null;
        if (inventoryCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            cam = inventoryCanvas.worldCamera;
        }
        else if (inventoryCanvas.renderMode == RenderMode.WorldSpace)
        {
            cam = uiCamera != null ? uiCamera : Camera.main;
        }
        
        for (int i = 0; i < 4; i++)
        {
            Vector2 screenPoint;
            
            if (inventoryCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                // Per Overlay, le coordinate world sono già screen coordinates
                screenPoint = worldCorners[i];
            }
            else
            {
                // Per Camera modes, converti da world a screen
                screenPoint = cam != null ? cam.WorldToScreenPoint(worldCorners[i]) : worldCorners[i];
            }
            
            min = Vector2.Min(min, screenPoint);
            max = Vector2.Max(max, screenPoint);
        }
        
        return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
    }
    
    /// <summary>
    /// 🚀 GESTIONE DROP ULTRA SEMPLICE
    /// </summary>
    private void HandleDropUltraSimple()
    {
        if (inputProcessedThisFrame) return;
        
        if (enableMouseHoverDebug)
        {
            Debug.Log($"🚀 Q pressed - Using ultra simple detection");
        }
        
        InventorySlot slotUnderMouse = GetSlotUnderMouseUltraSimple();
        
        if (slotUnderMouse != null && slotUnderMouse.GetItem() != null)
        {
            Debug.Log($"🚀 DROPPING from ultra simple: {slotUnderMouse.GetItem().itemName}");
            slotUnderMouse.TriggerDrop();
            inputProcessedThisFrame = true;
            return;
        }
        
        // Fallback keyboard
        if (enableKeyboardSelection && IsValidSlotIndex(currentSelectedSlot))
        {
            var selectedSlot = inventoryManager.bagSlots[currentSelectedSlot];
            if (selectedSlot?.GetItem() != null)
            {
                Debug.Log($"🚀 DROPPING from keyboard: {selectedSlot.GetItem().itemName}");
                DropFromSlotIndex(currentSelectedSlot);
                inputProcessedThisFrame = true;
                return;
            }
        }
        
        Debug.Log("🚀 No item to drop with ultra simple system");
    }
    
    private void OnInventoryInput(InputAction.CallbackContext context)
    {
        if (inventoryManager == null || inputProcessedThisFrame) return;
        if (!enableKeyboardSelection) return;
        
        switch (context.control.name)
        {
            case "1": SelectSlot(0); inputProcessedThisFrame = true; break;
            case "2": SelectSlot(1); inputProcessedThisFrame = true; break;
            case "3": SelectSlot(2); inputProcessedThisFrame = true; break;
            case "leftArrow": NavigateSlot(-1); inputProcessedThisFrame = true; break;
            case "rightArrow": NavigateSlot(1); inputProcessedThisFrame = true; break;
        }
    }
    
    private void SelectSlot(int slotIndex)
    {
        if (!enableKeyboardSelection) return;
        
        if (IsValidSlotIndex(slotIndex))
        {
            currentSelectedSlot = slotIndex;
            UpdateSlotHighlight();
            
            if (inventoryManager.bagSlots[slotIndex] != null)
            {
                ItemCategory category = CategoryHelper.GetCategoryForSlotIndex(slotIndex);
                Debug.Log($"Selected slot {slotIndex} ({CategoryHelper.GetCategoryDisplayName(category)})");
            }
        }
    }
    
    private void NavigateSlot(int direction)
    {
        if (!enableKeyboardSelection) return;
        
        currentSelectedSlot += direction;
        int slotCount = inventoryManager.bagSlots.Length;
        currentSelectedSlot = (currentSelectedSlot + slotCount) % slotCount;
        UpdateSlotHighlight();
    }
    
    private void DropFromSlotIndex(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex)) return;
        
        var slot = inventoryManager.bagSlots[slotIndex];
        var item = slot?.GetItem();
        
        if (item != null)
        {
            GameObject droppedItem = inventoryManager.DropItemFromSlotIndex(slotIndex);
        }
    }
    
    private void UpdateSlotHighlight()
    {
        if (!enableKeyboardSelection) return;
        
        if (slotHighlight != null && IsValidSlotIndex(currentSelectedSlot))
        {
            slotHighlight.transform.SetParent(inventoryManager.bagSlots[currentSelectedSlot].transform);
            slotHighlight.transform.localPosition = Vector3.zero;
            slotHighlight.SetActive(true);
        }
    }
    
    private bool IsValidSlotIndex(int index)
    {
        return inventoryManager != null && 
               inventoryManager.bagSlots != null && 
               index >= 0 && 
               index < inventoryManager.bagSlots.Length &&
               inventoryManager.bagSlots[index] != null;
    }
    
    public bool IsInputProcessedThisFrame()
    {
        return inputProcessedThisFrame;
    }
    
    // Legacy methods - Non usati
    public void SetHoveredSlot(InventorySlot slot) { }
    public void ClearHoveredSlot(InventorySlot slot) { }
    
    public string GetCurrentSlotInfo()
    {
        if (!enableKeyboardSelection) return "Keyboard selection disabled";
        if (!IsValidSlotIndex(currentSelectedSlot)) return "Invalid slot";
        
        var slot = inventoryManager.bagSlots[currentSelectedSlot];
        var item = slot?.GetItem();
        ItemCategory category = CategoryHelper.GetCategoryForSlotIndex(currentSelectedSlot);
        
        string itemName = item != null ? $"{item.itemName} ({item.GetCategoryDisplayName()})" : "Empty";
        return $"Slot {currentSelectedSlot} ({CategoryHelper.GetCategoryDisplayName(category)}): {itemName}";
    }
    
    public string GetHoveredSlotInfo()
    {
        if (useUltraSimpleSystem)
        {
            InventorySlot slot = GetSlotUnderMouseUltraSimple();
            if (slot != null)
            {
                var item = slot.GetItem();
                return item != null ? $"Detected: {item.itemName} ({item.GetCategoryDisplayName()})" : "Detected: Empty slot";
            }
            return "No slot detected";
        }
        return "Legacy hover system";
    }
    
    /// <summary>
    /// 🚀 TEST MANUALE del sistema ultra semplice
    /// </summary>
    [ContextMenu("🚀 Test Ultra Simple Detection")]
    public void TestUltraSimpleDetection()
    {
        Debug.Log("🚀 === TESTING ULTRA SIMPLE DETECTION ===");
        Debug.Log($"Mouse Position: {Input.mousePosition}");
        Debug.Log($"Canvas: {(inventoryCanvas != null ? inventoryCanvas.name : "NULL")}");
        Debug.Log($"Canvas Mode: {(inventoryCanvas != null ? inventoryCanvas.renderMode.ToString() : "NULL")}");
        
        if (inventoryManager?.bagSlots != null)
        {
            Debug.Log($"Checking {inventoryManager.bagSlots.Length} slots...");
            
            for (int i = 0; i < inventoryManager.bagSlots.Length; i++)
            {
                if (inventoryManager.bagSlots[i] != null)
                {
                    Rect rect = GetSlotScreenRect(inventoryManager.bagSlots[i]);
                    bool contains = rect.Contains(Input.mousePosition);
                    Debug.Log($"Slot {i}: {rect} - Contains mouse: {contains}");
                }
            }
        }
        
        InventorySlot slot = GetSlotUnderMouseUltraSimple();
        if (slot != null)
        {
            Debug.Log($"🚀 FOUND: {slot.GetSlotInfo()}");
        }
        else
        {
            Debug.Log($"🚀 NO SLOT FOUND");
        }
    }
    
    /// <summary>
    /// 🚀 Setup automatico della Canvas
    /// </summary>
    [ContextMenu("🚀 Auto Setup Canvas")]
    public void AutoSetupCanvas()
    {
        inventoryCanvas = FindCanvasWithSlots();
        
        if (inventoryCanvas != null)
        {
            Debug.Log($"✅ Found Canvas: {inventoryCanvas.name}");
            Debug.Log($"   Render Mode: {inventoryCanvas.renderMode}");
            
            if (inventoryCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                uiCamera = inventoryCanvas.worldCamera;
                Debug.Log($"   World Camera: {(uiCamera != null ? uiCamera.name : "NULL")}");
            }
        }
        else
        {
            Debug.LogError("❌ No Canvas found with slots!");
        }
    }
}