using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TutorialEnemy : MonoBehaviour
{
    // Variables that need to be assigned in Unity
    public GameObject enemyGun, gunTip;
    public Transform enemyEyes;
    public AudioSource alertAudioSource;

    // Variables assigned in script
    public GameObject player;

    // Variables that need adjusting in unity
    public float attackRange = 5;
    public float gunDamage = 5;
    public float gunRateOfFire = 1.5f;

    [SerializeField] private bool seePlayer, spottedPlayer;
    [SerializeField] private float gunShotTimer, playerVisionLevel;

    private void Awake()
    {
        // Assigning Variables
        player = GameObject.Find("Player");
        enemyEyes = this.transform.Find("EnemyHead/EnemyEyes");
        gunShotTimer = gunRateOfFire;
    }


    private void Update()
    {
        EnemyVision();

        // Main behaviour branch
        if (seePlayer)
        {
            Aim();
            AttackPlayer();
        }
    }

    private void EnemyVision()
    {
        // Draws a linecast to see if the enemy has a direct line of sight to the player
        Debug.DrawLine(enemyEyes.transform.position, player.transform.position);
        Physics.Linecast(enemyEyes.transform.position, player.transform.position, out RaycastHit hitInfo);
        if (hitInfo.collider != null && hitInfo.collider.tag == "Player" && !player.GetComponent<FirstPersonController>().cloaked && !FindObjectOfType<FirstPersonController>().dead)
        {
            playerVisionLevel += Time.deltaTime * 4;
            if (playerVisionLevel > 1)
            {
                playerVisionLevel = 1;
            }
            if (playerVisionLevel >= 1)
            {
                seePlayer = true;
                if (!spottedPlayer)
                {
                    spottedPlayer = true;
                    Debug.Log("Enemy Spotted Player");
                    alertAudioSource.Play();
                }

            }
        }
        else
        {
            seePlayer = false;
            // Suspicion decreases as long as the enemy does not have a line of sight to the player
            playerVisionLevel -= Time.deltaTime;
            if (playerVisionLevel < 0)
            {
                playerVisionLevel = 0;
                spottedPlayer = false;
            }
        }
    }

    private void AttackPlayer()
    {
        Vector3 distanceToPlayer = transform.position - player.transform.position;
        if (distanceToPlayer.magnitude < attackRange)
        {
            gunShotTimer -= Time.deltaTime;
        }
        enemyGun.transform.LookAt(player.transform);
        //Debug.DrawLine(gunTip.transform.position, player.transform.position);
        Physics.Linecast(gunTip.transform.position, player.transform.position, out RaycastHit hit);
        //Debug.Log(hit.collider.name);
        if (gunShotTimer <= 0 && hit.collider.tag == "Player")
        {
            ShootGun();
            gunShotTimer = gunRateOfFire;
        }
    }

    private void Aim()
    {
        transform.LookAt(player.transform);
    }

    private void ShootGun()
    {
        enemyGun.GetComponent<ShootBullet>().Shoot();
    }
}
