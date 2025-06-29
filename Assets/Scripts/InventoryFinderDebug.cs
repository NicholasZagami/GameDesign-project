using UnityEngine;

public class InventoryFinderDebug : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== INVENTORY MANAGER DEBUG ===");
        
        // Try to find InventoryManager
        InventoryManager[] allInventoryManagers = FindObjectsOfType<InventoryManager>();
        Debug.Log($"Found {allInventoryManagers.Length} InventoryManager(s) in scene");
        
        for (int i = 0; i < allInventoryManagers.Length; i++)
        {
            Debug.Log($"InventoryManager {i}: {allInventoryManagers[i].gameObject.name}");
            Debug.Log($"Active: {allInventoryManagers[i].gameObject.activeInHierarchy}");
            Debug.Log($"Enabled: {allInventoryManagers[i].enabled}");
        }
        
        // Check for GameObject named "InventoryComponent"
        GameObject inventoryGO = GameObject.Find("InventoryComponent");
        if (inventoryGO != null)
        {
            Debug.Log($"Found GameObject 'Inventory': {inventoryGO.name}");
            Debug.Log($"Active: {inventoryGO.activeInHierarchy}");
            
            InventoryManager invManager = inventoryGO.GetComponent<InventoryManager>();
            if (invManager != null)
            {
                Debug.Log("InventoryManager component found on 'InventoryComponent' GameObject");
                Debug.Log($"Component enabled: {invManager.enabled}");
            }
            else
            {
                Debug.LogError("No InventoryManager component on 'InventoryComponent' GameObject!");
                
                // List all components on the GameObject
                Component[] components = inventoryGO.GetComponents<Component>();
                Debug.Log($"Components on 'InventoryComponent' GameObject:");
                foreach (Component comp in components)
                {
                    Debug.Log($"- {comp.GetType().Name}");
                }
            }
        }
        else
        {
            Debug.LogError("No GameObject named 'Inventory' found in scene!");
        }
        
        Debug.Log("=== END DEBUG ===");
    }
}