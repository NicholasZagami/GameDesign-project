using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;

public class PatrollState : StateMachineBehaviour
{
    float timer;
    float chaseRange = 3;

    Transform player;
    List<Transform> patrolPoints = new List<Transform>();
    UnityEngine.AI.NavMeshAgent agent;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       timer = 0;
       player = GameObject.FindGameObjectWithTag("Player").transform;
       GameObject pp = GameObject.FindGameObjectWithTag("PatrolPoints");

       agent = animator.GetComponent<UnityEngine.AI.NavMeshAgent>();
       //agent.speed = 1.5f;
       foreach(Transform p in pp.transform)
            patrolPoints.Add(p);
    
       agent.SetDestination(patrolPoints[Random.Range(0, patrolPoints.Count)].position);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(agent.remainingDistance <= agent.stoppingDistance)
            agent.SetDestination(patrolPoints[Random.Range(0, patrolPoints.Count)].position);
        
       timer += Time.deltaTime;
       if(timer > 10)
            animator.SetBool("isPatrolling", false);
       
       float distance = Vector3.Distance(player.position, animator.transform.position);
       if(distance < chaseRange)
            animator.SetBool("isChasing", true);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       agent.SetDestination(agent.transform.position);
    }

    override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       // Implement code that processes and affects root motion
    }

    override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       // Implement code that sets up animation IK (inverse kinematics)
    }
}
