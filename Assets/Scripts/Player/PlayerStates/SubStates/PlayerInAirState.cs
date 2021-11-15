using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInAirState : PlayerState
{
    //Input
    private int xInput;
    private bool grabInput;
    private bool dashInput;
    private bool jumpInput;
    private bool jumpInputStop;

    //Check
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isTouchingWallBack;
    private bool oldIsTouchingWall;
    private bool oldIstouchingWallBack;
    private bool isTouchingLedge;
    
    private bool coyoteTime;
    private bool wallJumpCoyoteTime;
    private bool isJumping;
    

    private float startWallJumpCoyoteTime;
    public PlayerInAirState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName)
    {
    }

    public override void DoChecks()
    {
        base.DoChecks();

        isGrounded = player.CheckIfGrounded();
        oldIsTouchingWall = isTouchingWall;
        oldIstouchingWallBack = isTouchingWallBack; //These are used because if we do our checks and the olds are true but the news are false then thats when start coyote time
        
        isTouchingWall = player.CheckIfTouchingWall();
        isTouchingWallBack = player.CheckIfBackTouchingWall();
        isTouchingLedge = player.CheckIfTouchingLedge();

        if(isTouchingWall && !isTouchingLedge)
        {
            player.LedgeClimbState.SetDetectedPosition(player.transform.position);//Saves the exact x,y pos of the player, MUST be done hear or else physics update might
            //move the palyer slightly and disrupt the math
        }

        if (!wallJumpCoyoteTime && !isTouchingWall & !isTouchingWallBack && (oldIsTouchingWall || oldIstouchingWallBack))
        {
            //so if coyote time hasn't started, we aren;t touching the walls, but in the previous frame we were
            StartWallJumpCoyoteTime();
        }
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();

        oldIsTouchingWall = false;
        oldIstouchingWallBack = false;
        isTouchingWall = false;
        isTouchingWallBack = false;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        CheckCoyoteTime();
        CheckWallJumpCoyoteTime();

        xInput = player.InputHandler.NormInputX;//So we can move in air, but only if we're not moving to a different state;
        jumpInput = player.InputHandler.JumpInput;
        jumpInputStop = player.InputHandler.JumpInputStop;
        grabInput = player.InputHandler.GrabInput;
        dashInput = player.InputHandler.DashInput;

        CheckJumpMultipier();

        if (isGrounded && player.CurrentVelocity.y < 0.01f)
        {
            stateMachine.ChangeState(player.LandState);
        }

        else if (isTouchingWall && !isTouchingLedge && !isGrounded)
        {
            stateMachine.ChangeState(player.LedgeClimbState);
        }
        else if(jumpInput && (isTouchingWall || isTouchingWallBack || wallJumpCoyoteTime)) //this goes above jump state because we want this to happen over a double jump
        {
            StoptWallJumpCoyoteTime();
            isTouchingWall = player.CheckIfTouchingWall();//if the player does a frame perfect flip, they jump straight up, this prevents that
            player.WallJumpState.DetermineWallJumpDirection(isTouchingWall);
            stateMachine.ChangeState(player.WallJumpState);
        }
        else if(jumpInput && player.JumpState.CanJump())
        {
            StoptWallJumpCoyoteTime();
            player.InputHandler.UseJumpInput();
            stateMachine.ChangeState(player.JumpState);
        }
        else if(isTouchingWall && grabInput && isTouchingLedge)
        {
            stateMachine.ChangeState(player.WallGrabState);
        }
        else if(isTouchingWall && xInput == player.FacingDirection && player.CurrentVelocity.y <= 0)//touching wall is true and input direction is in the direction of the wall
        {
            stateMachine.ChangeState(player.WallSlideState);
        }
        else if(dashInput && player.DashState.CheckIfCanDash())
        {
            stateMachine.ChangeState(player.DashState);
        }
        else
        {
            player.CheckIfShouldFlip(xInput);
            player.SetVelocityX(playerData.movementVelocity * xInput);//If we wanted to dampen air movement speed we could do that here

            player.Anim.SetFloat("yVelocity", player.CurrentVelocity.y);
            player.Anim.SetFloat("xVelocity", Mathf.Abs(player.CurrentVelocity.x));
        }
    }

    private void CheckJumpMultipier()
    {
        if (isJumping)
        {
            if (jumpInputStop)
            {
                player.SetVelocityY(player.CurrentVelocity.y * playerData.variableJumpHeightMultiplier);
                isJumping = false;//So it only happens once
            }
            else if (player.CurrentVelocity.y <= 0f) //We WERE jumping but now we're just falling
            {
                isJumping = false;
            }

        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    private void CheckCoyoteTime()
    {
        if(coyoteTime && Time.time > startTime + playerData.coyoteTime)
        {
            coyoteTime = false;
            player.JumpState.DecreaseAmountOfJumpsLeft();
        }
    }

    private void CheckWallJumpCoyoteTime()
    {
        if(wallJumpCoyoteTime && Time.time > startWallJumpCoyoteTime + playerData.coyoteTime)
        {
            wallJumpCoyoteTime = false;
        }
    }

    public void StartWallJumpCoyoteTime()
    {
        wallJumpCoyoteTime = true;
        startWallJumpCoyoteTime = Time.time;
    }
    public void StartCoyoteTime() => coyoteTime = true;

    public void StoptWallJumpCoyoteTime() => wallJumpCoyoteTime = false;

    public void SetIsJumping() => isJumping = true; //We need because we need to set is jumping = to true to begin with
    //Because if we did jump it will be process properly, if we fall off a ledge it will also account for that
    //THATS why we do this in the inAir state because we are only in the jump state for a brief second

}
