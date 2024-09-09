using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Just a joke class to reward the play tester, should be remove if used in an actual game.
/// </summary>
public class YayTrigger : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.TryGetComponent<StateComponent>(out StateComponent stateComponent)) return;

        audioSource.Play();
    }
}
