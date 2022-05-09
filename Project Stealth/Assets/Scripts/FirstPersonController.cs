using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    // Variables that need assigning
    public CharacterController playerCharacterController;
    public FeetCollider playerFeet;
    public DebugMenu debugMenu;
    public Camera playerCamera;

    // Variables that need adjusting
    public float movementSpeed = 10;
    public float mouseSensitivity = 2;
    public float jumpHeight = 2;
    public float fallSpeed = 5;

    // Private Variables
    private float xSpeed, ySpeed, zSpeed, mouseX, mouseY, stepOffset;
    private bool isGrounded;

    // Start is called before the first frame update
    void Start()
    {
        // Deactivates the cursor and locks it to the middle of the screen
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Sets the step off set to what it is set in Unity
        stepOffset = playerCharacterController.stepOffset;
    }

    // Update is called once per frame
    void Update()
    {
        FirstPersonCamera();
        HorizontalMovement();
        VerticalMovement();
        Movement();
        DebugMenu();
    }
    void Movement()
    {
        // Sets move as a Vector 3 to gather all speeds into one variable
        Vector3 move = transform.right * xSpeed + transform.forward * zSpeed + transform.up * ySpeed;

        // Connects to the character controller and makes the player move
        playerCharacterController.Move(move * Time.deltaTime * movementSpeed);
    }

    void FirstPersonCamera()
    {
        // Gets mouse movements
        mouseX += Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseY -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Clamps and restricts mouse movements
        mouseY = Mathf.Clamp(mouseY, -80, 80);

        // Applies mouse movements to the player and camera
        playerCamera.transform.rotation = Quaternion.Euler(mouseY, mouseX, 0);
        playerCharacterController.transform.rotation = Quaternion.Euler(0, mouseX, 0);
    }

    void HorizontalMovement()
    {
        // Gets the movement of wsad
        xSpeed = Input.GetAxis("Horizontal");
        zSpeed = Input.GetAxis("Vertical");

        // Checks if both horizontal and vertical movement is active
        if (Input.GetAxis("Horizontal") != 0 && Input.GetAxis("Vertical") != 0)
        {
            // Speeds are divided to prevent diagonal movement from being faster
            xSpeed *= 0.75f;
            zSpeed *= 0.75f;
        }
    }

    void VerticalMovement()
    {
        isGrounded = playerFeet.isGrounded;

        // Checks if the player can jump
        if (Input.GetAxis("Jump") != 0 && isGrounded == true && ySpeed == 0)
        {
            // Starts the jump and prevents stepping
            ySpeed = jumpHeight;
            playerCharacterController.stepOffset = 0;
        }

        if (isGrounded == false)
        {
            // Makes the player fall and prevents stepping
            ySpeed -= fallSpeed * Time.deltaTime;
            playerCharacterController.stepOffset = 0;
        }

        // Checks if player lands from a jump
        if (isGrounded == true && ySpeed < 0)
        {
            // Removes falling speed and enables stepping
            ySpeed = 0;
            playerCharacterController.stepOffset = stepOffset;
        }

    }

    private void DebugMenu()
    {
        // Sends values to the debug menu script
        debugMenu.xSpeed = xSpeed;
        debugMenu.ySpeed = ySpeed;
        debugMenu.zSpeed = zSpeed;
        debugMenu.isGrounded = isGrounded;
    }
}
