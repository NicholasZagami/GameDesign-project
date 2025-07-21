using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ChestInventoryManager : MonoBehaviour
{
    [Header("Chest UI Configuration")]
    public InventorySlot[] chestUISlots;
    
    [Header("Interaction Settings")]
    public InputActionReference interactAction;
    public int selectedSlotIndex = 0;
    
    private InventoryManager playerInventoryManager;
    private bool isActive = false;
    
    private void Awake()
    {
        FindInventoryManager();
        SetupInputAction();
    }
    
    private void Start()
    {
        if (chestUISlots == null || chestUISlots.Length == 0)
        {
            AutoFindChestSlots();
        }
    }
    
    private void Update()
    {
        if (!isActive) return;
        
        if (interactAction != null && interactAction.action.WasPressedThisFrame())
        {
            HandlePickupInput();
        }
    }
    
    private void SetupInputAction()
    {
        if (interactAction != null)
        {
            interactAction.action.Enable();
        }
    }
    
    private void OnDestroy()
    {
        if (interactAction != null)
        {
            interactAction.action.Disable();
        }
    }
    
    public void SetActive(bool active)
    {
        isActive = active;
        if (active)
        {
            selectedSlotIndex = 0;
        }
    }
    
    private void HandlePickupInput()
    {
        ChestController chestController = FindObjectOfType<ChestController>();
        if (chestController == null) return;
        
        OnChestSlotClicked(selectedSlotIndex, chestController);
        
        MoveToNextSlot(chestController);
    }
    
    private void MoveToNextSlot(ChestController chestController)
    {
        List<Item> chestItems = chestController.GetChestItems();
        
        for (int i = 0; i < chestItems.Count; i++)
        {
            if (chestItems[i] != null)
            {
                selectedSlotIndex = i;
                return;
            }
        }
        
        selectedSlotIndex = 0;
    }
    
    private void AutoFindChestSlots()
    {
        InventorySlot[] foundSlots = GetComponentsInChildren<InventorySlot>(true);
        
        if (foundSlots.Length > 0)
        {
            chestUISlots = foundSlots;
        }
    }
    
    public void DisplayChestItems(List<Item> chestItems)
    {
        if (chestUISlots == null) return;
        
        ClearAllChestSlots();
        
        for (int i = 0; i < chestItems.Count && i < chestUISlots.Length; i++)
        {
            if (chestItems[i] != null && chestUISlots[i] != null)
            {
                chestUISlots[i].DisplayItem(chestItems[i]);
            }
        }
    }
    
    public void ClearAllChestSlots()
    {
        if (chestUISlots == null) return;
        
        foreach (InventorySlot slot in chestUISlots)
        {
            if (slot != null)
            {
                slot.ClearSlot();
            }
        }
    }
    
    public void OnChestSlotClicked(int slotIndex, ChestController chestController)
    {
        if (playerInventoryManager == null)
        {
            FindInventoryManager();
        }
        
        if (chestController == null || playerInventoryManager == null) return;
        
        List<Item> chestItems = chestController.GetChestItems();
        
        if (slotIndex < 0 || slotIndex >= chestItems.Count) return;
        
        Item itemToPickup = chestItems[slotIndex];
        if (itemToPickup == null) return;
        
        bool addedSuccessfully = playerInventoryManager.AddItemToBag(itemToPickup);
        
        if (addedSuccessfully)
        {
            bool removed = chestController.RemoveItemFromChest(itemToPickup);
            
            if (removed)
            {
                DisplayChestItems(chestController.GetChestItems());
                chestController.PlayPickupSound();
            }
        }
    }
    
    private void FindInventoryManager()
    {
        playerInventoryManager = InventoryManager.Instance;
        
        if (playerInventoryManager == null)
        {
            playerInventoryManager = FindObjectOfType<InventoryManager>();
        }
        
        if (playerInventoryManager == null)
        {
            GameObject inventoryGO = GameObject.Find("InventoryManager");
            if (inventoryGO != null)
            {
                playerInventoryManager = inventoryGO.GetComponent<InventoryManager>();
            }
        }
    }
    
    public void SetupChestSlotClickHandlers(ChestController chestController)
    {
        // Input action handling is done in Update() method
    }
    
    [ContextMenu("Auto Setup Chest Slots")]
    public void AutoSetupChestSlots()
    {
        AutoFindChestSlots();
    }
}