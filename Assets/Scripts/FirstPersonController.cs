using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirstPersonController : MonoBehaviour
{
    // Variables that need assigning
    public CharacterController playerCharacterController;
    public FeetCollider playerFeet;
    public DebugMenu debugMenu;
    public Camera playerCamera;
    public GameObject invisEffect;
    public Image bloodEffect;
    public Slider cloakMeter, healthMeter, staminaMeter;
    public GameObject enemylistenerWalk, enemylistenerRun, enemylistenerSprint;

    // Variables that need adjusting
    public float walkingSpeed = 3;
    public float mouseSensitivity = 2;
    public float jumpHeight = 1.5f;
    public float fallSpeed = 6;
    public float maxStamina = 20;
    public float staminaTimeToRegen = 5;
    public float cloakDuration = 8;
    public float cloakBufferRegenTime = 4;
    public float cloakRegenSpeed = 0.5f;
    public float timeToRegen = 10;
    public float healthRegenRate = 3;

    // Variables that need accesing
    public bool cloaked, dead;
    public float health;

    // Private Variables
    private float xSpeed, ySpeed, zSpeed, mouseX, mouseY, stepOffset, sprintTimer, movementMultiplier, cloakCurrentDuration, cloakRegenTimer, footStepTimer, listenerTimer, hurtTimer, stamina, staminaRegenTimer, deathTimer;
    private bool isGrounded, pressedShift, releasedShift, bool1, bool2, bool3, crouched, pressedCtrl, moving, pressedSpace, startedRunning, canRun, running, sprinting;
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

        cloakCurrentDuration = cloakDuration;

        invisEffect = GameObject.Find("InvisEffect");
        invisEffect.SetActive(false);

        bloodEffect = GameObject.Find("BloodEffect").GetComponent<Image>();

        cloakMeter = GameObject.Find("CloakMeter").GetComponent<Slider>();
        cloakMeter.maxValue = cloakDuration;
        cloakMeter.value = cloakDuration;

        health = 100;
        healthMeter = GameObject.Find("HealthMeter").GetComponent<Slider>();

        footStepTimer = 0.6f;

        stamina = maxStamina;
        staminaRegenTimer = staminaTimeToRegen;
        staminaMeter = GameObject.Find("StaminaMeter").GetComponent<Slider>();
        staminaMeter.maxValue = maxStamina;
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0 && !dead)
        {
            Death();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        if (dead)
        {
            deathTimer -= Time.deltaTime;
            if (deathTimer <= 0)
            {
                FindObjectOfType<SceneManagerScript>().ReloadScene();
            }
            DebugMenu();
            return;
        }
        HealthRegen();
        BloodEffect();
        if (Input.GetMouseButtonDown(0))
        {
            Cloak();
        }
        else if (cloaked)
        {
            CloakActive();
        }
        else
        {
            CloakDeactive();
        }

        FirstPersonCamera();
        ButtonPressedCheck();
        Stamina();
        Crouch();
        Sprinting();
        HorizontalMovement();
        VerticalMovement();
        Movement();
        FootSteps();
        DebugMenu();
    }
    private void Death()
    {
        dead = true;
        deathTimer = 4;
        invisEffect.SetActive(false);
        healthMeter.value = health;
        BloodEffect();
        playerCamera.transform.Rotate(0, 0, -20);
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
        if (pressedSpace && isGrounded && ySpeed == 0)
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

    private void Crouch()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (!crouched)
            {
                crouched = true;
                crouchAnimator.SetTrigger("crouched");
            }
            else
            {
                crouched = false;
                crouchAnimator.ResetTrigger("crouched");
            }
        }
    }

    private void Sprinting()
    {
        if (stamina == 0)
        {
            running = false;
            sprinting = false;
        }

        // Running and Sprinting Code
        if (pressedShift && stamina > 0)
        {
            if (!running)
            {
                running = true;
                startedRunning = true;
                sprintTimer = 0.2f;
            }
            else
            {
                startedRunning = false;
                sprintTimer = 0.2f;
            }    
        }
        else if (releasedShift && !startedRunning)
        {
            running = false;
        }

        if (Input.GetAxis("Fire3") != 0 && stamina > 0)
        {
            sprintTimer -= Time.deltaTime;

            if (sprintTimer <= 0 && running && !crouched)
            {
                sprinting = true;
            }
        }
        if (sprinting && releasedShift)
        {
            sprinting = false;
            running = false;
        }
        if (!moving)
        {
            sprinting = false;
            running = false;
        }

                                        // Movement Speed //
        if (sprinting && !crouched)
        {
            // Sprinting
            movementMultiplier = 2.2f;
        }
        else if (running)
        {
            if (!crouched)
            {
                // Running
                movementMultiplier = 1.4f;
            }
            else
            {
                // Crouch Running
                movementMultiplier = 1.1f;
            }
        }
        else
        {
            if (!crouched)
            {
                // Walking
                movementMultiplier = 1;
            }
            else
            {
                // Crouch Walking
                movementMultiplier = 0.6f;
            }
        }
    }

    private void Stamina()
    {
        // Stamina Use for sprinting and running
        if (movementMultiplier > 1 && stamina > 0)
        {
            if (sprinting)
            {
                stamina -= Time.deltaTime * 2.5f;
                if (stamina < 0)
                {
                    stamina = 0;
                }
                staminaRegenTimer = staminaTimeToRegen;
            }
            else
            {
                stamina -= Time.deltaTime;
                if (stamina < 0)
                {
                    stamina = 0;
                }
                staminaRegenTimer = staminaTimeToRegen;
            }
        }

        // Stamina Regen Timer
        if (!running && !sprinting && staminaRegenTimer > 0)
        {
            staminaRegenTimer -= Time.deltaTime;
            if (staminaRegenTimer < 0)
            {
                staminaRegenTimer = 0;
            }
        }
        // Stamina Regeneration
        if (stamina < maxStamina && staminaRegenTimer == 0)
        {
            if (!moving)
            {
                stamina += Time.deltaTime * 2;
            }
            else
            {
                stamina += Time.deltaTime;
            }
            if (stamina > maxStamina)
            {
                stamina = maxStamina;
            }
        }


        if (stamina == 0)
        {
            canRun = false;
            if (crouched)
            {
                movementMultiplier = 0.6f;
            }
            else
            {
                movementMultiplier = 1f;
            }
        }
        else
        {
            canRun = true;
        }

        staminaMeter.value = stamina;
    }

    private void Cloak()
    {
        if (!cloaked)
        {
            cloaked = true;
            MeshRenderer mr = this.GetComponent<MeshRenderer>();
            mr.enabled = false;
            invisEffect.SetActive(true);
        }
        else
        {
            cloaked = false;
            MeshRenderer mr = this.GetComponent<MeshRenderer>();
            mr.enabled = true;
            cloakRegenTimer = cloakBufferRegenTime;
            invisEffect.SetActive(false);
        }
    }

    private void CloakActive()
    {
        float movementValue = Mathf.Abs(xSpeed * movementMultiplier) + Mathf.Abs(zSpeed * movementMultiplier);
        cloakCurrentDuration -= Time.deltaTime * movementValue;
        if (movementValue < 1.2f)
        {
            cloakCurrentDuration -= Time.deltaTime * 1.2f;
        }
        else
        {
            cloakCurrentDuration -= Time.deltaTime * movementValue;
        }


        if (cloakCurrentDuration <= 0)
        {
            cloakCurrentDuration = 0;
            Cloak();
        }
        cloakMeter.value = cloakCurrentDuration;
    }

    private void CloakDeactive()
    {
        cloakRegenTimer -= Time.deltaTime;
        if (cloakRegenTimer <= 0)
        {
            cloakRegenTimer = 0;
            cloakCurrentDuration += Time.deltaTime * cloakRegenSpeed;
            if (cloakCurrentDuration >= cloakDuration)
            {
                cloakCurrentDuration = cloakDuration;
            }
        }
        cloakMeter.value = cloakCurrentDuration;
    }

    private void BloodEffect()
    {
        float alpha = 1 - health / 100;
        bloodEffect.color = new Color (255,255,255,alpha);
    }

    public void Hurt()
    {
        hurtTimer = timeToRegen;
    }

    private void HealthRegen()
    {
        hurtTimer -= Time.deltaTime;
        if (hurtTimer < 0)
        {
            hurtTimer = 0;
            health += Time.deltaTime * healthRegenRate;
            if (health > 100)
            {
                health = 100;
            }
        }
        healthMeter.value = health;
    }

    private void FootSteps()
    {
        if (moving)
        {
            footStepTimer -= Time.deltaTime * movementMultiplier;
        }

        if (footStepTimer <= 0)
        {
            if (movementMultiplier >= 2f)
            {
                FootStepSprint();
            }
            else if (movementMultiplier >= 1.5f)
            {
                FootStepRun();
            }
            else if (movementMultiplier >= 1f)
            {
                FootStepWalk();
            }
        }
        else
        {
            listenerTimer -= Time.deltaTime;
            if (listenerTimer <= 0)
            {
                enemylistenerRun.SetActive(false);
                enemylistenerWalk.SetActive(false);
                enemylistenerSprint.SetActive(false);
            }
        }
    }
    private void FootStepWalk()
    {
        FindObjectOfType<AudioManager>().Play("Player_Footstep_Walk");
        footStepTimer = 0.6f;
        enemylistenerWalk.SetActive(true);
        listenerTimer = 0.05f;
    }

    private void FootStepRun()
    {
        FindObjectOfType<AudioManager>().Play("Player_Footstep_Run");
        footStepTimer = 0.6f;
        enemylistenerRun.SetActive(true);
        listenerTimer = 0.05f;
    }

    private void FootStepSprint()
    {
        FindObjectOfType<AudioManager>().Play("Player_Footstep_Sprint");
        footStepTimer = 0.6f;
        enemylistenerSprint.SetActive(true);
        listenerTimer = 0.05f;
    }

    private void DebugMenu()
    {
        // Sends values to the debug menu script
        debugMenu.xSpeed = xSpeed * movementMultiplier;
        debugMenu.ySpeed = ySpeed;
        debugMenu.zSpeed = zSpeed * movementMultiplier;
        debugMenu.isGrounded = isGrounded;
        debugMenu.stamina = stamina;
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
        else if (bool1 == true)
        {
            releasedShift = true;
            //Debug.Log("Released Shift");
            bool1 = false;
        }
        else
        {
            releasedShift = false;
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

        // Checks if the player has pressed space
        if (Input.GetAxis("Jump") != 0)
        {
            if (!bool3)
            {
                pressedSpace = true;
                bool3 = true;
            }
            else
            {
                pressedSpace = false;
            }
        }
        else
        {
            bool3 = false;
        }
    }
}
