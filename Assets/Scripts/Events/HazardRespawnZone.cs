using UnityEngine;

public class HazardRespawnZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerRespawn respawn = other.GetComponent<PlayerRespawn>();
        if (respawn == null) return;

        respawn.SetHazardRespawn(transform.position);
    }
}