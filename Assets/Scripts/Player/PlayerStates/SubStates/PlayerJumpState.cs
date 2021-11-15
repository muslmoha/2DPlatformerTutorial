using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerAbilityState
{
    public int amountofJumpsLeft;
    public PlayerJumpState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName)
    {
        amountofJumpsLeft = playerData.amountOfJumps;
    }

    public override void Enter()
    {
        base.Enter();

        player.InputHandler.UseJumpInput(); //putting it here so we don't need to remember to add it when we are transitioning to this state
        player.SetVelocityY(playerData.jumpVelocity);
        isAbilityDone = true;
        amountofJumpsLeft--;
        player.InAirState.SetIsJumping();        
    }

    public bool CanJump()
    {
        if(amountofJumpsLeft > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ResetAmountOfJumpsLeft() => amountofJumpsLeft = playerData.amountOfJumps;

    public void DecreaseAmountOfJumpsLeft() => amountofJumpsLeft--;
}
