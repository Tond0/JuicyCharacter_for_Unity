using System;
using UnityEngine;
using UnityEngine.Splines;

public class Grinding : Grounded, ILookableState
{
    [SerializeField] private StateLookComponent lookComponent;
    public StateLookComponent LookComponent => lookComponent;
    [SerializeField] private SplineAnimate splineAnimator;

    [NonSerialized] public SplineContainer splineContainer;

    public override void Enter()
    {
        base.Enter();

        //Inputs
        lookComponent.BindInput();

        //Starts to move along the spline
        splineAnimator.Play();
    }

    public override void FixedRun()
    {
        base.FixedRun();
        lookComponent.Look(stateMachine, rb);
    }

    /// <summary>
    /// OnCollisionEnter is called when this collider/rigidbody has begun
    /// touching another rigidbody/collider.
    /// </summary>
    /// <param name="other">The Collision data associated with this collision.</param>
    void OnCollisionEnter(Collision other)
    {
        Debug.Log("Piero");
    }

    public override void Exit()
    {
        //Stop moving along the spline
        splineAnimator.Pause();

        lookComponent.UnbindInput();
    }
}