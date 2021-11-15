using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDashState : PlayerAbilityState
{

    public bool CanDash { get; private set; } //Can we currently dash or not?
    
    private bool isHolding;
    private bool dashInputStop;

    private Vector2 dashDirection;
    private Vector2 dashDirectionInput;
    private Vector2 lastAIPos;

    private float lastDashTime;
    public PlayerDashState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName)
    {
    }
    public override void Enter()
    {
        base.Enter();

        CanDash = false;
        player.InputHandler.UseDashInput();

        isHolding = true;//always true by default because we need to be pressing the button to enter the state
        dashDirection = Vector2.right * player.FacingDirection; //Default dash direction will be the direction the player is facing

        Time.timeScale = playerData.holdTimeScale; //This will break our Time.time because it scales with time scale
        startTime = Time.unscaledTime;

        player.DashDirectionIndicator.gameObject.SetActive(true);
    }

    public override void Exit()
    {
        base.Exit();

        if(player.CurrentVelocity.y > 0)
        {
            player.SetVelocityY(player.CurrentVelocity.y * playerData.dashEndYMultiplier); //don't want to decrease y velocity if we are dashing down
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        if (!isExitingState)
        {
            player.Anim.SetFloat("yVelocity", player.CurrentVelocity.y);
            player.Anim.SetFloat("xVelocity", Mathf.Abs(player.CurrentVelocity.x));

            if (isHolding)
            {
                dashDirectionInput = player.InputHandler.DashDirectionInput;
                dashInputStop = player.InputHandler.DashInputStop;

                if(dashDirectionInput != Vector2.zero)
                {
                    dashDirection = dashDirectionInput;
                    dashDirection.Normalize();
                }
                //This makes it so that way we have an 8 direction dash and not a free angled dash
                float angle = Vector2.SignedAngle(Vector2.right, dashDirection);//returns the angle in degrees between the 2 vectors (from, to)
                player.DashDirectionIndicator.rotation = Quaternion.Euler(0f, 0f, angle - 45f);//Because our sprite starts at a 45 decree angle

                if(dashInputStop || Time.unscaledTime >= startTime + playerData.maxHoldTime)
                {
                    isHolding = false;
                    Time.timeScale = 1f;
                    startTime = Time.time;//track how long we've tracked for, but if we want to know how long we've been in the dash state, use a new variable
                    player.CheckIfShouldFlip(Mathf.RoundToInt(dashDirection.x));
                    player.RB.drag = playerData.drag; //This makes our dash have acceleration
                    player.SetVelocity(playerData.dashVelocity, dashDirection); //we set this twice because we start speeding up here with the high drag for build up
                    player.DashDirectionIndicator.gameObject.SetActive(false);
                    PlaceAfterImage(); //Place the initial AI
                }
            }
            else//actually perform the dash
            {
                player.SetVelocity(playerData.dashVelocity, dashDirection);//we do it again here to add more velocity and give the dash some actual distance and speed
                CheckIfShouldPlaceAfterImage(); //check for the next one
                //check time if we should leave or not
                if (Time.time >= startTime + playerData.dashTime)
                {
                    player.RB.drag = 0f;//should make it a var
                    isAbilityDone = true;
                    lastDashTime = Time.time;//make cd work
                }
            }
        }
    }

    private void CheckIfShouldPlaceAfterImage()
    {
        //compare current pos and lastAIpos
        if(Vector2.Distance(player.transform.position, lastAIPos) >= playerData.distBetweenAfterImages) //.Distance takes in 2 vector2's and returns the distance
        {
            PlaceAfterImage();//if enough distance is there, place an after image
        }
    }

    private void PlaceAfterImage()
    {//tells pool to place an after image, turns on an AI component and is placed by itself
        PlayerAfterImagePool.Instance.GetFromPool();//tell our pool to place an after image and save that image's position
        lastAIPos = player.transform.position; //saves the AI pos as the player pos when this function was called
    }

    public bool CheckIfCanDash()
    {
        return CanDash && Time.time >= lastDashTime + playerData.dashCooldown;//true if we can dash (i.e. on the ground) and cd has run out
        //This is fine only because it is never called when we change the time scale from 1
    }

    public void ResetCanDash() => CanDash = true;

}
