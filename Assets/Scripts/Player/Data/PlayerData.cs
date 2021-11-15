using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "newPlayerData", menuName = "Data/Player Data/Base Data")]
public class PlayerData : ScriptableObject
{
    [Header("Move State")]
    public float movementVelocity = 10f;

    [Header("Jump State")]
    public float jumpVelocity = 15f;
    public int amountOfJumps = 1;

    [Header("In Air State")]
    public float coyoteTime = 0.2f;
    public float variableJumpHeightMultiplier = 0.5f;

    [Header("Check Variables")]
    public float groundCheckRadius = 0.3f;
    public float wallCheckDistance = 0.5f;

    [Header("Wall Slide State")]
    public float wallSlideVelocity = 3f;

    [Header("Wall Climb State")]
    public float wallClimbVelocity = 3f;

    [Header("Ledge Climb State")]
    public Vector2 startOffset;
    public Vector2 stopOffset;

    [Header("Wall Jump State")]
    public float wallJumpVelocity = 20f;
    public float wallJumpTime = 0.4f;//Stay in wall jump state for a certain time, so we can't move BACK to the wall
    public Vector2 wallJumpAngle = new Vector2(1, 2);

    [Header("Dash State")]
    public float dashCooldown = 0.5f;
    public float maxHoldTime = 1f;//Max amount of time we can stay in dash state
    public float holdTimeScale = 0.25f;//Time scale during dash hold state
    public float dashTime = 0.2f;//dash duration
    public float dashVelocity = 30f;
    public float drag = 10f;//improve the feeling of the dash, feels weightier I guess?
    public float dashEndYMultiplier = 0.2f; //like variable jump multiplier so we don''t fly off
    public float distBetweenAfterImages = 0.5f;

    [Header("Crouch States")]
    public float crouchMovementVelocity = 5f;
    public float crouchColliderHeight = 0.8f;
    public float standColliderHeight = 1.6f;

    [Header("Attack State")]
    public float attackDamage;
    public float stunDamage;
    public float attackRadius;
    public float attackCD;
    public LayerMask whatIsDamageable;

    public LayerMask whatIsGround;
}
