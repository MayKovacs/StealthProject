using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerLoadScene : MonoBehaviour
{
    public SceneManagerScript sceneScript;
    public string sceneName;

    private void Start()
    {
        sceneScript = FindObjectOfType<SceneManagerScript>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            sceneScript.LoadScene(sceneName);
        }
    }
}
