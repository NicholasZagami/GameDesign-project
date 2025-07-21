using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryInputHandler : MonoBehaviour
{
    [Header("References")]
    public InventoryManager inventoryManager;
    public Canvas inventoryCanvas;
    public Camera uiCamera;
    
    private bool inputProcessedThisFrame = false;
    
    private void Awake()
    {
        if (inventoryManager == null)
            inventoryManager = FindObjectOfType<InventoryManager>();
    }
    
    private void Start()
    {
        if (inventoryCanvas == null)
        {
            inventoryCanvas = FindCanvasWithSlots();
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
    
    private void Update()
    {
        inputProcessedThisFrame = false;
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            HandleDrop();
        }
    }
    
    private InventorySlot GetSlotUnderMouse()
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
    
    private Rect GetSlotScreenRect(InventorySlot slot)
    {
        RectTransform rectTransform = slot.GetComponent<RectTransform>();
        if (rectTransform == null) return new Rect(0, 0, 0, 0);
        
        Vector3[] worldCorners = new Vector3[4];
        rectTransform.GetWorldCorners(worldCorners);
        
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
                screenPoint = worldCorners[i];
            }
            else
            {
                screenPoint = cam != null ? cam.WorldToScreenPoint(worldCorners[i]) : worldCorners[i];
            }
            
            min = Vector2.Min(min, screenPoint);
            max = Vector2.Max(max, screenPoint);
        }
        
        return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
    }
    
    private void HandleDrop()
    {
        if (inputProcessedThisFrame) return;
        
        InventorySlot slotUnderMouse = GetSlotUnderMouse();
        
        if (slotUnderMouse != null && slotUnderMouse.GetItem() != null)
        {
            slotUnderMouse.TriggerDrop();
            inputProcessedThisFrame = true;
        }
    }
    
    public bool IsInputProcessedThisFrame()
    {
        return inputProcessedThisFrame;
    }
    
    public void SetHoveredSlot(InventorySlot slot) { }
    public void ClearHoveredSlot(InventorySlot slot) { }
}