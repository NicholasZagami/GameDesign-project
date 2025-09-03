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

    private Animator animator;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource non trovato sul nemico. Aggiungilo per sentire il suono d'attacco.");
        }
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator non trovato sul nemico.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    public void ApplyAttackDamage()
    {

        Vector3 origin = transform.position + Vector3.up * 1.2f;
        float maxAttackAngle = 60f;

        Collider[] hits = Physics.OverlapSphere(origin, attackRange, playerLayer);
        foreach (Collider hit in hits)
        {
            HealthBar playerHealth = hit.GetComponent<HealthBar>();
            if (playerHealth != null && playerHealth.hasUI)
            {
                Vector3 direction = (hit.transform.position + Vector3.up * 1f) - origin;
                float distance = direction.magnitude;
                direction.Normalize();

                float angle = Vector3.Angle(transform.forward, direction);
                if (angle > maxAttackAngle)
                {
                    Debug.Log("Giocatore fuori dal cono di attacco");
                    continue;
                }

                int mask = ~LayerMask.GetMask("Enemy");

                if (Physics.Raycast(origin, direction, out RaycastHit rayHit, distance, mask))
                {
                    if (rayHit.collider.gameObject == hit.gameObject)
                    {
                        playerHealth.TakeDamage(attackDamage);

                        if (hitSound != null && audioSource != null)
                            audioSource.PlayOneShot(hitSound);

                        Debug.Log($"{gameObject.name} ha colpito {hit.name}");

                        break;
                    }
                }
            }
        }
    }
}