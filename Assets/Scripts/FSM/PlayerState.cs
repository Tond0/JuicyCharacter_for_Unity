using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using static UnityEngine.UI.Image;


public abstract class PlayerState
{
    [SerializeField] protected StateComponent stateComponent;
    protected PlayerState nextState;

    protected Stopwatch stopwatch_state;
    public float StateDuration { get { return (float)stopwatch_state.Elapsed.TotalSeconds; } }

    //Probably to move in Playerstate but we will see
    [SerializeField] protected GroundCheck_Stats stats_GroundCheck;
    public GroundCheck_Stats Stats_GroundCheck { get => stats_GroundCheck; }

    public virtual void Enter()
    {
        stopwatch_state = new();
        stopwatch_state.Start();
        nextState = null;
    }

    public abstract void FixedRun();
    public abstract PlayerState Run();
    public virtual void Exit()
    {
        stopwatch_state.Reset();
        nextState = null;
    }

    [Serializable] public class MovementStats
    {
        [SerializeField] private float maxSpeed;
        [SerializeField] private float maxAcceleration;
        [SerializeField] private float maxDeceleration;
        [SerializeField] private AnimationCurve accelerationFactor;

        public float MaxSpeed { get => maxSpeed; }
        public float MaxAcceleration { get => maxAcceleration; }
        public float MaxDeceleration { get => maxDeceleration; }
        public AnimationCurve AccelerationFactor { get => accelerationFactor; }
    }

    [Serializable] public struct GroundCheck_Stats
    {
        [SerializeField] private float height;
        [SerializeField] private float heightOffset;
        [SerializeField] private float heightCheckBuffer;
        [SerializeField] private float wideCheckBuffer;
        [SerializeField] private float springStrength;
        [SerializeField] private float dampingForce;

        public float Height { get => height; }
        public float HeightOffset { get => heightOffset; }
        public float HeightCheckBuffer { get => heightCheckBuffer; }
        public float WideCheckBuffer { get => wideCheckBuffer; }
        public float SpringStrength { get => springStrength; }
        public float DampingForce { get => dampingForce; }
    }
}
