using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiniteStateMachine //keeps track of what state the enemy is currently in and run the correct code
{
    public State currentState { get; private set; }

    public void Initialize(State startingState)
    {
        currentState = startingState;
        currentState.Enter();
    }

    public void ChangeState(State newState)
    {
        currentState.Exit(); //e.g. AliveState.Exit()
        currentState = newState;
        currentState.Enter(); //DeadState.Enter
    }
}
