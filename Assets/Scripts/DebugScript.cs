using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugScript : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Do Debug Thing");
            FindObjectOfType<AudioManager>().Play("VineBoomSoundEffect");
        }
    }
}
