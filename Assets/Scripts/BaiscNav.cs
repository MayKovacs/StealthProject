using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BaiscNav : MonoBehaviour
{
    // Variables that need to be assigned
    public Transform player, enemyEyes;
    public NavMeshAgent agent;
    public LayerMask isGround, isPlayer;

    // Variables that need adjusting
    public float waitTime = 5;
    public float walkPointRange = 5;
    public float attackRange = 5;

    // Private variables
    private Vector3 walkPoint;
    private bool walkPointSet, reachedWalkPoint, seePlayer;
    private float currentWaitTime;

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        enemyEyes = this.transform.Find("EnemyHead/EnemyEyes");
        agent = GetComponent<NavMeshAgent>();
        walkPointSet = false;
    }

    private void Update()
    {
        Debug.DrawLine(enemyEyes.transform.position, player.transform.position);
        Physics.Linecast(enemyEyes.transform.position, player.transform.position, out RaycastHit hitInfo);
        if (hitInfo.collider.tag == "Player")
        {
            Debug.Log("I see you");
            seePlayer = true;
        }
        else
        {
            seePlayer = false;
        }

        if (seePlayer)
        {
            ChasePlayer();
        }
        if (!reachedWalkPoint)
        {
            Patrolling();
        }
        else
        {
            Wait();
        }
    }

    private void Patrolling()
    {
        if (!walkPointSet)
        {
            int i = 0;
            while(!walkPointSet)
            {
                i++;
                SearchWalkPoint();
                if (i == 20)
                {
                    Debug.LogError("Couldn't find a waypoint for " + this.name + " after " + i + " attempts");
                    break;
                }
            }
        }
        if (walkPointSet)
        {
            agent.SetDestination(walkPoint);
        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if (distanceToWalkPoint.magnitude < 1)
        {
            reachedWalkPoint = true;
        }
    }

    private void Wait()
    {
        currentWaitTime -= Time.deltaTime;
        if (currentWaitTime <= 0)
        {
            currentWaitTime = waitTime;
            reachedWalkPoint = false;
            walkPointSet = false;
        }
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, 1, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, isGround))
        {
            walkPointSet = true;
        }
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
        Vector3 distanceToPlayer = transform.position - player.transform.position;
        if (distanceToPlayer.magnitude < attackRange)
        {
            AttackPlayer();
        }
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);

        transform.LookAt(player);
    }
}
