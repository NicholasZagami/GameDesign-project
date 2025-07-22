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
        // Disable automatic F key handling since ChestInputHandler handles it
        // Only keep this if you want fallback sequential pickup
        /*
        if (!isActive) return;
        
        if (interactAction != null && interactAction.action.WasPressedThisFrame())
        {
            HandlePickupInput();
        }
        */
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
        for (int i = 0; i < chestUISlots.Length; i++)
        {
            if (chestUISlots[i] != null && chestUISlots[i].GetItem() != null)
            {
                selectedSlotIndex = i;
                return;
            }
        }
        
        selectedSlotIndex = 0;
    }
    
    // Legacy method for compatibility (if still called somewhere)
    public void DisplayChestItems(List<Item> chestItems)
    {
        if (chestUISlots == null) return;
        
        ClearChest();
        
        for (int i = 0; i < chestItems.Count && i < chestUISlots.Length; i++)
        {
            if (chestItems[i] != null && chestUISlots[i] != null)
            {
                chestUISlots[i].AddItem(chestItems[i]);
            }
        }
    }
    
    // Legacy method for compatibility
    public void ClearAllChestSlots()
    {
        ClearChest();
    }
    
    private void AutoFindChestSlots()
    {
        InventorySlot[] foundSlots = GetComponentsInChildren<InventorySlot>(true);
        
        if (foundSlots.Length > 0)
        {
            chestUISlots = foundSlots;
        }
    }
    
    public void LoadItemsIntoChest(Item[] items)
    {
        if (chestUISlots == null || items == null) return;
        
        for (int i = 0; i < items.Length && i < chestUISlots.Length; i++)
        {
            if (items[i] != null && chestUISlots[i] != null)
            {
                chestUISlots[i].AddItem(items[i]);
            }
        }
    }
    
    public bool AddItemToChest(Item item)
    {
        if (chestUISlots == null || item == null) return false;
        
        for (int i = 0; i < chestUISlots.Length; i++)
        {
            if (chestUISlots[i] != null && chestUISlots[i].GetItem() == null)
            {
                return chestUISlots[i].AddItem(item);
            }
        }
        
        return false;
    }
    
    public bool RemoveItemFromChest(Item item)
    {
        if (chestUISlots == null || item == null) return false;
        
        for (int i = 0; i < chestUISlots.Length; i++)
        {
            if (chestUISlots[i] != null && chestUISlots[i].GetItem() == item)
            {
                chestUISlots[i].ClearSlot();
                return true;
            }
        }
        
        return false;
    }
    
    public List<Item> GetChestItems()
    {
        List<Item> items = new List<Item>();
        if (chestUISlots == null) return items;
        
        for (int i = 0; i < chestUISlots.Length; i++)
        {
            if (chestUISlots[i] != null)
            {
                Item item = chestUISlots[i].GetItem();
                if (item != null)
                {
                    items.Add(item);
                }
            }
        }
        
        return items;
    }
    
    public void ClearChest()
    {
        if (chestUISlots == null) return;
        
        for (int i = 0; i < chestUISlots.Length; i++)
        {
            if (chestUISlots[i] != null)
            {
                chestUISlots[i].ClearSlot();
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
        
        if (slotIndex < 0 || slotIndex >= chestUISlots.Length) return;
        
        InventorySlot chestSlot = chestUISlots[slotIndex];
        if (chestSlot == null) return;
        
        Item itemToPickup = chestSlot.GetItem();
        if (itemToPickup == null) return;
        
        bool addedSuccessfully = playerInventoryManager.AddItemToBag(itemToPickup);
        
        if (addedSuccessfully)
        {
            chestSlot.ClearSlot();
            chestController.PlayPickupSound();
            
            // Don't automatically move to next slot - let mouse hover control it
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