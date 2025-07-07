using UnityEngine;

public class InventoryDebugHelper : MonoBehaviour
{
    [Header("Debug References")]
    public InventoryInputHandler inputHandler;
    public InventoryManager inventoryManager;
    
    [Header("Debug Settings")]
    public bool enableDebugInput = false;
    public bool logMouseHoverInfo = true; // NUOVO: Log info mouse hover
    
    [Header("Test Items per Category")]
    public Item[] testWeapons;    // Armi da testare
    public Item[] testPotions;    // Pozioni da testare
    public Item[] testGenericItems; // Item generici da testare
    
    private void Start()
    {
        Debug.Log("=== INVENTORY DEBUG HELPER WITH MOUSE PRIORITY ===");
        
        FindReferences();
        ValidateInventoryStructure();
        ShowCategoryConfiguration();
        CheckMousePrioritySetup();
        
        Debug.Log("=== END DEBUG ===");
    }
    
    private void FindReferences()
    {
        // Find InventoryInputHandler
        if (inputHandler == null)
            inputHandler = FindObjectOfType<InventoryInputHandler>();
        
        if (inputHandler != null)
        {
            Debug.Log($"✓ Found InventoryInputHandler on: {inputHandler.gameObject.name}");
            Debug.Log($"  - Enabled: {inputHandler.enabled}");
            Debug.Log($"  - GameObject active: {inputHandler.gameObject.activeInHierarchy}");
            Debug.Log($"  - Keyboard Selection: {inputHandler.enableKeyboardSelection}");
        }
        else
        {
            Debug.LogError("✗ NO InventoryInputHandler found in scene!");
        }
        
        // Check InventoryManager
        if (inventoryManager == null)
            inventoryManager = InventoryManager.Instance;
        
        if (inventoryManager != null)
        {
            Debug.Log($"✓ InventoryManager found: {inventoryManager.gameObject.name}");
        }
        else
        {
            Debug.LogError("✗ NO InventoryManager found!");
        }
    }
    
    private void CheckMousePrioritySetup()
    {
        Debug.Log("=== MOUSE PRIORITY SETUP CHECK ===");
        
        if (inputHandler != null)
        {
            if (inputHandler.enableKeyboardSelection)
            {
                Debug.LogWarning("⚠ Keyboard selection is ENABLED. Mouse priority may not work as expected.");
                Debug.Log("💡 Set 'enableKeyboardSelection = false' for pure mouse hover + Q drop");
            }
            else
            {
                Debug.Log("✅ Keyboard selection is DISABLED - Mouse hover + Q will work perfectly!");
            }
        }
        
        Debug.Log("🖱️ MOUSE CONTROLS:");
        Debug.Log("  - Hover over slot + Q = Drop item");
        Debug.Log("  - Right-click on slot = Drop item (alternative)");
        Debug.Log("  - No need to select slots with numbers/arrows");
    }
    
    private void ValidateInventoryStructure()
    {
        if (inventoryManager?.bagSlots == null)
        {
            Debug.LogError("✗ BagSlots not found!");
            return;
        }
        
        int totalSlots = inventoryManager.bagSlots.Length;
        Debug.Log($"Total slots: {totalSlots} (Expected: {CategoryHelper.TOTAL_SLOTS})");
        
        if (totalSlots != CategoryHelper.TOTAL_SLOTS)
        {
            Debug.LogWarning($"⚠ Expected {CategoryHelper.TOTAL_SLOTS} slots but found {totalSlots}!");
        }
        
        // Verifica ogni slot
        for (int i = 0; i < totalSlots; i++)
        {
            var slot = inventoryManager.bagSlots[i];
            if (slot == null)
            {
                Debug.LogError($"✗ Slot {i} is NULL!");
                continue;
            }
            
            ItemCategory category = CategoryHelper.GetCategoryForSlotIndex(i);
            var item = slot.GetItem();
            
            if (item != null)
            {
                bool isCompatible = item.CanBeInSlot(i);
                string status = isCompatible ? "✓" : "✗ INCOMPATIBLE";
                Debug.Log($"Slot {i} ({CategoryHelper.GetCategoryDisplayName(category)}): {item.itemName} ({item.GetCategoryDisplayName()}) {status}");
                
                if (item.itemPrefab == null)
                {
                    Debug.LogWarning($"⚠ Item {item.itemName} has NO PREFAB assigned!");
                }
            }
            else
            {
                Debug.Log($"Slot {i} ({CategoryHelper.GetCategoryDisplayName(category)}): Empty");
            }
        }
    }
    
    private void ShowCategoryConfiguration()
    {
        Debug.Log("=== CATEGORY CONFIGURATION ===");
        
        foreach (ItemCategory category in System.Enum.GetValues(typeof(ItemCategory)))
        {
            var (startIndex, endIndex) = CategoryHelper.GetSlotRangeForCategory(category);
            int emptySlots = inventoryManager.GetEmptySlotCountForCategory(category);
            int totalSlots = endIndex - startIndex + 1;
            
            Debug.Log($"{CategoryHelper.GetCategoryDisplayName(category)}: Slot {startIndex}-{endIndex} ({emptySlots}/{totalSlots} empty)");
        }
    }
    
    private void Update()
    {
        if (!enableDebugInput) return;
        
        // Log mouse hover info
        if (logMouseHoverInfo && inputHandler != null)
        {
            string hoverInfo = inputHandler.GetHoveredSlotInfo();
            if (hoverInfo != "No hovered slot" && Time.frameCount % 60 == 0) // Log ogni secondo circa
            {
                Debug.Log($"🖱️ {hoverInfo}");
            }
        }
        
        // NOTA: Non controlliamo più se l'input è stato processato dall'InputHandler
        // perché ora l'InputHandler gestisce tutto centralizzato
        
        // Test categorie con tasti funzione
        if (Input.GetKeyDown(KeyCode.F1))
        {
            TestAddWeapon();
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            TestAddPotion();
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            TestAddGenericItem();
        }
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            inventoryManager.ShowCategoryStatus();
        }
        else if (Input.GetKeyDown(KeyCode.F5))
        {
            ShowMouseHoverStatus();
        }
        
        // Test selezione slot (solo se keyboard selection è abilitato)
        if (inputHandler != null && inputHandler.enableKeyboardSelection)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TestSlotSelection(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TestSlotSelection(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                TestSlotSelection(2);
            }
        }
        
        // Test drop diretto (bypassa il sistema normale per debug)
        if (Input.GetKeyDown(KeyCode.F9))
        {
            TestDirectDrop();
        }
    }
    
    /// <summary>
    /// NUOVO: Mostra lo stato del mouse hover
    /// </summary>
    private void ShowMouseHoverStatus()
    {
        if (inputHandler == null) return;
        
        Debug.Log("=== MOUSE HOVER STATUS ===");
        Debug.Log(inputHandler.GetHoveredSlotInfo());
        
        // Trova slot attualmente hovato
        if (inventoryManager?.bagSlots != null)
        {
            for (int i = 0; i < inventoryManager.bagSlots.Length; i++)
            {
                var slot = inventoryManager.bagSlots[i];
                if (slot != null && slot.IsHovered())
                {
                    Debug.Log($"Currently hovered slot: {slot.GetSlotInfo()}");
                    break;
                }
            }
        }
    }
    
    private void TestAddWeapon()
    {
        if (testWeapons.Length == 0)
        {
            Debug.LogWarning("No test weapons assigned!");
            return;
        }
        
        Item weapon = testWeapons[Random.Range(0, testWeapons.Length)];
        Debug.Log($"Testing add weapon: {weapon.itemName}");
        
        bool added = inventoryManager.AddItemToBag(weapon);
        Debug.Log($"Weapon add result: {(added ? "SUCCESS" : "FAILED")}");
    }
    
    private void TestAddPotion()
    {
        if (testPotions.Length == 0)
        {
            Debug.LogWarning("No test potions assigned!");
            return;
        }
        
        Item potion = testPotions[Random.Range(0, testPotions.Length)];
        Debug.Log($"Testing add potion: {potion.itemName}");
        
        bool added = inventoryManager.AddItemToBag(potion);
        Debug.Log($"Potion add result: {(added ? "SUCCESS" : "FAILED")}");
    }
    
    private void TestAddGenericItem()
    {
        if (testGenericItems.Length == 0)
        {
            Debug.LogWarning("No test generic items assigned!");
            return;
        }
        
        Item genericItem = testGenericItems[Random.Range(0, testGenericItems.Length)];
        Debug.Log($"Testing add generic item: {genericItem.itemName}");
        
        bool added = inventoryManager.AddItemToBag(genericItem);
        Debug.Log($"Generic item add result: {(added ? "SUCCESS" : "FAILED")}");
    }
    
    private void TestSlotSelection(int slotIndex)
    {
        if (inventoryManager?.bagSlots == null) return;
        
        if (slotIndex >= 0 && slotIndex < inventoryManager.bagSlots.Length)
        {
            var slot = inventoryManager.bagSlots[slotIndex];
            var item = slot?.GetItem();
            ItemCategory category = CategoryHelper.GetCategoryForSlotIndex(slotIndex);
            
            Debug.Log($"DEBUG: Slot {slotIndex} ({CategoryHelper.GetCategoryDisplayName(category)}) - Item: {(item != null ? $"{item.itemName} ({item.GetCategoryDisplayName()})" : "Empty")}");
        }
    }
    
    /// <summary>
    /// NUOVO: Test drop diretto per debug
    /// </summary>
    private void TestDirectDrop()
    {
        if (inventoryManager?.bagSlots == null)
        {
            Debug.LogError("Cannot test drop - InventoryManager or bagSlots is null");
            return;
        }
        
        // Trova il primo slot con un item
        for (int i = 0; i < inventoryManager.bagSlots.Length; i++)
        {
            var slot = inventoryManager.bagSlots[i];
            if (slot != null && slot.GetItem() != null)
            {
                var item = slot.GetItem();
                ItemCategory category = CategoryHelper.GetCategoryForSlotIndex(i);
                
                Debug.Log($"F9 DEBUG: Direct dropping {item.itemName} ({item.GetCategoryDisplayName()}) from slot {i} ({CategoryHelper.GetCategoryDisplayName(category)})");
                
                slot.TriggerDrop();
                return;
            }
        }
        
        Debug.Log("F9 DEBUG: No items found to drop");
    }
    
    // Metodi di debug pubblici
    [ContextMenu("Test Add Random Weapon")]
    public void DebugAddRandomWeapon() => TestAddWeapon();
    
    [ContextMenu("Test Add Random Potion")]
    public void DebugAddRandomPotion() => TestAddPotion();
    
    [ContextMenu("Test Add Random Generic")]
    public void DebugAddRandomGeneric() => TestAddGenericItem();
    
    [ContextMenu("Show Mouse Hover Status")]
    public void DebugShowMouseHoverStatus() => ShowMouseHoverStatus();
    
    [ContextMenu("Show All Slot Info")]
    public void DebugShowAllSlotInfo()
    {
        if (inventoryManager?.bagSlots != null)
        {
            for (int i = 0; i < inventoryManager.bagSlots.Length; i++)
            {
                var slot = inventoryManager.bagSlots[i];
                if (slot != null)
                {
                    Debug.Log(slot.GetSlotInfo());
                }
            }
        }
    }
    
    [ContextMenu("Clear All Slots")]
    public void DebugClearAllSlots()
    {
        if (inventoryManager?.bagSlots != null)
        {
            foreach (var slot in inventoryManager.bagSlots)
            {
                slot?.ClearSlot();
            }
            Debug.Log("All slots cleared!");
        }
    }
    
    [ContextMenu("Fill Categories Test")]
    public void DebugFillCategories()
    {
        // Riempie ogni categoria con item di test
        for (int i = 0; i < 3; i++) TestAddWeapon();
        for (int i = 0; i < 3; i++) TestAddPotion();
        for (int i = 0; i < 3; i++) TestAddGenericItem();
    }
    
    [ContextMenu("Test Mouse Priority System")]
    public void DebugTestMousePriority()
    {
        Debug.Log("=== TESTING MOUSE PRIORITY SYSTEM ===");
        Debug.Log("1. Add some items with F1/F2/F3");
        Debug.Log("2. Move mouse over a slot with an item");
        Debug.Log("3. Press Q to drop the hovered item");
        Debug.Log("4. No need to select slots with numbers!");
        Debug.Log("5. Right-click also works as alternative");
        
        if (inputHandler != null)
        {
            Debug.Log($"Keyboard Selection: {(inputHandler.enableKeyboardSelection ? "ENABLED (not recommended)" : "DISABLED (recommended)")}");
        }
    }
}