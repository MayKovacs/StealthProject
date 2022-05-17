using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyListener : MonoBehaviour
{
    public int priorityLevel;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {
            if (priorityLevel == 1)
            {
                other.gameObject.GetComponent<EnemyAIScript>().InvestigatePointPriorityOne(this.transform.position);
            }
            else if (priorityLevel == 2)
            {
                other.gameObject.GetComponent<EnemyAIScript>().InvestigatePointPriorityTwo(this.transform.position);
            }
            else if (priorityLevel == 3)
            {
                other.gameObject.GetComponent<EnemyAIScript>().InvestigatePointPriorityThree(this.transform.position);
            }
            else if (priorityLevel == 4)
            {
                other.gameObject.GetComponent<EnemyAIScript>().InvestigatePointPriorityFour(this.transform.position);
            }
            else
            {
                Debug.LogError("Enemy listener " + this.gameObject.name + " has invalid priority level");
            }
        }
    }
}
