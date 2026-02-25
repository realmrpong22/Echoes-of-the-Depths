using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    public Vector3 CurrentCheckpoint { get; private set; }

    private void Start()
    {
        CurrentCheckpoint = transform.position;
    }

    public void SetCheckpoint(Vector3 position)
    {
        CurrentCheckpoint = position;
    }

    public void Respawn()
    {
        transform.position = CurrentCheckpoint;
    }
}