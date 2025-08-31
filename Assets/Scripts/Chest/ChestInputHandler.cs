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
        
        // Aggiorna continuamente lo slot selezionato basato sull'hover del mouse
        UpdateSelectedSlotFromMouse();
        
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
    
    private void UpdateSelectedSlotFromMouse()
    {
        if (chestInventoryManager == null) return;
        
        InventorySlot slotUnderMouse = GetSlotUnderMouseWithRaycast();
        
        if (slotUnderMouse != null)
        {
            int slotIndex = GetSlotIndex(slotUnderMouse);
            if (slotIndex >= 0)
            {
                // Aggiorna lo slot selezionato nel ChestInventoryManager
                chestInventoryManager.selectedSlotIndex = slotIndex;
            }
        }
    }
    
    private void HandlePickup()
    {
        if (inputProcessedThisFrame || chestController == null || chestInventoryManager == null) return;
        
        // Controlla se ChestInventoryManager ha già processato input questo frame
        if (chestInventoryManager.IsInputProcessedThisFrame()) return;
        
        // Usa selectedSlotIndex che viene aggiornato dall'hover del mouse
        int slotIndex = chestInventoryManager.selectedSlotIndex;
        
        if (slotIndex >= 0 && slotIndex < chestInventoryManager.chestUISlots.Length)
        {
            InventorySlot slot = chestInventoryManager.chestUISlots[slotIndex];
            if (slot != null && slot.GetItem() != null)
            {
                // true = keyboard input (F key), usa selectedSlotIndex
                chestInventoryManager.OnChestSlotClicked(slotIndex, chestController, true);
                inputProcessedThisFrame = true;
                Debug.Log($"F key pressed - pickup da slot {slotIndex}");
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