using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugMenu : MonoBehaviour
{
    // Variables that need assigning
    public GameObject debugMenu;
    public Text xSpeedText, ySpeedText, zSpeedText, isGroundedText, staminaText, fpsText;

    // Variables that need to be accessed
    public float xSpeed, ySpeed, zSpeed, stamina, fps;
    public bool isGrounded;

    // Private Variables
    private bool menuActive;

    // Menu off by default
    private void Start()
    {
        menuActive = false;
    }

    void Update()
    {
        fps = 1 / Time.unscaledDeltaTime;
        // Checks if the letters O and P are pressed
        if (Input.GetKey("o") == true && Input.GetKeyDown("p") == true ||
            Input.GetKeyDown("o") == true && Input.GetKey("p") == true)
        {
            menuActive = !menuActive;
        }

        // Turns on the debug menu
        if (menuActive == true)
        {
            debugMenu.SetActive(true);
        }
        // Turns it off
        else
        {
            debugMenu.SetActive(false);
        }

        // Updates debug values if on. Values are sent through the controller script
        if (menuActive == true)
        {
            xSpeedText.text = "xSpeed: " + xSpeed.ToString("F2");
            ySpeedText.text = "xSpeed: " + ySpeed.ToString("F2");
            zSpeedText.text = "xSpeed: " + zSpeed.ToString("F2");
            isGroundedText.text = "isGrounded: " + isGrounded;
            staminaText.text = "Stamina: " + stamina.ToString("F1");
            fpsText.text = "Fps: " + fps.ToString("F0");
        }
    }
}
