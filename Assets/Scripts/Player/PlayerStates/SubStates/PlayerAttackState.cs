using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackState : PlayerAbilityState
{
/*    private float timeSinceLastAttack;

    private bool isGrounded;*/
    private bool isAttacking;

    public bool CanAttack { get; private set; }

    Collider2D[] detectedObjects;

    private AttackDetails attackDetails;
    public PlayerAttackState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName)
    {
    }

    public override void AnimationFinishTrigger()
    {
        base.AnimationFinishTrigger();

        isAttacking = false;

        //timeSinceLastAttack = Time.time;
        player.Anim.SetBool("attack", isAttacking);
    }

    public override void AnimationTrigger()
    {
        base.AnimationTrigger();

        detectedObjects = player.DamageCheck();

        attackDetails.damageAmount = playerData.attackDamage;
        attackDetails.stunDamageAmount = playerData.stunDamage;
        attackDetails.position = player.transform.position; //This is the position of the person attacking

        foreach (Collider2D collider in detectedObjects)
        {
            collider.transform.parent.SendMessage("Damage", attackDetails);
        }
    }

    public override void DoChecks()
    {
        base.DoChecks();

        //isGrounded = player.CheckIfGrounded();
    }

    public override void Enter()
    {
        base.Enter();

        isAttacking = true;
        CanAttack = false;
        player.Anim.SetBool("attack", CanAttack);
        attackDetails.damageAmount = playerData.attackDamage;
        
    }

    public override void Exit()
    {
        base.Exit();

        isAttacking = false;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        player.SetVelocityZero();

        if (!isExitingState)
        {
            if (isAttacking)
            {
                player.Anim.SetBool("attack", isAttacking);
            }
            else
            {
                stateMachine.ChangeState(player.IdleState);
            }
            
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    public bool CheckIfCanAttack()
    {
        return CanAttack && Time.time >= startTime + playerData.attackCD;
    }

    public void ResetCanAttack() => CanAttack = true;
}
