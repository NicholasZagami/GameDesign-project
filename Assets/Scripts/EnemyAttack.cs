using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float attackDamage = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public LayerMask playerLayer;

    [Header("Audio")]
    public AudioClip hitSound;             // Il suono da riprodurre quando colpisce
    private AudioSource audioSource;       // L'audio source da cui farlo partire

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource non trovato sul nemico. Aggiungilo per sentire il suono d'attacco.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    public void ApplyAttackDamage() // Deve combaciare con Animation Event
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, playerLayer);
        foreach (Collider hit in hits)
        {
            HealthBar playerHealth = hit.GetComponent<HealthBar>();
            if (playerHealth != null && playerHealth.hasUI)
            {
                playerHealth.TakeDamage(attackDamage);

                // 🔊 Suono d'attacco quando colpisce il player
                if (hitSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(hitSound);
                }

                break; // Colpisce solo una volta
            }
        }
    }
}