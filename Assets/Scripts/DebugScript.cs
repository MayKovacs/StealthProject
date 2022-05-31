using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugScript : MonoBehaviour
{
    AudioSource audioSource;
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Do Debug Thing");
            FindObjectOfType<AudioManager>().Play("VineBoomSoundEffect");
            Time.timeScale = 20;
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log("Do Debug Thing 2");
            FindObjectOfType<AudioManager>().Play("VineBoomSoundEffect");
            Time.timeScale = 1;
        }
    }
}
