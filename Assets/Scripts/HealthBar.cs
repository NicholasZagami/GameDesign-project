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

    [Header("Boss Music (opzionale)")]
    public AudioSource combatMusic;
    public AudioSource backgroundMusic;

    [Header("Muri da aprire alla morte del boss")]
    public LootOpening[] lootEntrances;

    void Start()
    {
        health = maxHealth;

        if (hasUI && healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = health;

            if (isBoss)
                SetUIVisible(false); // barra visivamente nascosta ma attiva
        }

        animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (isDead) return;

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

        // Musica: stop combat, resume background
        if (isBoss)
        {
            if (combatMusic != null) combatMusic.Stop();
            if (backgroundMusic != null && !backgroundMusic.isPlaying)
                backgroundMusic.Play();

            // Apertura muri del loot: solo se boss
            if (lootEntrances != null && lootEntrances.Length > 0)
            {
                foreach (LootOpening loot in lootEntrances)
                {
                    if (loot != null)
                    {
                        loot.OpenWall();
                        Debug.Log("Aperto muro: " + loot.gameObject.name);
                    }
                }
            }
            else
            {
                Debug.LogWarning("Nessun muro da aprire assegnato nel boss.");
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

        if (hasUI && healthSlider != null)
        {
            SetUIVisible(false); // nasconde le parti interne
            healthSlider.gameObject.SetActive(false); // disattiva tutto lo slider
            Debug.Log("Slider disattivato: " + healthSlider.gameObject.name);
        }

        GetComponent<PlayerDetector>()?.OnDeath();
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
        if (isDead) return;

        if (healthSlider != null)
        {
            // Non disattivare tutto il gameObject
            Transform background = healthSlider.transform.Find("Background");
            Transform fill = healthSlider.transform.Find("Fill");

            if (background != null)
                background.gameObject.SetActive(visible);

            if (fill != null)
                fill.gameObject.SetActive(visible);

            // Opzionale: handle
            Transform handle = healthSlider.transform.Find("Handle Slide Area/Handle");
            if (handle != null)
                handle.gameObject.SetActive(visible);
        }
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
