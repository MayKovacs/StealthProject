using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAIScript : MonoBehaviour
{
    // Variables that need to be assigned
    public GameObject player, enemyGun;
    public Transform enemyEyes;
    public NavMeshAgent agent;
    public LayerMask isGround, isPlayer;
    public GameObject wayPoint1, wayPoint2, wayPoint3;
    public AudioSource audioSource;

    // Variables that need adjusting
    public float intenseChaseTime = 15;
    public float alertModeTime = 60;
    public float waitTime = 5;
    public float walkPointRange = 5;
    public float attackRange = 5;
    public float closeAimRange = 2.5f;
    public float gunDamage = 5;
    public float gunRateOfFire = 1.5f;

    // Private variables
    [SerializeField] private Vector3 walkPoint, lastKnownPlayerPosition;
    [SerializeField] private bool walkPointSet, reachedWalkPoint, seePlayer, chasingPlayer;
    [SerializeField] private float currentWaitTime, intenseModeTimer, alertModeTimer, gunShotTimer, footStepTimer, footStepSoundSpeedup;
    [SerializeField] private int wayPointNumber, wayPointCounter;

    private void Awake()
    {
        // Assigning Variables
        player = GameObject.Find("Player");
        enemyEyes = this.transform.Find("EnemyHead/EnemyEyes");
        agent = GetComponent<NavMeshAgent>();

        // Begins AI in casual mode;
        CasualMode();

        // Determines how many waypoints there are
        wayPointNumber = 1;
        if (wayPoint1 != null)
        {
            wayPointNumber++;
            if (wayPoint2 != null)
            {
                wayPointNumber++;
                if (wayPoint3 != null)
                {
                    wayPointNumber++;
                }
            }
        }
        gunShotTimer = gunRateOfFire;
        footStepTimer = 0.8f;
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
        if (hitInfo.collider != null && hitInfo.collider.tag == "Player" && player.GetComponent<FirstPersonController>().cloaked == false)
        {
            // Debug.Log("I see you");
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
            // Debug.Log("lost you, investigating your last known position.");
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
                Footsteps();
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

        footStepSoundSpeedup = 1;
    }

    private void AlertMode()
    {
        agent.speed = 2.5f;
        agent.angularSpeed = 200;
        waitTime = 6;
        walkPointRange = 7.5f;

        intenseModeTimer = 0;
        alertModeTimer = alertModeTime;

        footStepSoundSpeedup = 1.5f;
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

        footStepSoundSpeedup = 2f;
    }

    private void Patrolling()
    {
        if (wayPointCounter == 3)
        {
            GoToWaypoint();
        }
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
            wayPointCounter++;
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
        if (currentWaitTime == waitTime)
        {
            PlayFootStep();
        }
            
        currentWaitTime -= Time.deltaTime;
        if (currentWaitTime <= 0)
        {
            currentWaitTime = waitTime;
            reachedWalkPoint = false;
            walkPointSet = false;
            PlayFootStep();
        }
    }

    private void GoToWaypoint()
    {
        int i = Random.Range(1, wayPointNumber);
        wayPointCounter = 0;


        if (i == 1)
        {
            walkPoint = wayPoint1.transform.position;
        }
        else if (i == 2)
        {
            walkPoint = wayPoint2.transform.position;
        }
        else if (i == 3)
        {
            walkPoint = wayPoint3.transform.position;
        }
        else
        {
            Debug.LogError("Couldn't find a waypoint");
        }
    }

    private void InvestigatePoint(Vector3 searchPoint)
    {
        // investigatingPoint = true;
        walkPoint = searchPoint;
        wayPointCounter = 0;
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
        // Debug.Log("Finding a new random point");
        EquipGun(false);
    }

    private void ChasePlayer()
    {
        IntenseMode();
        wayPointCounter = 0;
        chasingPlayer = true;
        agent.SetDestination(player.transform.position);
        Vector3 distanceToPlayer = transform.position - player.transform.position;
        if (distanceToPlayer.magnitude < attackRange)
        {
            AttackPlayer();
            gunShotTimer -= Time.deltaTime;
        }
        if (distanceToPlayer.magnitude < closeAimRange)
        {
            Aim();
        }
    }

    private void AttackPlayer()
    {
        EquipGun(true);
        enemyGun.transform.LookAt(player.transform);
        if (gunShotTimer <= 0)
        {
            ShootGun();
            gunShotTimer = gunRateOfFire;
        }
    }

    private void Aim()
    {
        agent.SetDestination(transform.position);
        transform.LookAt(player.transform);
    }

    private void EquipGun(bool Equip)
    {
        if (Equip)
        {
            enemyGun.SetActive(true);
        }
        else
        {
            enemyGun.SetActive(false);
        }
    }

    private void ShootGun()
    {
        enemyGun.GetComponent<ShootBullet>().Shoot();
        // Debug.Log("ShotBullet");
    }

    private void Footsteps()
    {
        footStepTimer -= Time.deltaTime * footStepSoundSpeedup;
        if (footStepTimer <= 0)
        {
            audioSource.Play();
            footStepTimer = 0.8f;
        }
    }

    private void PlayFootStep()
    {
        audioSource.Play();
        footStepTimer = 0.8f;
    }
}
