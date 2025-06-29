using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class HealthBar : MonoBehaviour
{
    [Header("UI")]
    public Slider healthSlider; // Assegnato solo per il player
    public bool hasUI = true;

    [Header("Stats")]
    public float maxHealth = 100;
    public float health;
    private float lerpSpeed = 0.05f;

    private Animator animator; // AGGIUNGI QUESTA
    private bool isDead = false;


    void Start()
    {
        health = maxHealth;

        if (hasUI && healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = health;
        }

        // Prova a trovare l'Animator (può essere nel figlio)
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    void Update()
    {
        if (hasUI && healthSlider != null)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, health, lerpSpeed);
        }

        // Debug input solo per test player
#if UNITY_EDITOR
        if (hasUI && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TakeDamage(10);
        }
#endif
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (!hasUI)
        {
            Animator animator = GetComponent<Animator>();
            if (animator != null && HasParameter(animator, "IsDied"))
            {
                animator.SetBool("IsDied", true);
            }
        }
        else
        {
            Debug.Log("Il giocatore è morto");
            // Aggiungi qui logica di game over
        }

        Destroy(gameObject, 3f);
    }

    private bool HasParameter(Animator animator, string paramName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }


}
