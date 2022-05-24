using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    public static GameObject instance;
    [SerializeField] private string currentSceneName;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this.gameObject;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        currentSceneName = sceneName;
    }

    public void ReloadScene()
    {
        if (currentSceneName != "")
        {
            SceneManager.LoadScene(currentSceneName);
        }
        else
        {
            Debug.Log("Current scene value missing, loading sandbox level");
            LoadScene("TestMap2");
        }
    }
}
