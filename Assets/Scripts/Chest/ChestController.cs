using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ChestController : MonoBehaviour
{
    [Header("Chest Configuration")]
    public Item[] initialItems;

    [Header("World Event")]
    [Tooltip("ID stabile e univoco per questo baule, es: L1_Chest_A")]
    public string uniqueEventId;
    [Tooltip("Se true, il baule consegna i suoi oggetti una sola volta")]
    public bool oneShot = true;

    [Header("UI References")]
    public GameObject chestInventoryUI;
    public ChestInventoryManager chestInventoryManager;

    [Header("Audio")]
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip pickupItemSound;

    [Header("Input System")]
    public InputActionReference interactAction;

    private bool isPlayerNear = false;
    private bool isChestOpen = false;
    private GameObject playerObject;
    private AudioSource audioSource;
    private InventoryManager inventoryManager;
    private float interactionRange = 3f;

    [Header("UI Interazione")]
    public GameObject interactPromptUI;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    private void OnEnable()
    {
        // Se questo evento è già completato: non ricaricare gli oggetti e mostrare lo stato giusto
        if (SaveManager.Instance != null && SaveManager.Instance.IsWorldEventCompleted(uniqueEventId))
        {
            // Non caricare initialItems, assicura chest vuoto e visual aperta (o disabilitata)
            if (chestInventoryManager != null) chestInventoryManager.ClearChest();
            SetOpenedVisuals(true);
            // opzionale: disattiva interazione
            var col = GetComponent<Collider>();
            if (col) col.enabled = false;
        }
    }

    private void Start()
    {
        inventoryManager = InventoryManager.Instance ?? FindObjectOfType<InventoryManager>();

        if (chestInventoryManager == null && chestInventoryUI != null)
            chestInventoryManager = chestInventoryUI.GetComponent<ChestInventoryManager>();

        playerObject = GameObject.FindGameObjectWithTag("Player");

        if (chestInventoryUI == null)
        {
            var foundChestUI = GameObject.Find("ChestInventory");
            if (foundChestUI != null) chestInventoryUI = foundChestUI;
        }
        if (chestInventoryUI != null) chestInventoryUI.SetActive(false);

        SetupInputAction();

        // Carica gli oggetti SOLO se l’evento non è completato
        if (SaveManager.Instance == null || !SaveManager.Instance.IsWorldEventCompleted(uniqueEventId))
            LoadInitialItemsOnce();

        // (FACOLTATIVO) Se vuoi ricevere callback quando un item viene preso dalla UI:
        // Assicurati che ChestInventoryManager esponga un evento; vedi nota più sotto.
        if (chestInventoryManager != null)
            chestInventoryManager.onItemTaken += OnItemTakenFromChest;
    }

    private void OnDestroy()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteractPerformed;
            interactAction.action.Disable();
        }

        if (chestInventoryManager != null)
            chestInventoryManager.onItemTaken -= OnItemTakenFromChest;
    }

    private void SetupInputAction()
    {
        if (interactAction != null)
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteractPerformed;
        }
    }

    private void LoadInitialItemsOnce()
    {
        if (initialItems == null || initialItems.Length == 0) return;

        if (chestInventoryManager == null)
        {
            GameObject chestUI = GameObject.Find("ChestInventory");
            if (chestUI != null)
                chestInventoryManager = chestUI.GetComponent<ChestInventoryManager>() ?? chestUI.AddComponent<ChestInventoryManager>();
        }

        if (chestInventoryManager != null)
            chestInventoryManager.LoadItemsIntoChest(initialItems);
    }

    private void Update()
    {
        CheckPlayerProximity();
    }

    private void CheckPlayerProximity()
    {
        if (playerObject == null) return;

        float distance = Vector3.Distance(transform.position, playerObject.transform.position);
        bool wasPlayerNear = isPlayerNear;
        isPlayerNear = distance <= interactionRange;

        if (!isPlayerNear && wasPlayerNear)
            CloseChest();
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (!isPlayerNear) return;
        HandleInteraction();
    }

    private void HandleInteraction()
    {
        if (isChestOpen) CloseChest();
        else OpenChest();
    }

    private void OpenChest()
    {
        if (isChestOpen) return;
        // Se già completato e oneShot, ignora
        if (oneShot && SaveManager.Instance != null && SaveManager.Instance.IsWorldEventCompleted(uniqueEventId))
            return;

        isChestOpen = true;
        PlaySound(openSound);

        if (chestInventoryUI != null)
        {
            chestInventoryUI.SetActive(true);
            if (chestInventoryManager != null)
            {
                chestInventoryManager.SetupChestSlotClickHandlers(this);
                chestInventoryManager.SetActive(true);
            }
        }

        TriggerChestAnimation(true);
    }

    private void CloseChest()
    {
        if (!isChestOpen) return;
        isChestOpen = false;
        PlaySound(closeSound);

        if (chestInventoryUI != null)
        {
            chestInventoryUI.SetActive(false);
            chestInventoryManager?.SetActive(false);
        }

        TriggerChestAnimation(false);

        // Se il baule è vuoto e oneShot, marca evento come completato
        if (oneShot && chestInventoryManager != null && chestInventoryManager.GetChestItems().Count == 0)
        {
            SaveManager.Instance?.RegisterWorldEventCompleted(uniqueEventId);

            // Opzionale: disabilita interazione per sempre
            var col = GetComponent<Collider>();
            if (col) col.enabled = false;

            // Mantieni il baule “aperto” come feedback, se vuoi
            SetOpenedVisuals(true);
        }
    }

    public void PlayPickupSound() => PlaySound(pickupItemSound);
    private void PlaySound(AudioClip clip) { if (clip != null && audioSource != null) audioSource.PlayOneShot(clip); }

    private void TriggerChestAnimation(bool isOpening)
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null) animator.SetBool("IsOpen", isOpening);
    }

    // Callback quando un item viene preso dalla UI del baule
    private void OnItemTakenFromChest(Item item)
    {
        // Se vuoi completare l’evento al primo prelievo:
        // SaveManager.Instance?.RegisterWorldEventCompleted(uniqueEventId);

        // Oppure: completa quando il baule è completamente svuotato:
        if (oneShot && chestInventoryManager != null && chestInventoryManager.GetChestItems().Count == 0)
            SaveManager.Instance?.RegisterWorldEventCompleted(uniqueEventId);
    }

    public bool AddItemToChest(Item item)
    {
        if (chestInventoryManager != null)
        {
            return chestInventoryManager.AddItemToChest(item);
        }
        return false;
    }
    
    public bool RemoveItemFromChest(Item item)
    {
        if (chestInventoryManager != null)
        {
            return chestInventoryManager.RemoveItemFromChest(item);
        }
        return false;
    }
    
    public List<Item> GetChestItems()
    {
        if (chestInventoryManager != null)
        {
            return chestInventoryManager.GetChestItems();
        }
        return new List<Item>();
    }
    
    public void ClearChest()
    {
        if (chestInventoryManager != null)
        {
            chestInventoryManager.ClearChest();
        }
    }
    
    public bool IsOpen()
    {
        return isChestOpen;
    }
    
    public void ForceClose()
    {
        CloseChest();
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        if (Application.isPlaying)
        {
            Gizmos.color = isChestOpen ? Color.green : Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var movement = other.GetComponent<PlayerMovement>();
            if (movement != null)
                movement.SetNearChest(true);

            if (interactPromptUI != null)
                interactPromptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var movement = other.GetComponent<PlayerMovement>();
            if (movement != null)
                movement.SetNearChest(false);

            if (interactPromptUI != null)
                interactPromptUI.SetActive(false);
        }
    }

    private void SetOpenedVisuals(bool opened)
    {
        var animator = GetComponent<Animator>();
        if (animator != null) animator.SetBool("IsOpen", opened);
        // Qui puoi anche cambiare mesh/material, ecc.
    }
}