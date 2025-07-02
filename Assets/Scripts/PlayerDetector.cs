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

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        playerInRange = distance <= detectionRange;

        if (playerInRange)
        {
            animator.SetBool("PlayerDetected", true);
            GetComponent<HealthBar>()?.SetUIVisible(true); // mostra barra

            if (distance > attackRange)
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }
            else
            {
                agent.isStopped = true;

                if (!isAttacking)
                    StartCoroutine(AttackPlayer());
            }
        }
        else
        {
            animator.SetBool("PlayerDetected", false);
            agent.ResetPath();
            agent.isStopped = true;
        }
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
}