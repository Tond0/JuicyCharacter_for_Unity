using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A abstract class where CinemachineHeadBobber and CinemachineDutch we'll take place as children
/// </summary>
public abstract class CinemachinePlayerExtension : CinemachineExtension
{
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected StateComponent stateComponent;
}
