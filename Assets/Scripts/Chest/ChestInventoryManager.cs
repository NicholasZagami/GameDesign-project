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
    
    [Header("Audio")]
    public AudioClip itemTransferBlockedSound; // Suono per trasferimento bloccato
    
    private InventoryManager playerInventoryManager;
    private AudioSource audioSource;
    private bool isActive = false;
    
    private void Awake()
    {
        FindInventoryManager();
        SetupInputAction();
        InitializeAudioSource();
    }
    
    private void Start()
    {
        if (chestUISlots == null || chestUISlots.Length == 0)
        {
            AutoFindChestSlots();
        }
    }
    
    private void InitializeAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
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
    
    /// <summary>
    /// Verifica se un item può essere rimosso dalla chest
    /// Nota: Gli item nella chest possono sempre essere presi, 
    /// il controllo isDroppable si applica solo quando si vuole droppare dal player inventory
    /// </summary>
    public bool CanRemoveItemFromChest(Item item)
    {
        // Gli item nella chest possono sempre essere presi
        return item != null;
    }
    
    /// <summary>
    /// Verifica se un item può essere depositato nella chest dal player inventory
    /// </summary>
    public bool CanDepositItemInChest(Item item)
    {
        if (item == null) return false;
        
        if (!item.CanBeDropped())
        {
            Debug.Log($"Item '{item.itemName}' non può essere depositato nella chest perché non è droppabile!");
            PlaySound(itemTransferBlockedSound);
            return false;
        }
        
        return true;
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
    
    /// <summary>
    /// Rimuove un item dalla chest verificando se può essere rimosso
    /// </summary>
    public bool RemoveItemFromChestSafe(Item item)
    {
        if (!CanRemoveItemFromChest(item)) return false;
        
        return RemoveItemFromChest(item);
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
    
    /// <summary>
    /// Ottiene solo gli item che possono essere prelevati dalla chest
    /// (tutti, dato che dalla chest si può sempre prendere)
    /// </summary>
    public List<Item> GetPickupableChestItems()
    {
        return GetChestItems(); // Tutti gli item nella chest possono essere presi
    }
    
    /// <summary>
    /// Ottiene gli item del player che possono essere depositati nella chest
    /// </summary>
    public List<Item> GetDepositablePlayerItems()
    {
        List<Item> depositableItems = new List<Item>();
        
        if (playerInventoryManager == null) return depositableItems;
        
        foreach (var slot in playerInventoryManager.bagSlots)
        {
            if (slot != null)
            {
                Item item = slot.GetItem();
                if (item != null && CanDepositItemInChest(item))
                {
                    depositableItems.Add(item);
                }
            }
        }
        
        return depositableItems;
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
        
        if (!CanRemoveItemFromChest(itemToPickup))
        {
            Debug.Log($"Non è possibile prelevare '{itemToPickup.itemName}' dalla chest!");
            PlaySound(itemTransferBlockedSound);
            return;
        }
        
        bool addedSuccessfully = playerInventoryManager.AddItemToBag(itemToPickup);
        
        if (addedSuccessfully)
        {
            chestSlot.ClearSlot();
            chestController.PlayPickupSound();
            
            Debug.Log($"Prelevato '{itemToPickup.itemName}' dalla chest!");
            // Don't automatically move to next slot - let mouse hover control it
        }
        else
        {
            Debug.Log("Inventario pieno o impossibile aggiungere l'item!");
        }
    }
    
    /// <summary>
    /// Metodo per depositare un item del player nella chest
    /// </summary>
    public bool DepositPlayerItemInChest(Item item, ChestController chestController)
    {
        if (playerInventoryManager == null || item == null) return false;
        
        if (!CanDepositItemInChest(item))
        {
            return false;
        }
        
        // Rimuovi dall'inventario del player
        bool removedFromPlayer = playerInventoryManager.RemoveItemFromBag(item);
        if (!removedFromPlayer) return false;
        
        // Aggiungi alla chest
        bool addedToChest = AddItemToChest(item);
        if (addedToChest)
        {
            if (chestController != null)
            {
                chestController.PlayPickupSound(); // Riusa il suono del pickup
            }
            Debug.Log($"Depositato '{item.itemName}' nella chest!");
            return true;
        }
        else
        {
            // Se non riesco ad aggiungere alla chest, rimetti nell'inventario del player
            playerInventoryManager.AddItemToBag(item);
            Debug.Log("Chest piena! Item rimesso nell'inventario.");
            return false;
        }
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
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