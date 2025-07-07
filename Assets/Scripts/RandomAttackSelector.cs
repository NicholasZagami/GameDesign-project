using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RandomAttackSelector : MonoBehaviour
{
    public int numberOfAttackAnimations = 3; // quante animazioni di attacco hai
    private Animator animator;

    private bool attackHandled = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Se il trigger "Attack" è stato impostato dal Behavior Tree
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            attackHandled = false; // reset per il prossimo ciclo
            return;
        }

        if (!attackHandled && animator.GetAnimatorTransitionInfo(0).IsUserName("Attack"))
        {
            // Scegli casualmente l'AttackIndex
            int randomIndex = Random.Range(0, numberOfAttackAnimations);
            animator.SetInteger("AttackIndex", randomIndex);

            // Reinvia il trigger per forzare il corretto attacco
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Attack");

            attackHandled = true;
        }
    }
}