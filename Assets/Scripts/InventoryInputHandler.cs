using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryInputHandler : MonoBehaviour
{
    [Header("References")]
    public InventoryManager inventoryManager;
    
    [Header("Input Settings")]
    public InputActionAsset inputActions;
    
    private InputAction inventoryInputAction;
    
    // Current selected slot for navigation
    private int currentSelectedSlot = 0;
    
    [Header("UI Feedback")]
    public GameObject slotHighlight; // Optional: UI element to show selected slot
    
    private void Awake()
    {
        // Find inventory manager if not assigned
        if (inventoryManager == null)
        {
            inventoryManager = FindObjectOfType<InventoryManager>();
        }
        
        // Get the input action
        if (inputActions != null)
        {
            inventoryInputAction = inputActions.FindAction("InventoryInputActions");
        }
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
    
    private void OnInventoryInput(InputAction.CallbackContext context)
    {
        if (inventoryManager == null) return;
        
        string inputName = context.control.name;
        
        switch (inputName)
        {
            // Number keys 1, 2, 3 - Select slots directly
            case "1":
                SelectSlot(0);
                break;
            case "2":
                SelectSlot(1);
                break;
            case "3":
                SelectSlot(2);
                break;
                
            // Arrow keys - Navigate through slots
            case "leftArrow":
                NavigateSlot(-1);
                break;
            case "rightArrow":
                NavigateSlot(1);
                break;
                
            // Q - Drop item from current selected slot
            case "q":
                DropFromCurrentSlot();
                break;
                
            // Delete - Drop item from current selected slot (alternative)
            case "delete":
                DropFromCurrentSlot();
                break;
        }
    }
    
    private void SelectSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < inventoryManager.bagSlots.Length)
        {
            currentSelectedSlot = slotIndex;
            UpdateSlotHighlight();
            
            // Show what's in the selected slot
            Item item = inventoryManager.bagSlots[currentSelectedSlot].GetItem();
            if (item != null)
            {
                Debug.Log($"Selected slot {currentSelectedSlot + 1}: {item.itemName}");
            }
            else
            {
                Debug.Log($"Selected slot {currentSelectedSlot + 1}: Empty");
            }
        }
    }
    
    private void NavigateSlot(int direction)
    {
        currentSelectedSlot += direction;
        
        // Wrap around
        if (currentSelectedSlot < 0)
            currentSelectedSlot = inventoryManager.bagSlots.Length - 1;
        else if (currentSelectedSlot >= inventoryManager.bagSlots.Length)
            currentSelectedSlot = 0;
            
        UpdateSlotHighlight();
        
        // Show current selection
        Item item = inventoryManager.bagSlots[currentSelectedSlot].GetItem();
        if (item != null)
        {
            Debug.Log($"Navigated to slot {currentSelectedSlot + 1}: {item.itemName}");
        }
        else
        {
            Debug.Log($"Navigated to slot {currentSelectedSlot + 1}: Empty");
        }
    }
    
    private void DropFromCurrentSlot()
    {
        Item item = inventoryManager.bagSlots[currentSelectedSlot].GetItem();
        if (item != null)
        {
            Debug.Log($"Dropping {item.itemName} from slot {currentSelectedSlot + 1}");
            inventoryManager.DropItemFromSlot(currentSelectedSlot);
        }
        else
        {
            Debug.Log($"Slot {currentSelectedSlot + 1} is empty - nothing to drop");
        }
    }
    
    private void UpdateSlotHighlight()
    {
        // If you have a UI highlight element, update its position here
        if (slotHighlight != null && inventoryManager.bagSlots.Length > currentSelectedSlot)
        {
            // Position the highlight over the current slot
            // This assumes your inventory UI is set up with proper positioning
            slotHighlight.transform.SetParent(inventoryManager.bagSlots[currentSelectedSlot].transform);
            slotHighlight.transform.localPosition = Vector3.zero;
            slotHighlight.SetActive(true);
        }
    }
    
    // Optional: Display current controls in UI
    private void Start()
    {
        ShowControls();
    }
    
    private void ShowControls()
    {
        Debug.Log("=== INVENTORY CONTROLS ===");
        Debug.Log("1, 2, 3: Select inventory slots directly");
        Debug.Log("← →: Navigate through slots");
        Debug.Log("Q / Delete: Drop item from selected slot");
        Debug.Log("Space: Drop first item found");
        Debug.Log("R: Drop random item");
        Debug.Log("C: Drop all items (clear inventory)");
        Debug.Log("L: List inventory contents");
        Debug.Log("========================");
    }
}