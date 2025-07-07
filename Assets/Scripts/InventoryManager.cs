using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    
    [Header("Inventory Configuration")]
    public InventorySlot[] bagSlots; // 18 slot: 0-5 armi, 6-11 pozioni, 12-17 generici
    public InventorySlot[] equipmentSlots;
    
    [Header("Debug & Testing")]
    public Item[] debugItems;
    
    [Header("Events")]
    public UnityEvent<Item> OnItemAdded;
    public UnityEvent<Item> OnItemRemoved;
    public UnityEvent<Item> OnItemDropped;
    public UnityEvent OnInventoryFull;
    public UnityEvent<Item, string> OnItemRejected; // NUOVO: quando un item viene rifiutato per categoria
    
    [Header("Audio")]
    public AudioClip itemPickupSound;
    public AudioClip inventoryFullSound;
    public AudioClip itemDropSound;
    public AudioClip itemRejectedSound; // NUOVO: suono per item rifiutato
    
    private AudioSource audioSource;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
        LoadDebugItems();
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
            Debug.LogError($"BagSlots deve avere esattamente {CategoryHelper.TOTAL_SLOTS} slot! Attualmente: {bagSlots?.Length ?? 0}");
            return;
        }
        
        for (int i = 0; i < bagSlots.Length; i++)
        {
            if (bagSlots[i] == null)
                Debug.LogError($"BagSlot {i} è NULL! Assegna tutti gli slot.");
        }
        
        Debug.Log("=== CONFIGURAZIONE INVENTARIO ===");
        Debug.Log($"Slot 0-5: {CategoryHelper.GetCategoryDisplayName(ItemCategory.Weapon)}");
        Debug.Log($"Slot 6-11: {CategoryHelper.GetCategoryDisplayName(ItemCategory.Potion)}");
        Debug.Log($"Slot 12-17: {CategoryHelper.GetCategoryDisplayName(ItemCategory.Generic)}");
    }
    
    private void InitializeCategoryVisuals()
    {
        // Applica colori di categoria agli slot (opzionale)
        for (int i = 0; i < bagSlots.Length; i++)
        {
            if (bagSlots[i] != null)
            {
                ItemCategory category = CategoryHelper.GetCategoryForSlotIndex(i);
                bagSlots[i].SetCategoryVisual(category);
            }
        }
    }
    
    private void LoadDebugItems()
    {
        if (debugItems.Length > 0)
        {
            foreach (Item item in debugItems)
            {
                if (item != null)
                {
                    bool added = AddItemToBag(item);
                    Debug.Log($"Debug item {item.itemName} ({item.GetCategoryDisplayName()}): {(added ? "Aggiunto" : "Rifiutato")}");
                }
            }
        }
    }

    /// <summary>
    /// Aggiunge un item al bag rispettando le categorie
    /// </summary>
    public bool AddItemToBag(Item item)
    {
        if (bagSlots == null || item == null) return false;
        
        // Ottieni il range di slot validi per questa categoria
        var (startIndex, endIndex) = item.GetValidSlotRange();
        
        Debug.Log($"Tentativo di aggiungere {item.itemName} (categoria: {item.GetCategoryDisplayName()}) negli slot {startIndex}-{endIndex}");
        
        // Cerca uno slot vuoto nella categoria appropriata
        for (int i = startIndex; i <= endIndex; i++)
        {
            if (i < bagSlots.Length && bagSlots[i] != null && bagSlots[i].GetItem() == null)
            {
                bagSlots[i].AddItem(item);
                PlaySound(itemPickupSound);
                OnItemAdded?.Invoke(item);
                Debug.Log($"✓ {item.itemName} aggiunto allo slot {i}");
                return true;
            }
        }
        
        // Categoria piena - controlla se l'inventario generale è pieno
        if (IsInventoryFull())
        {
            PlaySound(inventoryFullSound);
            OnInventoryFull?.Invoke();
            Debug.Log($"✗ Inventario completamente pieno per {item.itemName}");
        }
        else
        {
            // Categoria specifica piena
            PlaySound(itemRejectedSound);
            string reason = $"Sezione {item.GetCategoryDisplayName()} piena!";
            OnItemRejected?.Invoke(item, reason);
            Debug.Log($"✗ {reason} - {item.itemName} non aggiunto");
        }
        
        return false;
    }
    
    /// <summary>
    /// Aggiunge un item forzatamente in uno slot specifico (per debug o riorganizzazione)
    /// </summary>
    public bool ForceAddItemToSlot(Item item, int slotIndex)
    {
        if (bagSlots == null || item == null || slotIndex < 0 || slotIndex >= bagSlots.Length)
            return false;
        
        if (bagSlots[slotIndex] == null)
            return false;
        
        // Verifica compatibilità categoria
        if (!item.CanBeInSlot(slotIndex))
        {
            Debug.LogWarning($"Item {item.itemName} ({item.GetCategoryDisplayName()}) non può essere posizionato nello slot {slotIndex} ({CategoryHelper.GetCategoryDisplayName(CategoryHelper.GetCategoryForSlotIndex(slotIndex))})");
            return false;
        }
        
        // Aggiungi allo slot se vuoto
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
    /// Droppa un item da uno slot specifico con validazione categoria
    /// </summary>
    public GameObject DropItemFromSlot(InventorySlot slot)
    {
        if (slot == null)
        {
            Debug.LogError("DropItemFromSlot: Slot is null!");
            return null;
        }
        
        Item item = slot.GetItem();
        if (item == null)
        {
            Debug.Log($"DropItemFromSlot: No item in slot to drop");
            return null;
        }
        
        if (item.itemPrefab == null)
        {
            Debug.LogError($"DropItemFromSlot: Item {item.itemName} has no prefab assigned!");
            return null;
        }
        
        // Ottieni posizione di drop
        Vector3 dropPosition = GetPlayerDropPosition();
        if (dropPosition == Vector3.zero)
        {
            Debug.LogError("DropItemFromSlot: Cannot get player drop position!");
            return null;
        }
        
        Debug.Log($"=== DROPPING {item.itemName} ({item.GetCategoryDisplayName()}) ===");
        
        // Crea wrapper
        GameObject wrapper = ColliderHelper.CreateDroppedObjectWrapper(item.itemPrefab, item, dropPosition);
        
        if (wrapper == null)
        {
            Debug.LogError($"DropItemFromSlot: Failed to create wrapper for {item.itemName}");
            return null;
        }
        
        // Clear slot PRIMA di gestire gli eventi
        slot.ClearSlot();
        
        // Gestisci eventi
        HandleItemDrop(item, slot);
        
        // Abilita collezione dopo delay
        StartCoroutine(EnableCollectionAfterDelay(wrapper.GetComponent<CollectableItem>()));
        
        Debug.Log($"=== SUCCESSFULLY DROPPED {item.itemName} ===");
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
        PlaySound(itemDropSound);
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

    /// <summary>
    /// Conta gli slot vuoti per categoria
    /// </summary>
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
    
    /// <summary>
    /// Conta gli slot vuoti totali
    /// </summary>
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
    
    /// <summary>
    /// Verifica se una categoria è piena
    /// </summary>
    public bool IsCategoryFull(ItemCategory category)
    {
        return GetEmptySlotCountForCategory(category) == 0;
    }
    
    public bool IsInventoryFull() => GetEmptySlotCount() == 0;
    
    /// <summary>
    /// Ottiene tutti gli item di una categoria specifica
    /// </summary>
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
    
    /// <summary>
    /// Debug: Mostra lo stato dell'inventario per categoria
    /// </summary>
    [ContextMenu("Show Category Status")]
    public void ShowCategoryStatus()
    {
        Debug.Log("=== STATO INVENTARIO PER CATEGORIA ===");
        
        foreach (ItemCategory category in System.Enum.GetValues(typeof(ItemCategory)))
        {
            var (startIndex, endIndex) = CategoryHelper.GetSlotRangeForCategory(category);
            int emptySlots = GetEmptySlotCountForCategory(category);
            int totalSlots = endIndex - startIndex + 1;
            int usedSlots = totalSlots - emptySlots;
            
            Debug.Log($"{CategoryHelper.GetCategoryDisplayName(category)} (slot {startIndex}-{endIndex}): {usedSlots}/{totalSlots} utilizzati");
            
            // Mostra gli item in questa categoria
            List<Item> categoryItems = GetItemsOfCategory(category);
            if (categoryItems.Count > 0)
            {
                foreach (Item item in categoryItems)
                {
                    Debug.Log($"  - {item.itemName}");
                }
            }
            else
            {
                Debug.Log($"  - Vuota");
            }
        }
    }
}