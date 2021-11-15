using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State //Defines how each state behaves
{
    protected FiniteStateMachine stateMachine;//private but any children of this class can access it
    protected Entity entity;

    public float startTime { get; protected set; }

    protected string animBoolName;

    public State(Entity entity, FiniteStateMachine stateMachine, string animBoolName)//Enemy will pass itself and the state machine it has to this class
    {
        this.entity = entity;
        this.stateMachine = stateMachine;
        this.animBoolName = animBoolName;
    }

    public virtual void Enter() //function can be redefined in child classes
    {
        startTime = Time.time; //always storing the start time, now not having to do this manually
        entity.anim.SetBool(animBoolName, true);
        DoChecks();
    } 

    public virtual void Exit()
    {
        entity.anim.SetBool(animBoolName, false);
    }

    public virtual void LogicUpdate()
    {

    }

    public virtual void PhysicsUpdate()
    {
        DoChecks(); //this will call all overritten DoCheck functions in child classes
    }

    public virtual void DoChecks() { }
}
