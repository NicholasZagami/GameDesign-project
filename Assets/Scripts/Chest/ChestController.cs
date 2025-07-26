using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ChestController : MonoBehaviour
{
    [Header("Chest Configuration")]
    public Item[] initialItems; // Items to load into chest slots on start
    
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
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }
    
    private void LoadInitialItemsOnce()
    {
        if (initialItems != null && initialItems.Length > 0)
        {
            // Find or create chest inventory manager
            if (chestInventoryManager == null)
            {
                GameObject chestUI = GameObject.Find("ChestInventory");
                if (chestUI != null)
                {
                    chestInventoryManager = chestUI.GetComponent<ChestInventoryManager>();
                    if (chestInventoryManager == null)
                    {
                        chestInventoryManager = chestUI.AddComponent<ChestInventoryManager>();
                    }
                }
            }
            
            if (chestInventoryManager != null)
            {
                chestInventoryManager.LoadItemsIntoChest(initialItems);
            }
        }
    }
    
    private void Start()
    {
        inventoryManager = InventoryManager.Instance;
        if (inventoryManager == null)
            inventoryManager = FindObjectOfType<InventoryManager>();
        
        if (chestInventoryManager == null && chestInventoryUI != null)
        {
            chestInventoryManager = chestInventoryUI.GetComponent<ChestInventoryManager>();
            if (chestInventoryManager == null)
            {
                chestInventoryManager = chestInventoryUI.AddComponent<ChestInventoryManager>();
            }
        }
        
        playerObject = GameObject.FindGameObjectWithTag("Player");
        
        if (chestInventoryUI != null)
        {
            chestInventoryUI.SetActive(false);
        }
        else
        {
            GameObject foundChestUI = GameObject.Find("ChestInventory");
            if (foundChestUI != null)
            {
                chestInventoryUI = foundChestUI;
                chestInventoryUI.SetActive(false);
            }
        }
        
        SetupInputAction();
        LoadInitialItemsOnce(); // Load items only once
    }
    
    private void SetupInputAction()
    {
        if (interactAction != null)
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteractPerformed;
        }
    }
    
    private void OnDestroy()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteractPerformed;
            interactAction.action.Disable();
        }
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
        
        if (isPlayerNear && !wasPlayerNear)
        {
            // Player entered range
        }
        else if (!isPlayerNear && wasPlayerNear)
        {
            CloseChest();
        }
    }
    
    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (!isPlayerNear) return;
        
        HandleInteraction();
    }
    
    private void HandleInteraction()
    {
        if (isChestOpen)
        {
            CloseChest();
        }
        else
        {
            OpenChest();
        }
    }
    
    private void OpenChest()
    {
        if (isChestOpen) return;
        
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
            
            if (chestInventoryManager != null)
            {
                chestInventoryManager.SetActive(false);
            }
        }
        
        TriggerChestAnimation(false);
    }
    
    public void PlayPickupSound()
    {
        PlaySound(pickupItemSound);
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    private void TriggerChestAnimation(bool isOpening)
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("IsOpen", isOpening);
        }
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
}