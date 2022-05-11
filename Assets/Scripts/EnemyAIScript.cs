using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAIScript : MonoBehaviour
{
    // Variables that need to be assigned
    public GameObject player;
    public Transform enemyEyes;
    public NavMeshAgent agent;
    public LayerMask isGround, isPlayer;

    // Variables that need adjusting
    public float waitTime = 5;
    public float walkPointRange = 5;
    public float attackRange = 5;

    // Private variables
    [SerializeField] private Vector3 walkPoint, lastKnownPlayerPosition;
    [SerializeField] private bool walkPointSet, reachedWalkPoint, seePlayer, chasingPlayer, investigatingPoint;
    [SerializeField] private float currentWaitTime;

    private void Awake()
    {
        // Assigning Variables
        player = GameObject.Find("Player");
        enemyEyes = this.transform.Find("EnemyHead/EnemyEyes");
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // Draws a linecast to see if the enemy has a line of sight to the player
        Debug.DrawLine(enemyEyes.transform.position, player.transform.position);
        Physics.Linecast(enemyEyes.transform.position, player.transform.position, out RaycastHit hitInfo);
        if (hitInfo.collider.tag == "Player" && player.GetComponent<FirstPersonController>().cloaked == false)
        {
            Debug.Log("I see you");
            seePlayer = true;
        }
        else
        {
            seePlayer = false;
        }


        if (chasingPlayer && !seePlayer)
        {
            chasingPlayer = false;
            lastKnownPlayerPosition = player.transform.position;
            InvestigatePoint(lastKnownPlayerPosition);
            Debug.Log("lost you, investigating your last known position.");
        }


        if (seePlayer)
        {
            ChasePlayer();
        }
        else
        {
            if (!reachedWalkPoint)
            {
                Patrolling();
            }
            else
            {
                Wait();
            }
        }

    }

    private void Patrolling()
    {
        if (!walkPointSet)
        {
            int i = 0;
            while (!walkPointSet)
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
            investigatingPoint = false;
        }
    }

    private void InvestigatePoint(Vector3 searchPoint)
    {
        // investigatingPoint = true;
        walkPoint = searchPoint;
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
        Debug.Log("Finding a new random point");
    }

    private void ChasePlayer()
    {
        chasingPlayer = true;
        agent.SetDestination(player.transform.position);
        Vector3 distanceToPlayer = transform.position - player.transform.position;
        if (distanceToPlayer.magnitude < attackRange)
        {
            AttackPlayer();
        }
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);

        transform.LookAt(player.transform);
    }
}
