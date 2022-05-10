using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BaiscNav : MonoBehaviour
{
    public NavMeshAgent agent;
    public LayerMask isGround, isPlayer;
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        walkPointSet = false;
    }

    private void Update()
    {
        Patrolling();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            float randomZ = Random.Range(-walkPointRange, walkPointRange);
            float randomX = Random.Range(-walkPointRange, walkPointRange);

            walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

            agent.SetDestination(walkPoint);
        }
    }

    private void Patrolling()
    {

    }

    private void SearchWalkPoint()
    {

    }
}
