using UnityEngine;
public abstract class Pickup : MonoBehaviour
{
    protected abstract void Apply(GameObject player);

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Apply(other.gameObject);
        Destroy(gameObject);
    }
}
