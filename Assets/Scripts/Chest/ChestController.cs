using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ChestController : MonoBehaviour
{
    [Header("Chest Configuration")]
    public List<Item> chestItems = new List<Item>();
    public int maxChestSize = 10;
    
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
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
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
                chestInventoryManager.DisplayChestItems(chestItems);
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
                chestInventoryManager.ClearAllChestSlots();
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
        if (item == null) return false;
        
        if (chestItems.Count >= maxChestSize)
        {
            return false;
        }
        
        chestItems.Add(item);
        return true;
    }
    
    public bool RemoveItemFromChest(Item item)
    {
        return chestItems.Remove(item);
    }
    
    public List<Item> GetChestItems()
    {
        return new List<Item>(chestItems);
    }
    
    public void ClearChest()
    {
        chestItems.Clear();
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
}