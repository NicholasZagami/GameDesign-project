using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    // Singleton pattern
    public static InventoryManager Instance { get; private set; }
    
    [Header("Inventory Configuration")]
    public InventorySlot[] bagSlots;
    public InventorySlot[] equipmentSlots;
    
    [Header("Debug & Testing")]
    public Item[] debugItems; // solo per test rapido
    
    [Header("Drop Settings")]
    public float dropDistance = 2f; // How far from player to drop items
    public Vector3 dropOffset = Vector3.up; // Height offset for dropped items
    
    [Header("Events")]
    public UnityEvent<Item> OnItemAdded;
    public UnityEvent<Item> OnItemRemoved;
    public UnityEvent<Item> OnItemDropped; // New event for when items are dropped
    public UnityEvent OnInventoryFull;
    
    [Header("Audio")]
    public AudioClip itemPickupSound;
    public AudioClip inventoryFullSound;
    public AudioClip itemDropSound; // New sound for dropping items
    
    private AudioSource audioSource;
    
    // NEW: Keep track of collected items and their original GameObjects
    private Dictionary<Item, List<GameObject>> collectedItemObjects = new Dictionary<Item, List<GameObject>>();
    
    private void Awake()
    {
        Debug.Log($"InventoryManager Awake called - InstanceID: {GetInstanceID()}");
        
        // Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"InventoryManager Instance set - InstanceID: {GetInstanceID()}");
        }
        else if (Instance != this)
        {
            Debug.LogWarning($"Duplicate InventoryManager found! Destroying InstanceID: {GetInstanceID()}, keeping InstanceID: {Instance.GetInstanceID()}");
            Destroy(gameObject);
            return;
        }
        
        // Ensure the dictionary is initialized
        if (collectedItemObjects == null)
        {
            collectedItemObjects = new Dictionary<Item, List<GameObject>>();
            Debug.Log("Initialized collectedItemObjects dictionary in Awake");
        }
    }
    
    private void Start()
    {
        Debug.Log($"InventoryManager Start called - InstanceID: {GetInstanceID()}");
        Debug.Log($"Dictionary count at Start: {collectedItemObjects.Count}");
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Test: aggiunge il primo oggetto nei primi slot
        if (debugItems.Length > 0)
        {
            bagSlots[0].AddItem(debugItems[0]);
            if (equipmentSlots.Length > 0)
                equipmentSlots[0].AddItem(debugItems[0]);
        }
    }

    public bool AddItemToBag(Item item)
    {
        // Find first empty slot
        foreach (var slot in bagSlots)
        {
            if (slot.GetItem() == null)
            {
                slot.AddItem(item);
                
                // Play pickup sound
                if (itemPickupSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(itemPickupSound);
                }
                
                // Trigger event
                OnItemAdded?.Invoke(item);
                
                Debug.Log($"Added {item.itemName} to inventory");
                return true;
            }
        }
        
        // Inventory is full
        Debug.Log("Inventario pieno.");
        
        // Play full inventory sound
        if (inventoryFullSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(inventoryFullSound);
        }
        
        // Trigger full inventory event
        OnInventoryFull?.Invoke();
        
        return false;
    }

    // NEW: Method to register a collected item's GameObject
    public void RegisterCollectedItem(Item item, GameObject itemObject)
    {
        Debug.Log($"=== REGISTERING ITEM ===");
        Debug.Log($"InventoryManager InstanceID: {GetInstanceID()}");
        Debug.Log($"Item: {item.itemName} (ID: {item.GetInstanceID()})");
        Debug.Log($"GameObject: {itemObject.name}");
        Debug.Log($"Dictionary current count: {collectedItemObjects.Count}");
        
        if (!collectedItemObjects.ContainsKey(item))
        {
            collectedItemObjects[item] = new List<GameObject>();
            Debug.Log($"Created new list for {item.itemName}");
        }
        
        collectedItemObjects[item].Add(itemObject);
        Debug.Log($"Added to list. New count for {item.itemName}: {collectedItemObjects[item].Count}");
        Debug.Log($"Total dictionary count: {collectedItemObjects.Count}");
    }
    
    public bool RemoveItemFromBag(Item item)
    {
        foreach (var slot in bagSlots)
        {
            if (slot.GetItem() == item)
            {
                slot.ClearSlot();
                OnItemRemoved?.Invoke(item);
                Debug.Log($"Removed {item.itemName} from inventory");
                return true;
            }
        }
        return false;
    }
    
    // NEW: Drop item method that reactivates original GameObject
    public bool DropItem(Item item)
    {        
        Debug.Log($"=== STARTING DROP PROCESS FOR: {item.itemName} ===");
        Debug.Log($"InventoryManager InstanceID: {GetInstanceID()}");
        Debug.Log($"Item reference: {item.GetInstanceID()}");
        Debug.Log($"Total items in dictionary: {collectedItemObjects.Count}");
        
        // Debug: Show all items in dictionary
        foreach (var kvp in collectedItemObjects)
        {
            Debug.Log($"Dictionary contains: {kvp.Key.itemName} (ID: {kvp.Key.GetInstanceID()}) with {kvp.Value.Count} objects");
            
            // Check if any of the GameObjects are null
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                GameObject obj = kvp.Value[i];
                if (obj == null)
                {
                    Debug.LogWarning($"  - Object {i}: NULL (destroyed?)");
                }
                else
                {
                    Debug.Log($"  - Object {i}: {obj.name} (Active: {obj.activeInHierarchy})");
                }
            }
        }
        
        // Try to find and reactivate an original GameObject for this item
        if (collectedItemObjects.ContainsKey(item))
        {
            Debug.Log($"✅ Found item in collected objects dictionary!");
            Debug.Log($"Count for this item: {collectedItemObjects[item].Count}");
            
            if (collectedItemObjects[item].Count > 0)
            {
                // Get the first collected object of this type
                GameObject originalObject = collectedItemObjects[item][0];
                
                Debug.Log($"Retrieved original object: {(originalObject != null ? originalObject.name : "NULL")}");
                
                if (originalObject != null)
                {
                    // Remove from list first
                    collectedItemObjects[item].RemoveAt(0);
                    Debug.Log($"Removed object from dictionary. Remaining count: {collectedItemObjects[item].Count}");
                    
                    // Remove item from inventory AFTER we confirm we have the original object
                    if (!RemoveItemFromBag(item))
                    {
                        // If we can't remove from inventory, put the object back in the list
                        collectedItemObjects[item].Insert(0, originalObject);
                        Debug.Log($"❌ Cannot drop {item.itemName} - not found in inventory");
                        return false;
                    }
                    
                    // Position the object in front of the player
                    Vector3 dropPosition = CalculateDropPosition();
                    originalObject.transform.position = dropPosition;
                    
                    Debug.Log($"Setting drop position to: {dropPosition}");
                    
                    // Reactivate the object
                    originalObject.SetActive(true);
                    Debug.Log($"✅ SetActive(true) called on: {originalObject.name}");
                    
                    // Reset the CollectableItem component
                    CollectableItem collectableComponent = originalObject.GetComponent<CollectableItem>();
                    if (collectableComponent != null)
                    {
                        collectableComponent.ReactivateItem();
                        Debug.Log($"✅ ReactivateItem() called");
                    }
                    else
                    {
                        Debug.LogWarning($"❌ No CollectableItem component found on {originalObject.name}");
                    }
                    
                    // Play drop sound
                    if (itemDropSound != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(itemDropSound);
                    }
                    
                    // Trigger drop event
                    OnItemDropped?.Invoke(item);
                    
                    Debug.Log($"🎉 REACTIVATED ORIGINAL OBJECT: {item.itemName} at {dropPosition}");
                    return true;
                }
                else
                {
                    // Original object was destroyed, remove null reference
                    collectedItemObjects[item].RemoveAt(0);
                    Debug.LogWarning($"❌ Original object for {item.itemName} was destroyed!");
                }
            }
            else
            {
                Debug.LogWarning($"❌ No objects stored for {item.itemName}");
            }
        }
        else
        {
            Debug.LogWarning($"❌ Item {item.itemName} not found in collected objects dictionary");
            Debug.LogWarning($"Available items in dictionary:");
            foreach (var kvp in collectedItemObjects)
            {
                Debug.LogWarning($"  - {kvp.Key.itemName} (ID: {kvp.Key.GetInstanceID()})");
            }
        }
        
        Debug.Log($"⚠️ GOING TO FALLBACK - removing item from inventory first");
        
        // Remove item from inventory since we're going to drop it (fallback)
        if (!RemoveItemFromBag(item))
        {
            Debug.Log($"❌ Cannot drop {item.itemName} - not found in inventory");
            return false;
        }
        
        // Fallback: Create new instance from prefab if no original object available
        if (item.itemPrefab != null)
        {
            Vector3 dropPosition = CalculateDropPosition();
            GameObject droppedObject = Instantiate(item.itemPrefab, dropPosition, Quaternion.identity);
            
            // Play drop sound
            if (itemDropSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(itemDropSound);
            }
            
            // Trigger drop event
            OnItemDropped?.Invoke(item);
            
            Debug.LogWarning($"⚠️ FALLBACK: Created new instance of {item.itemName} at {dropPosition} (original object not available)");
            return true;
        }
        
        Debug.LogError($"❌ Cannot drop {item.itemName} - no original object or prefab available");
        return false;
    }
    
    // NEW: Calculate where to drop items in front of the player
    private Vector3 CalculateDropPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            // Drop in front of the player
            Vector3 dropPosition = player.transform.position + player.transform.forward * dropDistance + dropOffset;
            
            // Optional: Raycast to ensure we're dropping on the ground
            RaycastHit hit;
            if (Physics.Raycast(dropPosition + Vector3.up * 5f, Vector3.down, out hit, 10f))
            {
                dropPosition = hit.point + dropOffset;
            }
            
            return dropPosition;
        }
        
        // Fallback: drop at origin with offset
        return Vector3.zero + dropOffset;
    }
    
    public bool DropItemFromSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < bagSlots.Length && bagSlots[slotIndex] != null)
        {
            Item item = bagSlots[slotIndex].GetItem();
            if (item != null)
            {
                return DropItem(item);
            }
        }
        
        Debug.Log($"Cannot drop item from slot {slotIndex} - slot is empty or invalid");
        return false;
    }

    public int GetEmptySlotCount()
    {
        int emptyCount = 0;
        foreach (var slot in bagSlots)
        {
            if (slot.GetItem() == null)
                emptyCount++;
        }
        return emptyCount;
    }
    
    public bool IsInventoryFull()
    {
        return GetEmptySlotCount() == 0;
    }
    
    // NEW: Get all items of a specific type in inventory
    public List<Item> GetItemsOfType(Item itemType)
    {
        List<Item> items = new List<Item>();
        foreach (var slot in bagSlots)
        {
            if (slot.GetItem() == itemType)
            {
                items.Add(slot.GetItem());
            }
        }
        return items;
    }
    
    // NEW: Debug method to show collected items
    public void DebugShowCollectedItems()
    {
        Debug.Log("=== COLLECTED ITEMS DEBUG ===");
        foreach (var kvp in collectedItemObjects)
        {
            Debug.Log($"{kvp.Key.itemName}: {kvp.Value.Count} objects stored");
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                GameObject obj = kvp.Value[i];
                if (obj != null)
                {
                    Debug.Log($"  - Object {i}: {obj.name} (Active: {obj.activeInHierarchy})");
                }
                else
                {
                    Debug.Log($"  - Object {i}: NULL (destroyed?)");
                }
            }
        }
        
        if (collectedItemObjects.Count == 0)
        {
            Debug.Log("No collected items registered!");
        }
    }
}