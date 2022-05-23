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
            audioSource.Play();
            //FindObjectOfType<AudioManager>().Play("VineBoomSoundEffect");
        }
    }
}
