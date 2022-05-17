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
    public float attentionSpan = 150;
    public float waitTime = 5;
    public float walkPointRange = 5;
    public float attackRange = 5;
    public float closeAimRange = 2.5f;
    public float gunDamage = 5;
    public float gunRateOfFire = 1.5f;

    // Private variables
    [SerializeField] private Vector3 walkPoint, lastKnownPlayerPosition;
    [SerializeField] private bool walkPointSet, reachedWalkPoint, seePlayer, chasingPlayer;
    [SerializeField] private float currentWaitTime, gunShotTimer, footStepTimer, footStepSoundSpeedup, susLevel;
    [SerializeField] private int wayPointNumber, wayPointCounter, investigatePriority;

    private void Awake()
    {
        // Assigning Variables
        player = GameObject.Find("Player");
        enemyEyes = this.transform.Find("EnemyHead/EnemyEyes");
        agent = GetComponent<NavMeshAgent>();
        gunShotTimer = gunRateOfFire;
        footStepTimer = 0.8f;
        investigatePriority = 10;

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
    }


    private void Update()
    {
        EnemyVision();
        SusLevel();

        // Main behaviour branch
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

    private void EnemyVision()
    {
        // Draws a linecast to see if the enemy has a direct line of sight to the player
        Debug.DrawLine(enemyEyes.transform.position, player.transform.position);
        Physics.Linecast(enemyEyes.transform.position, player.transform.position, out RaycastHit hitInfo);
        if (hitInfo.collider != null && hitInfo.collider.tag == "Player" && player.GetComponent<FirstPersonController>().cloaked == false)
        {
            seePlayer = true;
            susLevel = attentionSpan;
        }
        else
        {
            seePlayer = false;
            // Suspicion decreases as long as the enemy does not have a line of sight to the player
            susLevel -= Time.deltaTime;
        }
        // Triggers if enemy loses sight of the player during chase
        if (chasingPlayer && !seePlayer)
        {
            chasingPlayer = false;
            lastKnownPlayerPosition = player.transform.position;
            InvestigatePointPriorityOne(lastKnownPlayerPosition);
        }
    }
    private void SusLevel()
    {
        // Keeps susLevel between valid numbers
        if (susLevel < 0)
        {
            susLevel = 0;
        }
        else if (susLevel > attentionSpan)
        {
            susLevel = attentionSpan;
        }

        // Changes the movement speed of the enemy based on suspicion
        if (susLevel < 30)
        {
            CasualMode();
        }
        else if (susLevel >= attentionSpan - attentionSpan / 8)
        {
            IntenseMode();
        }
        else
        {
            AlertMode();
        }
    }

    private void CasualMode()
    {
        agent.speed = 2;
        agent.angularSpeed = 140;
        waitTime = 8;
        walkPointRange = 10;
        footStepSoundSpeedup = 1;
    }

    private void AlertMode()
    {
        agent.speed = 3f;
        agent.angularSpeed = 200;
        waitTime = 5;
        walkPointRange = 7.5f;
        footStepSoundSpeedup = 1.5f;
    }

    private void IntenseMode()
    {
        agent.speed = 3.5f;
        agent.angularSpeed = 240;
        waitTime = 2;
        walkPointRange = 6;
        footStepSoundSpeedup = 2f;
        EquipGun(true);
    }

    private void Patrolling()
    {
        if (wayPointCounter == 3)
        {
            GoToWaypoint();
        }
        if (!walkPointSet)
        {
            investigatePriority++;
            int i = 0;
            while (!walkPointSet)
            {
                i++;
                SearchWalkPoint();
                if (i == 20)
                {
                    Debug.LogError("Couldn't find a waypoint for " + this.name + " after " + i + " attempts");
                    GoToWaypoint();
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

        if (distanceToWalkPoint.magnitude < 1.5f)
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
            PlayFootStep();
        }
    }

    private void GoToWaypoint()
    {
        int i = Random.Range(1, wayPointNumber);
        wayPointCounter = 0;
        investigatePriority++;
        walkPointSet = true;

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
            Debug.LogError("Couldn't find a waypoint. Enemy needs at least one waypoint");
        }
    }

    public void InvestigatePointPriorityOne(Vector3 searchPoint)
    {
        susLevel += attentionSpan / 2;
        investigatePriority = 1;
        walkPointSet = true;
        reachedWalkPoint = false;
        walkPoint = searchPoint;
        wayPointCounter = 0;
        Debug.Log("Investigating Prio 1 Sound");
    }

    public void InvestigatePointPriorityTwo(Vector3 searchPoint)
    {
        susLevel += attentionSpan / 5;
        walkPoint = searchPoint;
        walkPointSet = true;
        wayPointCounter = 0;
        if (investigatePriority == 1)
        {
            Debug.Log("Ignoring prio 2 sound");
            return;
        }
        investigatePriority = 2;
        reachedWalkPoint = false;
        Debug.Log("Investigating Prio 2 Sound");
    }

    public void InvestigatePointPriorityThree(Vector3 searchPoint)
    {
        susLevel += attentionSpan / 12;
        walkPoint = searchPoint;
        walkPointSet = true;
        wayPointCounter = 0;
        if (investigatePriority <= 2)
        {
            Debug.Log("Ignoring prio 3 sound");
            return;
        }
        investigatePriority = 3;
        reachedWalkPoint = false;

        Debug.Log("Investigating Prio 3 Sound");
    }

    public void InvestigatePointPriorityFour(Vector3 searchPoint)
    {
        susLevel += attentionSpan / 20;
        walkPoint = searchPoint;
        walkPointSet = true;
        wayPointCounter = 0;
        if (investigatePriority <= 3)
        {
            Debug.Log("Ignoring prio 4 sound");
            return;
        }
        investigatePriority = 4;
        reachedWalkPoint = false;
        Debug.Log("Investigating Prio 4 Sound");
    }
    private void SearchWalkPoint()
    {
        EquipGun(false);
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y + 5, transform.position.z + randomZ);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        int i = 0;
        while (i <= 10)
        {
            i++;
            if (Physics.Raycast(walkPoint, -transform.up, 1f, isGround) && distanceToWalkPoint.magnitude > walkPointRange * 0.75f)
            {
                walkPointSet = true;
                break;
            }
            else
            {
                walkPoint = new Vector3(walkPoint.x, walkPoint.y - 1, walkPoint.z);
            }
        }
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
        else
        {
            Footsteps();
        }
    }

    private void AttackPlayer()
    {
        EquipGun(true);
        enemyGun.transform.LookAt(player.transform);
        RaycastHit hit;
        Physics.Linecast(enemyGun.transform.position, player.transform.position, out hit);
        if (gunShotTimer <= 0 && hit.collider.tag == "Player")
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
    }

    private void Footsteps()
    {
        footStepTimer -= Time.deltaTime * footStepSoundSpeedup;
        if (footStepTimer <= 0)
        {
            PlayFootStep();
        }
    }

    private void PlayFootStep()
    {
        audioSource.Play();
        footStepTimer = 0.8f;
    }
}
