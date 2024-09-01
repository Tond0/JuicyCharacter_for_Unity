using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerStats : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody rb;
    [Header("Standing")]
    [SerializeField] private GroundCheck_Stats groundCheck_Grounded;
    [SerializeField] private GroundCheck_Stats groundCheck_Air;
    [Header("Movement")]
    [SerializeField] private MovementStats movement_Ground;
    [SerializeField] private MovementStats movement_Air;
    [Header("Air")]
    [SerializeField] private float gravityForce = -9.81f;
    [Header("Jump")]
    [SerializeField, Range(0, 100)] private float jumpForce;
    [SerializeField, Range(0, 100)] private float maxJumpForceDuration;
    [SerializeField, Range(0, 100)] private float minJumpForceDuration;
    [SerializeField, Range(1,50)] private float airTime_HorizontalBoost;
    [SerializeField, Range(0, 10)] private float coyoteTime;
    [Space(5)]
    [SerializeField, Range(0, 50)] private float gravityMultiplaier_Ascending;
    [SerializeField, Range(0, 50)] private float gravityMultiplaier_InputReleased;
    [SerializeField, Range(0, 50)] private float gravityMultiplaier_TopHeight;
    [SerializeField, Range(0, 50)] private float gravityMultiplaier_Descending;
    [Space(20)]
    [Header("Debug")]
    [SerializeField] private StateComponent stateComponent;

    #region Getter
    //Components
    public Rigidbody Rb { get => rb; }
    
    //Float
    public GroundCheck_Stats GroundCheck_Grounded { get => groundCheck_Grounded; }
    public GroundCheck_Stats GroundCheck_Air { get => groundCheck_Air; }
    
    //Movement
    public MovementStats Movement_Ground { get => movement_Ground; }
    public MovementStats Movement_Air { get => movement_Air; }
    
    //Jump
    public float JumpForce { get => jumpForce; }
    public float MaxJumpForceDuration { get => maxJumpForceDuration; }
    public float MinJumpForceDuration { get => minJumpForceDuration; }
    public float GravityForce { get => gravityForce; }
    public float CoyoteTime { get => coyoteTime; }
    public float GravityMultiplaier_Ascending { get => gravityMultiplaier_Ascending; }
    public float GravityMultiplaier_InputReleased { get => gravityMultiplaier_InputReleased; }
    public float GravityMultiplaier_TopHeight { get => gravityMultiplaier_TopHeight; }
    public float GravityMultiplaier_Descending { get => gravityMultiplaier_Descending; }
    public float AirTime_HorizontalBoost { get => airTime_HorizontalBoost; }
    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        GroundCheck_Stats groundCheck_Stats = stateComponent.CurrentState is Grounded ? GroundCheck_Grounded : GroundCheck_Air;

        Vector3 origin = transform.position + (groundCheck_Stats.HeightOffset * Vector3.up);
        Vector3 springDir = transform.up;

        Gizmos.DrawRay(origin, -springDir * groundCheck_Stats.HeightCheckBuffer);
        Gizmos.DrawRay(origin + Vector3.right * groundCheck_Stats.WideCheckBuffer / 2, -springDir * groundCheck_Stats.HeightCheckBuffer);
        Gizmos.DrawRay(origin - Vector3.right * groundCheck_Stats.WideCheckBuffer / 2, -springDir * groundCheck_Stats.HeightCheckBuffer);
        Gizmos.DrawRay(origin + Vector3.forward * groundCheck_Stats.WideCheckBuffer / 2, -springDir * groundCheck_Stats.HeightCheckBuffer);
        Gizmos.DrawRay(origin - Vector3.forward * groundCheck_Stats.WideCheckBuffer / 2, -springDir * groundCheck_Stats.HeightCheckBuffer);
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
