using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatDummyController : MonoBehaviour
{
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;
    [SerializeField] private float knockbackSpeedX, knockbackSpeedY, knockbackDuration, knockbackDeathSpeedY, knockbackDeathSpeedX, deathTorque;
    [SerializeField] private GameObject aliveGO, brokenTopGO, brokenBotGO, hitParticle;
    [SerializeField] private bool applyKnockBack, knockback;

    private PCTest pc;

    private Rigidbody2D rbAlive, rbBrokenTop, rbBrokenBot;

    private Animator aliveAnim;

    private int playerFacingDirection;

    private float knockbackStart;

    private bool playerOnLeft;

    private void Start()
    {
        currentHealth = maxHealth;
        pc = FindObjectOfType<PCTest>();
        aliveAnim = aliveGO.GetComponent<Animator>();
        rbAlive = aliveGO.GetComponent<Rigidbody2D>();
        rbBrokenTop = brokenTopGO.GetComponent<Rigidbody2D>();
        rbBrokenBot = brokenBotGO.GetComponent<Rigidbody2D>();
        aliveGO.SetActive(true);
        brokenBotGO.SetActive(false);
        brokenTopGO.SetActive(false);
    }

    private void Update()
    {
        CheckKnockback();
    }

    private void Damage(AttackDetails attackDetails)
    {
        currentHealth -= attackDetails.damageAmount;
        if(attackDetails.position.x < aliveGO.transform.position.x)
        {
            playerFacingDirection = 1;
        }
        else
        {
            playerFacingDirection = -1;
        }

        Instantiate(hitParticle, aliveAnim.transform.position, Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360f)));//Instantiate at dummy position at random rotation

        if(playerFacingDirection == 1)
        {
            playerOnLeft = true;
        }
        else
        {
            playerOnLeft = false;
        }

        aliveAnim.SetBool("playerOnLeft", playerOnLeft);
        aliveAnim.SetTrigger("Damage");

        if(applyKnockBack && currentHealth > 0.0f)
        {
            //apply knockback function
            Knockback();
        }

        if(currentHealth <= 0.0f)
        {
            Die();
        }
    }

    private void Die()
    {
        aliveGO.SetActive(false);
        brokenBotGO.SetActive(true);
        brokenTopGO.SetActive(true);

        brokenTopGO.transform.position = aliveGO.transform.position;
        brokenBotGO.transform.position = aliveGO.transform.position;

        rbBrokenBot.velocity = new Vector2(knockbackSpeedX * playerFacingDirection, knockbackSpeedY);
        rbBrokenTop.velocity = new Vector2(knockbackDeathSpeedX * playerFacingDirection, knockbackDeathSpeedY);
        rbBrokenTop.AddTorque(deathTorque * -playerFacingDirection, ForceMode2D.Impulse);//causes the broken top piece to rotate after breaking
    }

    private void Knockback()
    {
        knockback = true;
        knockbackStart = Time.time;
        rbAlive.velocity = new Vector2(knockbackSpeedX*playerFacingDirection, knockbackSpeedY);
    }

    private void CheckKnockback()
    {
        if(Time.time >= knockbackStart+knockbackDuration && knockback)
        {
            knockback = false;
            rbAlive.velocity = new Vector2(0.0f, rbAlive.velocity.y);
        }
    }
}
