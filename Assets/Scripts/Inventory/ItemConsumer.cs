using UnityEngine;
using UnityEngine.Events;

public class ItemConsumer : MonoBehaviour
{
    public static ItemConsumer Instance { get; private set; }
    
    [Header("Events")]
    public UnityEvent<Item, float> OnItemConsumed;
    public UnityEvent<Item> OnConsumptionFailed;
    
    [Header("Audio")]
    public AudioSource audioSource;
    
    private HealthBar playerHealthBar;
    
    private void Awake()
    {
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
    }
    
    private void InitializeComponents()
    {
        // Trova la HealthBar del player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealthBar = player.GetComponent<HealthBar>();
        }
        
        // Inizializza AudioSource se non assegnato
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    /// <summary>
    /// Consuma un item dal sistema di inventario
    /// </summary>
    public bool ConsumeItem(Item item, InventorySlot fromSlot)
    {
        if (item == null || fromSlot == null)
        {
            Debug.LogWarning("Item o slot nullo nel tentativo di consumo");
            return false;
        }
        
        // Verifica se l'item può essere consumato
        if (!item.CanBeConsumed())
        {
            Debug.Log($"L'item '{item.itemName}' non può essere consumato");
            OnConsumptionFailed?.Invoke(item);
            return false;
        }
        
        // Verifica se il player ha bisogno di cura
        if (playerHealthBar == null)
        {
            Debug.LogError("HealthBar del player non trovata!");
            OnConsumptionFailed?.Invoke(item);
            return false;
        }
        
        // Se la salute è già al massimo, non consumare
        if (playerHealthBar.health >= playerHealthBar.maxHealth)
        {
            Debug.Log("Salute già al massimo, pozione non necessaria");
            OnConsumptionFailed?.Invoke(item);
            return false;
        }
        
        // Consuma l'item
        float healAmount = item.GetHealAmount();
        
        // Applica la cura
        ApplyHealing(healAmount);
        
        // Riproduci suono
        PlayConsumeSound(item);
        
        // Crea effetto visivo
        CreateConsumeEffect(item);
        
        // Rimuovi l'item dallo slot
        fromSlot.ClearSlot();
        
        // Notifica il sistema di inventario della rimozione
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnItemRemoved?.Invoke(item);
        }
        
        // Triggera evento di consumo
        OnItemConsumed?.Invoke(item, healAmount);
        
        Debug.Log($"Consumato {item.itemName} - Cura: {healAmount} HP");
        
        return true;
    }
    
    /// <summary>
    /// Applica la cura al player
    /// </summary>
    private void ApplyHealing(float amount)
    {
        if (playerHealthBar != null)
        {
            float oldHealth = playerHealthBar.health;
            playerHealthBar.health = Mathf.Min(playerHealthBar.health + amount, playerHealthBar.maxHealth);
            
            Debug.Log($"Salute: {oldHealth} -> {playerHealthBar.health} (+{playerHealthBar.health - oldHealth})");
        }
    }
    
    /// <summary>
    /// Riproduce il suono di consumo
    /// </summary>
    private void PlayConsumeSound(Item item)
    {
        AudioClip sound = item.GetConsumeSound();
        
        if (sound != null && audioSource != null)
        {
            audioSource.PlayOneShot(sound);
        }
    }
    
    /// <summary>
    /// Crea l'effetto visivo di consumo
    /// </summary>
    private void CreateConsumeEffect(Item item)
    {
        GameObject effect = item.GetConsumeEffect();
        
        if (effect != null && playerHealthBar != null)
        {
            Vector3 effectPosition = playerHealthBar.transform.position;
            effectPosition.y += 1f; // Spawna sopra il player
            
            GameObject instantiatedEffect = Instantiate(effect, effectPosition, Quaternion.identity);
            
            // Distruggi l'effetto dopo 3 secondi
            Destroy(instantiatedEffect, 3f);
        }
    }
    
    /// <summary>
    /// Verifica se un item può essere consumato in base alle condizioni attuali
    /// </summary>
    public bool CanConsumeItemNow(Item item)
    {
        if (item == null || !item.CanBeConsumed()) return false;
        
        if (playerHealthBar == null) return false;
        
        // Non permettere consumo se salute è già al massimo
        return playerHealthBar.health < playerHealthBar.maxHealth;
    }
}