using UnityEngine;
using UnityEngine.UI;

public class InventoryHealthBinder : MonoBehaviour
{
    public static InventoryHealthBinder Instance { get; private set; }

    [Header("Refs")]
    public HealthBar playerHealth;   // HealthBar del player
    public Slider inventorySlider;   // Slider nell'inventario
    public GameObject inventoryPanel; // (opzionale) root del pannello

    [Header("Opzioni")]
    public bool enforceWhileVisible = false; // true se vuoi ribloccare il valore ogni frame a pannello aperto

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    private void OnEnable()
    {
        // Iscriviti all'evento per danni/cambi vita che partono da HealthBar
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += HandleHealthChanged;
            // Sync immediato all’apertura del pannello
            ForceSyncFrom(playerHealth);
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= HandleHealthChanged;
    }

    private void LateUpdate()
    {
        // (opzionale) Se serve “blindare” il valore mentre è visibile
        if (enforceWhileVisible && inventoryPanel != null && inventoryPanel.activeInHierarchy)
        {
            ForceSyncFrom(playerHealth);
        }
    }

    private void HandleHealthChanged(float current, float max)
    {
        if (inventorySlider == null) return;
        if (!Mathf.Approximately(inventorySlider.maxValue, max))
            inventorySlider.maxValue = max;

        // Evita feedback loop di OnValueChanged
        if (!Mathf.Approximately(inventorySlider.value, current))
            inventorySlider.SetValueWithoutNotify(current);
    }

    /// <summary>
    /// Chiamata diretta per forzare la sync (es. dopo una cura in ItemConsumer)
    /// </summary>
    public void ForceSyncFrom(HealthBar hb)
    {
        if (hb == null || inventorySlider == null) return;

        if (!Mathf.Approximately(inventorySlider.maxValue, hb.maxHealth))
            inventorySlider.maxValue = hb.maxHealth;

        if (!Mathf.Approximately(inventorySlider.value, hb.health))
            inventorySlider.SetValueWithoutNotify(hb.health);
    }
}