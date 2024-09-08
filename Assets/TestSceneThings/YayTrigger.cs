using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YayTrigger : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.TryGetComponent<StateComponent>(out StateComponent stateComponent)) return;

        audioSource.Play();
    }
}
