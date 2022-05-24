using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootBullet : MonoBehaviour
{
    // Need Assigning
    public GameObject gunTip, enemyShotListener, enemyGunListener;
    public FirstPersonController player;
    public EnemyAIScript enemy;
    public AudioSource gunshotSound;

    // Adjusting
    public float gunSpreadRange, shotDistance;

    // Private
    private float timer;

    private void Awake()
    {
        player = FindObjectOfType<FirstPersonController>();
        enemy = GetComponentInParent<EnemyAIScript>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && enemy == null)
        {
            AudioSource[] sounds;
            sounds = this.transform.Find("GunTip").GetComponents<AudioSource>();
            AudioSource x = sounds[0];
            x.Play();
        }

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            enemyShotListener.SetActive(false);
            //enemyGunListener.SetActive(false);
        }
    }

    public void ReadySound()
    {
        timer = 0.05f;
        enemyGunListener.SetActive(true);
    }

    public void Shoot()
    {
        timer = 0.05f;
        enemyShotListener.SetActive(true);

        gunshotSound.pitch = 1 + Random.Range(-0.05f, 0.05f);
        gunshotSound.Play();

        RaycastHit hit;
        Vector3 gunAccuracy = gunTip.transform.forward;

        float xRange = Random.Range(-gunSpreadRange, gunSpreadRange);
        float yRange = Random.Range(-gunSpreadRange, gunSpreadRange);
        gunAccuracy = new Vector3(gunAccuracy.x + xRange, gunAccuracy.y +yRange, gunAccuracy.z);

        Physics.Raycast(gunTip.transform.position, gunAccuracy, out hit, shotDistance);
        if (hit.collider != null && hit.collider.gameObject.tag == "Player")
        {
            PlayerTakeDamage();
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
        player.Hurt();
    }
}
