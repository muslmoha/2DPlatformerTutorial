using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : State //Melle and ranged attackes will be handled differently, as well the scripts aren't on the same object as the animator so we need a
    //mediating script
{
    protected Transform attackPosition;

    protected bool isAnimationFinished;
    protected bool isPlayerInMinAgroRange;

    public AttackState(Entity entity, FiniteStateMachine stateMachine, string animBoolName, Transform attackPosition) : base(entity, stateMachine, animBoolName) //added transform because attackPosition will come from a child of our alive gameobject
    {
        this.attackPosition = attackPosition;
    }

    public override void DoChecks()
    {
        base.DoChecks();

        isPlayerInMinAgroRange = entity.CheckPlayerInMinAgroRange(); 
    }

    public override void Enter()
    {
        base.Enter();

        isAnimationFinished = false;
        entity.atsm.attackState = this;
        entity.SetVelocity(0f);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    public virtual void TriggerAttack()
    {

    }

    public virtual void FinishAttack()
    {
        isAnimationFinished = true;
    }
}
