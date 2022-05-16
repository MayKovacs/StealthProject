using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyListener : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {
            other.gameObject.GetComponent<EnemyAIScript>().InvestigatePoint(this.transform.position);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Enemy")
        {
            other.gameObject.GetComponent<EnemyAIScript>().InvestigatePoint(this.transform.position);
        }
    }
}
