using UnityEngine;
using System.Collections.Generic;

public class PatrollState : StateMachineBehaviour
{
    float timer;
    [SerializeField] float chaseRange = 3f;
    [SerializeField] float waitAtPoint = 0.2f;  // piccolo delay per stabilità
    [SerializeField] bool startFromNearest = true;

    Transform player;
    UnityEngine.AI.NavMeshAgent agent;

    // Invece dei Transform, memorizziamo posizioni "baked" in world space
    readonly List<Vector3> patrolPositions = new List<Vector3>();
    int currentIndex = 0;
    float arrivedTime = -1f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer = 0f;
        arrivedTime = -1f;
        patrolPositions.Clear();

        agent = animator.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent == null || !agent.isActiveAndEnabled)
        {
            animator.SetBool("isPatrolling", false);
            return;
        }

        var pGo = GameObject.FindGameObjectWithTag("Player");
        player = pGo ? pGo.transform : null;

        // Trova il contenitore "PatrolPoints" sotto il mostro
        var root = animator.transform;
        var patrolRoot = root.Find("PatrolPoints");
        if (patrolRoot == null)
        {
            Debug.LogWarning($"{animator.name}: nessun child 'PatrolPoints' trovato.");
            animator.SetBool("isPatrolling", false);
            return;
        }

        // Copia le posizioni dei figli in world space (bake)
        foreach (Transform t in patrolRoot)
        {
            patrolPositions.Add(t.position); // world position!
        }

        if (patrolPositions.Count == 0)
        {
            Debug.LogWarning($"{animator.name}: 'PatrolPoints' è vuoto.");
            animator.SetBool("isPatrolling", false);
            return;
        }

        // Opzionale: parti dal punto più vicino
        if (startFromNearest)
        {
            float best = float.MaxValue;
            for (int i = 0; i < patrolPositions.Count; i++)
            {
                float d = Vector3.SqrMagnitude(patrolPositions[i] - animator.transform.position);
                if (d < best) { best = d; currentIndex = i; }
            }
        }
        else
        {
            currentIndex = 0;
        }

        MoveToCurrent();
    }

    void MoveToCurrent()
    {
        // Se il punto non è su NavMesh, prova a "snapparlo" alla NavMesh
        if (UnityEngine.AI.NavMesh.SamplePosition(patrolPositions[currentIndex], out var hit, 2f, UnityEngine.AI.NavMesh.AllAreas))
            agent.SetDestination(hit.position);
        else
            agent.SetDestination(patrolPositions[currentIndex]);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (agent == null || patrolPositions.Count == 0) return;

        // Avanza in ordine, non più random
        if (!agent.pathPending)
        {
            // Considera anche una tolleranza per arresto
            if (agent.remainingDistance <= agent.stoppingDistance + 0.05f)
            {
                if (arrivedTime < 0f) arrivedTime = Time.time; // appena arrivato
                // aspetta un attimo per stabilità
                if (Time.time - arrivedTime >= waitAtPoint)
                {
                    arrivedTime = -1f;
                    currentIndex = (currentIndex + 1) % patrolPositions.Count;
                    MoveToCurrent();
                }
            }
        }

        // Timer per uscire dal pattugliamento (come prima)
        timer += Time.deltaTime;
        if (timer > 10f)
            animator.SetBool("isPatrolling", false);

        // Passaggio a chase se il player è vicino
        if (player != null)
        {
            float distance = Vector3.Distance(player.position, animator.transform.position);
            if (distance < chaseRange)
                animator.SetBool("isChasing", true);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (agent != null && agent.isActiveAndEnabled)
            agent.ResetPath();
    }
}
