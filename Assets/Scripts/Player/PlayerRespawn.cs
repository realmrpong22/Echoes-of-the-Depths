using Game.Player;
using UnityEngine;
using System.Collections;

public class PlayerRespawn : MonoBehaviour
{
    public Vector3 CurrentCheckpoint { get; private set; }
    public Vector3 CurrentHazardRespawn { get; private set; }

    [SerializeField] private float respawnLockTime = 0.35f;

    private PlayerMovement movement;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        CurrentCheckpoint = transform.position;
        CurrentHazardRespawn = transform.position;
    }

    public void SetCheckpoint(Vector3 position)
    {
        CurrentCheckpoint = position;
        CurrentHazardRespawn = position;
    }

    public void SetHazardRespawn(Vector3 position)
    {
        CurrentHazardRespawn = position;
    }

    public void Respawn()
    {
        transform.position = CurrentCheckpoint;
        StartCoroutine(LockMovement());
    }

    public void HazardRespawn()
    {
        transform.position = CurrentHazardRespawn;
        StartCoroutine(LockMovement());
    }

    private IEnumerator LockMovement()
    {
        if (movement != null)
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            movement.enabled = false;
        }

        yield return new WaitForSeconds(respawnLockTime);

        if (movement != null)
            movement.enabled = true;
    }
}