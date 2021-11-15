using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : PlayerGroundedState
{
    public PlayerMoveState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName)
    {
    }

    public override void DoChecks()
    {
        base.DoChecks();
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        player.CheckIfShouldFlip(xInput);

        player.SetVelocityX(playerData.movementVelocity*xInput); //COULD * input.x BUT that makes it so on controller you can move proportional to how far the stick is tilted to one side
        if (!isExitingState)
        {
            if (xInput == 0)//This causes a problem because we run all our playerGrounded else if statements, but before changing states we can come here and trigger this statechange
            {//This resets our jump state, but then we also go to jump state
             //by adding !isExiting state, we stop this from hapening by saying if we're changing to another state we should ignore this code
                stateMachine.ChangeState(player.IdleState);
            }
            else if (yInput == -1)
            {
                stateMachine.ChangeState(player.CrouchMovestate);
            }
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
}
