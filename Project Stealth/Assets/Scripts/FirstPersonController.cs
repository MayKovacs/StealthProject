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
    public float walkingSpeed = 3;
    public float mouseSensitivity = 2;
    public float jumpHeight = 1.5f;
    public float fallSpeed = 6;

    // Private Variables
    private float xSpeed, ySpeed, zSpeed, mouseX, mouseY, stepOffset, sprintTimer, movementMultiplier;
    private bool isGrounded, running, pressedShift, bool1, bool2, crouched, pressedCtrl, moving;
    private Animator crouchAnimator;

    // Start is called before the first frame update
    void Start()
    {
        // Deactivates the cursor and locks it to the middle of the screen
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Sets the step off set to what it is set in Unity
        stepOffset = playerCharacterController.stepOffset;

        // Finds the debug menu in the scene
        debugMenu = FindObjectOfType<DebugMenu>();

        // Sets the movement to walk
        movementMultiplier = 1;

        crouchAnimator = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        MovementType();
        FirstPersonCamera();
        HorizontalMovement();
        VerticalMovement();
        Movement();
        DebugMenu();
    }
    void Movement()
    {
        // Sets move as a Vector 3 to gather all speeds into one variable
        Vector3 move = transform.right * xSpeed * movementMultiplier + transform.forward * zSpeed * movementMultiplier + transform.up * ySpeed;

        // Connects to the character controller and makes the player move
        playerCharacterController.Move(move * Time.deltaTime * walkingSpeed);
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
        // Makes backwards movement slower
        if (Input.GetAxis("Vertical") < 0)
        {
            zSpeed *= 0.8f;
        }

        // Checks if the player is moving
        if (xSpeed != 0 || zSpeed != 0)
        {
            moving = true;
        }
        else
        {
            moving = false;
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

    private void MovementType()
    {
        ButtonPressedCheck();

        // Checks toggle for player to crouch
        if (pressedCtrl)
        {
            if (!crouched)
            {
                crouched = true;
                crouchAnimator.SetTrigger("crouched");
                running = false;
                // CrouchWalk Speed
                movementMultiplier = 0.6f; 
            }
            else
            {
                crouched = false;
                crouchAnimator.ResetTrigger("crouched");
                running = false;
                // Walking Speed
                movementMultiplier = 1;
            }
        }

        // Checks for player to run or sprint, while not crouched
        if (Input.GetAxis("Fire3") != 0 && !crouched)
        {
            if (pressedShift)
            {
                if (!running)
                {
                    running = true;
                    // Sets timer for holding sprint
                    sprintTimer = 0.2f;
                    // Running Speed
                    movementMultiplier = 1.5f;
                }
                else
                {
                    running = false;
                    // Walking Speed
                    movementMultiplier = 1;
                }
            }
            else if (running)
            {
                sprintTimer -= Time.deltaTime;
                if (sprintTimer <= 0)
                {
                    sprintTimer = 0;
                    // Sprinting Speed
                    movementMultiplier = 2;
                }
            }
        }
        else if (sprintTimer <= 0)
        {
            running = false;
            // Walking Speed
            movementMultiplier = 1;
        }

        // Checks toggle for player to crouch run
        if (pressedShift && crouched)
        {
            if (!running)
            {
                running = true;
                // CrouchRun Speed
                movementMultiplier = 1.25f;
            }
            else
            {
                running = false;
                // CrouchWalk Speed
                movementMultiplier = 0.6f;
            }
        }

        if (!moving && !crouched)
        {
            running = false;
            // Walking Speed
            movementMultiplier = 1;
        }
    }

    private void ButtonPressedCheck()
    {
        // Checks if the player has pressed run
        if (Input.GetAxis("Fire3") != 0)
        {
            if (!bool1)
            {
                pressedShift = true;
                bool1 = true;
            }
            else
            {
                pressedShift = false;
            }
        }
        else
        {
            bool1 = false;
        }

        // Checks if the player has pressed run
        if (Input.GetAxis("Fire1") != 0)
        {
            if (!bool2)
            {
                pressedCtrl = true;
                bool2 = true;
            }
            else
            {
                pressedCtrl = false;
            }
        }
        else
        {
            bool2 = false;
        }
    }
    private void DebugMenu()
    {
        // Sends values to the debug menu script
        debugMenu.xSpeed = xSpeed * movementMultiplier;
        debugMenu.ySpeed = ySpeed;
        debugMenu.zSpeed = zSpeed * movementMultiplier;
        debugMenu.isGrounded = isGrounded;
    }
}
