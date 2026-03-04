using UnityEngine;
using Game.Player;
using System.Collections;

public class SpikeHazard : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private Transform respawnPoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth health = other.GetComponent<PlayerHealth>();
        PlayerRespawn respawn = other.GetComponent<PlayerRespawn>();
        if (health == null || respawn == null) return;

        health.TakeDamage(damage);

        respawn.HazardRespawn();
    }
} 