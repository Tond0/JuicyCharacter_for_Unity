using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

/// <summary>
/// Any state that inherit from this abstract class is going to be on the ground
/// </summary>
public abstract class Grounded : PlayerState, IGroundCheckableState, IFloatableState
{
    [Header("Components")]
    [SerializeField] protected StateGroundCheckComponent groundCheckComponent;
    public StateGroundCheckComponent GroundCheckComponent => groundCheckComponent;
    [SerializeField] protected StateFloatComponent floatComponent;
    public StateFloatComponent FloatableComponent => floatComponent;
    
    [Space(15)]
    [SerializeField, Tooltip("How much should the head move up and down?"), Range(0, 5)] private float headBobbingFrequency = 1;
    //Getter so that CinemachineHeadbobber.cs can adjust the frequency of the noise effect
    public float HeadBobbingFrequency { get => headBobbingFrequency; }



    public override void Enter()
    {
        base.Enter();

        //FIXME: What if the wallrunning state could bind to the OnStateChanged action and do it by itself?
        stateMachine.State_Wallrunning.OnGroundTouched();
    }
    
    public override void FixedRun()
    {
        //Check if we're still touching the ground
        if (groundCheckComponent.CheckGround(stateMachine, out RaycastHit rayHit))
            //If we are, then float!
            floatComponent.Float(stateMachine, rb, rayHit);
        else
            //If we are not then we're falling
            nextState = stateMachine.State_Falling;
    }
}
