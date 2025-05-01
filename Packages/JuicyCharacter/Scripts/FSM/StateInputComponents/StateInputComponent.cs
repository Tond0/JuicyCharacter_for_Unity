using System;
using UnityEngine;

public abstract class StateComponent {}

public abstract class StateInputComponent : StateComponent
{
    /// <summary>
    /// Bind all the necessary input from the InputManager.
    /// </summary>
    public abstract void BindInput();
    /// <summary>
    /// Unbind all the necessary input from the InputManger.
    /// </summary>
    public abstract void UnbindInput();
}