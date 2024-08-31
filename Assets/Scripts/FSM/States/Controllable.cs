using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Controllable : PlayerState
{
    protected Vector2 direction;
    protected PlayerStats.MovementStats movementStats;
    protected float acceleration_Multiplaier = 1;

    protected PlayerStats stats;

    private Rigidbody rb;

    protected Controllable(StateComponent stateComponent, Vector3 startDirection, PlayerStats.MovementStats movementStats) : base(stateComponent)
    {
        direction = startDirection;
        this.movementStats = movementStats;

        stats = stateComponent.PlayerStats;
    }

    public override void Enter()
    {
        base.Enter();

        InputManager.OnMoveFired += (Vector2 direction) => this.direction = direction;
        
        rb = stats.Rb;
    }

    public override void Exit()
    {
        InputManager.OnMoveFired -= (Vector2 direction) => this.direction = direction;
    }

    public override void FixedRun()
    {
        Move(stateComponent);
    }

    public override PlayerState Run()
    {        
        Look(stateComponent);

        return nextState;
    }

    #region GroundCheck
    protected bool CheckGround(Transform transform, PlayerStats.GroundCheck_Stats groundCheck_stats, out RaycastHit rayHit)
    {
        //Siccome deve stare in piedi la forza andrà verso l'alto
        Vector3 springDir = transform.up;

        Vector3 origin = transform.position + (groundCheck_stats.HeightOffset * Vector3.up);

        //Old version
        //Vector3 cubeCenter = new(origin.x, origin.y - groundCheck_stats.HeightCheckBuffer / 2, origin.z);
        //Vector3 size = new(wideCheckBuffer, heightCheckBuffer, wideCheckBuffer);
        //Vector3 size = groundCheck_stats.WideCheckBuffer * Vector3.one;

        //Spariamo 5 raycast in 5 posizioni diverse per il controllo del terreno
        //Centro
        if (Physics.Raycast(origin, -springDir, out rayHit, groundCheck_stats.HeightCheckBuffer)) return true;
        //Destra
        if (Physics.Raycast(origin + Vector3.right * groundCheck_stats.WideCheckBuffer / 2, -springDir, out rayHit, groundCheck_stats.HeightCheckBuffer)) return true;
        //Sinistra
        if (Physics.Raycast(origin - Vector3.right * groundCheck_stats.WideCheckBuffer / 2, -springDir, out rayHit, groundCheck_stats.HeightCheckBuffer)) return true;
        //Di fronte
        if (Physics.Raycast(origin + Vector3.forward * groundCheck_stats.WideCheckBuffer / 2, -springDir, out rayHit, groundCheck_stats.HeightCheckBuffer)) return true;
        //Dietro
        if (Physics.Raycast(origin - Vector3.forward * groundCheck_stats.WideCheckBuffer / 2, -springDir, out rayHit, groundCheck_stats.HeightCheckBuffer)) return true;

        //bool isGrounded = Physics.BoxCast(origin, size / 2, -springDir, out rayHit, Quaternion.identity, heightCheckBuffer);
        //bool isGrounded = Physics.OverlapBox(cubeCenter, size / 2, Quaternion.identity, groundMask, QueryTriggerInteraction.Ignore);

        return false;
    }

    protected bool CheckGround(Transform transform, PlayerStats.GroundCheck_Stats groundCheck_stats)
    {
        //Siccome deve stare in piedi la forza andrà verso l'alto
        Vector3 springDir = transform.up;

        Vector3 origin = transform.position + (groundCheck_stats.HeightOffset * Vector3.up);
        Ray standingRay = new(origin, -springDir);
        Vector3 size = groundCheck_stats.WideCheckBuffer * Vector3.one;

        //bool isGrounded = Physics.Raycast(standingRay, out RaycastHit rayHit, heightCheckBuffer);
        bool isGrounded = Physics.BoxCast(origin, size / 2, -springDir, Quaternion.identity, groundCheck_stats.HeightCheckBuffer);

        Debug.DrawRay(origin, -springDir * groundCheck_stats.HeightCheckBuffer, Color.red);

        return isGrounded;
    }
    #endregion

    #region Move
    private void Move(StateComponent stateComponent)
    {
        Vector3 currentVelocity = rb.velocity;

        // Calcola la direzione relativa alla telecamera
        Vector3 cameraRelativeDirection = RelateTo(direction, Camera.main.transform);

        // Calcola la velocità desiderata in base alla direzione della telecamera
        Vector3 desireVelocity = new Vector3(cameraRelativeDirection.x, 0, cameraRelativeDirection.z) * movementStats.MaxSpeed;

        float maxAcceleration;
        if (direction != Vector2.zero)
        {
            // Calcola il prodotto scalare tra la velocità corrente e quella desiderata
            float velDot = Vector3.Dot(currentVelocity.normalized, desireVelocity.normalized);
            maxAcceleration = movementStats.MaxAcceleration * movementStats.AccelerationFactor.Evaluate(velDot);
        }
        else
        {
            maxAcceleration = movementStats.MaxDeceleration;
        }

        // Calcola la variazione massima della velocità in questo frame
        float maxSpeedChange = maxAcceleration * Time.deltaTime * acceleration_Multiplaier;

        // Aggiorna la velocità corrente verso la velocità desiderata
        Vector3 finalVelocity = Vector3.MoveTowards(currentVelocity, desireVelocity, maxSpeedChange);

        finalVelocity.y = rb.velocity.y;

        Debug.DrawRay(stateComponent.PlayerStats.transform.position, desireVelocity, Color.red);
        Debug.DrawRay(stateComponent.PlayerStats.transform.position, currentVelocity, Color.green);

        // Assegna la nuova velocità al rigidbody
        rb.velocity = finalVelocity;
    }

    private Vector3 RelateTo(Vector2 inputDirection, Transform relativeTransform)
    {
        Vector3 camForward = relativeTransform.forward;
        Vector3 camRight = relativeTransform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 cameraRelativeForward = inputDirection.y * camForward;
        Vector3 cameraRelativeRight = inputDirection.x * camRight;

        Vector3 cameraRelativeMovement = cameraRelativeForward + cameraRelativeRight;
        return cameraRelativeMovement;
    }
    #endregion

    private void Look(StateComponent stateComponent)
    {
        Quaternion playerRot = stateComponent.transform.rotation;
        playerRot.y = Camera.main.transform.rotation.y;
        stateComponent.transform.rotation = playerRot;
    }
}
