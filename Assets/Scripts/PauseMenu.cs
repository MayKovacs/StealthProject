using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit");
    }

    public void ContinueGame()
    {
        FirstPersonController charCont = FindObjectOfType<FirstPersonController>();
        charCont.paused = false;
        charCont.Pause(false);
        Debug.Log("Game Play");
    }
}
