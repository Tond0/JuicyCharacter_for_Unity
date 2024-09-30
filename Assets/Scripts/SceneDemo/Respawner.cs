using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<StateComponent>(out StateComponent player)) return;

        Transform playerTransform = player.transform;

        playerTransform.position = spawnPoint.position;
        playerTransform.rotation = spawnPoint.rotation;
    }
}
