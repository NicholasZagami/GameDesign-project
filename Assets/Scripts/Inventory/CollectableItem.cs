using UnityEngine;

public class CollectableItem : MonoBehaviour
{
    [Header("Item Configuration")]
    public Item itemData;

    [Header("Persistence")]
    [Tooltip("ID UNIVOCO di QUESTA istanza in scena (es: L1_Potion_03)")]
    public string uniqueInstanceId;

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
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void OnEnable()
    {
        // Se già raccolta in un salvataggio precedente, rimuovi subito
        var s = SaveManager.Instance?.CurrentSave;
        if (s != null && !string.IsNullOrEmpty(uniqueInstanceId) &&
            s.collectedItems.Contains(uniqueInstanceId))
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!enabled || isCollected || itemData == null) return;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null && rb.linearVelocity.magnitude > 0.1f) return;

        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance <= collectionRange)
            {
                if (requireInteraction)
                {
                    if (Input.GetKeyDown(interactionKey))
                        CollectItem();
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

        var inv = InventoryManager.Instance;
        if (inv != null && inv.AddItemToBag(itemData))
        {
            isCollected = true;

            // segna questa ISTANZA come raccolta
            if (!string.IsNullOrEmpty(uniqueInstanceId))
                SaveManager.Instance?.RegisterItemCollected(uniqueInstanceId);

            if (pickupSound != null)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            if (pickupEffect != null)
                Instantiate(pickupEffect, transform.position, transform.rotation);

            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCollected && !requireInteraction)
            CollectItem();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectionRange);
    }
}