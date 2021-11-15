using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//What should this state do?
/*
 if player in close action range, Dodge
 */
public class DodgeState : State
{
    private D_DodgeStateData stateData;

    protected bool isGrounded;//isDodging; don't need this because if we are here then we are dodging, but we need to check if we're grounded
    protected bool isDodgeOver;
/*    protected bool isPlayerInMinAgroRange; don't need this because we are only dodging if they are in close action range*/
    protected bool isPlayerInMaxAgroRange;
    protected bool performCloseRangeAction;
    public DodgeState(Entity entity, FiniteStateMachine stateMachine, string animBoolName, D_DodgeStateData stateData) : base(entity, stateMachine, animBoolName)
    {
        this.stateData = stateData;
    }

    public override void DoChecks()
    {
        base.DoChecks();
        isPlayerInMaxAgroRange = entity.CheckPlayerInMaxAgroRange();
        performCloseRangeAction = entity.CheckPlayerInCloseRangeAction();
        isGrounded = entity.CheckGround();
    }

    public override void Enter()
    {
        base.Enter();

        isDodgeOver = false;

        entity.SetVelocity(stateData.dodgeSpeed, stateData.dodgeAngle, -entity.facingDirection);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void LogicUpdate()
    {//over compicated it, we're not trying to process anything here, We're simply trying to decide when we come here what are the BASE truths and conditions
        //e.g when do we stop dodging
        base.LogicUpdate();
        
        if(Time.time >= startTime + stateData.dodgeTime && isGrounded)
        {
            isDodgeOver = true;
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
}
