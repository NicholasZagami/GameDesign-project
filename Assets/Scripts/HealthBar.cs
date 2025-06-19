using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class HealthBar : MonoBehaviour
{
    public Slider healthSlider;
    public float maxHealth = 100;
    public float health;
    private float lerpSpeed = 0.05f;

    void Start()
    {
        health = maxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = health;
    }

    void Update()
    {
        // Interpolazione smooth (facoltativa)
        healthSlider.value = Mathf.Lerp(healthSlider.value, health, lerpSpeed);

        // Nuovo sistema di input
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            takeDamage(10);
        }
    }

    void takeDamage(float damage)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);
    }
}
