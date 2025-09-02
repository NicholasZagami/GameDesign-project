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

    [Header("Prompt UI")]
    [Tooltip("Canvas/Panel da mostrare quando il player � nel range (es. 'Premi [E] per interagire')")]
    public GameObject interactPromptUI;
    [Tooltip("Se true: mostra il prompt solo se requireInteraction � abilitato")]
    public bool showPromptOnlyWhenRequireInteraction = true;

    [Header("Audio & Effects")]
    public AudioClip pickupSound;
    public GameObject pickupEffect;

    private bool isCollected = false;
    private Transform player;

    void Start()
    {
        var pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj) player = pObj.transform;

        // Assicurati che il prompt parta nascosto
        SetPromptVisible(false);
    }

    void OnEnable()
    {
        // Se gi� raccolta in un salvataggio precedente, rimuovi subito
        var s = SaveManager.Instance?.CurrentSave;
        if (s != null && !string.IsNullOrEmpty(uniqueInstanceId) &&
            s.collectedItems.Contains(uniqueInstanceId))
        {
            SetPromptVisible(false);
            Destroy(gameObject);
        }
    }

    void OnDisable()
    {
        // Sicurezza: se questo oggetto viene disattivato, nascondi il prompt
        SetPromptVisible(false);
    }

    void Update()
    {
        if (!enabled || isCollected || itemData == null || player == null)
        {
            SetPromptVisible(false);
            return;
        }

        // Se l'oggetto sta ancora cadendo/rotolando, evita la raccolta automatica
        var rb = GetComponent<Rigidbody>();
        if (rb != null && rb.linearVelocity.magnitude > 0.1f)
        {
            SetPromptVisible(false);
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        bool inRange = distance <= collectionRange;

        // Gestione prompt
        bool allowPrompt = (!showPromptOnlyWhenRequireInteraction) || (requireInteraction);
        SetPromptVisible(inRange && allowPrompt);

        // Raccolta
        if (!inRange) return;

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

            // Nascondi prompt prima di distruggere
            SetPromptVisible(false);

            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCollected && !requireInteraction)
            CollectItem();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectionRange);
    }

    private void SetPromptVisible(bool visible)
    {
        if (interactPromptUI != null && interactPromptUI.activeSelf != visible)
            interactPromptUI.SetActive(visible);
    }
}