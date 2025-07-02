using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class HealthBar : MonoBehaviour
{
    [Header("UI")]
    public Slider healthSlider; // Solo per il player
    public bool hasUI = true;

    [Header("Stats")]
    public float maxHealth = 100;
    public float health;

    [Header("Boss Settings")]
    public bool isBoss = false;

    private Animator animator;
    private bool isDead = false;

    public GameObject gameOverPanel;

    void Start()
    {
        health = maxHealth;

        if (hasUI && healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = health;
        }

        animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (hasUI && healthSlider != null)
        {
            healthSlider.value = health;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);

        if (animator != null && health > 0)
        {
            string hitTrigger = isBoss ? "Hit" : (hasUI ? "PlayerHit" : "Hit");

            if (HasParameter(animator, hitTrigger))
            {
                animator.SetTrigger(hitTrigger);
            }

            if (!hasUI && HasParameter(animator, "SpeedMagnitude"))
            {
                animator.SetFloat("SpeedMagnitude", 0.01f);
            }
        }

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        if (animator != null)
        {
            string deathParam = isBoss ? "IsDied" : (hasUI ? "PlayerIsDied" : "IsDied");

            if (HasParameter(animator, deathParam))
            {
                animator.SetBool(deathParam, true);
            }
        }

        if (hasUI && !isBoss)
        {
            Debug.Log("Il giocatore è morto");
            StartCoroutine(ShowGameOverPanelAfterDelay(2f));
        }
        else
        {
            Destroy(gameObject, 3f);
        }
    }

    private System.Collections.IEnumerator ShowGameOverPanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Disattiva i controlli del giocatore dopo la morte
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }
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

    public void SetUIVisible(bool visible)
    {
        if (healthSlider != null)
            healthSlider.gameObject.SetActive(visible);
    }

    void OnEnable()
    {
        if (hasUI && healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = health;
        }
    }
}
