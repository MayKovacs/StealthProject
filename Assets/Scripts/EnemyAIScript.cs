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
    public float intenseChaseTime = 15;
    public float alertModeTime = 60;
    public float waitTime = 5;
    public float walkPointRange = 5;
    public float attackRange = 5;

    // Private variables
    [SerializeField] private Vector3 walkPoint, lastKnownPlayerPosition;
    [SerializeField] private bool walkPointSet, reachedWalkPoint, seePlayer, chasingPlayer;
    [SerializeField] private float currentWaitTime, intenseModeTimer, alertModeTimer;

    private void Awake()
    {
        // Assigning Variables
        player = GameObject.Find("Player");
        enemyEyes = this.transform.Find("EnemyHead/EnemyEyes");
        agent = GetComponent<NavMeshAgent>();

        // Begins AI in casual mode;
        CasualMode();
    }

    private void Update()
    {
        if (intenseModeTimer > 0)
        {
            intenseModeTimer -= Time.deltaTime;
        }
        else if (alertModeTimer > 0)
        {
            alertModeTimer -= Time.deltaTime;
        }

        if (intenseModeTimer < 0)
        {
            AlertMode();
        }
        if (alertModeTimer < 0 && currentWaitTime <= 1)
        {
            CasualMode();
        }

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

    private void CasualMode()
    {
        agent.speed = 2;
        agent.angularSpeed = 140;
        waitTime = 8;
        walkPointRange = 5;

        alertModeTimer = 0;
        intenseModeTimer = 0;
    }

    private void AlertMode()
    {
        agent.speed = 2.5f;
        agent.angularSpeed = 200;
        waitTime = 6;
        walkPointRange = 7.5f;

        intenseModeTimer = 0;
        alertModeTimer = alertModeTime;
    }

    private void IntenseMode()
    {
        agent.speed = 3;
        agent.angularSpeed = 220;
        waitTime = 4;
        currentWaitTime = waitTime;
        walkPointRange = 10;

        alertModeTimer = alertModeTime;
        intenseModeTimer = intenseChaseTime;
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

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if (Physics.Raycast(walkPoint, -transform.up, 2f, isGround) && distanceToWalkPoint.magnitude > walkPointRange / 2)
        {
            walkPointSet = true;
        }
        Debug.Log("Finding a new random point");
    }

    private void ChasePlayer()
    {
        IntenseMode();
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
