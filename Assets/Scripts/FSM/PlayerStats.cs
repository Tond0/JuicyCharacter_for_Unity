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
    [SerializeField] private float height;
    [SerializeField] private float heightOffset = 0.15f;
    [SerializeField] private float heightCheckBuffer;
    [SerializeField] private float wideCheckBuffer;
    [SerializeField] private float springStrength;
    [SerializeField] private float dampingForce;
    [Header("Movement")]
    [SerializeField] private MovementStats movement_Ground;
    [SerializeField] private MovementStats movement_Air;
    [Header("Air")]
    [SerializeField] private float gravityForce = -9.81f;
    [Header("Jump")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float maxJumpForceDuration;
    [SerializeField] private float minJumpForceDuration;
    [Space(5)]
    [SerializeField] private float gravityMultiplaier_Ascending;
    [SerializeField] private float gravityMultiplaier_InputReleased;
    [SerializeField] private float gravityMultiplaier_TopHeight;
    [SerializeField] private float gravityMultiplaier_Descending;

    #region Getter
    //Components
    public Rigidbody Rb { get => rb; }
    
    //Float
    public float Height { get => height; }
    public float HeightCheckBuffer { get => heightCheckBuffer; }
    public float DampingForce { get => dampingForce; }
    public float SpringStrength { get => springStrength; }
    public float WideCheckBuffer { get => wideCheckBuffer; }
    public float HeightOffset { get => heightOffset; }
    
    //Movement
    public MovementStats Movement_Ground { get => movement_Ground; }
    public MovementStats Movement_Air { get => movement_Air; }
    
    //Jump
    public float JumpForce { get => jumpForce; }
    public float MaxJumpForceDuration { get => maxJumpForceDuration; }
    public float MinJumpForceDuration { get => minJumpForceDuration; }
    public float GravityForce { get => gravityForce; }
    public float GravityMultiplaier_Ascending { get => gravityMultiplaier_Ascending; }
    public float GravityMultiplaier_InputReleased { get => gravityMultiplaier_InputReleased; }
    public float GravityMultiplaier_TopHeight { get => gravityMultiplaier_TopHeight; }
    public float GravityMultiplaier_Descending { get => gravityMultiplaier_Descending; }
    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Vector3 origin = transform.position + (HeightOffset * Vector3.up);
        Vector3 springDir = transform.up;

        Gizmos.DrawRay(origin, -springDir * heightCheckBuffer);
        Gizmos.DrawRay(origin + Vector3.right * wideCheckBuffer / 2, -springDir * heightCheckBuffer);
        Gizmos.DrawRay(origin - Vector3.right * wideCheckBuffer / 2, -springDir * heightCheckBuffer);
        Gizmos.DrawRay(origin + Vector3.forward * wideCheckBuffer / 2, -springDir * heightCheckBuffer);
        Gizmos.DrawRay(origin - Vector3.forward * wideCheckBuffer / 2, -springDir * heightCheckBuffer);
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
}
