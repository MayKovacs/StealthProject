using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootBullet : MonoBehaviour
{
    // Need Assigning
    public GameObject gunTip;
    public FirstPersonController player;
    public EnemyAIScript enemy;

    // Adjusting
    public float gunSpreadRange, shotDistance;

    // Private


    private void Awake()
    {
        player = FindObjectOfType<FirstPersonController>();
        enemy = GetComponentInParent<EnemyAIScript>();
    }

    public void Shoot()
    {
        RaycastHit hit;
        Vector3 gunAccuracy = gunTip.transform.forward;

        float xRange = Random.Range(-gunSpreadRange, gunSpreadRange);
        float yRange = Random.Range(-gunSpreadRange, gunSpreadRange);
        gunAccuracy = new Vector3(gunAccuracy.x + xRange, gunAccuracy.y +yRange, gunAccuracy.z);

        Physics.Raycast(gunTip.transform.position, gunAccuracy, out hit, shotDistance);
        if (hit.collider != null && hit.collider.gameObject.tag == "Player")
        {
            PlayerTakeDamage();
            // Debug.Log("PlayerHurt");
        }
    }

    private void PlayerTakeDamage()
    {
        if (enemy != null)
        {
            player.health -= enemy.gunDamage;
        }
        else
        {
            player.health -= 5;
            Debug.LogError("The player took damage from a gun, but no enemy was attached to this gun");
        }
    }
}
