using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    
    [Header("Inventory Configuration")]
    public InventorySlot[] bagSlots;
    public InventorySlot[] equipmentSlots;
    
    [Header("Events")]
    public UnityEvent<Item> OnItemAdded;
    public UnityEvent<Item> OnItemRemoved;
    public UnityEvent<Item> OnItemDropped;
    public UnityEvent OnInventoryFull;
    public UnityEvent<Item> OnItemDropBlocked;
    
    [Header("Audio")]
    public AudioClip itemPickupSound;
    public AudioClip itemRejectedSound;
    public AudioClip itemDropBlockedSound;
    
    private AudioSource audioSource;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        InitializeComponents();
        ValidateSlots();
        InitializeCategoryVisuals();
    }
    
    private void InitializeComponents()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }
    
    private void ValidateSlots()
    {
        if (bagSlots == null || bagSlots.Length != CategoryHelper.TOTAL_SLOTS)
        {
            return;
        }
        
        for (int i = 0; i < bagSlots.Length; i++)
        {
            if (bagSlots[i] == null)
                return;
        }
    }
    
    private void InitializeCategoryVisuals()
    {
        for (int i = 0; i < bagSlots.Length; i++)
        {
            if (bagSlots[i] != null)
            {
                ItemCategory category = CategoryHelper.GetCategoryForSlotIndex(i);
                bagSlots[i].SetCategoryVisual(category);
            }
        }
    }
    
    public bool AddItemToBag(Item item)
    {
        if (bagSlots == null || item == null) return false;
        
        var (startIndex, endIndex) = item.GetValidSlotRange();
        
        for (int i = startIndex; i <= endIndex; i++)
        {
            if (i < bagSlots.Length && bagSlots[i] != null && bagSlots[i].GetItem() == null)
            {
                bagSlots[i].AddItem(item);
                PlaySound(itemPickupSound);
                OnItemAdded?.Invoke(item);
                return true;
            }
        }
        
        if (IsInventoryFull())
        {
            OnInventoryFull?.Invoke();
        }
        else
        {
            PlaySound(itemRejectedSound);
        }
        
        return false;
    }
    
    public bool ForceAddItemToSlot(Item item, int slotIndex)
    {
        if (bagSlots == null || item == null || slotIndex < 0 || slotIndex >= bagSlots.Length)
            return false;
        
        if (bagSlots[slotIndex] == null)
            return false;
        
        if (!item.CanBeInSlot(slotIndex))
        {
            return false;
        }
        
        if (bagSlots[slotIndex].GetItem() == null)
        {
            bagSlots[slotIndex].AddItem(item);
            OnItemAdded?.Invoke(item);
            return true;
        }
        
        return false;
    }
    
    public bool RemoveItemFromBag(Item item)
    {
        if (bagSlots == null) return false;
        
        foreach (var slot in bagSlots)
        {
            if (slot?.GetItem() == item)
            {
                slot.ClearSlot();
                OnItemRemoved?.Invoke(item);
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Verifica se un item può essere droppato dallo slot
    /// </summary>
    public bool CanDropItemFromSlot(InventorySlot slot)
    {
        if (slot == null) return false;
        
        Item item = slot.GetItem();
        if (item == null) return false;
        
        return item.CanBeDropped();
    }
    
    /// <summary>
    /// Verifica se un item può essere droppato dall'indice dello slot
    /// </summary>
    public bool CanDropItemFromSlotIndex(int slotIndex)
    {
        if (bagSlots == null || slotIndex < 0 || slotIndex >= bagSlots.Length)
            return false;
            
        return CanDropItemFromSlot(bagSlots[slotIndex]);
    }
    
    public GameObject DropItemFromSlot(InventorySlot slot)
    {
        if (slot == null) return null;
        
        Item item = slot.GetItem();
        if (item == null) return null;
        
        if (!item.CanBeDropped())
        {
            Debug.Log($"Item '{item.itemName}' non può essere droppato!");
            PlaySound(itemDropBlockedSound);
            OnItemDropBlocked?.Invoke(item);
            return null;
        }
        
        if (item.itemPrefab == null) return null;
        
        Vector3 dropPosition = GetPlayerDropPosition();
        if (dropPosition == Vector3.zero) return null;
        
        GameObject wrapper = ColliderHelper.CreateDroppedObjectWrapper(item.itemPrefab, item, dropPosition);
        
        if (wrapper == null) return null;
        
        slot.ClearSlot();
        
        HandleItemDrop(item, slot);
        
        StartCoroutine(EnableCollectionAfterDelay(wrapper.GetComponent<CollectableItem>()));
        
        return wrapper;
    }
    
    public GameObject DropItemFromSlotIndex(int slotIndex)
    {
        if (bagSlots == null || slotIndex < 0 || slotIndex >= bagSlots.Length)
            return null;
            
        return DropItemFromSlot(bagSlots[slotIndex]);
    }
    
    private Vector3 GetPlayerDropPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return Vector3.zero;
        
        Vector3 position = player.transform.position + player.transform.forward * 2f;
        position.y = player.transform.position.y + 0.5f;
        return position;
    }
    
    private IEnumerator EnableCollectionAfterDelay(CollectableItem collectable)
    {
        yield return new WaitForSeconds(1.5f);
        if (collectable != null)
            collectable.enabled = true;
    }
    
    public void HandleItemDrop(Item droppedItem, InventorySlot fromSlot)
    {
        OnItemDropped?.Invoke(droppedItem);
    }
    
    public bool RemoveItemFromSlot(InventorySlot slot)
    {
        if (slot?.GetItem() != null)
        {
            Item item = slot.GetItem();
            slot.ClearSlot();
            OnItemRemoved?.Invoke(item);
            return true;
        }
        return false;
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
    
    public int GetEmptySlotCountForCategory(ItemCategory category)
    {
        if (bagSlots == null) return 0;
        
        var (startIndex, endIndex) = CategoryHelper.GetSlotRangeForCategory(category);
        int count = 0;
        
        for (int i = startIndex; i <= endIndex && i < bagSlots.Length; i++)
        {
            if (bagSlots[i]?.GetItem() == null)
                count++;
        }
        
        return count;
    }
    
    public int GetEmptySlotCount()
    {
        if (bagSlots == null) return 0;
        
        int count = 0;
        foreach (var slot in bagSlots)
        {
            if (slot?.GetItem() == null)
                count++;
        }
        return count;
    }
    
    public bool IsCategoryFull(ItemCategory category)
    {
        return GetEmptySlotCountForCategory(category) == 0;
    }
    
    public bool IsInventoryFull() => GetEmptySlotCount() == 0;
    
    public List<Item> GetItemsOfCategory(ItemCategory category)
    {
        List<Item> items = new List<Item>();
        if (bagSlots == null) return items;
        
        var (startIndex, endIndex) = CategoryHelper.GetSlotRangeForCategory(category);
        
        for (int i = startIndex; i <= endIndex && i < bagSlots.Length; i++)
        {
            Item item = bagSlots[i]?.GetItem();
            if (item != null)
                items.Add(item);
        }
        
        return items;
    }
    
    public List<Item> GetItemsOfType(Item itemType)
    {
        List<Item> items = new List<Item>();
        if (bagSlots == null) return items;
        
        foreach (var slot in bagSlots)
        {
            if (slot?.GetItem() == itemType)
                items.Add(slot.GetItem());
        }
        return items;
    }
    
    public int GetSlotIndex(InventorySlot slot)
    {
        if (bagSlots == null) return -1;
        
        for (int i = 0; i < bagSlots.Length; i++)
        {
            if (bagSlots[i] == slot)
                return i;
        }
        return -1;
    }

    public void ClearAll()
    {
        if (bagSlots != null)
            foreach (var s in bagSlots) s?.ClearSlot();

        if (equipmentSlots != null)
            foreach (var s in equipmentSlots) s?.ClearSlot();
    }

    public IEnumerable<Item> GetAllItems()
    {
        if (bagSlots != null)
            foreach (var s in bagSlots)
            {
                var it = s?.GetItem();
                if (it != null) yield return it;
            }

        if (equipmentSlots != null)
            foreach (var s in equipmentSlots)
            {
                var it = s?.GetItem();
                if (it != null) yield return it;
            }
    }
}