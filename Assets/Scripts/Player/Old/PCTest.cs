using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCTest : MonoBehaviour
{
    //config parameters
    [SerializeField] private float movementSpeed = 10f;

    //Dash Params
    [SerializeField] private float dashTime;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float distanceBetweenImages;
    [SerializeField] private float dashCoolDown;
    [SerializeField] private float knockbackDuration;
    private float knockbackStartTime;
    [SerializeField] Vector2 knockbackSpeed;
    private float dashTimeLeft; //How much long we should be dashing
    private float lastImageXpos;//last x coordinate where we placed an after image
    private float lastDash = -100;//Last time we started a dash, used against cooldown

    //Jumping Parameters
    [SerializeField] private float jumpSpeed = 16f;
    [SerializeField] private int amountOfJumps = 2;
    [SerializeField] private float wallSlidingSpeed;
    [SerializeField] private float movementForceInAir;
    [SerializeField] private float airDragMultiplier = 0.95f;
    [SerializeField] private float variableJumpHeightMultiplier = 0.5f;
    [SerializeField] private float ledgeClimbXOffset1 = 0f;
    [SerializeField] private float ledgeClimbYOffset1 = 0f;
    [SerializeField] private float ledgeClimbXOffset2 = 0f;
    [SerializeField] private float ledgeClimbYOffset2 = 0f;


    //Config Timers
    [SerializeField] private float wallJumpTimer;
    [SerializeField] private float wallJumpTimerSet = 0.5f;
    [SerializeField] private float jumpTimerSet = 0.15f;
    [SerializeField] private float turnTimerSet = 0.1f;
    private float turnTimer;

    private float jumpTimer;

    //Wall Jump Parameters
    [SerializeField] private Vector2 wallHopDirection;
    [SerializeField] private Vector2 wallJumpDirection;
    [SerializeField] private float wallHopForce;
    [SerializeField] private float wallJumpForce;
    private int facingDirection = 1;
    private int jumpsLeft = 1;
    private int lastWallJumpDirection;


    //Player state variables
    private float movementDirection;
    private bool isFacingRight = true;
    private bool isWalking = false;
    private bool isGrounded;
    private bool canNormalJump;
    private bool canWallJump;
    private bool isAttemptingToJump;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool checkJumpMultiplier;
    private bool canMove;
    private bool canFlip;
    private bool hasWallJumped;
    private bool isTouchingLedge;
    private bool canClimbLedge = false;
    private bool ledgeDetected;
    private bool isDashing;
    private bool knockback;


    //cached refs
    private Rigidbody2D playerRigidbody;
    private Animator myAnimator;

    //Object children
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private Transform ledgeCheck;

    private Vector2 ledgePosBot;
    private Vector2 ledgePos1;
    private Vector2 ledgePos2;

    // Start is called before the first frame update
    void Start()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        wallJumpDirection.Normalize();
        wallHopDirection.Normalize();
        //jumpsLeft = amountOfJumps;
        //whatisGround = LayerMask.GetMask("Foreground");
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
        CheckMovementDirection();
        UpdateAnimations();
        CheckIfCanJump();
        CheckIfWallSliding();
        //CheckLedgeClimb();
        CheckJump();
        CheckDash();
        CheckKnockback();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        CheckSurroundings();
    }

    private void CheckIfWallSliding()
    {
        if (isTouchingWall && movementDirection == facingDirection && playerRigidbody.velocity.y < 0 && !canClimbLedge)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void CheckInput()
    {
        movementDirection = Input.GetAxisRaw("Horizontal"); //returns 1 or -1 dependent on which direction we're looking

        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded || (jumpsLeft > 0 && isTouchingWall))
            {
                NormalJump();
            }
            else
            {
                jumpTimer = jumpTimerSet;
                isAttemptingToJump = true;
            }
            // myAnimator.SetTrigger("Jump");
            //This way the actually jump function is called by a key frame in the animation. Letting it wind up before jump velocity is added
        }

        if (Input.GetButtonDown("Horizontal") && isTouchingWall)
        {
            if (!isGrounded && movementDirection != facingDirection)
            {
                canMove = false;
                canFlip = false;

                turnTimer = turnTimerSet;
            }
        }

        if (turnTimer >= 0)
        {
            turnTimer -= Time.deltaTime;

            if (turnTimer <= 0)
            {
                canMove = true;
                canFlip = true;
            }
        }

        if (checkJumpMultiplier && !Input.GetButton("Jump"))
        {
            checkJumpMultiplier = false;
            playerRigidbody.velocity = new Vector2(playerRigidbody.velocity.x, playerRigidbody.velocity.y * variableJumpHeightMultiplier);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (Time.time >= lastDash + dashCoolDown)
            {
                AttemptToDash();
            }
        }
    }

    public bool GetDashStatus()
    {
        return isDashing;
    }

    private void AttemptToDash()
    {
        isDashing = true;
        dashTimeLeft = dashTime;
        lastDash = Time.time;

        PlayerAfterImagePool.Instance.GetFromPool(); //Get an after image from the pool
        lastImageXpos = transform.position.x;
    }

    private void CheckKnockback()
    {
        if(Time.time >= knockbackStartTime + knockbackDuration && knockback)
        {
            knockback = false;
            playerRigidbody.velocity = new Vector2(0f, playerRigidbody.velocity.y);
        }
    }

    public void Knockback(int direction)
    {
        knockback = true;
        knockbackStartTime = Time.time;
        playerRigidbody.velocity = new Vector2(knockbackSpeed.x * direction, knockbackSpeed.y);
    }

    private void CheckDash() //Set Dash Velocity and check if we should be dashing or if we should stop
    {
        if (isDashing)
        {
            if (dashTimeLeft > 0)
            {
                canMove = false; //Shouldnt be able to control the character while we dash
                canFlip = false; //Can't flip character mid dash
                playerRigidbody.velocity = new Vector2(dashSpeed * facingDirection, 0);//y velocity is 0 so they don't fall while dashing
                dashTimeLeft -= Time.deltaTime;

                if (Mathf.Abs(transform.position.x - lastImageXpos) > distanceBetweenImages) //If there is sufficiant space
                {
                    PlayerAfterImagePool.Instance.GetFromPool();//Get another instace form the pool
                    lastImageXpos = transform.position.x; //Reset the last image's x pos to the most recent Instance dequeued
                }
            }

            if(dashTimeLeft <= 0 || isTouchingWall)
            {
                isDashing = false;
                canMove = true;
                canFlip = true;
            }
        }
    }

    private void CheckIfCanJump()
    {
        if (isGrounded && playerRigidbody.velocity.y <= 0.01f)
        {
            jumpsLeft = amountOfJumps;
        }

        if (isTouchingWall)
        {
            canWallJump = true;
        }

        if (jumpsLeft <= 0) //scripts for double jumping
        {
            canNormalJump = false;
        }
        else
        {
            canNormalJump = true;
        }
    }

    private void UpdateAnimations()
    {
        myAnimator.SetBool("Walking", isWalking);
        myAnimator.SetBool("isGrounded", isGrounded);
        myAnimator.SetFloat("yVelocity", playerRigidbody.velocity.y);
        myAnimator.SetBool("isWallSliding", isWallSliding);
    }

    /*private void CheckLedgeClimb()
    {
        if (ledgeDetected && !canClimbLedge)
        {
            canClimbLedge = true;

            if (isFacingRight)
            {
                ledgePos1 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckDistance) - ledgeClimbXOffset1, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset1); // X param, returns the highest int below current values, this returning bottom left pos of tile
                ledgePos2 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckDistance) + ledgeClimbXOffset2, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset2);
            } // - OffSet to have the character be off the bottom left cornor
            else //I.E. character is facing left
            {
                ledgePos1 = new Vector2(Mathf.Ceil(ledgePos1.x - wallCheckDistance) + ledgeClimbXOffset1, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset1); //using Ceil because we want the RIGHT most int, or the right most cornor
                ledgePos2 = new Vector2(Mathf.Ceil(ledgePosBot.x - wallCheckDistance) - ledgeClimbXOffset2, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset2);
            }

            canMove = false; //Stop player from being able to move character or flip sprite
            canFlip = false;

            myAnimator.SetBool("CanClimbLedge", canClimbLedge);
        }

        if (canClimbLedge) //Hold pos of player at pos1
        {
            transform.position = ledgePos1; //if we can climb the ledge, keep the character sprite there every fram until we've climbed
        }
    }

    public void FinishLedgeClimb()
    {
        canClimbLedge = false;
        transform.position = ledgePos2;//new Vector2(groundCheck.position.x, groundCheck.position.y + 0.988f);
        canMove = true;
        canFlip = true;
        ledgeDetected = false;
        myAnimator.SetBool("CanClimbLedge", canClimbLedge);
    }*/

    private void CheckSurroundings()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);

        isTouchingLedge = Physics2D.Raycast(ledgeCheck.position, transform.right, wallCheckDistance, whatIsGround);

        if (isTouchingWall && !isTouchingLedge && !ledgeDetected)
        {
            ledgeDetected = true;
            ledgePosBot = wallCheck.position;
        }
    }

    private void CheckJump()
    {
        if (jumpTimer > 0)
        {
            //WallJump
            if (!isGrounded && isTouchingWall && movementDirection != 0 && movementDirection != facingDirection)
            {
                WallJump();
            }
            else if (isGrounded)
            {
                NormalJump();
            }


        }

        if (isAttemptingToJump)
        {
            jumpTimer -= Time.deltaTime;
        }

        if (wallJumpTimer > 0)
        {
            if (hasWallJumped && movementDirection == -lastWallJumpDirection)
            {
                playerRigidbody.velocity = new Vector2(playerRigidbody.velocity.x, 0f);
                hasWallJumped = false;
            }
            else if (wallJumpTimer <= 0)
            {
                hasWallJumped = false;
            }
            else
            {
                wallJumpTimer -= Time.deltaTime;
            }
        }

        /*        else if(isWallSliding && movementDirection == 0 && canJump) //Wall hop
                {
                    isWallSliding = false;
                    jumpsLeft--;
                    Vector2 forceToAdd = new Vector2(wallHopForce * wallHopDirection.x * -facingDirection, wallHopForce*wallHopDirection.y);
                    playerRigidbody.AddForce(forceToAdd, ForceMode2D.Impulse);
                }*/

    }

    private void NormalJump()
    {
        if (canNormalJump && !isWallSliding)
        {
            playerRigidbody.velocity = new Vector2(playerRigidbody.velocity.x, jumpSpeed);
            jumpsLeft--;
            jumpTimer = 0;
            isAttemptingToJump = false;
            checkJumpMultiplier = true;
        }

    }

    private void WallJump()
    {
        if ((isWallSliding || isTouchingWall) && movementDirection != 0 && canWallJump)
        {
            playerRigidbody.velocity = new Vector2(playerRigidbody.velocity.x, 0);
            isWallSliding = false;
            jumpsLeft = amountOfJumps;
            jumpsLeft--;
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * movementDirection, wallJumpForce * wallJumpDirection.y);
            playerRigidbody.AddForce(forceToAdd, ForceMode2D.Impulse);
            checkJumpMultiplier = true;
            jumpTimer = 0;
            isAttemptingToJump = false;
            turnTimer = 0;
            canMove = true;
            canFlip = true;
            hasWallJumped = true;
            wallJumpTimer = wallJumpTimerSet;
            lastWallJumpDirection = -facingDirection;
        }
    }

    private void ApplyMovement()
    {
        if (!isGrounded && !isWallSliding && movementDirection == 0 && !knockback)
        {
            playerRigidbody.velocity = new Vector2(playerRigidbody.velocity.x * airDragMultiplier, playerRigidbody.velocity.y);
        }
        else if (canMove && !knockback)//This way movement can't override knockback
        {
            playerRigidbody.velocity = new Vector2(movementSpeed * movementDirection, playerRigidbody.velocity.y);
        }

        if (isWallSliding)
        {
            if (playerRigidbody.velocity.y < -wallSlidingSpeed)
            {
                playerRigidbody.velocity = new Vector2(playerRigidbody.velocity.x, -wallSlidingSpeed);
            }
        }
    }

    public int GetFacingDirection()
    {
        return facingDirection;
    }

    private void CheckMovementDirection()
    {
        if (isFacingRight && movementDirection < 0)
        {
            Flip();
        }
        else if (!isFacingRight && (movementDirection > 0))
        {
            Flip();
        }
        if (Mathf.Abs(playerRigidbody.velocity.x) >= 0.01f && isGrounded)
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }
    }

    public void DisableFlip()
    {
        canFlip = false;
    }

    public void EnableFlip()
    {
        canFlip = true;
    }

    private void Flip()
    {
        if (!isWallSliding && canFlip && !knockback)
        {
            facingDirection *= -1;
            isFacingRight = !isFacingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }
    }

    private void OnDrawGizmos()
    {
        if (!groundCheck) { return; }
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
    }
}
