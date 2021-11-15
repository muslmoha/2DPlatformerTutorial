using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInput playerInput;//Player input component on the player, used to get information about what the current control scheme is
    private Camera cam; //ref to main cam, access to functin that lets us make screen coordinates for mouse into world coordinates

    public Vector2 RawMovementInput { get; private set; }//We need to be able to track movement input from our states,
    public Vector2 RawDashDirectionInput { get; private set; }
    public Vector2Int DashDirectionInput { get; private set; } //Vector2 but only in x and y's
    public int NormInputX { get; private set; }
    public int NormInputY { get; private set; }
    //We have a reference of this in our player class, and our states have a reference to our player and thus can access this through public getters

    [SerializeField] private float inputHoldTime = 0.2f;

    private float jumpInputStartTime;
    private float dashInputStartTime;

    public bool JumpInput { get; private set; }
    public bool JumpInputStop { get; private set; }
    public bool GrabInput { get; private set; }
    public bool DashInput { get; private set; }
    public bool DashInputStop { get; private set; }
    public bool AttackInput { get; private set; }

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        cam = Camera.main;
    }

    private void Update()
    {
        CheckJumpInputHoldTime();
        CheckDashInputHoldTime();
    }
    public void OnMoveInput(InputAction.CallbackContext context)//allows the input to pass through important values
    {
        RawMovementInput = context.ReadValue<Vector2>(); //store input vector 2 in movementInput

        if(Mathf.Abs(RawMovementInput.x) > 0.5f)
        {
            NormInputX = (int)(RawMovementInput * Vector2.right).normalized.x; //Takes the input and makes it either 0 or 1, but normalized returns a float so cast to int
            //This is only for controllers because otherwise any small amount of y input will become 1 or -1
            //this creates a minimun threshold
        }
        else
        {
            NormInputX = 0;
        }
        if (Mathf.Abs(RawMovementInput.y) > 0.5f)
        {
            NormInputY = (int)(RawMovementInput * Vector2.up).normalized.y; //Takes the input and makes it either 0 or 1, but normalized returns a float so cast to int
        }
        else
        {
            NormInputY = 0;
        }
        
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            JumpInput = true;
            JumpInputStop = false;
            jumpInputStartTime = Time.time;
        }

        if (context.canceled)
        {
            JumpInputStop = true;
        }
    }

    public void OnGrabInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            GrabInput = true;
        }

        if (context.canceled)
        {
            GrabInput = false;
        }
    }

    public void OnDashInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            DashInput = true;
            DashInputStop = false;
            dashInputStartTime = Time.time;
        }
        else if (context.canceled)
        {
            DashInputStop = true;
        }
    }

    public void OnDashDirectionInput(InputAction.CallbackContext context)
    {
        //This function will have code designed to adapt to keyboard and controller inputs because they read in Vector2 inputs differently
        RawDashDirectionInput = context.ReadValue<Vector2>();

        if (playerInput.currentControlScheme == "Keyboard")
        {
            RawDashDirectionInput = cam.ScreenToWorldPoint((Vector3)RawDashDirectionInput) - transform.position;//makes it from player to cursor location
        }

        DashDirectionInput = Vector2Int.RoundToInt(RawDashDirectionInput.normalized);//converts V2 to V2Int by rounding to int on each V2 component
        //.normalized makes it so only have values of (0,1), (1,1) (1,0) etc...
    }

    public void OnAttackInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            AttackInput = true;
        }

        if (context.canceled)
        {
            AttackInput = false;
        }
    }

    public void UseJumpInput() => JumpInput = false;

    public void UseDashInput() => DashInput = false;

    private void CheckJumpInputHoldTime() //This sets a timer on our jump so that 0.2 seconds after it is buffered in, if it isn't used it is changed to false
    {//This way it doesn't feel like the input wasn't registered if the player pressed jump a split second before they hit the ground
        if(Time.time >= jumpInputStartTime + inputHoldTime)
        {
            JumpInput = false;
        }
    }
    
    private void CheckDashInputHoldTime()
    {
        if(Time.time >= dashInputStartTime + inputHoldTime)
        {
            DashInput = false;
        }
    }
}
//context gives value of the input
//also has a phase, when started, when performed, when cancelled - parallel getbuttondown, getbutton, getbuttonup