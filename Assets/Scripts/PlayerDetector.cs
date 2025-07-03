using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class PlayerDetector : MonoBehaviour
{
    public float detectionRange = 10f;     // Distanza di rilevamento
    public float attackRange = 2f;         // Distanza per attaccare
    public float attackCooldown = 1.5f;    // Tempo tra attacchi
    public Transform player;               // Assegna Player da Inspector

    private bool playerInRange = false;
    private bool isAttacking = false;

    private Animator animator;
    private NavMeshAgent agent;

    // Aggiunta per animazione fluida all'avvio
    private float animationStartupTimer = 0f;
    private bool justDetectedPlayer = false;

    public AudioSource backgroundMusic;
    public AudioSource combatMusic;
    private bool inCombat = false;

    private bool isDead = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (isDead) return;

        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= detectionRange;

        if (playerInRange)
        {
            animator.SetBool("PlayerDetected", true);
            GetComponent<HealthBar>()?.SetUIVisible(true);

            if (!wasInRange)
            {
                justDetectedPlayer = true;
                animationStartupTimer = 0.15f;

                if (!inCombat)
                {
                    inCombat = true;
                    if (backgroundMusic != null) backgroundMusic.Stop();
                    if (combatMusic != null) combatMusic.Play();
                }
            }

            if (distance > attackRange)
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }
            else
            {
                agent.isStopped = true;
                agent.ResetPath();

                if (!isAttacking)
                    StartCoroutine(AttackPlayer());
            }
        }
        else
        {
            animator.SetBool("PlayerDetected", false);
            agent.ResetPath();
            agent.isStopped = true;

            justDetectedPlayer = false;
            animationStartupTimer = 0f;

            if (inCombat)
            {
                inCombat = false;
                if (combatMusic != null) combatMusic.Stop();
                if (backgroundMusic != null) backgroundMusic.Play();
            }
        }

        // Aggiorna velocità e applica forzatura nei primi frame di movimento
        float speed = agent.velocity.magnitude;
        if (animationStartupTimer > 0f)
        {
            speed = Mathf.Max(speed, 0.2f); // forza almeno walk per partire
            animationStartupTimer -= Time.deltaTime;
        }
        else if (speed < 0.05f)
        {
            speed = 0f; // evita tremolii
        }

        animator.SetFloat("Speed", speed);
    }

    private System.Collections.IEnumerator AttackPlayer()
    {
        isAttacking = true;
        agent.ResetPath();

        animator.SetTrigger("Attack"); // Assicurati che esista il trigger nell'Animator

        // Qui puoi infliggere danno, opzionale:
        // player.GetComponent<HealthBar>()?.TakeDamage(danno);

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    public void OnDeath()
    {
        isDead = true;
        Debug.Log("Il boss è morto → fermato PlayerDetector");
    }
}