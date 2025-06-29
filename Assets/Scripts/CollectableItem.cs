using UnityEngine;

public class CollectableItem : MonoBehaviour
{
    [Header("Item Configuration")]
    public Item itemData; // Reference to the ScriptableObject
    public float rotationSpeed = 50f; // Optional: make items rotate
    public float bobSpeed = 2f; // Optional: make items bob up and down
    public float bobHeight = 0.5f;
    
    [Header("Collection Settings")]
    public float collectionRange = 2f; // Distance at which item can be collected
    public bool requireInteraction = false; // If true, player must press a key
    public KeyCode interactionKey = KeyCode.E;
    
    [Header("Audio & Effects")]
    public AudioClip pickupSound;
    public GameObject pickupEffect; // Particle effect when collected
    
    private Vector3 startPosition;
    private AudioSource audioSource;
    private bool isCollected = false;
    private GameObject player;
    private Collider itemCollider;
    private Renderer itemRenderer;
    private Rigidbody itemRigidbody;
    
    // NEW: Store original position and rotation for dropping
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    
    private void Start()
    {
        startPosition = transform.position;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        
        audioSource = GetComponent<AudioSource>();
        itemCollider = GetComponent<Collider>();
        itemRenderer = GetComponent<Renderer>();
        itemRigidbody = GetComponent<Rigidbody>();
        
        // Find player - assumes player has "Player" tag
        player = GameObject.FindGameObjectWithTag("Player");
        
        // Fix mesh collider issues
        FixColliderSetup();
    }
    
    private void FixColliderSetup()
    {
        // Check if we have a MeshCollider that might cause issues
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        if (meshCollider != null)
        {
            // If we have a rigidbody and the mesh collider is not convex, fix it
            if (itemRigidbody != null && !meshCollider.convex)
            {
                // Option 1: Make the mesh collider convex
                meshCollider.convex = true;
                Debug.Log($"Made mesh collider convex for {itemData?.itemName}");
            }
        }
        
        // Ensure we have a trigger collider for detection
        Collider triggerCollider = GetTriggerCollider();
        if (triggerCollider == null)
        {
            SphereCollider col = gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = collectionRange;
            itemCollider = col;
        }
        else
        {
            itemCollider = triggerCollider;
        }
    }
    
    private Collider GetTriggerCollider()
    {
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            if (col.isTrigger)
                return col;
        }
        return null;
    }
    
    private void Update()
    {
        if (isCollected || (itemRigidbody != null && !itemRigidbody.isKinematic)) return;
        
        // DEBUG: Check if Update is being called
        if (Time.frameCount % 120 == 0) // Log every 2 seconds at 60fps
        {
            Debug.Log($"[UPDATE] {gameObject.name}: isCollected={isCollected}, active={gameObject.activeInHierarchy}, position={transform.position}, startPosition={startPosition}");
        }
        
        // Optional: Add rotation and bobbing animation
        if (rotationSpeed > 0)
        {
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
        
        // TEMPORARY FIX: Disable bobbing for now to test if this is the issue
        bool enableBobbing = false; // Set to true to re-enable bobbing
        
        // FIXED: Only do bobbing if we're not moving the object elsewhere
        if (enableBobbing && bobSpeed > 0 && bobHeight > 0)
        {
            // Use startPosition as the BASE for bobbing (this should now be the drop position)
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            
            // CRITICAL: Keep X and Z at startPosition, only modify Y for bobbing
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
            
            // Debug every few frames
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"[BOBBING] {gameObject.name}: startPos={startPosition}, currentPos={transform.position}");
            }
        }
        else
        {
            // FORCE OBJECT TO STAY AT startPosition (no bobbing)
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"[NO BOBBING] {gameObject.name}: staying at startPosition={startPosition}, currentPos={transform.position}");
            }
            
            // Force the object to stay exactly at startPosition
            if (Vector3.Distance(transform.position, startPosition) > 0.1f)
            {
                Debug.LogWarning($"[FORCE POSITION] {gameObject.name}: Object moved from {transform.position} back to {startPosition}");
                transform.position = startPosition;
            }
        }
        
        // Check for collection
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            
            // DEBUG: Log distance check
            if (Time.frameCount % 60 == 0) // Log every second
            {
                Debug.Log($"[DISTANCE CHECK] {gameObject.name}: distance={distance:F2}, collectionRange={collectionRange}, requireInteraction={requireInteraction}");
            }
            
            if (distance <= collectionRange)
            {
                if (requireInteraction)
                {
                    // Show interaction prompt (you might want to display UI here)
                    if (Input.GetKeyDown(interactionKey))
                    {
                        Debug.Log($"[INPUT] {interactionKey} pressed for {gameObject.name}");
                        CollectItem();
                    }
                }
                else
                {
                    // Auto-collect when in range
                    Debug.Log($"[AUTO-COLLECT] Attempting to collect {gameObject.name}");
                    CollectItem();
                }
            }
        }
        else
        {
            // DEBUG: Player not found
            if (Time.frameCount % 240 == 0) // Log every 4 seconds
            {
                Debug.Log($"[WARNING] {gameObject.name}: Player not found!");
            }
        }
    }
    
    private void CollectItem()
    {
        if (isCollected || itemData == null) return;
        
        // Use the singleton instance instead of FindObjectOfType
        InventoryManager inventoryManager = InventoryManager.Instance;
        
        if (inventoryManager != null)
        {
            Debug.Log($"Using InventoryManager Singleton with InstanceID: {inventoryManager.GetInstanceID()}");
            
            // Try to add item to inventory
            if (inventoryManager.AddItemToBag(itemData))
            {
                isCollected = true;
                
                // NEW: Register this GameObject with the inventory manager BEFORE disabling
                Debug.Log($"REGISTERING COLLECTED ITEM: {itemData.itemName} - GameObject: {gameObject.name}");
                inventoryManager.RegisterCollectedItem(itemData, gameObject);
                
                // Play pickup sound
                if (pickupSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(pickupSound);
                }
                
                // Spawn pickup effect
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, transform.rotation);
                }
                
                // Optional: Show collection message
                Debug.Log($"Collected: {itemData.itemName}");
                
                // Instead of destroying, disable the item
                DisableItem();
            }
            else
            {
                // Inventory is full
                Debug.Log("Inventory is full! Cannot collect " + itemData.itemName);
            }
        }
        else
        {
            Debug.LogError("InventoryManager not found in scene!");
        }
    }
    
    // Disable the item instead of destroying it
    public void DisableItem()
    {
        isCollected = true;
        gameObject.SetActive(false);
    }
    
    // NEW: Reactivate the item when dropped
    public void ReactivateItem()
    {
        Debug.Log($"REACTIVATING ITEM: {itemData.itemName} - GameObject: {gameObject.name}");
        Debug.Log($"[REACTIVATE] Current transform position: {transform.position}");
        Debug.Log($"[REACTIVATE] Old startPosition: {startPosition}");
        
        isCollected = false;
        
        // CRITICAL FIX: Force update startPosition to the NEW drop position
        // This must happen AFTER the InventoryManager sets the transform.position
        startPosition = transform.position;
        Debug.Log($"[REACTIVATE] NEW startPosition set to: {startPosition}");
        
        // FORCE STOP any existing bobbing animation by resetting the position immediately
        transform.position = startPosition;
        Debug.Log($"[REACTIVATE] FORCED transform position to: {transform.position}");
        
        // CRITICAL: Re-find the player after reactivation
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError($"[REACTIVATE] Player not found after reactivation for {gameObject.name}!");
        }
        else
        {
            Debug.Log($"[REACTIVATE] Player found: {player.name}");
        }
        
        // Reset any physics
        if (itemRigidbody != null)
        {
            itemRigidbody.linearVelocity = Vector3.zero;
            itemRigidbody.angularVelocity = Vector3.zero;
            
            // If rigidbody was kinematic during collection, make it non-kinematic again
            if (itemRigidbody.isKinematic)
            {
                itemRigidbody.isKinematic = false;
            }
        }
        
        // IMPORTANT: Re-enable ALL colliders, not just the trigger one
        Collider[] allColliders = GetComponents<Collider>();
        foreach (Collider col in allColliders)
        {
            col.enabled = true;
            Debug.Log($"Re-enabled collider: {col.GetType().Name}, isTrigger: {col.isTrigger}");
        }
        
        // Make sure the renderer is enabled
        if (itemRenderer != null)
        {
            itemRenderer.enabled = true;
        }
        
        // SIMPLIFIED FIX: Just reset the existing trigger collider
        SphereCollider triggerCollider = GetTriggerCollider() as SphereCollider;
        if (triggerCollider != null)
        {
            triggerCollider.center = Vector3.zero;
            triggerCollider.radius = collectionRange;
            itemCollider = triggerCollider;
            Debug.Log($"RESET trigger collider - center: {triggerCollider.center}, radius: {triggerCollider.radius}");
        }
        else
        {
            // Create new trigger if none exists
            SphereCollider newTrigger = gameObject.AddComponent<SphereCollider>();
            newTrigger.isTrigger = true;
            newTrigger.center = Vector3.zero;
            newTrigger.radius = collectionRange;
            itemCollider = newTrigger;
            Debug.Log($"CREATED NEW trigger collider - center: {newTrigger.center}, radius: {newTrigger.radius}");
        }
        
        // Force physics update
        Physics.SyncTransforms();
        
        Debug.Log($"✅ Successfully reactivated item: {itemData.itemName} at FINAL position {transform.position}");
        Debug.Log($"✅ FINAL startPosition: {startPosition}");
    }
    
    // NEW: Coroutine to recreate trigger collider after a frame delay
    private System.Collections.IEnumerator RecreateTrigggerAfterFrame()
    {
        // Wait one frame to ensure position is fully set
        yield return null;
        
        Debug.Log($"[COROUTINE] Recreating trigger collider for {gameObject.name} at position {transform.position}");
        
        // First, destroy the existing trigger collider
        SphereCollider oldTrigger = GetTriggerCollider() as SphereCollider;
        if (oldTrigger != null)
        {
            Debug.Log($"Destroying old trigger collider");
            DestroyImmediate(oldTrigger);
        }
        
        // Wait another frame after destroying
        yield return null;
        
        // Create a fresh trigger collider
        SphereCollider newTrigger = gameObject.AddComponent<SphereCollider>();
        newTrigger.isTrigger = true;
        newTrigger.center = Vector3.zero; // Local center
        newTrigger.radius = collectionRange;
        
        // Update references
        itemCollider = newTrigger;
        
        Debug.Log($"[COROUTINE] CREATED NEW trigger collider - center: {newTrigger.center}, radius: {newTrigger.radius}");
        Debug.Log($"[COROUTINE] GameObject position: {transform.position}");
        Debug.Log($"[COROUTINE] Collider world bounds: {newTrigger.bounds}");
        
        // Force physics update
        Physics.SyncTransforms();
        
        // Final debug
        DebugColliderState();
    }
    
    // NEW: Method to drop this specific item (can be called from UI)
    public void DropThisItem()
    {
        InventoryManager inventoryManager = InventoryManager.Instance;
        if (inventoryManager != null)
        {
            inventoryManager.DropItem(itemData);
        }
    }
    
    // Optional: Visual feedback when player is in range
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && requireInteraction && !isCollected)
        {
            // Show interaction UI or highlight item
            Debug.Log($"Press {interactionKey} to collect {itemData.itemName}");
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && requireInteraction)
        {
            // Hide interaction UI
        }
    }
    
    // Helper method to visualize collection range in Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectionRange);
        
        // NEW: Show drop position preview
        if (Application.isPlaying)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector3 dropPos = player.transform.position + player.transform.forward * 2f + Vector3.up;
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(dropPos, Vector3.one * 0.5f);
            }
        }
    }
    
    // NEW: Debug method to check collider state
    [ContextMenu("Debug Collider State")]
    public void DebugColliderState()
    {
        Debug.Log($"=== COLLIDER DEBUG FOR {gameObject.name} ===");
        Debug.Log($"isCollected: {isCollected}");
        Debug.Log($"GameObject active: {gameObject.activeInHierarchy}");
        
        Collider[] allColliders = GetComponents<Collider>();
        Debug.Log($"Total colliders: {allColliders.Length}");
        
        for (int i = 0; i < allColliders.Length; i++)
        {
            Collider col = allColliders[i];
            Debug.Log($"Collider {i}: {col.GetType().Name}");
            Debug.Log($"  - Enabled: {col.enabled}");
            Debug.Log($"  - IsTrigger: {col.isTrigger}");
            Debug.Log($"  - Bounds: {col.bounds}");
        }
        
        if (itemRigidbody != null)
        {
            Debug.Log($"Rigidbody isKinematic: {itemRigidbody.isKinematic}");
        }
    }
}