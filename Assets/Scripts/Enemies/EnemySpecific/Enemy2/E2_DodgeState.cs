using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E2_DodgeState : DodgeState
{
    private Enemy2 enemy;
    public E2_DodgeState(Entity entity, FiniteStateMachine stateMachine, string animBoolName, D_DodgeStateData stateData, Enemy2 enemy) : base(entity, stateMachine, animBoolName, stateData)
    {
        this.enemy = enemy;
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

        if (isDodgeOver)
        {

            if(isPlayerInMaxAgroRange && performCloseRangeAction)
            {
                stateMachine.ChangeState(enemy.meleeAttackState);
            }
            else if(isPlayerInMaxAgroRange && !performCloseRangeAction)
            {
                stateMachine.ChangeState(enemy.rangedAttackState);
            }
            else if (!isPlayerInMaxAgroRange)
            {
                stateMachine.ChangeState(enemy.lookForPlayerState);
            }
            /*if (!isPlayerInMaxAgroRange) Done differently
            {
                stateMachine.ChangeState(enemy.lookForPlayerState);
            }
            else if(isPlayerInMaxAgroRange && !performCloseRangeAction)
            {
                //TODO: perform ranged attack
            }
            else if (performCloseRangeAction)
            {
                stateMachine.ChangeState(enemy.meleeAttackState);
            }*/
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
}
