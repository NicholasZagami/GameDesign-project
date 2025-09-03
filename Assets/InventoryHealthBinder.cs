using UnityEngine;
using UnityEngine.UI;

public class InventoryHealthBinder : MonoBehaviour
{
    public HealthBar playerHealth;   // riferimento alla HealthBar del player
    public Slider inventorySlider;   // lo slider nell'inventario

    void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += HandleHealthChanged;
            // Sync iniziale (nel caso l’evento sia già partito prima)
            HandleHealthChanged(playerHealth.health, playerHealth.maxHealth);
        }
    }

    void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(float current, float max)
    {
        if (inventorySlider == null) return;
        inventorySlider.maxValue = max;
        inventorySlider.value = current;
    }
}