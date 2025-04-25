using System;
using UnityEngine;

public interface IMoveableState
{
    public StateMovementComponent MovementComponent { get; }
}

public interface ILookableState
{
    public StateLookComponent LookComponent { get; }
}

public interface IGroundCheckableState
{
    public StateGroundCheckComponent GroundCheckComponent { get; }
}

public interface IFloatableState
{
    public StateFloatComponent FloatableComponent { get; }
}