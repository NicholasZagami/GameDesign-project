using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChestInputHandler : MonoBehaviour
{
    [Header("References")]
    public ChestInventoryManager chestInventoryManager;
    public Canvas chestCanvas;
    public Camera uiCamera;
    
    private bool inputProcessedThisFrame = false;
    private ChestController chestController;
    private GraphicRaycaster graphicRaycaster;
    
    private void Awake()
    {
        if (chestInventoryManager == null)
            chestInventoryManager = FindObjectOfType<ChestInventoryManager>();
            
        chestController = FindObjectOfType<ChestController>();
    }
    
    private void Start()
    {
        if (chestCanvas == null)
        {
            chestCanvas = FindCanvasWithSlots();
        }
        
        if (chestCanvas != null)
        {
            graphicRaycaster = chestCanvas.GetComponent<GraphicRaycaster>();
        }
    }
    
    private Canvas FindCanvasWithSlots()
    {
        if (chestInventoryManager?.chestUISlots != null && chestInventoryManager.chestUISlots.Length > 0)
        {
            InventorySlot firstSlot = chestInventoryManager.chestUISlots[0];
            if (firstSlot != null)
            {
                return firstSlot.GetComponentInParent<Canvas>();
            }
        }
        return null;
    }
    
    private void Update()
    {
        if (!IsChestOpen()) return;
        
        inputProcessedThisFrame = false;
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            HandlePickup();
        }
    }
    
    private bool IsChestOpen()
    {
        return chestController != null && chestController.IsOpen();
    }
    
    private InventorySlot GetSlotUnderMouseWithRaycast()
    {
        if (graphicRaycaster == null || chestInventoryManager?.chestUISlots == null) return null;
        
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        
        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        graphicRaycaster.Raycast(pointerEventData, raycastResults);
        
        foreach (RaycastResult result in raycastResults)
        {
            InventorySlot slot = result.gameObject.GetComponent<InventorySlot>();
            if (slot != null)
            {
                for (int i = 0; i < chestInventoryManager.chestUISlots.Length; i++)
                {
                    if (chestInventoryManager.chestUISlots[i] == slot)
                    {
                        return slot;
                    }
                }
            }
        }
        
        return null;
    }
    
    private void HandlePickup()
    {
        if (inputProcessedThisFrame || chestController == null || chestInventoryManager == null) return;
        
        InventorySlot slotUnderMouse = GetSlotUnderMouseWithRaycast();
        
        if (slotUnderMouse != null && slotUnderMouse.GetItem() != null)
        {
            int slotIndex = GetSlotIndex(slotUnderMouse);
            if (slotIndex >= 0)
            {
                chestInventoryManager.OnChestSlotClicked(slotIndex, chestController);
                inputProcessedThisFrame = true;
            }
        }
    }
    
    private int GetSlotIndex(InventorySlot slot)
    {
        if (chestInventoryManager?.chestUISlots == null) return -1;
        
        for (int i = 0; i < chestInventoryManager.chestUISlots.Length; i++)
        {
            if (chestInventoryManager.chestUISlots[i] == slot)
            {
                return i;
            }
        }
        
        return -1;
    }
    
    public bool IsInputProcessedThisFrame()
    {
        return inputProcessedThisFrame;
    }
}