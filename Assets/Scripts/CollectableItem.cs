using UnityEngine;

public class CollectableItem : MonoBehaviour
{
    [Header("Item Configuration")]
    public Item itemData;
    
    [Header("Collection Settings")]
    public float collectionRange = 2f;
    public bool requireInteraction = false;
    public KeyCode interactionKey = KeyCode.E;
    
    [Header("Audio & Effects")]
    public AudioClip pickupSound;
    public GameObject pickupEffect;
    
    private bool isCollected = false;
    private GameObject player;
    
    private void Start()
    {
        // Find player
        player = GameObject.FindGameObjectWithTag("Player");
        
        Debug.Log($"CollectableItem initialized for {itemData?.itemName}");
    }
    
    private void Update()
    {
        if (!enabled || isCollected || itemData == null) return;
        
        // Skip if rigidbody is moving fast (just dropped)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null && rb.linearVelocity.magnitude > 0.1f) return;
        
        // Check for collection
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            
            if (distance <= collectionRange)
            {
                if (requireInteraction)
                {
                    if (Input.GetKeyDown(interactionKey))
                    {
                        CollectItem();
                    }
                }
                else
                {
                    CollectItem();
                }
            }
        }
    }
    
    private void CollectItem()
    {
        if (isCollected || itemData == null) return;
        
        InventoryManager inventoryManager = InventoryManager.Instance;
        
        if (inventoryManager != null)
        {
            if (inventoryManager.AddItemToBag(itemData))
            {
                isCollected = true;
                
                // Play pickup sound
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }
                
                // Spawn pickup effect
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, transform.rotation);
                }
                
                Debug.Log($"Collected: {itemData.itemName}");
                
                // Destroy the object
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Inventory is full! Cannot collect " + itemData.itemName);
            }
        }
        else
        {
            Debug.LogError("InventoryManager not found!");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Controlla se è il player che entra nel trigger
        if (other.CompareTag("Player") && !isCollected)
        {
            if (requireInteraction)
            {
                Debug.Log($"Press {interactionKey} to collect {itemData?.itemName}");
            }
            else
            {
                // Auto-collezione se non richiede interazione
                CollectItem();
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectionRange);
    }
}