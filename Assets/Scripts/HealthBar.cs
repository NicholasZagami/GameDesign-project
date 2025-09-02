using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

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

    [Header("Dissolve Effect")]
    public Material dissolveMaterial;
    public float dissolveSpeed = 1f;
    public bool useDissolveEffect = true;

    private Animator animator;
    private bool isDead = false;
    private Renderer objectRenderer;
    private Material[] originalMaterials;
    private Material[] materialsWithDissolve;

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
                SetUIVisible(false); // barra visualmente nascosta ma attiva
        }

        animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // Setup per l'effetto dissolve
        if (useDissolveEffect && dissolveMaterial != null)
        {
            SetupDissolveEffect();
        }
    }

    private void SetupDissolveEffect()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer == null)
            objectRenderer = GetComponentInChildren<Renderer>();

        if (objectRenderer != null)
        {
            originalMaterials = objectRenderer.materials;
            
            // Crea un'istanza del material dissolve per questo oggetto
            Material dissolveInstance = new Material(dissolveMaterial);
            
            // Sostituisce tutti i materiali con quello di dissolve
            materialsWithDissolve = new Material[] { dissolveInstance };
            
            // Aggiorna il riferimento al material dissolve
            dissolveMaterial = dissolveInstance;
            
            // Inizializza il dissolve amount a 0 (completamente visibile)
            dissolveMaterial.SetFloat("_DissolveAmount", 0.3f);
            
            Debug.Log($"Dissolve setup completato per {gameObject.name}");
        }
    }

    void Update()
    {
        if (isDead) return;

        if (hasUI && healthSlider != null)
        {
            healthSlider.value = health;
        }
        
        // TEST: Premi T per testare dissolve manualmente
        if (Input.GetKeyDown(KeyCode.T) && dissolveMaterial != null)
        {
            float testValue = 0.8f; // Valore alto per vedere l'effetto
            dissolveMaterial.SetFloat("_DissolveAmount", testValue);
            Debug.Log($"Test dissolve applicato: {testValue}");
        }
        
        // TEST: Premi Y per resettare
        if (Input.GetKeyDown(KeyCode.Y) && dissolveMaterial != null)
        {
            dissolveMaterial.SetFloat("_DissolveAmount", 0f);
            Debug.Log("Dissolve resettato a 0");
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
            string deathParam = isBoss ? "isDied" : (hasUI ? "PlayerIsDied" : "isDied");

            if (HasParameter(animator, deathParam))
            {
                animator.SetBool(deathParam, true);
            }
        }

        // Avvia l'effetto dissolve
        if (useDissolveEffect && dissolveMaterial != null && objectRenderer != null)
        {
            StartDissolveEffect();
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
            // Se non usa dissolve, distruggi normalmente
            if (!useDissolveEffect)
            {
                Destroy(gameObject, 3f);
            }
        }

        if (hasUI && healthSlider != null)
        {
            SetUIVisible(false); // nasconde le parti interne
            healthSlider.gameObject.SetActive(false); // disattiva tutto lo slider
            Debug.Log("Slider disattivato: " + healthSlider.gameObject.name);
        }

        GetComponent<PlayerDetector>()?.OnDeath();
        GetComponent<EnemySave>()?.OnDeath();
    }

    private void StartDissolveEffect()
    {
        // Applica tutti i materiali (originali + dissolve)
        objectRenderer.materials = materialsWithDissolve;
        
        // Avvia la coroutine di dissolve
        StartCoroutine(DissolveCoroutine());
    }

    private IEnumerator DissolveCoroutine()
    {
        float dissolveAmount = 0f;
        
        Debug.Log($"Iniziando dissolve per {gameObject.name}");
        
        while (dissolveAmount < 1f)
        {
            dissolveAmount += Time.deltaTime * dissolveSpeed;
            dissolveAmount = Mathf.Clamp01(dissolveAmount);
            
            // Aggiorna il parametro dissolve nel material
            dissolveMaterial.SetFloat("_DissolveAmount", dissolveAmount);
            
            // Debug per verificare che il material riceva il valore
            if (dissolveAmount % 0.1f < 0.01f) // Log ogni 0.1
                Debug.Log($"Dissolve amount: {dissolveAmount}");
            
            yield return null;
        }
        
        // Dissolve completato, distruggi l'oggetto
        Debug.Log($"Dissolve completato per {gameObject.name}");
        Destroy(gameObject);
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

    void OnDestroy()
    {
        // Pulisci l'istanza del material per evitare memory leak
        if (dissolveMaterial != null && materialsWithDissolve != null)
        {
            for (int i = 0; i < materialsWithDissolve.Length; i++)
            {
                if (materialsWithDissolve[i] == dissolveMaterial)
                {
                    DestroyImmediate(dissolveMaterial);
                    break;
                }
            }
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