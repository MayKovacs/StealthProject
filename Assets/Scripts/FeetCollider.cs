using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeetCollider : MonoBehaviour
{
    // Variables that need assigning
    public bool isGrounded;

    // This script will detect if the player is grounded or not by using a mesh collider cylinder below the player.
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag != "Player" && other.tag != "Trigger")
        {
            isGrounded = true;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.tag != "Player" && other.tag != "Trigger")
        {
            isGrounded = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag != "Player" && other.tag != "Trigger")
        {
            isGrounded = false;
        }
    }
}
