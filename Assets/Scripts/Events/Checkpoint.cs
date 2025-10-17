using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [Tooltip("Unique ID for this checkpoint (for save system)")]
    public string checkpointID = "Checkpoint_01";

    [Tooltip("Should this checkpoint be active from the start?")]
    public bool activeOnStart = false;

    [Header("Visual Feedback")]
    [Tooltip("Renderer to change color (optional)")]
    public SpriteRenderer checkpointRenderer;

    [Tooltip("Color when inactive")]
    public Color inactiveColor = Color.gray;

    [Tooltip("Color when activated")]
    public Color activeColor = Color.cyan;

    [Tooltip("Particle system to play on activation (optional)")]
    public ParticleSystem activationParticles;

    [Header("Audio")]
    [Tooltip("Sound effect name to play on activation")]
    public string activationSFX = "CheckpointActivated";

    [Header("Respawn Point")]
    [Tooltip("Where the player respawns (leave empty to use checkpoint position)")]
    public Transform respawnPoint;

    private bool isActivated = false;

    void Start()
    {
        if (respawnPoint == null)
        {
            respawnPoint = transform;
        }

        if (activeOnStart)
        {
            ActivateCheckpoint(false);
        }
        else
        {
            SetVisualState(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            ActivateCheckpoint(true);
        }
    }

    void ActivateCheckpoint(bool playEffects)
    {
        isActivated = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCheckpoint(respawnPoint);
            Debug.Log($"Checkpoint activated: {checkpointID}");
        }

        SetVisualState(true);

        if (playEffects)
        {
            if (activationParticles != null)
            {
                activationParticles.Play();
            }

            if (AudioManager.Instance != null && !string.IsNullOrEmpty(activationSFX))
            {
                AudioManager.Instance.PlaySFX(activationSFX);
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage("Checkpoint Activated", 2f);
            }
        }
    }

    void SetVisualState(bool active)
    {
        if (checkpointRenderer != null)
        {
            checkpointRenderer.color = active ? activeColor : inactiveColor;
        }
    }

    public void DeactivateCheckpoint()
    {
        isActivated = false;
        SetVisualState(false);
    }

    public bool IsActivated()
    {
        return isActivated;
    }

    void OnDrawGizmos()
    {
        Transform spawnPoint = respawnPoint != null ? respawnPoint : transform;

        Gizmos.color = isActivated ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
        Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + Vector3.up * 1f);
    }

    void OnDrawGizmosSelected()
    {
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            Gizmos.DrawCube(transform.position + (Vector3)boxCollider.offset, boxCollider.size);
        }

        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            Gizmos.DrawWireSphere(transform.position + (Vector3)circleCollider.offset, circleCollider.radius);
        }
    }
}